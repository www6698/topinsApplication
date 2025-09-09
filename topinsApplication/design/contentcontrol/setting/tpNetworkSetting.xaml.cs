using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class tpNetworkSetting : UserControl
{
    public tpNetworkSetting()
    {
        InitializeComponent();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpNetworkSettingMvvm : tpMvvm
{

}