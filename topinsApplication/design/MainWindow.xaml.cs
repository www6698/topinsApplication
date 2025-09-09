using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace topinsApplication;

public partial class MainWindow : Window
{
    private readonly tpWorkspace __mvvm;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = __mvvm = tpWorkspace.Workspace;

        SourceInitialized += (s, e) =>
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);

            source.AddHook(WndProc);
        };
        Closing += new CancelEventHandler(OnTopinsClosing);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_NCLBUTTONDBLCLK = 0xA3;
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HTCAPTION = 0x2;

        switch (msg)
        {
            case WM_NCLBUTTONDBLCLK:
            case WM_NCLBUTTONDOWN:
                if (wParam.ToInt32() == HTCAPTION)
                {
                    handled = true;
                }
                break;
        }
        return IntPtr.Zero;
    }

    private void OnTopinsClosing(object sender, CancelEventArgs e)
    {
        Closing -= new CancelEventHandler(OnTopinsClosing);

        tpWorkspace.Workspace.Dispose();
    }

    private void OnMinimizeClicked(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void OnMaximizeClicked(object sender, RoutedEventArgs e) => WindowState = WindowState.Maximized;
    private void OnRestoredClicked(object sender, RoutedEventArgs e) => WindowState = WindowState.Normal;
    private void OnShutDownClicked(object sender, RoutedEventArgs e)
    {
        if (tpWorkspace.Workspace.ShutDown()) Close();
    }
}