using System;
using System.Windows.Forms;

namespace UV_DLP_3D_Printer.GUI;

public partial class SliceForm : Form
{
    public SliceForm()
    {
        InitializeComponent();
        //UVDLPApp.Instance().m_slicer.Sliced +=new Slicer.LayerSliced(LayerSliced);
        UVDLPApp.Instance().m_slicer.Slice_Event += new Slicer.SliceEvent(SliceEv);
    }

    private void SliceEv(Slicer.ESliceEvent ev, int layer, int totallayers)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new MethodInvoker(delegate() { SliceEv(ev, layer, totallayers); }));
        }
        else
        {
            switch (ev)
            {
                case Slicer.ESliceEvent.ESliceStarted:
                    cmdSlice.Text = "Cancel";
                    prgSlice.Maximum = totallayers - 1;
                    break;
                case Slicer.ESliceEvent.ELayerSliced:
                    prgSlice.Maximum = totallayers - 1;
                    prgSlice.Value = layer;
                    lblMessage.Text = "Slicing Layer " + (layer + 1).ToString() + " of " + totallayers.ToString();
                    
                    break;
                case Slicer.ESliceEvent.ESliceCompleted:
                    lblMessage.Text = "Slicing Completed";
                    cmdSlice.Text = "Slice!";
                    Close();
                    break;
                case Slicer.ESliceEvent.ESliceCancelled:
                    cmdSlice.Text = "Slice!";
                    lblMessage.Text = "Slicing Cancelled";
                    prgSlice.Value = 0;
                    break;
            }
        }
    }

    private void cmdSliceOptions_Click(object sender, EventArgs e)
    {
        SliceOptionsForm m_frmsliceopt = new SliceOptionsForm();
        m_frmsliceopt.Show();
    }
    private void frmSlice_FormClosed(object sender, FormClosedEventArgs e)
    {
        UVDLPApp.Instance().m_slicer.Slice_Event -=new Slicer.SliceEvent(SliceEv);
    }

    private void cmdSlice_Click(object sender, EventArgs e)
    {
        try
        {
            if (UVDLPApp.Instance().m_slicer.IsSlicing)
            {
                UVDLPApp.Instance().m_slicer.CancelSlicing();
            }
            else 
            {
                SliceBuildConfig sp = UVDLPApp.Instance().m_buildparms;
                sp.UpdateFrom(UVDLPApp.Instance().m_printerinfo);
                int numslices = UVDLPApp.Instance().m_slicer.GetNumberOfSlices(sp, UVDLPApp.Instance().m_obj);
                UVDLPApp.Instance().m_slicefile = UVDLPApp.Instance().m_slicer.Slice(sp, UVDLPApp.Instance().m_obj, ".");                
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Instance().LogRecord(ex.Message);
        }
    }


}
