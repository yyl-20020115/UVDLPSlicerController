﻿using System;
using System.Windows.Forms;
using UV_DLP_3D_Printer.Drivers;
namespace UV_DLP_3D_Printer;

public partial class MachineConfigForm : Form
{
    private EDriverType m_saved;
    public MachineConfigForm()
    {
        InitializeComponent();
        FillMonitors(); // list out the system monitors
    }

    private void SetData() 
    {
        try
        {
            foreach (string s in Enum.GetNames(typeof(EDriverType))) 
            {
                lstDrivers.Items.Add(s);
            }
            lstDrivers.SelectedItem = UVDLPApp.Instance().m_printerinfo.m_driverconfig.m_drivertype.ToString();
            m_saved = UVDLPApp.Instance().m_printerinfo.m_driverconfig.m_drivertype;
            //() check connection
            if (UVDLPApp.Instance().m_deviceinterface.Connected)
            {
                grpDriver.Enabled = false;
            }
            else 
            {
                grpDriver.Enabled = true; ;
            }


            //list the drivers
            txtPlatWidth.Text = "" + UVDLPApp.Instance().m_printerinfo.m_PlatXSize;
            txtPlatHeight.Text = "" + UVDLPApp.Instance().m_printerinfo.m_PlatYSize;
            txtPlatTall.Text = UVDLPApp.Instance().m_printerinfo.m_PlatZSize.ToString();
            projwidth.Text = "" + UVDLPApp.Instance().m_printerinfo.XRes;
            projheight.Text = "" + UVDLPApp.Instance().m_printerinfo.YRes;
            txtZFeed.Text = UVDLPApp.Instance().m_printerinfo.m_ZMaxFeedrate.ToString();
            //select the current monitor
            int idx = 0;
            foreach (string s in lstMonitors.Items) 
            {
                if (s.Equals(UVDLPApp.Instance().m_printerinfo.m_monitorid)) 
                {
                    lstMonitors.SelectedIndex = idx;
                }
                idx++;
            }
        }
        catch (Exception) 
        {
        
        }
    }
    private bool GetData() 
    {
        try
        {
            if (lstDrivers.SelectedIndex != -1) 
            {
                UVDLPApp.Instance().m_printerinfo.m_driverconfig.m_drivertype = (EDriverType)Enum.Parse(typeof(EDriverType), lstDrivers.SelectedItem.ToString());
            }
            if (m_saved != UVDLPApp.Instance().m_printerinfo.m_driverconfig.m_drivertype) 
            {
                UVDLPApp.Instance().SetupDriver();
            }

            UVDLPApp.Instance().m_printerinfo.m_PlatXSize = double.Parse(txtPlatWidth.Text);
            UVDLPApp.Instance().m_printerinfo.m_PlatYSize = double.Parse(txtPlatHeight.Text);
            UVDLPApp.Instance().m_printerinfo.m_PlatZSize = double.Parse(txtPlatTall.Text);
            UVDLPApp.Instance().m_printerinfo.m_XDLPRes = double.Parse(projwidth.Text);
            UVDLPApp.Instance().m_printerinfo.m_YDLPRes = double.Parse(projheight.Text);
            UVDLPApp.Instance().m_printerinfo.m_ZMaxFeedrate = double.Parse(txtZFeed.Text);
            if (lstMonitors.SelectedIndex != -1)
            {
                UVDLPApp.Instance().m_printerinfo.m_monitorid = Screen.AllScreens[lstMonitors.SelectedIndex].DeviceName;// lstMonitors.Items[lstMonitors.SelectedIndex].ToString();
            }
            return true;
        }
        catch (Exception ex) 
        {
            DebugLogger.Instance().LogRecord(ex.Message);
            MessageBox.Show("Please check input parameters\r\n" + ex.Message, "Input Error");
            return false;
        }
    }

    private void CmdOK_Click(object sender, EventArgs e)
    {
        if (GetData())
        {
            UVDLPApp.Instance().SaveCurrentMachineConfig();
            Close();
        }
    }

    private void CmdCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void FrmMachineConfig_Load(object sender, EventArgs e)
    {
        SetData();
    }
    private void FillMonitors()
    {
        try
        {
            lstMonitors.Items.Clear();
            foreach (Screen s in Screen.AllScreens)
            {
                lstMonitors.Items.Add(s.DeviceName);
            }
            if (lstMonitors.Items.Count > 0)
                lstMonitors.SelectedIndex = 0;
        }
        catch (Exception)
        {

        }

    }

    private void CmdRefreshMonitors_Click(object sender, EventArgs e)
    {
        FillMonitors();
    }

    private void LstMonitors_SelectedIndexChanged(object sender, EventArgs e)
    {
        // get the projector width and fill in the projwidth and projheight
        if (lstMonitors.SelectedIndex == -1) return;
        try
        {
            projwidth.Text = "" + Screen.AllScreens[lstMonitors.SelectedIndex].Bounds.Width;
            projheight.Text = "" + Screen.AllScreens[lstMonitors.SelectedIndex].Bounds.Height;
        }
        catch (Exception ex) 
        {
            DebugLogger.Instance().LogRecord(ex.Message);
        }

    }
}
