namespace UV_DLP_3D_Printer.Drivers;

public class DriverFactory
{
    public static DeviceDriver Create(EDriverType type) => type switch
    {
        EDriverType.ENULL_DRIVER => new NULLdriver(),
        EDriverType.EGENERIC => new GenericDriver(),
        _ => null,
    };
}
