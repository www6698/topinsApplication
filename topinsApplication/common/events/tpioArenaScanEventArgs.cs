using ArenaNET;
using System.Diagnostics.CodeAnalysis;

namespace topinsApplication.Common.Events;

public delegate void ArenaScanEventHandler(object sender, tpioArenaScanEventArgs e);

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioArenaScanEventArgs(List<IDeviceInfo> devices)
{
    public List<IDeviceInfo> Devices { get; } = devices;
    public int Count { get; } = devices is null ? 0 : devices.Count;
}
