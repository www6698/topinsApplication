using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpCONST
{
    public const string TOPINSTITLE = "TOPINS CAMERA Management";
    public const string TOPINS = "topins";
    public const string SETTINGSFOLDER = "settings";
    public const string CPFOLDER = "cp";
    public const string STREAMFOLDER = "stream";
    public const string SCREENSHOTFOLDER = "screenshot";
    public const string XML = "xml";

    public const string OUTPUT = "Output";
    public const string ERRORS = "Errors";
    public const string PROPERTIES = "Properties";

    public const string TIMESTAMP01 = "yyMMdd:HH:mm:ss";

    public const string CAMERASELECTIONWINDOW = "Select Network CAMERA";
    public const string OPTIONSWINDOW = "Options";
    public const string OPTIONSOSDWINDOW = "Lens OSD Setting";

    public const int OSDTXCOUNT = 2;

    public const int RUN = 0;
    public const int SHUTDOWN = 1;
    public const int RECORD = 2;

    public const double FPS = 30.0;

    public const string ZOOMSPEED = "ZOOMSPEED";
    public const string FOCUSPEED = "FOCUSPEED";
    public const string IRISSPEED = "IRISSPEED";

    public const string DEVICEUSERID = "DeviceUserID";
    public const string USERDEFINEDNAME = "UserDefinedName";
    public const string DEVICEMODELNAME = "DeviceModelName";
    public const string MODELNAME = "ModelName";

    public const string INVALIDDEVICENAME = "Invalid DeviceName";

    public const string SETLENZSPEED = "Set Speed";

    public static Grid FindGridByName(string gridName)
    {
        var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

        if (window is null) return null;

        return FindChild<Grid>(window, gridName);
    }

    private static T FindChild<T>(DependencyObject parent, string childName) where T : FrameworkElement
    {
        if (parent is null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i) as T;

            if (child is not null && child.Name == childName) return child;

            var result = FindChild<T>(child, childName);

            if (result is not null) return result;
        }
        return null;
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public enum eTASKCREATION
{
    Init,
    Release
}