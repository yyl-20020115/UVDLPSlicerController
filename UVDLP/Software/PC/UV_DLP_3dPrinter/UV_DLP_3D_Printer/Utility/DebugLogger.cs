﻿using System;

namespace UV_DLP_3D_Printer;

public class DebugLogger : Logger
{
    private static DebugLogger _instance = null;            //Make this class a singleton

    public DebugLogger()
    {
        m_loggertype = ELoggerType.EDebugLogger;
        SetLogFile("Creation.log");
    }

    /// <summary>
    /// Destructor:: Clear out the instance
    /// </summary>
    ~DebugLogger()
    {
        CloseLogFile();
        _instance = null;
    }

    /// <summary>
    /// Gets the instance to the File Logger, creates if necessary
    /// </summary>
    /// <returns>Instance of this object</returns>
    public static DebugLogger Instance()
    {
        if (_instance == null)
            _instance = new DebugLogger();
        return _instance;
    }

    /// <summary>
    /// Outputs a record to the log File
    /// </summary>
    /// <param name="OutStr">string to write</param>
    public override void LogRecord(string OutStr)
    {
        string MsgOut;
        try
        {
            DateTime CurTime = DateTime.Now;
            MsgOut = CurTime.ToString("HH:mm:ss.fff") + "> " + OutStr;
            base.LogRecord(MsgOut);
        }
        catch (Exception) { }
    }

}
