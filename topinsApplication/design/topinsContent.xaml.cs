using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using topinsApplication.camera.lucid;
using topinsApplication.Common.Events;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class topinsContent : UserControl
{
    public topinsContent() => InitializeComponent();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class topinsContentMvvm : tpMvvm, IDisposable
{
    private const int TASKCOUNT = 2;

    private const int TASKRUNLIVEPLAY = 0;
    private const int TASKRECORDVIDEO = 1;

    private readonly Task[] __task;
    private EventWaitHandle[] __events;
    private CancellationTokenSource __cancel;

    private tpioSetting __iosetting;
    private tpArenaNet __arena;

    private topinsCamImageMvvm __camImage;
    private tpToolBarExtendedMvvm __toolBarEx;
    private topinsControlPadMvvm __controlPad;

    private bool __recording = false;
    private bool __capturing = false;

    private GridLength __controlPadWidth = new(0);
    private bool __isPopupOpen;
    private bool __isStayOpen;

    private double __horizontalOffset;
    private double __verticalOffset;
    private bool __isDragging;
    private Point __position;

    private bool __streaming = false;

    private BitmapImage __image;
    private Stretch __stretchImage;

    private ArenaNET.IDeviceInfo __device;

    private System.Diagnostics.Stopwatch __recordWatch;

    private string __streamH264 = string.Empty;
    private string __screenshot = string.Empty;

    private Visibility __isOriginalVisible;
    private double __orgWidth;
    private double __orgHeight;

    public ObservableCollection<object> DockPanelControl { get; set; } = [];
    public ObservableCollection<object> PopupContent { get; set; } = [];

    #region tpRelayCommand
    private ICommand __popupMouseDownCommand;
    private ICommand __popupMouseMoveCommand;
    private ICommand __popupMouseUpCommand;
    #endregion

    #region Events
    public event EventHandler<tpioArenaScanEventArgs> Refresh;
    public event EventHandler<tpioTxCommandEventArgs> TxCommand;
    #endregion

    public topinsContentMvvm()
    {
        __task = new Task[TASKCOUNT];
        __controlPad ??= new topinsControlPadMvvm();
        __controlPad.IOSetting = IOSetting = tpWorkspace.Workspace.IOSetting;

        TxCommand += __controlPad.OnTxCommand;

        InitOperation(eTASKCREATION.Init);

        PopupContent.Add(__controlPad);

        StretchImage = Stretch.Uniform;
        Image = TOPINSLOGO();

        Capturing = true;
    }

    public void Dispose()
    {
        InitOperation(eTASKCREATION.Release);

        __controlPad.Dispose();
    }

    #region ICommand
    public ICommand PopupMouseDown => __popupMouseDownCommand ??= new tpRelayCommand<MouseEventArgs>(OnPopupMouseDown);
    public ICommand PopupMouseMove => __popupMouseMoveCommand ??= new tpRelayCommand<MouseEventArgs>(OnPopupMouseMove);
    public ICommand PopupMouseUp => __popupMouseUpCommand ??= new tpRelayCommand<MouseEventArgs>(OnPopupMouseUp);
    #endregion

    public tpioSetting IOSetting
    {
        get => __iosetting;
        set => Set(ref __iosetting, value);
    }
    public ArenaNET.IDeviceInfo Device => __device;
    public BitmapImage Image
    {
        get => __image;
        set
        {
            Set(ref __image, value);
        }
    }
    public Stretch StretchImage
    {
        get => __stretchImage;
        set => Set(ref __stretchImage, value);
    }

    public bool Recording
    {
        get => __recording;
        set => Set(ref __recording, value);
    }
    public bool Capturing
    {
        get => __capturing;
        set => Set(ref __capturing, value);
    }

    public bool IsStreaming => __streaming;

    public GridLength ControlPadWidth
    {
        get => __controlPadWidth;
        set
        {
            if (Set(ref __controlPadWidth, value))
            {
                if (GridLength.Auto == ControlPadWidth)
                {
                    MoveToDockPanel();
                }
                else
                {
                    MoveToPopup();
                }
            }
        }
    }

    public bool IsPopupOpen
    {
        get => __isPopupOpen;
        set
        {
            if (Set(ref __isPopupOpen, value))
            {
                if (Visibility.Visible == ControlPad.SpeedSelection.IsVisible) ControlPad.SpeedSelection.IsVisible = Visibility.Collapsed;
            }
        }
    }

    public bool IsStayOpen
    {
        get => __isStayOpen;
        set => Set(ref __isStayOpen, value);
    }

    public double HorizontalOffset
    {
        get => __horizontalOffset;
        set => Set(ref __horizontalOffset, value);
    }

    public double VerticalOffset
    {
        get => __verticalOffset;
        set => Set(ref __verticalOffset, value);
    }

    public Point Position
    {
        get => __position;
        set => Set(ref __position, value);
    }

    public Visibility IsOriginalVisible
    {
        get => __isOriginalVisible;
        set => Set(ref __isOriginalVisible, value);
    }

    public double OrgWidth
    {
        get => __orgWidth;
        set => Set(ref __orgWidth, value);
    }

    public double OrgHeight
    {
        get => __orgHeight;
        set => Set(ref __orgHeight, value);
    }

    public tpToolBarExtendedMvvm ToolBarEx => __toolBarEx ??= new tpToolBarExtendedMvvm();
    public topinsControlPadMvvm ControlPad => __controlPad ??= new topinsControlPadMvvm();

    private async void InitOperation(eTASKCREATION taskCreation)
    {
        try
        {
            switch (taskCreation)
            {
                case eTASKCREATION.Init:
                    __arena = new tpArenaNet();

                    __events = [new EventWaitHandle(false, EventResetMode.ManualReset), new EventWaitHandle(false, EventResetMode.ManualReset), new EventWaitHandle(false, EventResetMode.ManualReset)];
                    __cancel = new CancellationTokenSource();

                    __task[TASKRUNLIVEPLAY] = LiveScreenTask();
                    __task[TASKRECORDVIDEO] = RecordVideoTask();

                    break;
                case eTASKCREATION.Release:
                    __arena.Dispose();
                    __events[tpCONST.SHUTDOWN].Set();

                    await Task.WhenAny(Task.WhenAll(__task), Task.Delay(Timeout.Infinite, __cancel.Token)).ConfigureAwait(true);

                    for (int i = 0; i < __events.Length; i++) __events[i].Dispose();

                    __cancel.Dispose();

#if DEBUG && !FORDEBUG
                    System.Diagnostics.Debug.WriteLine($"       topinsContentMvvm::InitOperation     >>>     Released!!");
#endif
                    break;
            }
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
    }

    private async Task LiveScreenTask() => await Task.Run(LiveScreen);
    private async Task LiveScreen()
    {
        try
        {
            do
            {
                switch (await Task.Run(() => WaitHandle.WaitAny([__events[tpCONST.RUN], __events[tpCONST.SHUTDOWN]])))
                {
                    case tpCONST.RUN:
                        await GetImage(__recording);

                        break;
                }
            } while (!__events[tpCONST.SHUTDOWN].WaitOne(0));
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
        finally
        {
#if DEBUG && !FORDEBUG
            System.Diagnostics.Debug.WriteLine($"       topinsContentMvvm::RunLivePlay     >>>     closed!!");
#endif
            __cancel.Cancel();
        }
        __task[TASKRUNLIVEPLAY] = Task.CompletedTask;
    }

    private async Task GetImage(bool recording)
    {
        if (__arena.Connected)
        {
            try
            {
                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                {
                    ArenaNET.IImage image = __arena.GetImage(recording);

                    // ★ null 체크
                    if (image == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to get image from camera");
                        return;
                    }

                    if (recording && __iosetting.IOGeneric.RecordingTime <= __recordWatch.ElapsedMilliseconds)
                        SaveStream();

                    using MemoryStream memory = new();
                    if (image.Bitmap != null)
                    {
                        image.Bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                        memory.Position = 0;

                        BitmapImage bmpImage = BitmapToBitmapImage(image.Bitmap);
                        image.Bitmap.Dispose();
                        Image = bmpImage;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetImage error: {ex.Message}");
            }
        }
    }

    private BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
    {
        using MemoryStream memory = new();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;

        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        return bitmapImage;
    }

    private async Task RecordVideoTask() => await Task.Run(RecordVideo);
    private async Task RecordVideo()
    {
        try
        {
            do
            {
                switch (await Task.Run(() => WaitHandle.WaitAny([__events[tpCONST.RECORD], __events[tpCONST.SHUTDOWN]])))
                {
                    case tpCONST.RECORD:
                        break;
                }
            } while (!__events[tpCONST.SHUTDOWN].WaitOne(0));
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
        finally
        {
#if DEBUG && !FORDEBUG
            System.Diagnostics.Debug.WriteLine($"       topinsContentMvvm::RecordVideo     >>>     closed!!");
#endif
            __cancel.Cancel();
        }
        __task[TASKRECORDVIDEO] = Task.CompletedTask;
    }

    private void SaveStream(int count = 0)
    {
        try
        {
            if (__arena?.IImages == null || __arena.IImages.Count == 0)
                return;

            if (__arena.IImages[0] == null)
                return;

            if (string.IsNullOrEmpty(__streamH264))
                return;

            string directory = System.IO.Path.GetDirectoryName(__streamH264);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            SaveNET.VideoParams parameters = new(__arena.IImages[0].Width, __arena.IImages[0].Height, __arena.FPS);
            SaveNET.VideoRecorder recorder = new(parameters, __streamH264);

            recorder.SetH264Mp4BGR8();
            recorder.Open();

            for (int i = 0; i < __arena.IImages.Count; i++)
            {
                if (__arena.IImages[i]?.DataArray != null)
                {
                    recorder.AppendImage(__arena.IImages[i].DataArray);
                }
            }

            recorder.Close();
            __recordWatch?.Restart();
        }
        catch (Exception ex)
        {
            // 오류 무시하고 계속 진행
        }
    }

    private void SaveBMP()
    {

    }
    private BitmapImage TOPINSLOGO() => new(new Uri("pack://application:,,,/images/topinslogobg.png"));

    public void Run(object parameter)
    {
        if (__arena.Connected) __arena.Disconnect();
        if (__arena.Connect(__device = parameter as ArenaNET.IDeviceInfo))
        {
            __arena.StartStream();
            __events[tpCONST.RUN].Set();
            __streaming = true;

            SetStreamingState(__streaming);
        }
    }

    public async Task Stop()
    {
        if (__streaming)
        {
            __events[tpCONST.RUN].Reset();

            await Task.Delay(100);

            __arena.StopStream();
            __streaming = false;

            if (__arena.Connected) __arena.Disconnect();

            SetStreamingState(__streaming);
        }
    }

    private void SetStreamingState(bool streaming) => Application.Current.Dispatcher.Invoke(() =>
    {
        if (tpWorkspace.Workspace.ToolBar is topinsToolBarMvvm toolBar)
        {
            if (streaming)
            {
                StretchImage = Stretch.Fill;
            }
            else
            {
                StretchImage = Stretch.Uniform;
                Image = TOPINSLOGO();
            }
            if (toolBar.PlayStream is tpRelayCommand play)
            {
                play.CanExecute(__device is not null && !streaming);
                play.RaiseCanExecuteChanged();
            }
            if (toolBar.StopStream is tpRelayCommand stop)
            {
                stop.CanExecute(streaming);
                stop.RaiseCanExecuteChanged();
            }
            toolBar.IsRecord = streaming;

            if (toolBar.Screen is tpRelayCommand capture)
            {
                capture.CanExecute(streaming);
                capture.RaiseCanExecuteChanged();
            }
        }
        //if (!streaming) Image = null;
    });

    public void Scan(object parameter) => Task.Delay(0).ContinueWith(_ =>
    {
        OnRefresh(__arena.RefreshDevices());
    });

    private readonly object __recordLock = new object();

    public async void Record(bool record)
    {
        lock (__recordLock)
        {
            try
            {
                if (record)
                {
                    __streamH264 = Path.Combine(__iosetting.IOGeneric.StreamFolder, $"{tpCONST.TOPINS}_{DateTime.Now:yyyyMMdd_HHmmss}_stream.mp4");
                    __arena.IImages = new List<ArenaNET.IImage>();

                    __recordWatch = new System.Diagnostics.Stopwatch();
                    __recordWatch.Restart();

                    Recording = record;
                }
                else
                {
                    Recording = false; // 먼저 상태 변경
                }
            }
            catch { }
        }

        // lock 밖에서 비동기 작업
        if (!record)
        {
            await Task.Delay(200); // 대기 시간 증가

            lock (__recordLock)
            {
                try
                {
                    if (__arena.IImages != null && __arena.IImages.Count > 0)
                    {
                        SaveStream();
                    }

                    __recordWatch?.Stop();
                    __recordWatch = null;
                    __arena.IImages = null;
                    __streamH264 = string.Empty;
                }
                catch
                {
                    __recordWatch?.Stop();
                    __recordWatch = null;
                    __arena.IImages = null;
                    Recording = false;
                }
            }
        }
    }

    public void Screen(string timestamp)
    {
        try
        {
            if (__arena.Connected && Image != null)
            {
                // BMP 확장자로 변경
                __screenshot = $"{__iosetting.IOGeneric.ScreenShotFolder}\\{tpCONST.TOPINS}_{DateTime.Now:yyyyMMdd_HHmmss}.bmp";

                // 폴더 생성
                string directory = System.IO.Path.GetDirectoryName(__screenshot);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                // 현재 화면 이미지를 파일로 저장
                if (Image is BitmapImage bitmapImage)
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // 또는 JpegBitmapEncoder
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                    using (var fileStream = new System.IO.FileStream(__screenshot.Replace(".bmp", ".png"), System.IO.FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 에러 발생 시에만 출력 (필요하면 이것도 제거 가능)
            // System.Diagnostics.Debug.WriteLine($"스크린샷 저장 오류: {ex.Message}");
        }
    }

    //public void Focus(string key) => TxCommand?.Invoke(this, new tpioTxCommandEventArgs(key, __iosetting.Protocol.GetCameraCommand(key)));
    public void Focus(string key)
    {
        //TxCommand?.Invoke(this, new tpioTxCommandEventArgs(key));
    }

    public void Iris(bool iris)
    {
        if (iris)
        {
            // key = tpCommandKeys.IRISON
        }
        else
        {
            // key = tpCommandKeys.IRISMANUAL
        }
    }

    public void PresetTool(string key, int presetID)
    {
        tpioLensCommand command = __iosetting.CAMLens.GetCommand((string)(tpWorkspace.Workspace.ToolBar).PresetTag);

        command.Data[3] = (byte)presetID;

        TxCommand?.Invoke(this, new tpioTxCommandEventArgs(key, command));
    }

    public void MoveToPopup()
    {
        if (0 < DockPanelControl.Count && DockPanelControl.First() is var item)
        {
            DockPanelControl.Remove(item);
            PopupContent.Add(item);
        }
    }

    public void MoveToDockPanel()
    {
        if (0 < PopupContent.Count && PopupContent.First() is var item)
        {
            PopupContent.Remove(item);
            DockPanelControl.Add(item);
        }
    }

    public void OnPopupMouseDown(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && Element is FrameworkElement element)
        {
            __isDragging = true;
            __position = e.GetPosition(element);

            if (Element is Popup popup)
            {
                popup.IsOpen = true;
            }
#if DEBUG && FORDEBUG
            System.Diagnostics.Debug.WriteLine($"       topinsContentMvvm::OnPopupMouseDown     >>>     __isDragging : {__isDragging}");
            System.Diagnostics.Debug.WriteLine($"       topinsContentMvvm::OnPopupMouseDown     >>>     __position : {__position}");
#endif
        }
    }
    public void OnPopupMouseMove(MouseEventArgs e)
    {
        if (Element is FrameworkElement element && e.LeftButton == MouseButtonState.Pressed && __isDragging)
        {
            Point position = e.GetPosition(element);

            double offsetX = position.X - __position.X;
            double offsetY = position.Y - __position.Y;

            if (1 < Math.Abs(offsetX) || 1 < Math.Abs(offsetY))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HorizontalOffset += offsetX;
                    VerticalOffset += offsetY;

                });
                __position = position;
            }
#if DEBUG && FORDEBUG
            System.Diagnostics.Debug.WriteLine($"       topinsContentMvvm::OnPopupMouseMove     >>>     __position : {__position}   >>>   offset({offsetX}, {offsetY})");
#endif
        }
    }
    public void OnPopupMouseUp(MouseEventArgs e)
    {
        __isDragging = false;

#if DEBUG && FORDEBUG
        System.Diagnostics.Debug.WriteLine($"       topinsContentMvvm::OnPopupMouseUp     >>>     __isDragging : {__isDragging}");
#endif
    }
    private void OnRefresh(List<ArenaNET.IDeviceInfo> devices) => Refresh?.Invoke(this, new tpioArenaScanEventArgs(devices));
}

//[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
//public class tpControlItem
//{
//    public UIElement Control { get; set; }
//}
