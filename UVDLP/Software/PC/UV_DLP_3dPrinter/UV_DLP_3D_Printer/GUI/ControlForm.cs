using System;
using System.Windows.Forms;

namespace UV_DLP_3D_Printer.GUI;

public partial class ControlForm : Form
{
    public ControlForm()
    {
        InitializeComponent();
    }

    private void CmdUp_Click(object sender, EventArgs e)
    {
        try
        {
            double dist = double.Parse(txtdist.Text);
            //string movecommand = "G1 Z" + dist.ToString() + "\r\n";
            UVDLPApp.Instance().m_deviceinterface.Move(dist, UVDLPApp.Instance().m_printerinfo.m_ZMaxFeedrate); // (movecommand);
        }
        catch (Exception ex) 
        {
            DebugLogger.Instance().LogRecord(ex.Message);
            MessageBox.Show("Please check input parameters\r\n" + ex.Message, "Input Error");
        }
    }

    private void CmdDown_Click(object sender, EventArgs e)
    {
        try
        {
            double dist = double.Parse(txtdist.Text);
            dist *= -1.0;
            string movecommand = "G1 Z" + dist.ToString() + "\r\n";
            //UVDLPApp.Instance().m_deviceinterface.SendCommandToDevice(movecommand);
            UVDLPApp.Instance().m_deviceinterface.Move(dist, UVDLPApp.Instance().m_printerinfo.m_ZMaxFeedrate); //
        }
        catch (Exception ex)
        {
            DebugLogger.Instance().LogRecord(ex.Message);
            MessageBox.Show("Please check input parameters\r\n" + ex.Message, "Input Error");
        }
    }
}
