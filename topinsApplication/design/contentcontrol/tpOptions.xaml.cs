using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class tpOptions : UserControl
{
    public tpOptions()
    {
        InitializeComponent();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpOptionsMvvm : tpMvvm
{
    private bool __isApplyEnabled = false;
    private bool __isOKEnabled = false;

    #region tpRelayCommand
    private tpRelayCommand __applyWindowCommand;
    private tpRelayCommand __closeWindowCommand;
    #endregion

    public tpOptionsMvvm()
    {
        Tabs =
        [
            new tpTabItemMvvm("General", new tpGeneralSettingMvvm()),
            new tpTabItemMvvm("IP Camera", new tpIPCameraSettingMvvm()),
            new tpTabItemMvvm("Serial", new tpSerialSettingMvvm()),
            new tpTabItemMvvm("Presets", new tpPresetSettingMvvm()),
            new tpTabItemMvvm("Network", new tpNetworkSettingMvvm(), false)
        ];
    }

    #region ICommand
    public ICommand ApplyWindow => __applyWindowCommand ??= new tpRelayCommand(OnApplyWindow, CanApplyWindow);
    public ICommand CloseWindow => __closeWindowCommand ??= new tpRelayCommand(OnCloseWindow, CanCloseWindow);
    #endregion

    private tpTabItemMvvm __selectedTab;
    public tpTabItemMvvm SelectedTab
    {
        get => __selectedTab;
        set => Set(ref __selectedTab, value);
    }


    public bool IsApplyEnabled
    {
        get => __isApplyEnabled;
        set => Set(ref __isApplyEnabled, value);
    }
    public bool IsOKEnabled
    {
        get => __isOKEnabled;
        set => Set(ref __isOKEnabled, value);
    }

    public ObservableCollection<tpTabItemMvvm> Tabs { get; set; }

    public void SetTabEnabled(string header, bool isEnabled)
    {
        if (Tabs.FirstOrDefault(t => t.Header == header) is var tab)
        {
            tab.IsEnabled = isEnabled;
        }
    }

    private bool CanApplyWindow(object parameter) => true;
    private void OnApplyWindow(object parameter)
    {
        _ = __selectedTab;

        tpioSetting.Save((Application.Current as App).Filename, tpWorkspace.Workspace.IOSetting);

        if (parameter is Window window)
        {
            window.Close();
        }
    }

    private bool CanCloseWindow(object parameter) => true;
    private void OnCloseWindow(object parameter) => (parameter as Window)?.Close();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpTabItemMvvm(string header, object content, bool isEnabled = true) : tpMvvm
{
    private bool __isEnabled = isEnabled;

    public bool IsEnabled
    {
        get => __isEnabled;
        set => Set(ref __isEnabled, value);
    }

    public string Header { get; set; } = header;
    public object Content { get; set; } = content;
}
