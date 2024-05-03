using System;
using System.IO;

namespace UV_DLP_3D_Printer;

//public delegate void LoggerMessageHandler(Logger o, string message);
public delegate void LoggerStatusHandler(Logger o, ELogStatus status, string message);
public enum ELogStatus
{
    ELogOpened,
    ELogClosed,
    ELogOpenError,
    ELogCloseError,
    ELogLowDiskSpace,
    ELogWriteError,
    ELogWroteRecord,
    ELogEnabled,
    ELogDisabled
}
public enum ELoggerType
{
    EDataLogger,
    EDebugLogger,
    ELogger,
    EHexLogger,
}
public class Logger
{
    public ELoggerType m_loggertype;
    // public event LoggerMessageHandler LoggerMessageEvent;
    public event LoggerStatusHandler LoggerStatusEvent;
    private StreamWriter sw = null;                 //Stream Writer
    private string m_logpath;
    private bool m_enablelogging = true;
    /// <summary>
    /// Logs a message to a Log File
    /// </summary> 
    public Logger()
    {
        m_loggertype = ELoggerType.ELogger;
    }

    /// <summary>
    /// Destructor:: Clear out the instance
    /// </summary>
    ~Logger()
    {
        CloseLogFile();
    }
    public string CreateTimeStampFileName(string basename)
    {
        string fn = basename;
        DateTime dt = DateTime.Now;
        // MM_DD_YYYY_HH_MM_SS.LOG
        fn += "_" + dt.Month.ToString();
        fn += "_" + dt.Day.ToString();
        fn += "_" + dt.Year.ToString();
        fn += "_" + dt.Hour.ToString();
        fn += "_" + dt.Minute.ToString();
        fn += "_" + dt.Second.ToString();
        fn += ".log";
        return fn;
    }
    public void RaiseLogStatusEvent(Logger log, ELogStatus status, string message)
    {
        LoggerStatusEvent?.Invoke(log, status, message);
    }
    public bool EnableLogging
    {
        get => m_enablelogging;
        set => RaiseLogStatusEvent(this, ELogStatus.ELogEnabled, ((m_enablelogging = value) ? "Logging Enabled" : "Logging Disabled"));
    }

    /// <summary>
    /// Outputs a record to the log File
    /// </summary>
    /// <param name="OutStr">string to write</param>
    public virtual void LogRecord(string OutStr)
    {
        RaiseLogStatusEvent(this, ELogStatus.ELogWroteRecord, OutStr);
        if (m_enablelogging == false) return;
        if (IsLogFileOpen() == false) return;
        try
        {
            sw.WriteLine(OutStr);
            sw.Flush();
            
        }
        catch (Exception)
        {
            RaiseLogStatusEvent(this, ELogStatus.ELogWriteError, "Error Writing to Log file");
        }
    }
    /// <summary>
    /// this function will take an array of bytes and log the bytes as
    /// a hex string with spaces between each byte (2 digit hex #).
    /// It logs 16 bytes per row 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="len"></param>
    public void LogHexRecord(byte[] data, int offset, int len)
    {
        //
        if (m_enablelogging == false) return;
        if (IsLogFileOpen() == false) return;
        try
        {

            int position = offset;
            bool done = false;
            string outstr = "";
            while (!done)
            {
                for (int c = 0; c < 16; c++)
                {
                    if (position >= offset + len) { done = true; break; }
                    byte d = data[position];
                    outstr += string.Format("{0:x2} ", d);
                    position++;
                }
                outstr += "\r\n";
            }
            sw.WriteLine(outstr);
            sw.Flush();
            RaiseLogStatusEvent(this, ELogStatus.ELogWroteRecord, outstr);
        }
        catch (Exception)
        {
            RaiseLogStatusEvent(this, ELogStatus.ELogWriteError, "Error Writing to Log file");
        }
    }
    private void OpenLogFile()
    {
        try
        {
            if (IsLogFileOpen()) // if it's already open, close it
            {
                CloseLogFile();
            }
            sw = new StreamWriter(m_logpath, false); // open it
            RaiseLogStatusEvent(this, ELogStatus.ELogOpened, "Log Opened");
        }
        catch (Exception)
        {
            RaiseLogStatusEvent(this, ELogStatus.ELogOpenError, "Error Opening log file");
        }
    }
    public void CloseLogFile()
    {
        try
        {
            if (IsLogFileOpen())
            {
                sw.Close();
                sw = null;
                RaiseLogStatusEvent(this, ELogStatus.ELogClosed, "Log file closed");
            }
        }
        catch (Exception)
        {
            RaiseLogStatusEvent(this, ELogStatus.ELogCloseError, "Error closing log file");
        }
    }
    private bool IsLogFileOpen()
    {
        if (sw == null) return false;
        return true;
    }
    public void SetLogFile(string fname)
    {
        try
        {
            m_logpath = fname;
            if (m_enablelogging)
            {
                if (IsLogFileOpen())
                {
                    CloseLogFile(); // Close and re-open to reset the name                        
                }
                OpenLogFile();
            }
        }
        catch (Exception)
        {
            // couldn't open log file
        }
    }
}
