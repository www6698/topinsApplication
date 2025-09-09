using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Windows.Controls;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class tpSerialSetting : UserControl
{
    public tpSerialSetting()
    {
        InitializeComponent();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpSerialSettingMvvm : tpMvvm
{
    private readonly tpioRSerial __tpioSerial = tpWorkspace.Workspace.IOSetting.IORSerial;

    private string __selectedPort;
    private int __selectedBaudRate = 9600;
    private int __selectedDataBits = 8;
    private StopBits __selectedStopBits = System.IO.Ports.StopBits.One;
    private Parity __selectedParity = System.IO.Ports.Parity.None;

    private Handshake __selectedHandshake = System.IO.Ports.Handshake.None;
    private SerialData __selectedSerialData = System.IO.Ports.SerialData.Chars;
    private SerialError __selectedSerialError = System.IO.Ports.SerialError.RXParity;
    private SerialPinChange __selectedSerialPinChange = System.IO.Ports.SerialPinChange.Break;

    private bool __changed = false;

    public tpSerialSettingMvvm()
    {
        EnumerateSerialPorts();

        StopBits = new ObservableCollection<StopBits>(Enum.GetValues(typeof(StopBits)) as StopBits[]);
        Parity = new ObservableCollection<Parity>(Enum.GetValues(typeof(Parity)) as Parity[]);
        Handshake = new ObservableCollection<Handshake>(Enum.GetValues(typeof(Handshake)) as Handshake[]);
        SerialData = new ObservableCollection<SerialData>(Enum.GetValues(typeof(SerialData)) as SerialData[]);
        SerialError = new ObservableCollection<SerialError>(Enum.GetValues(typeof(SerialError)) as SerialError[]);
        SerialPinChange = new ObservableCollection<SerialPinChange>(Enum.GetValues(typeof(SerialPinChange)) as SerialPinChange[]);

        __selectedPort = __tpioSerial.PortName;
        __selectedBaudRate = __tpioSerial.BaudRate;
        __selectedDataBits = __tpioSerial.DataBits;
        __selectedStopBits = __tpioSerial.StopBits;
        __selectedParity = __tpioSerial.Parity;

        __selectedHandshake = __tpioSerial.Handshake;
        __selectedSerialData = __tpioSerial.SerialData;
        __selectedSerialError = __tpioSerial.SerialError;
        __selectedSerialPinChange = __tpioSerial.SerialPinChange;

    }

    public ObservableCollection<string> Ports { get; set; }
    public ObservableCollection<int> BaudRate { get; set; } =
    [
        150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600
    ];
    public ObservableCollection<int> DataBits { get; set; } =
    [
        5, 6, 7, 8, 9
    ];
    public ObservableCollection<StopBits> StopBits { get; set; }
    public ObservableCollection<Parity> Parity { get; set; }
    public ObservableCollection<Handshake> Handshake { get; set; }
    public ObservableCollection<SerialData> SerialData { get; set; }
    public ObservableCollection<SerialError> SerialError { get; set; }
    public ObservableCollection<SerialPinChange> SerialPinChange { get; set; }

    public string SelectedPort
    {
        get => __selectedPort;
        set => Set(ref __selectedPort, value);
    }

    public int SelectedBaudRate
    {
        get => __selectedBaudRate;
        set => Set(ref __selectedBaudRate, value);
    }

    public int SelectedDataBits
    {
        get => __selectedDataBits;
        set => Set(ref __selectedDataBits, value);
    }

    public StopBits SelectedStopBits
    {
        get => __selectedStopBits;
        set => Set(ref __selectedStopBits, value);
    }

    public Parity SelectedParity
    {
        get => __selectedParity;
        set => Set(ref __selectedParity, value);
    }

    public Handshake SelectedHandshake
    {
        get => __selectedHandshake;
        set => Set(ref __selectedHandshake, value);
    }

    public SerialData SelectedSerialData
    {
        get => __selectedSerialData;
        set => Set(ref __selectedSerialData, value);
    }

    public SerialError SelectedSerialError
    {
        get => __selectedSerialError;
        set => Set(ref __selectedSerialError, value);
    }

    public SerialPinChange SelectedSerialPinChange
    {
        get => __selectedSerialPinChange;
        set => Set(ref __selectedSerialPinChange, value);
    }

    public bool Changed
    {
        get => __changed;
        set => Set(ref __changed, value);
    }

    private void EnumerateSerialPorts() => Ports = new ObservableCollection<string>(SerialPort.GetPortNames());
    private bool IsValid()
    {
        if (string.IsNullOrEmpty(__selectedPort))
        {
            return false;
        }
        return true;
    }
}