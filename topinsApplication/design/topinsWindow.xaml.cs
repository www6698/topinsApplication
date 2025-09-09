using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace topinsApplication;

[ToolboxItem(false), DesignTimeVisible(false)]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class topinsWindow : Window
{
    private Mutex __instance;
    private bool __createNew;

    public topinsWindow()
    {
        InitializeComponent();

        ResizeMode = ResizeMode.NoResize;

        Loaded += new RoutedEventHandler(OnTopinsWindowLoaded);
    }

    public topinsWindow(string title) : this()
    {
#if DEBUG && !FORDEBUG
        System.Diagnostics.Debug.WriteLine($"   topinsWindow::topinsWindow   >>>   Width : {Width}, Height : {Height}");
        System.Diagnostics.Debug.WriteLine($"                                >>>   ActualWidth : {ActualWidth}, ActualHeight : {ActualHeight}");
#endif
        try
        {
            __instance = new(true, title, out __createNew);

            if (!__createNew)
            {
                __instance?.Dispose();
                __instance = null;
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public bool CreateNew => __createNew;

    private void AdjustChromeSize()
    {
        if (Content is FrameworkElement content)
        {
            SizeToContent = SizeToContent.Manual;
            Width = content.ActualWidth + (chrome.ResizeBorderThickness.Left + chrome.ResizeBorderThickness.Right);
            Height = content.ActualHeight + (chrome.ResizeBorderThickness.Top + chrome.ResizeBorderThickness.Bottom);
            SizeToContent = SizeToContent.WidthAndHeight;
        }
    }

    private void SetCenterScreen()
    {
        WindowStartupLocation = WindowStartupLocation.Manual;

        Left = (SystemParameters.WorkArea.Width - ActualWidth) / 2 + SystemParameters.WorkArea.Left;
        Top = (SystemParameters.WorkArea.Height - ActualHeight) / 2 + SystemParameters.WorkArea.Top;
    }

    private void OnTopinsWindowClosing(object sender, CancelEventArgs e)
    {
        Closing -= new CancelEventHandler(OnTopinsWindowClosing);
        // DataContextChanged -= OnDataContextChanged;

        //DialogResult = true;
        __instance?.ReleaseMutex();
        __instance?.Dispose();
    }

    private void OnTopinsWindowLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= new RoutedEventHandler(OnTopinsWindowLoaded);
        Closing += new CancelEventHandler(OnTopinsWindowClosing);

        AdjustChromeSize();
        SetCenterScreen();
    }

    private void OnMinimizeClicked(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void OnMaximizeClicked(object sender, RoutedEventArgs e) => WindowState = WindowState.Maximized;
    private void OnRestoredClicked(object sender, RoutedEventArgs e) => WindowState = WindowState.Normal;
    private void OnShutDownClicked(object sender, RoutedEventArgs e) => Close();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class topinsWindowMvvm : tpMvvm
{
    private tpioSetting __ioSetting;

    private ObservableCollection<object> __viewModels;
    private object __currentViewModel;

    private readonly tpioSetting __iosetting;
    private string __caption = "Topins Window";

    #region tpRelayCommand
    private tpRelayCommand __changeViewModelCommand;
    #endregion

    public topinsWindowMvvm(tpioSetting iosetting)
    {
        __iosetting = iosetting;

        __viewModels = new ObservableCollection<object>
        {

        };
        __changeViewModelCommand = new tpRelayCommand(OnChangeViewModel, CanChangeViewModel);
    }

    public tpioSetting IOSetting
    {
        get => __ioSetting;
        set => Set(ref __ioSetting, value);
    }

    public ObservableCollection<object> ViewModels => __viewModels;
    public object CurrentViewModel
    {
        get => __currentViewModel;
        set
        {
            __currentViewModel = value;

            OnRaisePropertyChanged(nameof(CurrentViewModel));
        }
    }

    #region ICommand
    public ICommand LoadViewModelCommand { get; }
    #endregion

    public string CaptionName
    {
        get => __caption;
        set => Set(ref __caption, value);
    }

    private bool CanChangeViewModel(object parameter) => true;
    private void OnChangeViewModel(object parameter)
    {
        CurrentViewModel = parameter;
        // if (parameter is ViewModel1)
        // {
        //     CurrentViewModel = ViewModels.FirstOrDefault(vm => vm is ViewModel1);
        // }
        // else if (parameter is ViewModel2)
        // {
        //     CurrentViewModel = ViewModels.FirstOrDefault(vm => vm is ViewModel2);
        // }
    }
}
