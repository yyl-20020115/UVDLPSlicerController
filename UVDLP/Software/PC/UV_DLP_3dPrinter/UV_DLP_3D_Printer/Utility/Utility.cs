using System;
using System.IO;
namespace UV_DLP_3D_Printer;

public static class Utility
{
    public static bool CreateDirectory(string path) 
    {
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (Exception ) 
        {
            return false;
        }
    }
}
