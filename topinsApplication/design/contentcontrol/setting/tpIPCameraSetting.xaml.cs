using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace topinsApplication;

public partial class tpIPCameraSetting : UserControl
{
    public tpIPCameraSetting()
    {
        InitializeComponent();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpIPCameraSettingMvvm : tpMvvm
{
    
}