using System;
using System.Timers;
using UV_DLP_3D_Printer.Drivers;
using UV_DLP_3D_Printer.Configs;


namespace UV_DLP_3D_Printer;

/*
 This class is the main interface to communicate with the printer
 * it controls the serial connection to the machine and provides 
 * data back from the machine (such as temperature readout)
 * This printer interface will support a *very limited subset of g-code commands
 * that it will use to control the printer
 * I'm not sure if choosing GCodes/MCodes is the *right choice,
 * but hey, whatever works for now...
 * 
 * I'm including a few manual commands in here because i intend on implementing a printer control panel
 * that can manually jog and set temperatures
 * 
 * the DeviceInterface can only handle one command at a time (by design)
 * the previous command must either time out or respond
 * 
 * GCode listing 
 * 
 * G1 - Coordinated Motion
 * G28 - Home given Axes to maximum
 * G92 - Define current position on axes
 * 
 * 
 * MCode listing
 * M0 Unconditional Halt
 * M17 enable motor(s)
 * M18 disable motor(s)     
 * M109 Snnn set build platform temperature in degrees Celsuis
 * M128 get position
 * 
 * 
 * 
 * The printer has the following features:
 * Z axis stepper motor
 * HBP 
 * fluid pump
 * z Min / z Max limit switches
 * 
 * 
 */

public enum EPIStatus 
{
    ETimedout, // the last command timed out
    EReady, // ready for next command
    EError, // something went wrong
    EConnected, // device is now connected
    EDisconnected // device disconnected
}

public class DeviceInterface
{
    //declare a delegate for the outside world to listen in
    public delegate void DeviceInterfaceStatus(EPIStatus status, string Command);
    public delegate void DeviceDataReceived(DeviceDriver device, byte[] data, int length);

    private int m_HBPtemp = -1;
    public DeviceInterfaceStatus StatusEvent;
    public DeviceDataReceived DataEvent;
    private DeviceDriver m_driver;
    protected Timer m_timeouttimer;
    private const int DEF_TIMEOUT = 500;// 1 second default timeout
    protected int m_timeoutms; 

    public DeviceInterface() 
    {
        m_timeoutms = DEF_TIMEOUT;
        m_timeouttimer = new Timer();
        m_timeouttimer.Elapsed += new ElapsedEventHandler(T_Elapsed);
        m_timeouttimer.Interval = m_timeoutms;
        m_driver = null;
        
    }

    void T_Elapsed(object sender, ElapsedEventArgs e)
    {
        // the command that was sent last has now timed out
        if (StatusEvent != null)
        {
            StatusEvent(EPIStatus.ETimedout, "Command Timed Out");
            DebugLogger.Instance().LogRecord("Command Timed out");
            m_timeouttimer.Enabled = false;
        }
    }

    // get and set the printdriver
    public DeviceDriver Driver
    {
        get { return m_driver; }
        set 
        {
            if (m_driver != null)
            {
                DeviceDriver olddriver = m_driver;
                olddriver.Disconnect(); // disconnect the old driver
                //remove the old device driver delegates
                olddriver.DataReceived -= new DeviceDriver.DataReceivedEvent(DriverDataReceivedEvent);
                olddriver.DeviceStatus -= new DeviceDriver.DeviceStatusEvent(DriverDeviceStatusEvent);
            }
            //set the new driver
            m_driver = value; 
            //and bind the delegates to listen to events
            m_driver.DataReceived += new DeviceDriver.DataReceivedEvent(DriverDataReceivedEvent);
            m_driver.DeviceStatus += new DeviceDriver.DeviceStatusEvent(DriverDeviceStatusEvent);
        }
    }
    public void Configure(ConnectionConfig cc) 
    {
        Driver.Configure(cc);
    }
    public void DriverDeviceStatusEvent(DeviceDriver device, EDeviceStatus status) 
    {
        switch (status) 
        {
            case EDeviceStatus.EError:
                if (StatusEvent != null)
                {
                    StatusEvent(EPIStatus.EError, "I/O Error");
                }                    
                break;
            case EDeviceStatus.EConnect:                    
                if (StatusEvent != null) 
                {
                    StatusEvent(EPIStatus.EConnected, "Connected");
                }
                break;
            case EDeviceStatus.EDisconnect:
                if (StatusEvent != null)
                {
                    StatusEvent(EPIStatus.EDisconnected, "Disconnected");
                }
                break;

        }
    }
    // this is called when we receive data from the device driver
    void DriverDataReceivedEvent(DeviceDriver device, byte[] data, int length) 
    {
        // stop the watchdog timer
        m_timeouttimer.Enabled = false;
        // raise the data event
        if (DataEvent != null) 
        {
            DataEvent(device, data, length);
        }
        //raise a data event notifying that we're ready for the next command
        if (StatusEvent != null)
        {
            StatusEvent(EPIStatus.EReady, "Ready");
        }
    }

    public int TimeoutMS
    {
        get { return m_timeoutms; }
        set { m_timeoutms = value; }
    }

    public int GetHBPTemp { get { return m_HBPtemp; } }

    public bool Connected { get { return m_driver.Connected; } }

    /*
     This function moves the Z axis to the specified position in mm 
     * at the specified feed rate
     */
    public void MoveTo(double zpos,double rate) 
    {
        string command = "G1 Z"  + zpos + " F" + rate + "\r\n";
        SendCommandToDevice(command);
    }
    /*
     This function moves the Z axis to by the distance in mm 
     * at the specified feed rate
     */
    public void Move(double zpos, double rate)
    {
        string command = "G1 Z" + zpos + " F" + rate + "\r\n";
        SendCommandToDevice("G91\r\n"); 
        SendCommandToDevice(command);
        SendCommandToDevice("G90\r\n");
        
    }

    /*
     This function stops all movement and motion
     */
    public void StopAll() 
    {
        SendCommandToDevice("M0\r\n");
    }
    /*
     This function will enable or disable the motors in the printer
     * based on the sent value
     */
    public void EnableMotors(bool val) 
    {
        if (val)
        {
            SendCommandToDevice("M17\r\n");
        }
        else 
        {
            SendCommandToDevice("M18\r\n");
        }            
    }
    /*
     This function sets the HBP Temperature
     */
    public void SetHBPTemp(int celsius) 
    {
        string sendstr = "M109 S";
        sendstr += celsius + "\r\n";
        SendCommandToDevice(sendstr);
        //M109 Snnn
    }
    public bool Disconnect() 
    {
        return m_driver.Disconnect();
    }
    public bool Connect()
    {
        try
        {
            return m_driver.Connect();
        }
        catch (Exception ) 
        {
            return false;
        }
    }


    public bool SendCommandToDevice(string command) 
    {
        try
        {
            if (m_driver.Write(command) > 0) 
            {
                //start a timer                    
                m_timeouttimer.Enabled = true;
                return true;
            }                                
            return false;
        }
        catch (Exception) 
        {
            return false;
        }
    }
}
