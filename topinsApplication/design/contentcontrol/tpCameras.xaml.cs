using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using topinsApplication.Common.Events;

namespace topinsApplication;

[ToolboxItem(false), DesignTimeVisible(false)]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class tpCameras : UserControl
{
    public tpCameras()
    {
        InitializeComponent();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpCamerasMvvm : tpMvvm
{
    private ObservableCollection<tpUIDItem> __uids;
    private tpUIDItem __selectedUID;

    private ArenaNET.IDeviceInfo __device;

    private bool __refreshing = true;

    private string __name;
    private string __id;
    private string __version;
    private string __address;
    private string __lla;
    private string __persistent;
    private string __dhcp;

    #region tpRelayCommand
    private tpRelayCommand __closeWindowCommand;
    #endregion

    #region ICommand
    public ICommand CloseWindow => __closeWindowCommand ??= new tpRelayCommand(OnCloseWindow, CanCloseWindow);
    #endregion

    public tpCamerasMvvm Mvvm => this;
    public ObservableCollection<tpUIDItem> IDS
    {
        get => __uids;
        set => Set(ref __uids, value);
    }

    public tpUIDItem SelectedIDItem
    {
        get => __selectedUID;
        set
        {
            if (Set(ref __selectedUID, value))
            {
                ModelName = __selectedUID.Device.ModelName;
                ID = __selectedUID.ID;

                if (__selectedUID.Device is not null)
                {
                    DeviceVersion = __selectedUID.Device.DeviceVersion;
                    Address = __selectedUID.Device.IpAddressStr;

                    Device = __selectedUID.Device;
                }
            }
        }
    }

    public ArenaNET.IDeviceInfo Device
    {
        get => __device;
        set => Set(ref __device, value);
    }

    public bool Refresh
    {
        get => __refreshing;
        set => Set(ref __refreshing, value);
    }

    public string ModelName
    {
        get => __name;
        set => Set(ref __name, value);
    }
    public string ID
    {
        get => __id;
        set => Set(ref __id, value);
    }
    public string DeviceVersion
    {
        get => __version;
        set => Set(ref __version, value);
    }
    public string Address
    {
        get => __address;
        set => Set(ref __address, value);
    }
    public string DHCP
    {
        get => __dhcp;
        set => Set(ref __dhcp, value);
    }
    public string Persistent
    {
        get => __persistent;
        set => Set(ref __persistent, value);
    }
    public string LLA
    {
        get => __lla;
        set => Set(ref __lla, value);
    }

    private void SetIPCamerasList(List<ArenaNET.IDeviceInfo> devices) => Application.Current.Dispatcher.BeginInvoke(() =>
    {
        if (IDS is not null && 0 < IDS.Count) IDS.Clear();

        IDS = [];

#if !OFFLINEIPCAMENUM
        foreach (var device in devices)
        {
            IDS.Add(new tpUIDItem
            {
                ID = device.SerialNumber,
                ModelName = device.ModelName,

                Device = device
            });
#else
        {
            UIDS.Add(new tpUIDItem { UID = "UIDIS 1", UIDName = "UID 1", Device = null });
            UIDS.Add(new tpUIDItem { UID = "UIDIS 2", UIDName = "UID 2", Device = null });
            UIDS.Add(new tpUIDItem { UID = "UIDIS 3", UIDName = "UID 3", Device = null });
            UIDS.Add(new tpUIDItem { UID = "UIDIS 4", UIDName = "UID 4", Device = null });
#endif
        }
        Refresh = true;
    });

    public void OnRefreshCompleted(object sedner, tpioArenaScanEventArgs e)
    {
        if (sedner is topinsContentMvvm content)
        {
            content.Refresh -= new EventHandler<tpioArenaScanEventArgs>(OnRefreshCompleted);

            SetIPCamerasList(e.Devices);
        }
    }
    private bool CanCloseWindow(object parameter) => true;
    private void OnCloseWindow(object parameter) => (parameter as Window)?.Close();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpUIDItem
{
    public string ID { get; set; }
    public string ModelName { get; set; }

    public ArenaNET.IDeviceInfo Device { get; set; }
}
