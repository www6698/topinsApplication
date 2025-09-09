using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class topinsToolBar : UserControl
{
    public topinsToolBar()
    {
        InitializeComponent();
    }

    private void OnToolBarLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnToolBarLoaded;

        MainWindow dwindow = Application.Current.MainWindow as MainWindow;

        dwindow.Closing += OnToolBarUnloaded;
    }

    private void OnToolBarUnloaded(object sender, CancelEventArgs e)
    {
        MainWindow dwindow = Application.Current.MainWindow as MainWindow;

        dwindow.Closing -= OnToolBarUnloaded;
    }

    private void OpenPresetContextMenu(object sender, RoutedEventArgs e)
    {
        if (sender is Button buton && buton.ContextMenu is not null)
        {
            buton.ContextMenu.PlacementTarget = buton;
            buton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            buton.ContextMenu.IsOpen = true;
        }
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class topinsToolBarMvvm : tpMvvm, IDisposable
{
    private readonly topinsContentMvvm __content;
    private readonly topinsControlPadMvvm __controlPad;

    private string __focusToolTip = "Manual";
    private string __irisToolTip = "Manual";

    private bool __isConnect = true;
    private bool __isDisconnect;
    private bool __isPip;
    private bool __isMovePip;
    private bool __isRecord;

    private bool __isLenzEnabled;

    private Visibility __isPreset;
    private bool __isFocus = true;
    private bool __isIris = true;

    private Geometry __fullScreenGeometry = (Geometry)Application.Current.TryFindResource("FullScreenExit");
    private Geometry __presetGeometry = (Geometry)Application.Current.TryFindResource("Load");

    private int __selectedPresetID = 0;

    private Visibility __isPopupVisible = Visibility.Visible;
    private bool __isPopupOpen = false;

    private DispatcherTimer __timer;

    private object __presetTag;
    private string __presetToolTip;

    private string __date = string.Empty;
    private string __time = string.Empty;

    #region tpRelayCommand
    private tpRelayCommand __connectCommand;
    private tpRelayCommand __disconnectCommand;

    private tpRelayCommand __startStreamCommand;
    private tpRelayCommand __stopStreamCommand;
    private tpRelayCommand __showPipCommand;
    private tpRelayCommand __movePipCommand;
    private tpRelayCommand __recordCommand;
    private tpRelayCommand __screenCommand;

    private tpRelayCommand __presetToolCommand;
    private tpRelayCommand __focusCommand;
    private tpRelayCommand __irisCommand;

    private tpRelayCommand __executePresetCommand;
    #endregion

    public topinsToolBarMvvm()
    {
        __timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        __timer.Tick += TopinsDisplayTimer;
        __timer.Start();

        __content = tpWorkspace.Workspace.Content;
        __controlPad = __content.ControlPad;
    }
    public void Dispose() => __timer.Stop();

    #region ICommand
    public ICommand Connect => __connectCommand ??= new tpRelayCommand(OnConnect, CanConnect);
    public ICommand Disconnect => __disconnectCommand ??= new tpRelayCommand(OnDisconnect, CanDisconnect);

    public ICommand PlayStream => __startStreamCommand ??= new tpRelayCommand(OnPlayStream, CanPlayStream);
    public ICommand StopStream => __stopStreamCommand ??= new tpRelayCommand(OnStopStream, CanStopStream);

    public ICommand ShowPip => __showPipCommand ??= new tpRelayCommand(OnShowPip, CanShowPip);
    public ICommand MovePip => __movePipCommand ??= new tpRelayCommand(OnMovePip, CanMovePip);
    public ICommand Record => __recordCommand ??= new tpRelayCommand(OnRecord, CanRecord);
    public ICommand Screen => __screenCommand ??= new tpRelayCommand(OnScreen, CanScreen);

    public ICommand PresetTool => __presetToolCommand ??= new tpRelayCommand(OnPresetTool, CanPresetTool);
    public ICommand Focus => __focusCommand ??= new tpRelayCommand(OnFocus, CanFocus);
    public ICommand Iris => __irisCommand ??= new tpRelayCommand(OnIris, CanIris);

    public ICommand ExecutePreset => __executePresetCommand ??= new tpRelayCommand(OnExecutePreset, CanExecutePreset);
    #endregion

    public string FocusToolTip
    {
        get => __focusToolTip;
        set => Set(ref __focusToolTip, value);
    }

    public string IrisToolTip
    {
        get => __irisToolTip;
        set => Set(ref __irisToolTip, value);
    }

    public bool IsConnect
    {
        get => __isConnect;
        set => Set(ref __isConnect, value);
    }

    public bool IsDisconnect
    {
        get => __isDisconnect;
        set => Set(ref __isDisconnect, value);
    }
    public bool IsPip
    {
        get => __isPip;
        set => Set(ref __isPip, value);
    }
    public bool IsMovePip
    {
        get => __isMovePip;
        set => Set(ref __isMovePip, value);
    }
    public bool IsLenzEnabled
    {
        get => __isLenzEnabled;
        set => Set(ref __isLenzEnabled, value);
    }
    public Visibility IsPreset
    {
        get => __isPreset;
        set => Set(ref __isPreset, value);
    }
    public bool IsRecord
    {
        get => __isRecord;
        set => Set(ref __isRecord, value);
    }
    public bool IsFocus
    {
        get => __isFocus;
        set => Set(ref __isFocus, value);
    }

    public bool IsIris
    {
        get => __isIris;
        set => Set(ref __isIris, value);
    }

    public Geometry FullScreenGeometry
    {
        get => __fullScreenGeometry;
        set => Set(ref __fullScreenGeometry, value);
    }

    public Geometry PresetGeometry
    {
        get => __presetGeometry;
        set => Set(ref __presetGeometry, value);
    }

    public object PresetTag
    {
        get => __presetTag;
        set => Set(ref __presetTag, value);
    }
    public string PresetToolTip
    {
        get => __presetToolTip;
        set => Set(ref __presetToolTip, value);
    }

    public FrameworkElement PresetToolBar { get; set; }

    public ObservableCollection<string> PresetID { get; set; } =
    [
        "id : 0x01", "id : 0x02", "id : 0x03", "id : 0x04", "id : 0x05", "id : 0x06", "id : 0x07"
    ];
    public int SelectedPresetID
    {
        get => __selectedPresetID;
        set => Set(ref __selectedPresetID, value);
    }
    public Visibility IsPopupVisible
    {
        get => __isPopupVisible;
        set => Set(ref __isPopupVisible, value);
    }

    public bool IsPopupOpen
    {
        get => __isPopupOpen;
        set
        {
            if (Set(ref __isPopupOpen, value))
            {
                tpWorkspace.Workspace.Content.IsPopupOpen = value;
            }
        }
    }

    public string Date
    {
        get => __date;
        set => Set(ref __date, value);
    }

    public string Time
    {
        get => __time;
        set => Set(ref __time, value);
    }

    public string RecordingTime
    {
        get => "";
    }

    private bool ButtonEnabled(object parameter)
    {
        if (parameter is null)
        {
            if (__content.IsStreaming)
            {
                return true;
            }
            return false;
        }
        return (bool)parameter;
    }

    public bool CanConnect(object parameter) => true;
    public void OnConnect(object parameter)
    {
        __controlPad.OpenPort(parameter);
    }

    public bool CanDisconnect(object parameter) => true;
    public void OnDisconnect(object parameter)
    {
        __controlPad.OpenPort(parameter);
    }
    public bool CanPlayStream(object parameter)
    {
        if (parameter is null)
        {
            if (__content.Device is not null && !__content.IsStreaming)
            {
                return true;
            }
            return false;
        }
        return (bool)parameter;
    }

    public void OnPlayStream(object parameter)
    {

    }

    public bool CanStopStream(object parameter) => ButtonEnabled(parameter);
    public void OnStopStream(object parameter) => _ = __content.Stop();

    public bool CanShowPip(object parameter) => true;
    public void OnShowPip(object parameter)
    {

    }
    public bool CanMovePip(object parameter) => true;
    public void OnMovePip(object parameter)
    {

    }
    public bool CanRecord(object parameter) => true;
    public void OnRecord(object parameter)
    {
        __content.Record((bool)parameter);
    }

    public bool CanScreen(object parameter) => ButtonEnabled(parameter);
    public void OnScreen(object parameter)
    {
        __content.Screen(string.Empty);
    }

    public bool CanPresetTool(object parameter) => true;
    public void OnPresetTool(object parameter)
    {
        //if (parameter is FrameworkElement toolbar)
        //{
        //    Storyboard storyboard;

        //    switch (IsPreset)
        //    {
        //        case Visibility.Visible:
        //            IsPreset = Visibility.Collapsed;
        //            storyboard = (Storyboard)toolbar.FindResource("ShowToolBarAnimation");
        //            break;
        //        case Visibility.Collapsed:
        //            IsPreset = Visibility.Visible;
        //            storyboard = (Storyboard)toolbar.FindResource("HideToolBarAnimation");
        //            break;
        //        default:
        //            return;
        //    }
        //    storyboard.Begin(PresetToolBar);
        //}
        switch ((string)parameter)
        {
            case tpCommandKeys.PRESETSAVE:
                PresetGeometry = (Geometry)Application.Current.FindResource("Save");
                PresetTag = tpCommandKeys.PRESETSAVE;
                PresetToolTip = tpCommandKeys.SAVE;
                break;
            case tpCommandKeys.PRESETCALL:
                PresetGeometry = (Geometry)Application.Current.FindResource("Load");
                PresetTag = tpCommandKeys.PRESETCALL;
                PresetToolTip = tpCommandKeys.CALL;
                break;
            case tpCommandKeys.PRESETCLEAR:
                PresetGeometry = (Geometry)Application.Current.FindResource("Clear");
                PresetTag = tpCommandKeys.PRESETCLEAR;
                PresetToolTip = tpCommandKeys.CLEAR;
                break;
            default:
                return;
        }
    }

    public bool CanExecutePreset(object parameter) => true;
    public void OnExecutePreset(object parameter)
    {
        if (-1 == __selectedPresetID || string.IsNullOrEmpty(__presetToolTip)) return;

        __content.PresetTool(__presetToolTip, __selectedPresetID + 1);
    }

    public bool CanFocus(object parameter) => true;
    public void OnFocus(object parameter)
    {
#if DEBUG && !FORDEBUG
        System.Diagnostics.Debug.WriteLine($"       topinsToolBarMvvm::OnFocus     >>>     {parameter}");
#endif
#if REFERENCE
        string key;

        if (parameter is bool method)
        {
            if (method)
            {
                FocusToolTip = "Auto";
                key = tpCommandKeys.FOCUSAUTO;
            }
            else
            {
                FocusToolTip = "Manual";
                key = tpCommandKeys.FOCUSMANUAL;
            }
        }
        else
        {
            FocusToolTip = "Full";
            key = tpCommandKeys.FOCUSFULL;
        }
#endif
        string key;

        if ((bool)parameter)
        {
            FocusToolTip = "Auto";
            key = tpCommandKeys.FOCUSAUTO;
        }
        else
        {
            FocusToolTip = "Manual";
            key = tpCommandKeys.FOCUSMANUAL;
        }
        __content.Focus(key);
    }

    public bool CanIris(object parameter) => true;
    public void OnIris(object parameter)
    {
        __content.Iris((bool)parameter);
    }
    private void TopinsDisplayTimer(object sender, EventArgs e)
    {
        Date = DateTime.Now.ToString("yyyy-MM-dd");
        Time = DateTime.Now.ToString("HH:mm:ss");
    }
}
