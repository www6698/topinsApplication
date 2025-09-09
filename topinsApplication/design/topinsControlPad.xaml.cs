using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using topinsApplication.Common.Events;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class topinsControlPad : UserControl
{
    public topinsControlPad()
    {
        InitializeComponent();

        DataContext = tpWorkspace.Workspace.Content.ControlPad;
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class topinsControlPadMvvm : tpMvvm
{
    private Task __task;
    private EventWaitHandle[] __events;
    private CancellationTokenSource __cancel;

    private tpioSetting __iosetting;
    private readonly tpSerialCommunication __sp = new();

    private tpCircularBuffer __buffer = new(32);
    //private byte[] __rxbuffer;
    private int __count = 0;

    private readonly List<tpCommandKey> __commandList = [];

    private readonly List<string> __commandKeys = [];

    private bool __isControlPadEnabled;
    private bool __isTransmitEnabled = true;

    private double __zoomValue;
    private double __irisValue;
    private double __focusValue;

    private tpSpeedSelectionMvvm __speedSelection;

    private string __zoomSpeed = tpCONST.SETLENZSPEED;
    private string __irisSpeed = tpCONST.SETLENZSPEED;
    private string __focusSpeed = tpCONST.SETLENZSPEED;

    private double __canvasTop;

    #region tpRelayCommand
    private tpRelayCommand __transmitCommand;
    private tpRelayCommand __zoomCommand;
    private tpRelayCommand __focusCommand;
    private tpRelayCommand __irisCommand;

    private tpRelayCommand __hideSpeedSelectionCommand;
    #endregion

    public topinsControlPadMvvm()
    {
        InitSerial(eTASKCREATION.Init);

        ShowSpeedSelection = new tpRelayCommand<object>(OnShowSpeedSelection);
        CanvasMouseDownCommand = new tpRelayCommand<MouseButtonEventArgs>(OnCanvasMouseDown);
    }

    public void Dispose()
    {
        __sp.Dispose();

        InitSerial(eTASKCREATION.Release);
    }

    #region ICommand
    public ICommand Transmit => __transmitCommand ??= new tpRelayCommand(OnTransmit, CanTransmit);
    public ICommand SetZoom => __zoomCommand ??= new tpRelayCommand(OnSetZoom, CanSetZoom);
    public ICommand SetFocus => __focusCommand ??= new tpRelayCommand(OnSetFocus, CanSetFocus);
    public ICommand SetIris => __irisCommand ??= new tpRelayCommand(OnSetIris, CanSetIris);

    public ICommand CanvasMouseDownCommand { get; }
    public ICommand ShowSpeedSelection { get; }
    public ICommand HideSpeedSelection => __hideSpeedSelectionCommand ??= new tpRelayCommand(OnHideSpeedSelection);
    #endregion

    public tpioSetting IOSetting
    {
        get => __iosetting;
        set => Set(ref __iosetting, value);
    }

    public tpSpeedSelectionMvvm SpeedSelection => __speedSelection ??= new tpSpeedSelectionMvvm
    {
        IOSetting = IOSetting
    };
    public bool IsControlPadEnabled
    {
        get => __isControlPadEnabled;
        set => Set(ref __isControlPadEnabled, value);
    }
    public bool IsTransmitEnabled
    {
        get => __isTransmitEnabled;
        set => Set(ref __isTransmitEnabled, value);
    }
    public double ZoomValue
    {
        get => __zoomValue;
        set => Set(ref __zoomValue, value);
    }

    public double FocusValue
    {
        get => __focusValue;
        set => Set(ref __focusValue, value);
    }

    public double IrisValue
    {
        get => __irisValue;
        set => Set(ref __irisValue, value);
    }

    public double ZoomMin { get; set; } = 0;
    public double ZoomMax { get; set; } = 0xFFFF;
    public double FocusMin { get; set; } = 0;
    public double FocusMax { get; set; } = 0xFFFF;
    public double IrisMin { get; set; } = 0;
    public double IrisMax { get; set; } = 0xFFFF;

    public string ZoomSpeed
    {
        get => __zoomSpeed;
        set => Set(ref __zoomSpeed, value);
    }
    public string FocusSpeed
    {
        get => __focusSpeed;
        set => Set(ref __focusSpeed, value);
    }
    public string IrisSpeed
    {
        get => __irisSpeed;
        set => Set(ref __irisSpeed, value);
    }

    public double CanvasTop
    {
        get => __canvasTop;
        set => Set(ref __canvasTop, value);
    }


    private async void InitSerial(eTASKCREATION taskCreation)
    {
        switch (taskCreation)
        {
            case eTASKCREATION.Init:
                __events = [new EventWaitHandle(false, EventResetMode.AutoReset), new EventWaitHandle(false, EventResetMode.ManualReset)];
                __cancel = new CancellationTokenSource();

                __task = OperationTask();

                break;
            case eTASKCREATION.Release:
                __events[tpCONST.SHUTDOWN].Set();

                await Task.WhenAny(__task, Task.Delay(Timeout.Infinite, __cancel.Token)).ConfigureAwait(false);

                for (int i = 0; i < __events.Length; i++) __events[i].Dispose();

                __cancel.Dispose();

#if DEBUG && !FORDEBUG
                System.Diagnostics.Debug.WriteLine($"       topinsControlPadMvvm::InitOperation     >>>     Released!!");
#endif
                break;
        }
    }

    public async Task OperationTask() => await Task.Run(Operation);
    private Task Operation()
    {
        try
        {
            do
            {
                switch (WaitHandle.WaitAny([__events[tpCONST.RUN], __events[tpCONST.SHUTDOWN]]))
                {
                    case tpCONST.RUN:
                        Task.Delay(0).ContinueWith(_ =>
                        {
                            Run();
                        });
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
            System.Diagnostics.Debug.WriteLine($"       topinsControlPadMvvm::Operation     >>>     closed!!");
#endif
            __cancel.Cancel();
        }
        return __task = Task.CompletedTask;
    }

    private bool Open()
    {
        if (__sp.Open())
        {
            __sp.Serial.DataReceived += new SerialDataReceivedEventHandler(OnRxReceived);

            return true;
        }
        return __sp.IsOpen;
    }

    private void Close()
    {
        if (!__sp.IsOpen) return;

        __sp.Serial.Close();
        __sp.Serial.DataReceived -= new SerialDataReceivedEventHandler(OnRxReceived);
    }

    private void Run()
    {
        if (0 == __commandKeys.Count) return;

        TxCommand(__commandKeys[0]);

        __commandKeys.RemoveAt(0);
    }

    private void TxCommand(string key)
    {
#if DEBUG && FORDEBUG
        System.Diagnostics.Debug.WriteLine($"OnTransmit: {key}");
#endif
        tpioLensCommand command;

        if (key.Contains(tpCommandKeys.STOP))
        {
            command = __iosetting.CAMLens.GetCommand(tpCommandKeys.STOP);
        }
        else
        {
            command = __iosetting.CAMLens.GetCommand(key);
        }
        __commandList.Add(new tpCommandKey(key, command));
        __sp?.Transmit(command);
    }

    private void TxCommand(tpioTxCommandEventArgs e)
    {
        __commandList.Add(new tpCommandKey(e.Key, e.Command));
        __sp?.Transmit(e.Command);
    }

    public void OpenPort(object parameter)
    {
        topinsToolBarMvvm toolBar = tpWorkspace.Workspace.ToolBar;

#if DEBUG &&!FORDEBUG
        System.Diagnostics.Debug.WriteLine($"OnOpenPort: {parameter as string}");
#endif
        if (!__sp.IsOpen)
        {
            if (Open())
            {
                toolBar.IsConnect = !(toolBar.IsDisconnect = toolBar.IsLenzEnabled = true);

                IsControlPadEnabled = true;

                __commandKeys.AddRange([tpCommandKeys.ZOOMPOSITIONREAD, tpCommandKeys.FOCUSPOSITIONREAD, tpCommandKeys.IRISPOSITIONREAD]);

                //__events[tpCONST.RUN].Set();
            }
        }
        else
        {
            Close();

            toolBar.IsConnect = !(toolBar.IsDisconnect = toolBar.IsLenzEnabled = false);

            IsControlPadEnabled = false;
        }
    }

    private void UpdateControl(byte[] data)
    {
#if DEBUG && !FORDEBUG
        //System.Diagnostics.Debug.WriteLine($"       >>>>>   UpdateControl: {__commandList[0].Key} Response - {data.Length} bytes received");
        System.Diagnostics.Debug.WriteLine($"       >>>>>   {tpUtility.ToHexString(data)}");
#endif
        string key = string.Empty;

        switch (__commandList[0].Key)
        {
            case tpCommandKeys.STOPZOOMTELE:
            case tpCommandKeys.STOPZOOMWIDE:
                //key = tpCommandKeys.QUARYZOOMPOSITIONREAD;
                break;
            case tpCommandKeys.STOPFOCUSNEAR:
            case tpCommandKeys.STOPFOCUSFAR:
                key = tpCommandKeys.FOCUSPOSITIONREAD;
                break;
            case tpCommandKeys.STOPIRISOPEN:
            case tpCommandKeys.STOPIRISCLOSE:
                key = tpCommandKeys.IRISPOSITIONREAD;
                break;
            case tpCommandKeys.FOCUSPOSITIONREAD:
                //FocusValue = ;
                break;
            case tpCommandKeys.ZOOMPOSITIONREAD:
                //ZoomValue = ;
                break;
            //case tpCommandKeys.QUARYZOOMPOSITIONREAD:
            //    ZoomValue = MakeParam(data);
            //    break;
            case tpCommandKeys.IRISPOSITIONREAD:
                //IrisValue = ;
                break;
            case tpCommandKeys.VERSION:
                break;
        }
        __commandList.RemoveAt(0);
        //if (0 < __commandKeys.Count) __events[tpCONST.RUN].Set();

        if (!string.IsNullOrEmpty(key)) TxCommand(key);
    }

    //private int MakeParam(byte[] data) => (data[__sp.Command.RXHIGH] << 8) | data[__sp.Command.RXLOW];
    //private int MakeParam(tpioLensCommand command) => (command.Data[command.RXHIGH] << 8) | command.Data[command.RXLOW];
    private int MakeParam(byte[] data) => (data[__commandList[0].Command.RXHIGH] << 8) | data[__commandList[0].Command.RXLOW];

    //public bool CanTransmit(object parameter) => !__sp.IsOpen;
    public bool CanTransmit(object parameter) => true;
    public void OnTransmit(object parameter) => TxCommand(parameter as string);

    public bool CanSetZoom(object parameter) => true;
    public void OnSetZoom(object parameter)
    {
        ZoomValue = Math.Truncate(Convert.ToDouble(parameter));

        //TxCommand(tpCommandKeys.ZOOMPOSITIONMOVE);
    }

    public bool CanSetFocus(object parameter) => true;
    public void OnSetFocus(object parameter)
    {
        FocusValue = Math.Truncate(Convert.ToDouble(parameter));

        //TxCommand(tpCommandKeys.FOCUSPOSITIONMOVE);
    }

    public bool CanSetIris(object parameter) => true;
    public void OnSetIris(object parameter)
    {
        IrisValue = Math.Truncate(Convert.ToDouble(parameter));

        //TxCommand(tpCommandKeys.IRISPOSITIONMOVE);
    }

    private void OnShowSpeedSelection(object parameter)
    {
        if (parameter is Label element && FindTopUserControl(element) is UserControl userControl)
        {
            var position = element.TransformToAncestor(userControl).Transform(new Point(0, 0));

            //Application.Current.Dispatcher.BeginInvoke(() =>
            //{
            CanvasTop = position.Y - userControl.Margin.Top - element.ActualHeight - 5;

            __speedSelection.Content = (string)element.Tag;
            //__speedSelection.IsVisible = Visibility.Visible;

            //}, DispatcherPriority.Render);
        }
    }

    private void OnHideSpeedSelection(object parameter) => __speedSelection.Content = string.Empty;
    private void OnCanvasMouseDown(MouseButtonEventArgs e)
    {

    }


    private UserControl FindTopUserControl(FrameworkElement element)
    {
        FrameworkElement parent = element;

        while (parent is not null && parent is not UserControl)
        {
            parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
        }
        return parent as UserControl;
    }

    //    private void OnRxReceived(object sender, SerialDataReceivedEventArgs e)
    //    {
    //#if DEBUG && FORDEBUG
    //        System.Diagnostics.Debug.WriteLine($"OnRxReceived: {e.EventType} by {__commandList[0].Key}");
    //#endif
    //        if (sender is SerialPort sp && sp.IsOpen)
    //        {
    //            int bytesAvailable = sp.BytesToRead;
    //            var buffer = new byte[bytesAvailable];

    //            sp.Read(buffer, 0, buffer.Length);

    //            // 큐에 데이터 삽입 후 현재 저장된 데이터 개수 반환
    //            int availableData = __buffer.Enqueue(buffer);

    //            // 현재 명령 목록이 존재하고, 충분한 데이터가 쌓였을 경우 실행
    //            while (__commandList.Count > 0 && availableData >= __commandList[0].Command.RXCount)
    //            {
    //                byte[] receivedData = __buffer.Dequeue(__commandList[0].Command.RXCount);

    //                Application.Current.Dispatcher.Invoke(() => UpdateControl(receivedData));

    //                // 최신 버퍼 상태 반영
    //                availableData = __buffer.GetCurrentCount();
    //            }

    //        }
    //    }
    private void OnRxReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (sender is SerialPort sp && sp.IsOpen)
        {
            int bytesAvailable = sp.BytesToRead;
            //if (bytesAvailable > 0)
            {
                byte[] incomingData = new byte[bytesAvailable];
                sp.Read(incomingData, 0, bytesAvailable);

                System.Diagnostics.Debug.WriteLine($"OnRxReceived: sp.Read [{tpUtility.ToHexString(incomingData)}] - {bytesAvailable} bytes. ");
                // **새 데이터를 Enqueue**
                __buffer.Enqueue(incomingData);

                //System.Diagnostics.Debug.WriteLine($"OnRxReceived: Enqueued {bytesAvailable} bytes.");

                // **버퍼 내부 상태 출력**
                System.Diagnostics.Debug.WriteLine($"Buffer State After Enqueue -> {__buffer.ToHexString()}");

                // **버퍼 내부 head, tail 위치 확인**
                //System.Diagnostics.Debug.WriteLine($"Buffer Indices -> Head={__buffer.Head}, Tail={__buffer.Tail}, Count={__buffer.Count}");
            }

        }
    }
    public void OnTxCommand(object sender, tpioTxCommandEventArgs e) => TxCommand(e);
    //    private void OnRxReceived(object sender, SerialDataReceivedEventArgs e)
    //    {
    //#if DEBUG && FORDEBUG
    //        System.Diagnostics.Debug.WriteLine($"OnRxReceived: {e.EventType} by {__commandList[0].Key}");
    //#endif
    //        if (sender is SerialPort sp && sp.IsOpen)
    //        {
    //            __count += sp.BytesToRead;

    //            var buffer = new byte[sp.BytesToRead];

    //#if DEBUG && !FORDEBUG
    //            System.Diagnostics.Debug.WriteLine($"{sp.BytesToRead}");
    //#endif
    //            sp.Read(buffer, 0, buffer.Length);

    //            __buffer.Enqueue(buffer, 5);

    //            if (__commandList[0].Command.RXCount <= __count)
    //            {
    //                __count -= __commandList[0].Command.RXCount;

    //                Application.Current.Dispatcher.Invoke(() => UpdateControl(__buffer.Dequeue(__commandList[0].Command.RXCount)));
    //            }
    //        }
    //    }

    //    private void OnRxReceived(object sender, SerialDataReceivedEventArgs e)
    //    {
    //#if DEBUG && !FORDEBUG
    //        System.Diagnostics.Debug.WriteLine($"OnRxReceived: {e.EventType} by {__sp.CommandKey}");
    //#endif
    //        if (sender is SerialPort sp && sp.IsOpen)
    //        {
    //            if (__rxbuffer is null || 0 == __count)
    //            {
    //                __rxbuffer = new byte[__sp.Command.RXCount];
    //            }
    //            int count = sp.BytesToRead;

    //#if DEBUG && !FORDEBUG

    //            System.Diagnostics.Debug.WriteLine($"{sp.BytesToRead}");
    //#endif
    //            sp.Read(__rxbuffer, __count, count);

    //            if ((__count += count) == __sp.Command.RXCount)
    //            {
    //                __count = 0;

    //                //UpdateControl(__rxbuffer);

    //                //Task.Delay(0).ContinueWith(_ =>
    //                //{
    //                //    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () => { UpdateControl(__rxbuffer); });
    //                //});
    //                Application.Current.Dispatcher.BeginInvoke(() => UpdateControl(__rxbuffer));
    //            }
    //        }
    //    }
    //    private void OnRxReceived(object sender, SerialDataReceivedEventArgs e)
    //    {
    //#if DEBUG && !FORDEBUG
    //        System.Diagnostics.Debug.WriteLine($"OnRxReceived: {e.EventType} by {__sp.CommandKey}");
    //#endif
    //        if (sender is SerialPort sp && sp.IsOpen)
    //        {
    //            var buffer = new byte[sp.BytesToRead];

    //#if DEBUG && !FORDEBUG
    //            System.Diagnostics.Debug.WriteLine($"{sp.BytesToRead}");
    //#endif
    //            sp.Read(buffer, 0, buffer.Length);

    //            Task.Delay(0).ContinueWith(_ =>
    //            {
    //                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () => { UpdateControl(buffer); });
    //            });
    //        }
    //    }

    //private void OnTxCommand(string key, byte[] data) => TxCommand?.Invoke(this, new tpioTxCommandEventArgs(key, data));
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpCircularBuffer
{
    private readonly object __lock = new();

    private readonly byte[] __buffer;
    private int __head;
    private int __tail;
    private readonly int __capacity;
    private int __count;

    public tpCircularBuffer(int capacity)
    {
        __capacity = capacity;
        __buffer = new byte[__capacity];
        __head = 0;
        __tail = 0;
        __count = 0;
    }

    public bool IsFull => __count == __capacity;
    public bool IsEmpty => __count == 0;

    public int GetCurrentCount()
    {
        lock (__lock)
        {
            return __count;
        }
    }

    public int Enqueue(byte[] data)
    {
        lock (__lock)
        {
            foreach (byte item in data)
            {
                if (__count == __capacity)
                {
                    __buffer[__head] = 0x00;
                    __head = (__head + 1) % __capacity;
                    __count--;
                }
                __buffer[__tail] = item;
                __tail = (__tail + 1) % __capacity;
                __count++;
            }
        }
#if DEBUG && FORDEBUG
        System.Diagnostics.Debug.WriteLine($"Enqueue Complete: Tail={__tail}, Count={__count}, Buffer State -> {ToHexString(__buffer)}");
#endif

        return __count;
    }

    public byte[] Dequeue(int length)
    {
        lock (__lock)
        {
            if (__count < length)
            {
                throw new InvalidOperationException("Not enough data in buffer");
            }
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = __buffer[__head];

                __buffer[__head] = 0x00;
                __head = (__head + 1) % __capacity;
                __count--;

#if DEBUG && FORDEBUG
                System.Diagnostics.Debug.WriteLine($"Dequeue: Head moved to {__head}, Count={__count}");
#endif
            }
#if DEBUG && FORDEBUG
            System.Diagnostics.Debug.WriteLine($"Dequeue After Removal: Head={__head}, Count={__count}, Buffer State -> {ToHexString(__buffer)}");
#endif
            return result;
        }
    }

    public byte[] Peek(int length)
    {
        lock (__lock)
        {
            if (__count < length) throw new InvalidOperationException("Not enough data in buffer");

            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = __buffer[(__head + i) % __capacity];
            }
            return result;
        }
    }

    public string ToHexString()
    {
        var sb = new StringBuilder();

        for (int i = 0; i < __capacity; i++)
        {
            sb.AppendFormat("0x{0:X2} ", __buffer[i]);
        }
        return sb.ToString().Trim();
    }



    public void PrintBuffer()
    {
        lock (__lock)
        {
            System.Diagnostics.Debug.Write("[ ");

            for (int i = 0; i < __count; i++)
            {
                System.Diagnostics.Debug.Write(__buffer[(__head + i) % __capacity] + " ");
            }
            System.Diagnostics.Debug.WriteLine("]");
        }
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpCommandKey(string key, tpioLensCommand command)
{
    public string Key { get; set; } = key;
    public tpioLensCommand Command { get; set; } = command;
}