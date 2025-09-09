using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class tpPresetSetting : UserControl
{
    public tpPresetSetting()
    {
        InitializeComponent();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpPresetSettingMvvm : tpMvvm
{
    private readonly tpioSetting __tpioSetting = tpWorkspace.Workspace.IOSetting;

    private ObservableCollection<tpPresetItem> __presetItems = [];
    private Dictionary<string, tpioPreset> __preset = [];

    #region tpRelayCommand
    private tpRelayCommand __savePresetCommand;
    #endregion

    public tpPresetSettingMvvm() => InitializeProperties();

    #region ICommand
    public ICommand SavePreset => __savePresetCommand ??= new tpRelayCommand(OnSavePreset, CanSavePreset);
    #endregion

    public ObservableCollection<tpPresetItem> Items
    {
        get => __presetItems;
        set => Set(ref __presetItems, value);
    }

    private void InitializeProperties()
    {
        foreach (var item in __tpioSetting.CAMLens.Preset)
        {
            __presetItems.Add(new tpPresetItem
            {
                Key = item.Key,
                Number = item.Value.Number,
                Location = item.Value.Location,
                OldLocation = item.Value.Location,
                IsEnabled = true,

                Command = SavePreset
            });
        }
    }

    public void Save(Window window)
    {
        foreach (var item in Items)
        {
            if (__tpioSetting.CAMLens.Preset.TryGetValue(item.Key, out var value))
            {
                value.Location = item.Location;
            }
        }
        //tpioSetting.Save((Application.Current as App).Filename, __tpioSetting);

        //window?.Close();
    }

    private bool CanSavePreset(object parameter) => true;
    private void OnSavePreset(object parameter)
    {

    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpPresetItem : tpMvvm
{
    private string __location;
    private bool __isCommandEnabled;

    public tpPresetItem()
    {
        LostFocus = new tpRelayCommand(OnLostFocus, null);
    }

    #region ICommand
    public ICommand LostFocus { get; }
    #endregion

    public string Key { get; set; }
    public string Number { get; set; }
    public string Location
    {
        get => __location;
        set
        {
            if (Set(ref __location, value))
            {
#if DEBUG && FORDEBUG
                System.Diagnostics.Debug.WriteLine($"tpPresetItem: Location changed to {__location}");
#endif
                if (string.IsNullOrEmpty(__location) || string.IsNullOrEmpty(OldLocation))
                {
                    if (IsCommandEnabled) IsCommandEnabled = false;

                    return;
                }
                if (!__location.Equals(OldLocation))
                {
                    if (!IsCommandEnabled) IsCommandEnabled = true;
                }
                else
                {
                    if (IsCommandEnabled) IsCommandEnabled = false;
                }
            }
        }
    }
    public string OldLocation { get; set; }

    public bool IsEnabled { get; set; }
    public bool IsCommandEnabled
    {
        get => __isCommandEnabled;
        set => Set(ref __isCommandEnabled, value);
    }

    public ICommand Command { get; set; }

    private void OnLostFocus(object parameter)
    {
        if (string.IsNullOrEmpty(__location))
        {
#if DEBUG && FORDEBUG
            System.Diagnostics.Debug.WriteLine($"tpPresetItem::OnLostFocus     >>>     {__location}");
#endif
            Location = OldLocation;
        }
    }
}
