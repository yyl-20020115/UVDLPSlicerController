using System;
using Engine3D;
using System.Collections;
using System.Threading;
using UV_DLP_3D_Printer.Slicing;
using System.Collections.Generic;
namespace UV_DLP_3D_Printer;

public class Slicer
{
    public enum ESliceEvent 
    {
        ESliceStarted,
        ELayerSliced,
        ESliceCompleted,
        ESliceCancelled
    }
    public delegate void SliceEvent(ESliceEvent ev, int layer, int totallayers);

    private SliceFile m_sf; // the current file being sliced
    public SliceEvent Slice_Event;
    private Thread m_slicethread;
    private Object3D m_obj = null;
    private bool m_cancel = false;
    private bool isslicing = false;

    public Slicer() 
    {
    
    }
    public bool IsSlicing => isslicing;
    public void CancelSlicing() 
    {
        m_cancel = true;
    }
    public void RaiseSliceEvent(ESliceEvent ev, int curlayer, int totallayers)
    {
        Slice_Event?.Invoke(ev, curlayer, totallayers);
    }
    public int GetNumberOfSlices(SliceBuildConfig sp, Object3D obj) 
    {
        try
        {
            obj.FindMinMax();
            int numslices = (int)((obj.m_max.z - obj.m_min.z) / sp.ZThick);
            return numslices;
        }
        catch (Exception) 
        {
            return 0;
        }
    }

    // this function takes the object, the slicing parameters,
    // and the output directory. it generates the object slices
    // and saves them in the directory
    public SliceFile Slice(SliceBuildConfig sp, Object3D obj, string outdir) 
    {
            m_obj = obj;
            m_cancel = false;
            // create new slice file
            m_sf = new SliceFile(sp);
            m_slicethread = new Thread(new ThreadStart(slicefunc));
            m_slicethread.Start();
            isslicing = true;
            return m_sf;
    }

    private void slicefunc() 
    {
        try
        {

           // m_slices = new ArrayList();
            //iterate 
            //determine the number of slices
            m_obj.FindMinMax();
            int numslices = (int)((m_obj.m_max.z - m_obj.m_min.z) / m_sf.m_config.ZThick);

            double curz = (double)m_obj.m_min.z;
            RaiseSliceEvent(ESliceEvent.ESliceStarted, 0, numslices);
            DebugLogger.Instance().LogRecord("Slicing started");
            int c = 0;
            for (c = 0; c < numslices; c++)
            {
                if (m_cancel) 
                {
                    isslicing = false;
                    m_cancel = false;
                    RaiseSliceEvent(ESliceEvent.ESliceCancelled, c, numslices);
                    return;
                }
                //get a list of polygons at this slice z height that intersect
                var lstply = GetZPolys(m_obj, curz);
                //iterate through all the polygons and generat 2d line segments at this z level
                var lstintersections = GetZIntersections(lstply, curz);
                curz += m_sf.m_config.ZThick;
                Slice sl = new Slice();
                sl.m_segments = lstintersections;
                m_sf.m_slices.Add(sl);
                RaiseSliceEvent(ESliceEvent.ELayerSliced, c, numslices);
            }
            RaiseSliceEvent(ESliceEvent.ESliceCompleted, c, numslices);
            DebugLogger.Instance().LogRecord("Slicing Completed");
            isslicing = false;

        }
        catch (Exception ex)
        {
            DebugLogger.Instance().LogRecord(ex.Message);
        }        
    }

    /*
     This function takes in a list of polygons along with a z height.
     * What is returns is an ArrayList of 3d line segments. These line segments correspond
     * to the intersection of a plane through the polygons. Each polygon may return 0 or 1 line intersections 
     * on the 2d XY plane
     */
    public List<PolyLine3D> GetZIntersections(List<Polygon> polys,double zcur) 
    {
        try
        {
            List<PolyLine3D> lstlines = [];
            foreach (Polygon poly in polys) 
            {
                PolyLine3D s3d = poly.IntersectZPlane(zcur);
                if (s3d != null) 
                {
                    lstlines.Add(s3d);
                }
            }
            return lstlines;
        }
        catch (Exception) 
        {
            return null;
        }
    }

    /*
     Return a list of polygons that intersect at this zlevel
     */
    public List<Polygon> GetZPolys(Object3D obj, double zlev) 
    {
        List<Polygon> lst = [];
        foreach(Polygon p in obj.m_lstpolys)
        {
            //check and see if current z level is between any of the polygons z coords
            MinMax mm = p.CalcMinMax();
            if (mm.InRange(zlev)) 
            {
                lst.Add(p);
            }
        }
        return lst;
    }
}
