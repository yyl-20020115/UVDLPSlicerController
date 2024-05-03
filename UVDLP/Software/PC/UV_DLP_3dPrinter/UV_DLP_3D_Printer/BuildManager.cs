﻿using System;
using System.Drawing;
using System.Threading;
using UV_DLP_3D_Printer.Slicing;

namespace UV_DLP_3D_Printer;

/*
 * This class controls print jobs from start to finish. It feeds the generated sliced images
 * one at a time, along with control information and GCode over the PrinterInterface
 * 
 */
/*
 This class raises an event when the printing starts,
 * when it stops
 * when it's cancelled 
 * and each time the layer changes
 */
public enum EPrintStat
{
    EPrintStarted,
    EPrintCancelled,
    ELayerCompleted,
    EPrintCompleted
}



public delegate void delPrintStatus(EPrintStat printstat);
public delegate void delPrinterLayer(Bitmap bmplayer, int layernum, int layertype); // this is raised to display the next layer

public class BuildManager
{
    private  const int STATE_START                =0;
    private  const int STATE_DO_NEXT_LAYER        = 1;
    private  const int STATE_WAITING_FOR_LAYER    = 2;
    private  const int STATE_CANCELLED            = 3;
    private  const int STATE_IDLE                 = 4;
    private  const int STATE_DONE                 = 5;

    public const int SLICE_NORMAL                  =  0;
    public const int SLICE_BLANK                   = -1;
    public const int SLICE_CALIBRATION             = -2;

    

    public delPrintStatus PrintStatus; // the delegate to let the rest of the world know
    public delPrinterLayer PrintLayer; // the delegate to show a new layer
    private bool m_printing = false;
    private int m_curlayer = 0; // the current visible slice layer index #
    SliceFile m_sf = null; // current file we're building
    GCodeFile m_gcode = null; // a reference from the zactive gcode file
    int m_gcodeline = 0; // which line of GCode are we currently on.
    int m_state = STATE_IDLE; // the state machine variable
    private Thread m_runthread; // a thread to run all this..
    private bool m_running; // a var to control thread life

    Bitmap m_blankimage = null; // a blank image to display
    Bitmap m_calibimage = null; // a calibration image to display

    public BuildManager() 
    {
    
    }
    public bool IsPrinting => m_printing;

    private void RaiseStatusEvent(EPrintStat status) 
    {
        PrintStatus?.Invoke(status);
    }
    // This function is called to start the print job
    public void StartPrint(SliceFile sf, GCodeFile gcode) 
    {
        if (m_printing)  // already printing
            return;
        if (sf == null) 
        {
            DebugLogger.Instance().LogRecord("No slice file, build cannot start");
            RaiseStatusEvent(EPrintStat.EPrintCancelled);
            return;
        }
        if (gcode == null)
        {
            DebugLogger.Instance().LogRecord("No gcode file, build cannot start");
            RaiseStatusEvent(EPrintStat.EPrintCancelled);
            return;
        }
        // we really need to map onto the events of the PrinterInterface to determine
        // important stuff like current z position, HBP temp, etc...
        m_printing = true;
        m_sf = sf; // set the slicefile for rendering
        m_gcode = gcode; // set the file 
        m_state = STATE_START; // set the state machine as started
        m_runthread = new Thread(new ThreadStart(BuildThread));
        m_running = true;
        m_runthread.Start();
    }
    private int getvarfromline(string line) 
    {
        try
        {
            int val = 0;
            line = line.Replace(')', ' ');
            string[] lines = line.Split('>');
            if (lines[1].Contains("Blank"))
            {
                val = -1; // blank screen
            }
            else 
            {
                string []lns2 = lines[1].Trim().Split(' ');
                val = int.Parse(lns2[0].Trim()); // first should be variable
            }
            
            return val;
        }
        catch (Exception ex) 
        {
            DebugLogger.Instance().LogRecord(ex.Message);
            return 0;
        }            
    }
    /*
     This is the thread that controls the build process
     * it needs to read the lines of gcode, one by one
     * send them to the printer interface,
     * wait for the printer to respond,
     * and also wait for the layer interval timer
     */
    void BuildThread() 
    {            
        int now = Environment.TickCount;
        int nextlayertime = 0;
        while (m_running)
        {
            switch (m_state) 
            {
                case BuildManager.STATE_START:
                    //start things off, reset some variables
                    if (PrintStatus != null)
                    {
                        PrintStatus(EPrintStat.EPrintStarted);
                    }
                    m_state = BuildManager.STATE_DO_NEXT_LAYER; // go to the first layer
                    m_gcodeline = 0; // set the start line
                    m_curlayer = 0;

                    break;
                case BuildManager.STATE_WAITING_FOR_LAYER:
                    //check time var
                    if(Environment.TickCount >= nextlayertime)
                    {
                        m_state = BuildManager.STATE_DO_NEXT_LAYER; // move onto next layer
                    }
                    break;
                case BuildManager.STATE_IDLE:
                    // do nothing
                    break;
                case BuildManager.STATE_DO_NEXT_LAYER:
                    //check for done
                    if(m_gcodeline >= m_gcode.Lines.Length)
                    {
                        //we're done..
                        m_state = BuildManager.STATE_DONE;
                        continue;
                    }
                    // go through the gcode, line by line
                    string line = m_gcode.Lines[m_gcodeline++];
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        // if the line is a comment, parse it to see if we need to take action
                        if (line.Contains("(<Delay> "))// get the delay
                        {                                
                            nextlayertime = Environment.TickCount + getvarfromline(line);
                            m_state = STATE_WAITING_FOR_LAYER;
                            continue;
                        }
                        else if (line.Contains("(<Slice> "))//get the slice number
                        {
                            int layer = getvarfromline(line);
                            int curtype = BuildManager.SLICE_NORMAL; // assume it's a normal image to begin with
                            Bitmap bmp = null;

                            if (layer == SLICE_BLANK)
                            {
                                if (m_blankimage == null)  // blank image is null, create it
                                {
                                    m_blankimage = new Bitmap(m_sf.m_config.xres, m_sf.m_config.yres);
                                    // fill it with black
                                    using (Graphics gfx = Graphics.FromImage(m_blankimage))
                                    using (SolidBrush brush = new SolidBrush(Color.Black))
                                    {
                                        gfx.FillRectangle(brush, 0, 0, m_sf.m_config.xres, m_sf.m_config.yres);
                                    }
                                }
                                bmp = m_blankimage;
                                curtype = BuildManager.SLICE_BLANK;
                            }
                            else 
                            {
                                m_curlayer = layer;
                                bmp = m_sf.RenderSlice(m_curlayer); // get the rendered image slice
                            }
                            
                            //raise a delegate so the main form can catch it and display layer information.
                            if (PrintLayer != null)
                            {
                                PrintLayer(bmp, m_curlayer, curtype);
                            }
                        }
                        else if (line.Trim().StartsWith("("))// ignore line comment
                        {
                        }
                        else
                        {
                            //send to device
                            UVDLPApp.Instance().m_deviceinterface.SendCommandToDevice(line + "\r\n");
                        }
                    }
                    break;
                case BuildManager.STATE_DONE:
                    m_running = false;
                    m_state = BuildManager.STATE_IDLE;
                    //raise done message
                    if (PrintStatus != null)
                    {
                        PrintStatus(EPrintStat.EPrintCompleted);
                    }             
                    break;
            }
            Thread.Sleep(0);
        }
    }


    public int GenerateTimeEstimate() => -1;

    // This function manually cancels the print job
    public void CancelPrint() 
    {
        if (m_printing) // only if we're already printing
        {
            m_printing = false;
            m_curlayer = 0;
            m_state = BuildManager.STATE_IDLE;
            m_running = false;
            PrintStatus?.Invoke(EPrintStat.EPrintCancelled);
        }
    }
}
