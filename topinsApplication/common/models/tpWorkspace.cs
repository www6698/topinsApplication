using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using topinsApplication.Common.Events;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpWorkspace : tpMvvm, IDisposable
{
    private static readonly tpWorkspace __workspace = new();
    public static tpWorkspace Workspace => __workspace;

    private tpioSetting __iosetting;

    private topinsToolBarMvvm __toolBar;
    private topinsContentMvvm __content;

    private GridLength __titleHeight = new(0);

    private string __title = tpCONST.TOPINSTITLE;

    #region tpRelayCommand
    private tpRelayCommand __startWindowCommand;
    private tpRelayCommand __startDialogCommand;
    private tpRelayCommand __shutdownAppCommand;

    private tpRelayCommand __connectCAMCommand;
    private tpRelayCommand __refreshCAMCommand;
    private tpRelayCommand __fullscreenCommand;
    #endregion

    public tpWorkspace() => PreventAltF4 = new tpRelayCommand(OnPreventAltF4, null);

    public void Dispose()
    {
        __content.Dispose();
        __toolBar.Dispose();
    }

    public tpioSetting IOSetting
    {
        get => __iosetting;
        set => Set(ref __iosetting, value);
    }

    public GridLength TitleHeight
    {
        get => __titleHeight;
        set => Set(ref __titleHeight, value);
    }

    public topinsToolBarMvvm ToolBar => __toolBar ??= new topinsToolBarMvvm();
    public topinsContentMvvm Content => __content ??= new topinsContentMvvm();

    #region ICommand
    public ICommand ShutDownApp => __shutdownAppCommand ??= new tpRelayCommand(OnShutDownApp);
    public ICommand StartWindow => __startWindowCommand ??= new tpRelayCommand(OnStartWindow);
    public ICommand StartDialog => __startDialogCommand ??= new tpRelayCommand(OnStartDialog);

    public ICommand ConnectCAM => __connectCAMCommand ??= new tpRelayCommand(OnConnectCAM, CanConnectCAM);
    public ICommand RefreshCAM => __refreshCAMCommand ??= new tpRelayCommand(OnRefreshCAM, CanRefreshCAM);

    public ICommand FullScreen => __fullscreenCommand ??= new tpRelayCommand(OnFullScreen, CanFullScreen);
    public ICommand PreventAltF4 { get; }
    #endregion

    public string Caption
    {
        get => __title;
        set
        {
            if (Application.Current.MainWindow is MainWindow dwindow)
            {
                if (string.IsNullOrEmpty(__title) && dwindow.Title.Equals(__title)) return;

                __title = value;

                OnRaisePropertyChanged(nameof(Caption));
            }
        }
    }

    private string GetWindowTitle(object parameter)
    {
        if (parameter is Type type)
        {
            return type.Name switch
            {
                nameof(tpCamerasMvvm) => tpCONST.CAMERASELECTIONWINDOW,
                nameof(tpOptionsMvvm) => tpCONST.OPTIONSWINDOW,
                nameof(tpOSDToolMvvm) => tpCONST.OPTIONSOSDWINDOW,
                _ => throw new ArgumentException("Unsupported ViewModel type"),
            };
        }
        return string.Empty;
    }

    private topinsWindow WindowComposition(object parameter)
    {
        string title = GetWindowTitle(parameter);

        topinsWindow newWindow = new(title)
        {
            DataContext = new topinsWindowMvvm(__iosetting)
            {
                CurrentViewModel = parameter switch
                {
                    Type type when type == typeof(tpCamerasMvvm) => new tpCameras { DataContext = new tpCamerasMvvm() },
                    Type type when type == typeof(tpOptionsMvvm) => new tpOptions { DataContext = new tpOptionsMvvm() },
                    Type type when type == typeof(tpOSDToolMvvm) => new tpOSDTool { DataContext = new tpOSDToolMvvm() },
                    _ => throw new ArgumentException("Unsupported ViewModel type"),
                },
                CaptionName = title
            },
            Owner = Application.Current.MainWindow
        };
        return newWindow;
    }

    public bool ShutDown()
    {
        return true;
    }

    private void OnPreventAltF4(object e) { }
    private void OnShutDownApp(object parameter)
    {
        if (ShutDown())
        {
            Application.Current.Shutdown();
        }
    }

    private void OnStartWindow(object parameter)
    {
        if (WindowComposition(parameter) is topinsWindow newWindow && newWindow.CreateNew)
        {
            newWindow.Show();
        }
    }

    private void OnStartDialog(object parameter)
    {
        if (WindowComposition(parameter) is topinsWindow newWindow && newWindow.CreateNew)
        {
            newWindow.ShowDialog();
        }
    }

    private bool CanConnectCAM(object parameter) => true;
    private void OnConnectCAM(object parameter)
    {
        if (parameter is Tuple<object, Window> tuple)
        {
            Task.Delay(0).ContinueWith(_ =>
            {
                // title 변경!
                if (tuple.Item1 is not null) __content.Run(tuple.Item1);
            });
            //tuple.Item2?.Close();
            tuple.Item2.DialogResult = true;
        }
    }

    private bool CanRefreshCAM(object parameter) => true;
    private void OnRefreshCAM(object parameter)
    {
        if (parameter is tpCamerasMvvm cameras)
        {
            cameras.Refresh = false;

            __content.Refresh += new EventHandler<tpioArenaScanEventArgs>(cameras.OnRefreshCompleted);
            __content.Scan(parameter);
        }
    }
    private bool CanFullScreen(object parameter) => true;
    private void OnFullScreen(object parameter)
    {
        __toolBar.FullScreenGeometry = __toolBar.FullScreenGeometry == (Geometry)Application.Current.FindResource("FullScreen") ? (Geometry)Application.Current.FindResource("FullScreenExit") : (Geometry)Application.Current.FindResource("FullScreen");
        {
            TitleHeight = (GridLength.Auto == TitleHeight) ? new GridLength(0) : GridLength.Auto;

            if (__content is not null)
            {
                __content.ControlPadWidth = GridLength.Auto == __content.ControlPadWidth ? new GridLength(0) : GridLength.Auto;
            }
            if (__toolBar is not null)
            {
                __toolBar.IsPopupVisible = GridLength.Auto == __content.ControlPadWidth ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
