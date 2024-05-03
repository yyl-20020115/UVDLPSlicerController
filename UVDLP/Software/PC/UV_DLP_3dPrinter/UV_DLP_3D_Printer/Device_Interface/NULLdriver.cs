﻿using System;
using System.Threading;

namespace UV_DLP_3D_Printer.Drivers;

public class NULLdriver : DeviceDriver
{
    private Thread m_fakeresponse;
    private byte[] m_fakedatresp;

    public NULLdriver() 
    {
        m_drivertype = EDriverType.ENULL_DRIVER;
    }
    public override bool Connect() 
    {
        m_connected = true;
        RaiseDeviceStatus(this,EDeviceStatus.EConnect);
        return true;
    }
    public override bool Disconnect() 
    {
        m_connected = false;
        RaiseDeviceStatus(this,EDeviceStatus.EDisconnect);
        return true;
    }

    public override int Write(string line)
    {
        SendFakeResponse("OK\r\n");
        return line.Length;
    }

    public override int Write(byte[] data, int len) 
    {
        // raise an event to say we received an OK, may have to make this say 
        // we received after the "write" is complete
        // we may need to look for specific commands such as get HBP temp
        // to send back better fake data
        SendFakeResponse("OK\r\n");
        return len;
    }

    private void SendFakeResponse(string response) 
    {
        try
        {
            m_fakedatresp = GetBytes(response);
            m_fakeresponse = new Thread(new ThreadStart(FakeResponse));
            m_fakeresponse.Start();
        }
        catch (Exception ex) 
        {
            DebugLogger.Instance().LogRecord(ex.Message);
        }
    }
    // this is a small thread to send a fake response
    private void FakeResponse() 
    {
        try
        {
            Thread.Sleep(5);
            RaiseDataReceivedEvent(this, m_fakedatresp, m_fakedatresp.Length);
        }
        catch (Exception ex) 
        {
            DebugLogger.Instance().LogRecord(ex.Message);
        }

    }
    private static byte[] GetBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

}
