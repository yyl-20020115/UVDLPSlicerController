using System.IO;
namespace UV_DLP_3D_Printer;

public class Point3D
{
    public double x, y, z, a;
    public Point3D() 
    {
        x = y = z = a = 0.0;
    }
    public bool IsEqual(Point3D pnt) 
    {
        if (x == pnt.x && y == pnt.y && z == pnt.z)
            return true;
        return false;
    }
    public Point3D(double xp, double yp, double zp, double ap)
    {
        Set(xp, yp, zp, ap);
    }
    public void Set(double xp, double yp, double zp,double ap)
    {
        x = xp;
        y = yp;
        z = zp;
        a = ap;
    }
    public void Mul(ScaleFactor f) 
    {
        x *= f.x;
        y *= f.y;
        z *= f.z;
        a *= f.a;
    }
    public void Div(ScaleFactor f) 
    {
        x /= f.x;
        y /= f.y;
        z /= f.z;
        a /= f.a;
    }
    public void Set(Point3D pnt) 
    {
        Set(pnt.x, pnt.y, pnt.z,pnt.a);
    }

    public void Load(BinaryReader br) 
    {
        x = br.ReadSingle();
        y = br.ReadSingle();
        z = br.ReadSingle();
    }

    public void Load(StreamReader sr) 
    {
        x = double.Parse(sr.ReadLine());
        y = double.Parse(sr.ReadLine());
        z = double.Parse(sr.ReadLine());
        a = double.Parse(sr.ReadLine());
    }
    public void Save(StreamWriter sw) 
    {
        sw.WriteLine(x);
        sw.WriteLine(y);
        sw.WriteLine(z);
        sw.WriteLine(a);
    }
}
/// <summary>
/// This class is used to help convert points from steps per inch to 
/// steps, mm, or inches
/// </summary>
public class ScaleFactor : Point3D
{
    public ScaleFactor(ScaleFactor f) 
    {
        x = f.x;
        y = f.y;
        z = f.z;
        a = f.a;        
    }
    public ScaleFactor(double xp, double yp, double zp, double ap) 
    {
        x = xp;
        y = yp;
        z = zp;
        a = ap;
    }
    public Point3D Convert(Point3D pnt) 
    {
        return new Point3D(pnt.x * x, pnt.y * y, pnt.z * z,pnt.a *a);
    }
}
public class AxisLength : Point3D 
{
    public AxisLength(double xl, double yl, double zl,double a1)
    {
        x = xl;
        y = yl;
        z = zl;
        a = a1;
    }
}
