using System;

namespace UV_DLP_3D_Printer.Drivers;

public class GenericDriver : DeviceDriver
{
    public GenericDriver() 
    {
        m_drivertype = EDriverType.EGENERIC;
    }
    public override bool Connect() 
    {
        try
        {
            m_serialport.Open();
            if (m_serialport.IsOpen)
            {
                m_connected = true;
                RaiseDeviceStatus(this, EDeviceStatus.EConnect);
                return true;
            }
        }catch(Exception ex)
        {
            DebugLogger.Instance().LogRecord(ex.Message);
        }
        return false;
    }
    public override bool Disconnect() 
    {
        try
        {
            m_serialport.Close();
            m_connected = false;
            RaiseDeviceStatus(this, EDeviceStatus.EDisconnect);
            return true;
        }
        catch (Exception ex) 
        {
            DebugLogger.Instance().LogRecord(ex.Message);
            return false;
        }
        
    }
    public override int Write(byte[] data, int len) 
    {
        m_serialport.Write(data, 0, len);
        return len;
    }
    public override int Write(string line) 
    {
        m_serialport.Write(line);
        return line.Length; 
    }
}
