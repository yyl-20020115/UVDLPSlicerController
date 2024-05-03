using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using UV_DLP_3D_Printer;
namespace Engine3D;

public delegate void ModelAdded(Object3D model);
public delegate void ModelRemoved(Object3D model);
public class Engine3D
{
    public Camera m_camera = new();
    List<PolyLine3D> m_lines = [];
    public List<Object3D> m_objects = [];
    public event ModelAdded ModelAddedEvent;
    public event ModelRemoved ModelRemovedEvent;

    public Engine3D() 
    {
        AddGrid();
    }
    public void CameraRotate(double x,double y, double z)
    {
        m_camera.viewmat.Rotate(x, y, z);
    }
    public void CameraMove(Point3D pnt)
    {
        m_camera.viewmat.Translate(pnt.x, pnt.y, pnt.z);
    }
    public void CameraMove(double x,double y, double z)
    {
        m_camera.viewmat.Translate(x,y,z);
    }
    public void CameraReset() 
    {
        m_camera.Reset();
    }
    public void AddGrid() 
    {
        for (int x = -50; x < 51; x += 10)
        {
            AddLine(new PolyLine3D(new Point3D(x, -50, 0, 0), new Point3D(x, 50, 0, 0), Color.Blue));
        }
        for (int y = -50; y < 51; y += 10)
        {
            AddLine(new PolyLine3D(new Point3D(-50, y, 0, 0), new Point3D(50, y, 0, 0), Color.Blue));
        }
        AddLine(new PolyLine3D(new Point3D(0, 0, -10, 0), new Point3D(0, 0, 10, 0), Color.Blue));
    }
    //This function draws a cube the size of the build platform
    // The X/Y is centered along the 0,0 center point. Z extends from 0 to Z

    public void AddPlatCube() 
    {
        double platX, platY, platZ;
        double X, Y, Z;
        Color cubecol = Color.Gray;
        platX = UVDLPApp.Instance().m_printerinfo.m_PlatXSize;
        platY = UVDLPApp.Instance().m_printerinfo.m_PlatYSize;
        platZ = UVDLPApp.Instance().m_printerinfo.m_PlatZSize;
        X = platX / 2;
        Y = platY / 2;
        Z = platZ / 2;

        // bottom
        AddLine(new PolyLine3D(new Point3D(-X, Y, 0, 0), new Point3D(X, Y, 0, 0), cubecol));
        AddLine(new PolyLine3D(new Point3D(-X, -Y, 0, 0), new Point3D(X, -Y, 0, 0), cubecol));

        AddLine(new PolyLine3D(new Point3D(-X, -Y, 0, 0), new Point3D(-X, Y, 0, 0), cubecol));
        AddLine(new PolyLine3D(new Point3D( X, -Y, 0, 0), new Point3D( X, Y, 0, 0), cubecol));

        // Top
        AddLine(new PolyLine3D(new Point3D(-X, Y, Z, 0), new Point3D(X, Y, Z, 0), cubecol));
        AddLine(new PolyLine3D(new Point3D(-X, -Y, Z, 0), new Point3D(X, -Y, Z, 0), cubecol));

        AddLine(new PolyLine3D(new Point3D(-X, -Y, Z, 0), new Point3D(-X, Y, Z, 0), cubecol));
        AddLine(new PolyLine3D(new Point3D(X, -Y, Z, 0), new Point3D(X, Y, Z, 0), cubecol));

        // side edges
        AddLine(new PolyLine3D(new Point3D(X, Y, 0, 0), new Point3D(X, Y, Z, 0), cubecol));
        AddLine(new PolyLine3D(new Point3D(X, -Y, 0, 0), new Point3D(X, -Y, Z, 0), cubecol));

        AddLine(new PolyLine3D(new Point3D(-X, Y, 0, 0), new Point3D(-X, Y, Z, 0), cubecol));
        AddLine(new PolyLine3D(new Point3D(-X, -Y, 0, 0), new Point3D(-X, -Y, Z, 0), cubecol));


    
    }
    public void RemoveAllObjects() 
    {
        m_objects = [];

    }
    public void AddObject(Object3D obj) 
    {
        m_objects.Add(obj);
        ModelAddedEvent?.Invoke(obj);
    }
    public void RemoveObject(Object3D obj) 
    {
        m_objects.Remove(obj);
        if (ModelRemovedEvent != null)
        {
            ModelRemovedEvent(obj);
        }                 
    }
    public void AddLine(PolyLine3D ply) { m_lines.Add(ply); }
    public void RemoveAllLines() 
    {
        m_lines = [];
    }

    public void RenderGL() 
    {
        try
        {
            foreach (var obj in m_objects)
            {
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
                GL.Disable(EnableCap.LineSmooth);
                obj.RenderGL();
            }
            foreach (var ply in m_lines)
            {
                GL.Disable(EnableCap.Lighting);
                GL.Disable(EnableCap.Light0);
                GL.Enable(EnableCap.LineSmooth);
                ply.RenderGL();
            }
        }
        catch (Exception) { }
    }
    public void Render(PaintEventArgs e, int wid, int hei) 
    {

        foreach (var obj in m_objects) 
        {
            obj.Render(m_camera, e, wid, hei);
        }
        foreach (var ply in m_lines)
        {
            ply.Render(m_camera, e, wid, hei);
        }
    }
}
