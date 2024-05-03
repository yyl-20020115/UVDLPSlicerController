using System.Drawing;
using System.Windows.Forms;

namespace UV_DLP_3D_Printer;

/*
 This is the form that is used to display the Image that is sent to the DLP
 * it is sized to match the resolution of the screen that it displays on
 * and it contains 1 image / picture control
 */
public partial class DLPForm : Form
{
    public DLPForm()
    {
        InitializeComponent();
    }
    /*
     Shows the specified image on the picture control
     */
    public void ShowImage(Image i) 
    {
        picDLP.Image = i;
    }
}
