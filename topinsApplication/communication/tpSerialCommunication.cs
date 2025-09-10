using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpSerialCommunication : INotifyPropertyChanged, IDisposable
{
    private readonly SerialPort __serial;
    private tpioSetting __iosetting;

    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    public tpSerialCommunication() : this("COM3", 9600) { }
    public tpSerialCommunication(string portName, int baudRate) : this(portName, baudRate, 8, StopBits.One, Parity.None) { }
    public tpSerialCommunication(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
    {
        __serial = new SerialPort
        {
            PortName = portName,
            BaudRate = baudRate,
            DataBits = dataBits,
            StopBits = stopBits,
            Parity = parity,
        };
        __iosetting = tpWorkspace.Workspace.IOSetting;
    }

    public void Dispose()
    {
        Close();

        __serial.Dispose();
    }

    public SerialPort Serial => __serial;
    public string PortName => __serial.PortName;
    public int BaudRate => __serial.BaudRate;
    public int DataBits => __serial.DataBits;

    public StopBits StopBits => __serial.StopBits;
    public Parity Parity => __serial.Parity;

    public bool IsOpen => __serial.IsOpen;

    //public string CommandKey
    //{
    //    get => __commandKey;
    //    set
    //    {
    //        if (__commandKey == value) return;

    //        __commandKey = value;

    //        OnPropertyChanged(nameof(CommandKey));
    //    }
    //}
    //public tpioLensCommand Command
    //{
    //    get => __command;
    //    set
    //    {
    //        if (__command == value) return;

    //        __command = value;

    //        OnPropertyChanged(nameof(Command));
    //    }
    //}

    //public void Close()
    //{
    //    __serial.DataReceived -= new SerialDataReceivedEventHandler(OnRxReceived);

    //    __serial.Close();
    //}

    public void Close() => __serial.Close();
    public bool Open()
    {
        try
        {
            if (__serial.IsOpen) return __serial.IsOpen;

            //__serial.DataReceived += new SerialDataReceivedEventHandler(OnRxReceived);

            __serial.Open();
        }
        catch (Exception e)
        {
            _ = e.Message;

            return false;
        }
        return __serial.IsOpen;
    }

    //public void Transmit(byte[] data)
    //{
    //    if (__serial.IsOpen)
    //    {
    //        byte[] buffer = new byte[data.Length + 2];

    //        Array.Copy(data, 0, buffer, 1, data.Length);

    //        buffer[0] = __iosetting.Protocol.Sync;
    //        buffer[^1] = CHECKSUM(data);

    //        __serial.Write(buffer, 0, buffer.Length);
    //    }
    //}

    public void Transmit(tpioLensCommand command)
    {
        if (__serial.IsOpen)
        {
            byte[] buffer = new byte[command.TXCount];

            Array.Copy(command.Data, 0, buffer, 2, command.Data.Length);

            buffer[0] = __iosetting.CAMLens.Sync;
            buffer[1] = __iosetting.CAMLens.Address;
            buffer[^1] = CHECKSUM([.. buffer.Skip(1).Take(5)]);

#if DEBUG && FORDEBUG
            System.Diagnostics.Debug.WriteLine($"       >>>>>   TxCommand : {command.TXCount} bytes to transmit");
            System.Diagnostics.Debug.WriteLine($"       >>>>>   {tpCircularBuffer.ToHexString(buffer)}");
#endif
            __serial.Write(buffer, 0, buffer.Length);
        }
    }

    public byte CHECKSUM(byte[] data)
    {
        int checksum = 0;

        foreach (byte b in data)
        {
            checksum += b;
        }
        return (byte)(checksum & 0xFF);

    }

    public byte XORCHECKSUM(byte[] data)
    {
        byte checksum = 0;

        foreach (byte b in data)
        {
            checksum ^= b;
        }
        return checksum;
    }

    //private void PrintHex(byte[] data)
    //{
    //    StringBuilder sb = new();

    //    foreach (byte b in data)
    //    {
    //        sb.AppendFormat("{0:X2} ", b);
    //    }
    //    System.Diagnostics.Debug.WriteLine(sb.ToString());
    //}

    //    private void OnRxReceived(object sender, SerialDataReceivedEventArgs e)
    //    {
    //        if (sender is SerialPort sp && sp.IsOpen)
    //        {
    //            var buffer = new byte[sp.BytesToRead];

    //#if DEBUG && !FORDEBUG
    //            System.Diagnostics.Debug.WriteLine($"{sp.BytesToRead}");
    //#endif
    //            sp.Read(buffer, 0, buffer.Length);

    //            //OnPropertyChanged("OnRxReceived");
    //            //if (string.IsNullOrEmpty(__propertyName)) OnPropertyChanged(__propertyName);
    //        }
    //    }

    private void OnTxTransmit(object sender, SerialDataReceivedEventArgs e)
    {
        OnPropertyChanged("OnTxTransmit");
    }
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

