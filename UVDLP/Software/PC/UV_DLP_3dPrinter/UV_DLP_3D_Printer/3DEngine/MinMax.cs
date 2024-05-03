namespace UV_DLP_3D_Printer;

/* This class holds a Z min/max value for an object*/
public class MinMax
{
    public double m_min;
    public double m_max;
    public bool InRange(double z) => z >= m_min && z <= m_max;
}
