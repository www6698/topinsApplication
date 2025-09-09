using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public abstract class tpIO<T> : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    public virtual void Default() { }

    public abstract T Clone();
    public abstract void CopyFrom(T source);
    public abstract bool Equals(T compare);

    protected virtual void OnRaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

[XmlRoot("tpioSetting")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioSetting : tpIO<tpioSetting>, IDisposable
{
    [XmlElement("IOGeneric")] public tpioGeneric IOGeneric { get; set; }
    [XmlElement("IONetwork")] public tpioNetwork IONetwork { get; set; }
    [XmlElement("IORSerial")] public tpioRSerial IORSerial { get; set; }

    private tpioCAMLens __camLens = new();

    public void Dispose()
    {
        __camLens.Dispose();
    }

    [XmlElement("CAMLens")]
    public tpioCAMLens CAMLens
    {
        get => __camLens ??= new tpioCAMLens();
        set
        {
            if (__camLens == value) return;

            __camLens = value;

            OnRaisePropertyChanged(nameof(CAMLens));
        }
    }

    public static tpioSetting Load(string filename)
    {
        try
        {
            if (!File.Exists(filename)) return null;

            var serializer = new XmlSerializer(typeof(tpioSetting));

            using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            return serializer.Deserialize(stream) as tpioSetting;
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
        return null;
    }

    public static bool Save(string filename, tpioSetting iosetting)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(tpioSetting));

            using var writer = XmlWriter.Create(filename, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            });
            serializer.Serialize(writer, iosetting);

            return true;
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
        return false;
    }

    public override tpioSetting Clone() => throw new NotImplementedException();
    public override void CopyFrom(tpioSetting source) => throw new NotImplementedException();

    public override bool Equals(tpioSetting other)
    {
        throw new NotImplementedException();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioGeneric : tpIO<tpioGeneric>
{
    private string __cameraVendor;
    private string __streamFolder;
    private string __screenShotFolder;

    private int __recordingTime = 60000;

    public tpioGeneric()
    {
        __streamFolder = tpUtility.CombinePath(tpCONST.STREAMFOLDER);
        __screenShotFolder = tpUtility.CombinePath(tpCONST.SCREENSHOTFOLDER);
        __cameraVendor = "defaultcp.cp";
    }

    [XmlElement("CameraVendor")]
    public string CameraVendor
    {
        get => __cameraVendor;
        set
        {
            if (__cameraVendor == value) return;

            __cameraVendor = value;

            OnRaisePropertyChanged(nameof(CameraVendor));
        }
    }

    [XmlElement("StreamFolder")]
    public string StreamFolder
    {
        get => __streamFolder;
        set
        {
            if (__streamFolder == value) return;

            __streamFolder = value;

            OnRaisePropertyChanged(nameof(StreamFolder));
        }
    }

    [XmlElement("ScreenShotFolder")]
    public string ScreenShotFolder
    {
        get => __screenShotFolder;
        set
        {
            if (__screenShotFolder == value) return;

            __screenShotFolder = value;

            OnRaisePropertyChanged(nameof(ScreenShotFolder));
        }
    }

    [XmlElement("RecordingTime")]
    public int RecordingTime
    {
        get => __recordingTime;
        set
        {
            if (__recordingTime == value) return;

            __recordingTime = value;

            OnRaisePropertyChanged(nameof(RecordingTime));
        }
    }
    public static string SetDefaultSettinsName(string name) => tpUtility.CombinePath(tpCONST.SETTINGSFOLDER, $"{name}.{tpCONST.SETTINGSFOLDER}");
    public override tpioGeneric Clone() => new()
    {
        CameraVendor = CameraVendor,
        StreamFolder = StreamFolder,
        ScreenShotFolder = ScreenShotFolder,
        RecordingTime = RecordingTime
    };
    public override void CopyFrom(tpioGeneric source) => throw new NotImplementedException();
    public override bool Equals(tpioGeneric compare)
    {
        throw new NotImplementedException();
    }
}

//[XmlRoot("tpioNetwork")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioNetwork : tpIO<tpioNetwork>
{
    public override tpioNetwork Clone()
    {
        throw new NotImplementedException();
    }
    public override void CopyFrom(tpioNetwork source)
    {
        throw new NotImplementedException();
    }
    public override bool Equals(tpioNetwork compare)
    {
        throw new NotImplementedException();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioRSerial : tpIO<tpioRSerial>
{
    private string __portName = "COM3";
    private int __baudRate = 9600;
    private int __dataBits = 8;

    private StopBits __stopBits = StopBits.One;
    private Parity __parity = Parity.None;

    private Handshake __handshake = Handshake.None;
    private SerialData __serialData = SerialData.Chars;
    private SerialError __serialError = SerialError.RXParity;
    private SerialPinChange __serialPinChange = SerialPinChange.Break;

    private bool __haveOSD = false;

    [XmlElement("PortName")]
    public string PortName
    {
        get => __portName;
        set
        {
            if (__portName == value) return;

            __portName = value;

            OnRaisePropertyChanged(nameof(PortName));
        }
    }

    [XmlElement("DataBits")]
    public int DataBits
    {
        get => __dataBits;
        set
        {
            if (__dataBits == value) return;

            __dataBits = value;

            OnRaisePropertyChanged(nameof(DataBits));
        }
    }

    [XmlElement("BaudRate")]
    public int BaudRate
    {
        get => __baudRate;
        set
        {
            if (__baudRate == value) return;

            __baudRate = value;

            OnRaisePropertyChanged(nameof(BaudRate));
        }
    }

    [XmlElement("StopBits")]
    public StopBits StopBits
    {
        get => __stopBits;
        set
        {
            if (__stopBits == value) return;

            __stopBits = value;

            OnRaisePropertyChanged(nameof(StopBits));
        }
    }

    [XmlElement("Parity")]
    public Parity Parity
    {
        get => __parity;
        set
        {
            if (__parity == value) return;

            __parity = value;

            OnRaisePropertyChanged(nameof(Parity));
        }
    }

    [XmlElement("Handshake")]
    public Handshake Handshake
    {
        get => __handshake;
        set
        {
            if (__handshake == value) return;

            __handshake = value;

            OnRaisePropertyChanged(nameof(Handshake));
        }
    }

    [XmlElement("SerialData")]
    public SerialData SerialData
    {
        get => __serialData;
        set
        {
            if (__serialData == value) return;

            __serialData = value;

            OnRaisePropertyChanged(nameof(SerialData));
        }
    }

    [XmlElement("SerialError")]
    public SerialError SerialError
    {
        get => __serialError;
        set
        {
            if (__serialError == value) return;

            __serialError = value;

            OnRaisePropertyChanged(nameof(SerialError));
        }
    }

    [XmlElement("SerialPinChange")]
    public SerialPinChange SerialPinChange
    {
        get => __serialPinChange;
        set
        {
            if (__serialPinChange == value) return;

            __serialPinChange = value;

            OnRaisePropertyChanged(nameof(SerialPinChange));
        }
    }

    [XmlElement("HaveOSD")]
    public bool HaveOSD
    {
        get => __haveOSD;
        set
        {
            if (__haveOSD == value) return;

            __haveOSD = value;

            OnRaisePropertyChanged(nameof(HaveOSD));
        }
    }

    public override tpioRSerial Clone() => new()
    {
        PortName = PortName,
        DataBits = DataBits,
        BaudRate = BaudRate,
        StopBits = StopBits,
        Parity = Parity,

        Handshake = Handshake,
        SerialData = SerialData,
        SerialError = SerialError,
        SerialPinChange = SerialPinChange
    };
    public override void CopyFrom(tpioRSerial source) => throw new NotImplementedException();
    public override bool Equals(tpioRSerial compare) => throw new NotImplementedException();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioCAMLens : tpIO<tpioCAMLens>
{
    private Dictionary<string, tpioLensCommand> __command = [];
    private Dictionary<string, tpioLensInfo> __lens = [];
    private Dictionary<string, tpioPreset> __preset = [];

    private tpioOSDValue __osdValue = new();

    private byte __sync = 0xFF;
    private byte __address = 0x01;
    private byte __presetCount = 0x07;

    public tpioCAMLens() => SetPreset();

    public void Dispose()
    {
        __command?.Clear();
        __lens?.Clear();
        __preset?.Clear();
    }

    [XmlIgnore]
    public Dictionary<string, tpioLensCommand> Command
    {
        get => __command;
        set
        {
            if (__command == value) return;

            __command = value;

            OnRaisePropertyChanged(nameof(Command));
        }
    }

    [XmlIgnore]
    public Dictionary<string, tpioLensInfo> LENS
    {
        get => __lens;
        set
        {
            if (__lens == value) return;

            __lens = value;

            OnRaisePropertyChanged(nameof(LENS));
        }
    }

    [XmlIgnore]
    public Dictionary<string, tpioPreset> Preset
    {
        get => __preset;
        set
        {
            if (__preset == value) return;

            __preset = value;

            OnRaisePropertyChanged(nameof(Preset));
        }
    }

    [XmlElement("PresetList")]
    public List<tpioPresetListItem> PresetList
    {
        get => Preset?.Select(p => new tpioPresetListItem
        {
            Key = p.Key,
            Value = p.Value
        }).ToList(); set => Preset = value?.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    //public void ToDictionary() => Preset = PresetList.ToDictionary(p => p.Key, p => p.Value);
    //public void ToList(Dictionary<string, tpioPreset> dictionary) => PresetList = [.. dictionary.Select(p => new tpioPresetListItem
    //{
    //    Key = p.Key,
    //    Value = p.Value
    //})];

    [XmlElement("OSDValue")]
    public tpioOSDValue OSDValue
    {
        get => __osdValue;
        set
        {
            if (__osdValue == value) return;

            __osdValue = value;

            OnRaisePropertyChanged(nameof(OSDValue));
        }
    }

    [XmlElement("Sync")]
    public byte Sync
    {
        get => __sync;
        set
        {
            if (__sync == value) return;

            __sync = value;

            OnRaisePropertyChanged(nameof(Sync));
        }
    }

    [XmlElement("Address")]
    public byte Address
    {
        get => __address;
        set
        {
            if (__address == value) return;

            __address = value;

            OnRaisePropertyChanged(nameof(Address));
        }
    }

    [XmlElement("PresetCount")]
    public byte PresetCount
    {
        get => __presetCount;
        set
        {
            if (__presetCount == value) return;

            __presetCount = value;

            OnRaisePropertyChanged(nameof(PresetCount));
        }
    }

    //public void ToDictionary() => Preset = PresetList.ToDictionary(p => p.Key, p => p.Value);
    //public void ToList(Dictionary<string, tpioPreset> dictionary) => PresetList = [.. dictionary.Select(kvp => new tpioPresetListItem
    //{
    //    Key = kvp.Key,
    //    Value = kvp.Value
    //})];
    //public void ClearPresetList() => __presetList?.Clear();

    public void SetCommand()
    {
        tpioLensCommand[] command =
        [
             new tpioLensCommand { Feature = "", Name = "", Operation = "Stop", TXCount = 7, RXCount = 4, Data = [0x00, 0x00, 0x00, 0x00] },

             new tpioLensCommand { Feature = "Zoom", Name = "Tele", Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x00, 0x20, 0x00, 0x00] },
             new tpioLensCommand { Feature = "Zoom", Name = "Wide", Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x00, 0x40, 0x00, 0x00] },

             //  Continuously
             new tpioLensCommand
             {
                 Feature = "ContinuouslyZoom", Name = "Type1A", Operation = tpCONST.ZOOMSPEED, Description ="CONTINUOUSLY SLOWEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x81, 0x00, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyZoom", Name = "Type2", Operation = tpCONST.ZOOMSPEED, Description ="CONTINUOUSLY LOW MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x81, 0x01, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyZoom", Name = "Type3", Operation = tpCONST.ZOOMSPEED, Description ="CONTINUOUSLY HIGH MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x81, 0x02, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyZoom", Name = "Type4", Operation = tpCONST.ZOOMSPEED, Description ="CONTINUOUSLY HIGHEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x81, 0x03, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "Zoom", Name = "Type1", Operation = tpCONST.ZOOMSPEED, Description ="SLOWEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x25, 0x00, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "Zoom", Name = "Type2", Operation = tpCONST.ZOOMSPEED, Description ="LOW MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x25, 0x00, 0x01]
             },
             new tpioLensCommand
             {
                 Feature = "Zoom", Name = "Type3", Operation = tpCONST.ZOOMSPEED, Description ="HIGH MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x25, 0x00, 0x02]
             },
             new tpioLensCommand
             {
                 Feature = "Zoom", Name = "Type4", Operation = tpCONST.ZOOMSPEED, Description ="HIGHEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x25, 0x00, 0x03]
             },
             new tpioLensCommand { Feature = "Focus", Name = "Auto", Operation = "", TXCount = 7, RXCount = 4, Data = [0x00, 0x2B, 0x00, 0x00] },
             new tpioLensCommand { Feature = "Focus", Name = "Near", Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x01, 0x00, 0x00, 0x00] },
             new tpioLensCommand { Feature = "Focus", Name = "Far", Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x00, 0x80, 0x00, 0x00] },

             // Continuously
             new tpioLensCommand
             {
                 Feature = "ContinuouslyFocus", Name = "Type1", Operation = tpCONST.FOCUSPEED, Description ="CONTINUOUSLY SLOWEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x83, 0x00, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyFocus", Name = "Type2", Operation = tpCONST.FOCUSPEED, Description ="CONTINUOUSLY LOW MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x83, 0x01, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyFocus", Name = "Type3", Operation = tpCONST.FOCUSPEED, Description ="CONTINUOUSLY HIGH MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x83, 0x02, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyFocus", Name = "Type4", Operation = tpCONST.FOCUSPEED, Description ="CONTINUOUSLY HIGHEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x83, 0x03, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "Focus", Name = "Type1", Operation = tpCONST.FOCUSPEED, Description ="SLOWEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x27, 0x00, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "Focus", Name = "Type2", Operation = tpCONST.FOCUSPEED, Description ="LOW MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x27, 0x00, 0x01]
             },
             new tpioLensCommand
             {
                 Feature = "Focus", Name = "Type3", Operation = tpCONST.FOCUSPEED, Description ="HIGH MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x27, 0x00, 0x02]
             },
             new tpioLensCommand
             {
                 Feature = "Focus", Name = "Type4", Operation = tpCONST.FOCUSPEED, Description ="HIGHEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x27, 0x00, 0x03]
             },
             new tpioLensCommand { Feature = "Iris", Name = "On", Operation = "", TXCount = 7, RXCount = 4, Data = [0x00, 0x2D, 0x00, 0x02] },
             new tpioLensCommand { Feature = "Iris", Name = "Manual", Operation = "", TXCount = 7, RXCount = 4, Data = [0x00, 0x2D, 0x00, 0x01] },
             new tpioLensCommand { Feature = "Iris", Name = "Open", Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x02, 0x00, 0x00, 0x00] },
             new tpioLensCommand { Feature = "Iris", Name = "Close", Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x04, 0x00, 0x00, 0x00] },

             //  Continuously
             new tpioLensCommand
             {
                 Feature = "ContinuouslyIris", Name = "Type1", Operation = tpCONST.IRISSPEED, Description ="CONTINUOUSLY SLOWEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x85, 0x00, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyIris", Name = "Type2", Operation = tpCONST.IRISSPEED, Description ="CONTINUOUSLY LOW MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x85, 0x01, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyIris", Name = "Type3", Operation = tpCONST.IRISSPEED, Description ="CONTINUOUSLY HIGH MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x85, 0x02, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "ContinuouslyIris", Name = "Type4", Operation = tpCONST.IRISSPEED, Description ="CONTINUOUSLY HIGHEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x85, 0x03, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "Iris", Name = "Type1", Operation = tpCONST.IRISSPEED, Description ="SLOWEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x93, 0x00, 0x00]
             },
             new tpioLensCommand
             {
                 Feature = "Iris", Name = "Type2", Operation = tpCONST.IRISSPEED, Description ="LOW MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x93, 0x00, 0x01]
             },
             new tpioLensCommand
             {
                 Feature = "Iris", Name = "Type3", Operation = tpCONST.IRISSPEED, Description ="HIGH MEDIUM SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x93, 0x00, 0x02]
             },
             new tpioLensCommand
             {
                 Feature = "Iris", Name = "Type4", Operation = tpCONST.IRISSPEED, Description ="HIGHEST SPEED", TXCount = 7, RXCount = 4, Data = [0x00, 0x93, 0x00, 0x03]
             },
             new tpioLensCommand { Feature = "Preset", Name = "Save", Operation = "", TXCount = 7, RXCount = 4, Data = [0x00, 0x03, 0x00, 0x01] },
             new tpioLensCommand { Feature = "Preset", Name = "Call", Operation = "", TXCount = 7, RXCount = 4, Data = [0x00, 0x07, 0x00, 0x01] },
             new tpioLensCommand { Feature = "Preset", Name = "Clear", Operation = "", TXCount = 7, RXCount = 4, Data = [0x00, 0x05, 0x00, 0x01] },

             new tpioLensCommand { Feature = "FocusPosition", Name = "Read", Operation = "", TXCount = 7, RXCount = 7, RXHIGH = 4, RXLOW = 5, Data = [0x00, 0x8B, 0x00, 0x00] },
             new tpioLensCommand { Feature = "FocusPosition", Name = "Move", Operation = "", TXCount = 7, RXCount = 4, TXHIGH = 4, TXLOW = 5, Data = [0x00, 0x87, 0x00, 0x00] },

             new tpioLensCommand { Feature = "ZoomPosition", Name = "Read", Operation = "", TXCount = 7, RXCount = 7, RXHIGH = 4, RXLOW = 5, Data = [0x00, 0x55, 0x00, 0x00] },
             new tpioLensCommand { Feature = "ZoomPosition", Name = "Move", Operation = "", TXCount = 7, RXCount = 4, TXHIGH = 4, TXLOW = 5, Data = [0x00, 0x4F, 0x00, 0x00] },

             new tpioLensCommand { Feature = "IrisPosition", Name = "Read", Operation = "", TXCount = 7, RXCount = 7, RXHIGH = 4, RXLOW = 5, Data = [0x00, 0x8F, 0x00, 0x00] },
             new tpioLensCommand { Feature = "IrisPosition", Name = "Move", Operation = "", TXCount = 7, RXCount = 4, TXHIGH = 4, TXLOW = 5, Data = [0x00, 0x89, 0x00, 0x00] },

             new tpioLensCommand { Feature = "IRFilter", Name = "In", Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x00, 0x09, 0x00, 0x00] },
             new tpioLensCommand { Feature = "IRFilter", Name = "In", Operation = "Stop", TXCount = 7, RXCount = 4, Data = [0x00, 0x00, 0x00, 0x00] },
             new tpioLensCommand { Feature = "IRFilter", Name = "Out",Operation = "Start", TXCount = 7, RXCount = 4, Data = [0x00, 0x0b, 0x00, 0x00] },
             new tpioLensCommand { Feature = "IRFilter", Name = "Out", Operation = "Stop", TXCount = 7, RXCount = 4, Data = [0x00, 0x00, 0x00, 0x00] },

             new tpioLensCommand { Feature = "OSDSetting", Name = "Save", Operation = "", TXCount = 7, RXCount = 4, TXHIGH = 4, TXLOW = 5, Data = [0x00, 0xE1, 0x00, 0x00] },
             new tpioLensCommand { Feature = "OSDSetting", Name = "Load", Operation = "", TXCount = 7, RXCount = 7, RXHIGH = 4, RXLOW = 5, Data = [0x00, 0xE5, 0x00, 0x00] }
        ];
        foreach (var cmd in command) __command[$"{cmd.Operation}{cmd.Feature}{cmd.Name}"] = cmd;
    }

    //public byte[] GetCommand(string key)
    //{
    //    if (__command.TryGetValue(key, out tpioLensCommand command))
    //    {
    //        return command.Data;
    //    }
    //    return null;
    //}

    public tpioLensCommand GetCommand(string key)
    {
        if (__command.TryGetValue(key, out tpioLensCommand command))
        {
            return command;
        }
        return null;
    }

    public void SetOSDCommand()
    {
        tpioLensInfo[] lensInfo =
        [
            new tpioLensInfo
            {
                Name = tpOSDKeys.MODEL, ItemName = tpOSDKeys.COMMANDKEY_MODEL, Address = 0x01, Reboot = true,
                Data =
                [
                    new tpioOSData { Description = "LMZ1000", Data = 0x0000 },
                    new tpioOSData { Description = "LMZ20750", Data = 0x0001 },
                    new tpioOSData { Description = "LMZ20550", Data = 0x0002 },
                    new tpioOSData { Description = "LMZ1236", Data = 0x0003 },
                    new tpioOSData { Description = "LMZ0824", Data = 0x0004 },
                    new tpioOSData { Description = "LMZ25300", Data = 0x0005 },
                    new tpioOSData { Description = "LMZ10360", Data = 0x0006 },
                    new tpioOSData { Description = "LMZ14500", Data = 0x0007 },
                    new tpioOSData { Description = "LMZ16160", Data = 0x0008 },
                    new tpioOSData { Description = "LMZ11176", Data = 0x0009 },
                    new tpioOSData { Description = "LMZ7527", Data = 0x000A },
                    new tpioOSData { Description = "LMZ1177", Data = 0x000D },
                    new tpioOSData { Description = "LMZ0812", Data = 0x000E },
                    new tpioOSData { Description = "LMZ300", Data = 0x000F },
                    new tpioOSData { Description = "LMZ2200", Data = 0x0010 },
                    new tpioOSData { Description = "USER_LENS", Data = 0x0020 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.BAUDRATE, ItemName = tpOSDKeys.COMMANDKEY_BAUDRATE, Address = 0x05, Reboot = true,
                Data =
                [
                    new tpioOSData { Description = "2400", Data = 0x0000 },
                    new tpioOSData { Description = "4800", Data = 0x0001 },
                    new tpioOSData { Description = "9600", Data = 0x0002 },
                    new tpioOSData { Description = "19200", Data = 0x0003 },
                    new tpioOSData { Description = "38400", Data = 0x0004 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.AMPGAIN, ItemName = tpOSDKeys.COMMANDKEY_AMPGAIN, Address = 0x07, Block = false,
                Data =
                [
                    new tpioOSData { Description = "LOW", Data = 0x0000 },
                    new tpioOSData { Description = "HIGH", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.ZOOMAF, ItemName = tpOSDKeys.COMMANDKEY_ZOOMAF, Address = 0x09,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "ON", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.ZOOMAFDELAY, ItemName = tpOSDKeys.COMMANDKEY_ZOOMAFDELAY, Address = 0x0B, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x0000 },
                    new tpioOSData { Description = "", Data = 0x2710 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.PTAF, ItemName = tpOSDKeys.COMMANDKEY_PTAF, Address = 0x0D,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "ON", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.PTAFDELAY, ItemName = tpOSDKeys.COMMANDKEY_PTAFDELAY, Address = 0x0F, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x0000 },
                    new tpioOSData { Description = "", Data = 0x2710 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.PTID, ItemName = tpOSDKeys.COMMANDKEY_PTID, Address = 0x15, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x0000 },
                    new tpioOSData { Description = "", Data = 0x00FF }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.AFAREASIZE, ItemName = tpOSDKeys.COMMANDKEY_AFAREASIZE, Address = 0x19,
                Data =
                [
                    new tpioOSData { Description = "SMALL", Data = 0x0000 },
                    new tpioOSData { Description = "MEDIUM", Data = 0x0001 },
                    new tpioOSData { Description = "LARGE", Data = 0x0002 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.AFAREAFRAME, ItemName = tpOSDKeys.COMMANDKEY_AFAREAFRAME, Address = 0x1B,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "RECT", Data = 0x0001 },
                    new tpioOSData { Description = "FILL", Data = 0x0002 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.AFSEARCH, ItemName = tpOSDKeys.COMMANDKEY_AFSEARCH, Address = 0x1D, Block = false,
                Data =
                [
                    new tpioOSData { Description = "FULL", Data = 0x0000 },
                    new tpioOSData { Description = "HALF", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.ZOOMPOSINV, ItemName = tpOSDKeys.COMMANDKEY_ZOOMPOSINV, Address = 0x25, Reboot = true,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "ON", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.FOCUSPOSINV, ItemName = tpOSDKeys.COMMANDKEY_FOCUSPOSINV, Address = 0x27, Reboot = true,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "ON", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.IRISPOSINV, ItemName = tpOSDKeys.COMMANDKEY_IRISPOSINV, Address = 0x29, Reboot = true,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "ON", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.AFTIMEOUT, ItemName = tpOSDKeys.COMMANDKEY_AFTIMEOUT, Address = 0x33, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x000A },
                    new tpioOSData { Description = "", Data = 0x0078 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.CMDQINGAF, ItemName = tpOSDKeys.COMMANDKEY_CMDQINGAF, Address = 0x35,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "ON", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.CMDQINGPRST, ItemName = tpOSDKeys.COMMANDKEY_CMDQINGPRST, Address = 0x37,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x0000 },
                    new tpioOSData { Description = "ON", Data = 0x0001 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.PWMFREQ, ItemName = tpOSDKeys.COMMANDKEY_PWMFREQ, Address = 0x39, Block = false, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x0001 },
                    new tpioOSData { Description = "", Data = 0x0064 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.ZOOMLSPOS, ItemName = tpOSDKeys.COMMANDKEY_ZOOMLSPOS, Address = 0x3B, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x0000 },
                    new tpioOSData { Description = "", Data = 0x03E8 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.USERAFSPD, ItemName = tpOSDKeys.COMMANDKEY_USERAFSPD, Address = 0x43, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x000A },
                    new tpioOSData { Description = "", Data = 0x03E8 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.USERAF, ItemName = tpOSDKeys.COMMANDKEY_USERAF, Address = 0x45, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x000A },
                    new tpioOSData { Description = "", Data = 0x03E8 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.USERAFSTSPD, ItemName = tpOSDKeys.COMMANDKEY_USERAFSTSPD, Address = 0x47, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x000A },
                    new tpioOSData { Description = "", Data = 0x03E8 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.USERAFSTPOS1, ItemName = tpOSDKeys.COMMANDKEY_USERAFSTPOS1, Address = 0x49, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x0000 },
                    new tpioOSData { Description = "", Data = 0x64 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.USERMFOCUSL, ItemName = tpOSDKeys.COMMANDKEY_USERMFOCUSL, Address = 0x4D, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x000A },
                    new tpioOSData { Description = "", Data = 0x03E8 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.USERMFOCUSM, ItemName = tpOSDKeys.COMMANDKEY_USERMFOCUSM, Address = 0x4F, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x000A },
                    new tpioOSData { Description = "", Data = 0x03E8 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.USERMFOCUSH, ItemName = tpOSDKeys.COMMANDKEY_USERMFOCUSH, Address = 0x51, InputType = eINPUTTYPE.TEXTBOX,
                Data =
                [
                    new tpioOSData { Description = "", Data = 0x000A },
                    new tpioOSData { Description = "", Data = 0x03E8 }
                ]
            },
            new tpioLensInfo
            {
                Name = tpOSDKeys.GRESPONSE, ItemName = tpOSDKeys.COMMANDKEY_GRESPONSE, Address = 0x61,
                Data =
                [
                    new tpioOSData { Description = "OFF", Data = 0x00 },
                    new tpioOSData { Description = "ON", Data = 0x01 }
                ]
            },
        ];
        foreach (var lens in lensInfo) __lens[lens.ItemName] = lens;
    }

    public tpioLensInfo GetCAMLensInfo(string key)
    {
        if (__lens.TryGetValue(key, out tpioLensInfo lens))
        {
            return lens;
        }
        return null;
    }

    private void SetPreset()
    {
        for (int i = 1; i <= __presetCount; i++)
        {
            tpioPreset preset = new()
            {
                Number = $"#{i:D2}",
                Location = $"Please type in Location #{i:D2}",
            };
            __preset.Add(preset.Number, preset);
        }
    }

    public static tpioCAMLens Load(string filename)
    {
        try
        {
            if (new FileInfo(filename).Exists)
            {
                if (XElement.Load(filename).Element("Command")?.Value is string json && !string.IsNullOrEmpty(json))
                {
                    return new tpioCAMLens
                    {
                        Command = JsonConvert.DeserializeObject<Dictionary<string, tpioLensCommand>>(json)
                    };
                }
            }
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
        return null;
    }

    public static bool Save(string filename, tpioCAMLens protocol)
    {
        try
        {
            using var writer = XmlWriter.Create(filename, new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            });
            new XElement("CAMInformation", new XElement("Command", JsonConvert.SerializeObject(protocol.Command, Newtonsoft.Json.Formatting.Indented))).WriteTo(writer);

            return true;
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
        return false;
    }

    public override tpioCAMLens Clone() => new()
    {
        Sync = Sync,
        Address = Address,
        Command = Command.ToDictionary(entry => entry.Key, entry => entry.Value.Clone())
    };
    public override void CopyFrom(tpioCAMLens source) => throw new NotImplementedException();
    public override bool Equals(tpioCAMLens compare)
    {
        throw new NotImplementedException();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioLensCommand : tpIO<tpioLensCommand>
{
    [XmlElement("Feature")] public string Feature;
    [XmlElement("Name")] public string Name;
    [XmlElement("Operation")] public string Operation;
    [XmlElement("Description")] public string Description;
    [XmlElement("TXHIGH")] public int TXHIGH;
    [XmlElement("TXLOW")] public int TXLOW;
    [XmlElement("RXHIGH")] public int RXHIGH;
    [XmlElement("RXLOW")] public int RXLOW;

    [XmlElement("TXCount")] public int TXCount;
    [XmlElement("RXCount")] public int RXCount;
    [XmlElement("Data")] public byte[] Data;
    [XmlElement("Value")] public int Value;

    public int DefaultValue;
    public int MinValue = 0;
    public int MaxValue = 0;

    public override tpioLensCommand Clone()
    {
        throw new NotImplementedException();
    }
    public override void CopyFrom(tpioLensCommand source)
    {
        throw new NotImplementedException();
    }
    public override bool Equals(tpioLensCommand compare)
    {
        throw new NotImplementedException();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioLensInfo : tpIO<tpioLensInfo>
{
    [XmlElement("Name")] public string Name;
    [XmlElement("ItemName")] public string ItemName;
    [XmlElement("CommandKey")] public string CommandKey = tpCommandKeys.OSDSETTINGSAVE;
    [XmlElement("Address")] public byte Address;
    [XmlElement("Reboot")] public bool Reboot = false;
    [XmlElement("Block")] public bool Block = true;
    [XmlElement("InputType")] public eINPUTTYPE InputType = eINPUTTYPE.COMBOBOX;
    [XmlElement("Data")] public List<tpioOSData> Data;

    public override tpioLensInfo Clone()
    {
        throw new NotImplementedException();
    }
    public override void CopyFrom(tpioLensInfo source)
    {
        throw new NotImplementedException();
    }
    public override bool Equals(tpioLensInfo compare)
    {
        throw new NotImplementedException();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioOSData : tpIO<tpioOSData>
{
    private string __description;

    [XmlElement("Description")]
    public string Description
    {
        get => __description;
        set
        {
            if (__description == value) return;

            __description = value;

            OnRaisePropertyChanged(nameof(Description));
        }
    }

    [XmlElement("Data")] public ushort Data { get; set; }

    public override tpioOSData Clone()
    {
        throw new NotImplementedException();
    }

    public override void CopyFrom(tpioOSData source)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(tpioOSData compare)
    {
        throw new NotImplementedException();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioOSDValue : tpIO<tpioOSDValue>
{
    private bool __isSync = false;

    private ushort __modelValue = 0x000A;
    private ushort __baudRateValue = 0x0002;
    private ushort __ampgainValue = 0x0001;
    private ushort __zoomAFValue = 0x0000;
    private ushort __zoomAFDelayValue = 1000;
    private ushort __ptAFValue = 0x0000;
    private ushort __ptAFDelayValue = 1000;
    private ushort __ptIDValue = 0x0002;
    private ushort __afareaSizeValue = 0x0001;
    private ushort __afareaframeValue = 0x0000;
    private ushort __afsearchValue = 0x0000;
    private ushort __zoomPosInvValue = 0x0000;
    private ushort __focusPosInvValue = 0x0000;
    private ushort __irisPosInvValue = 0x0000;
    private ushort __afTimeoutValue = 0x001E;
    private ushort __cmdqingAFValue = 0x0001;
    private ushort __cmdqingprstValue = 0x0001;
    private ushort __pwmFreqValue = 0x0064;
    private ushort __zoomLSPosValue = 0x0032;
    private ushort __userAFspdValue = 0x003C;
    private ushort __userAFValue = 0x0064;
    private ushort __userAFstspdValue = 0x003C;
    private ushort __userAFstPos1Value = 0x000A;
    private ushort __usermFocusLValue = 0x0032;
    private ushort __usermFocusMValue = 0x012C;
    private ushort __usermFocusHValue = 0x03E8;
    private ushort __gresponseValue = 0x0001;

    [XmlElement("IsSync")]
    public bool IsSync
    {
        get => __isSync;
        set
        {
            if (__isSync == value) return;

            __isSync = value;

            OnRaisePropertyChanged(nameof(IsSync));
        }
    }

    [XmlElement("ModelValue")]
    public ushort ModelValue
    {
        get => __modelValue;
        set
        {
            if (__modelValue == value) return;

            __modelValue = value;

            OnRaisePropertyChanged(nameof(ModelValue));
        }
    }

    [XmlElement("BaudRateValue")]
    public ushort BaudRateValue
    {
        get => __baudRateValue;
        set
        {
            if (__baudRateValue == value) return;

            __baudRateValue = value;

            OnRaisePropertyChanged(nameof(BaudRateValue));
        }
    }

    [XmlElement("AmpGainValue")]
    public ushort AmpGainValue
    {
        get => __ampgainValue;
        set
        {
            if (__ampgainValue == value) return;

            __ampgainValue = value;

            OnRaisePropertyChanged(nameof(AmpGainValue));
        }
    }

    [XmlElement("ZoomAFValue")]
    public ushort ZoomAFValue
    {
        get => __zoomAFValue;
        set
        {
            if (__zoomAFValue == value) return;

            __zoomAFValue = value;

            OnRaisePropertyChanged(nameof(ZoomAFValue));
        }
    }

    [XmlElement("ZoomAFDelayValue")]
    public ushort ZoomAFDelayValue
    {
        get => __zoomAFDelayValue;
        set
        {
            if (__zoomAFDelayValue == value) return;

            __zoomAFDelayValue = value;

            OnRaisePropertyChanged(nameof(ZoomAFDelayValue));
        }
    }

    [XmlElement("PtAFValue")]
    public ushort PtAFValue
    {
        get => __ptAFValue;
        set
        {
            if (__ptAFValue == value) return;

            __ptAFValue = value;

            OnRaisePropertyChanged(nameof(PtAFValue));
        }
    }

    [XmlElement("PtAFDelayValue")]
    public ushort PtAFDelayValue
    {
        get => __ptAFDelayValue;
        set
        {
            if (__ptAFDelayValue == value) return;

            __ptAFDelayValue = value;

            OnRaisePropertyChanged(nameof(PtAFDelayValue));
        }
    }

    [XmlElement("PtIDValue")]
    public ushort PtIDValue
    {
        get => __ptIDValue;
        set
        {
            if (__ptIDValue == value) return;

            __ptIDValue = value;

            OnRaisePropertyChanged(nameof(PtIDValue));
        }
    }

    [XmlElement("AFAreaSizeValue")]
    public ushort AFAreaSizeValue
    {
        get => __afareaSizeValue;
        set
        {
            if (__afareaSizeValue == value) return;

            __afareaSizeValue = value;

            OnRaisePropertyChanged(nameof(AFAreaSizeValue));
        }
    }

    [XmlElement("AFAreaFrameValue")]
    public ushort AFAreaFrameValue
    {
        get => __afareaframeValue;
        set
        {
            if (__afareaframeValue == value) return;

            __afareaframeValue = value;

            OnRaisePropertyChanged(nameof(AFAreaFrameValue));
        }
    }

    [XmlElement("AFSearchValue")]
    public ushort AFSearchValue
    {
        get => __afsearchValue;
        set
        {
            if (__afsearchValue == value) return;

            __afsearchValue = value;

            OnRaisePropertyChanged(nameof(AFSearchValue));
        }
    }

    [XmlElement("ZoomPosInvValue")]
    public ushort ZoomPosInvValue
    {
        get => __zoomPosInvValue;
        set
        {
            if (__zoomPosInvValue == value) return;

            __zoomPosInvValue = value;

            OnRaisePropertyChanged(nameof(ZoomPosInvValue));
        }
    }

    [XmlElement("FocusPosInvValue")]
    public ushort FocusPosInvValue
    {
        get => __focusPosInvValue;
        set
        {
            if (__focusPosInvValue == value) return;

            __focusPosInvValue = value;

            OnRaisePropertyChanged(nameof(FocusPosInvValue));
        }
    }

    [XmlElement("IrisPosInvValue")]
    public ushort IrisPosInvValue
    {
        get => __irisPosInvValue;
        set
        {
            if (__irisPosInvValue == value) return;

            __irisPosInvValue = value;

            OnRaisePropertyChanged(nameof(IrisPosInvValue));
        }
    }

    [XmlElement("AFTimeoutValue")]
    public ushort AFTimeoutValue
    {
        get => __afTimeoutValue;
        set
        {
            if (__afTimeoutValue == value) return;

            __afTimeoutValue = value;

            OnRaisePropertyChanged(nameof(AFTimeoutValue));
        }
    }

    [XmlElement("CmdQingAFValue")]
    public ushort CmdQingAFValue
    {
        get => __cmdqingAFValue;
        set
        {
            if (__cmdqingAFValue == value) return;

            __cmdqingAFValue = value;

            OnRaisePropertyChanged(nameof(CmdQingAFValue));
        }
    }

    [XmlElement("CmdQingPrstValue")]
    public ushort CmdQingPrstValue
    {
        get => __cmdqingprstValue;
        set
        {
            if (__cmdqingprstValue == value) return;

            __cmdqingprstValue = value;

            OnRaisePropertyChanged(nameof(CmdQingPrstValue));
        }
    }

    [XmlElement("PwmFreqValue")]
    public ushort PwmFreqValue
    {
        get => __pwmFreqValue;
        set
        {
            if (__pwmFreqValue == value) return;

            __pwmFreqValue = value;

            OnRaisePropertyChanged(nameof(PwmFreqValue));
        }
    }

    [XmlElement("ZoomLSPosValue")]
    public ushort ZoomLSPosValue
    {
        get => __zoomLSPosValue;
        set
        {
            if (__zoomLSPosValue == value) return;

            __zoomLSPosValue = value;

            OnRaisePropertyChanged(nameof(ZoomLSPosValue));
        }
    }

    [XmlElement("UserAFspdValue")]
    public ushort UserAFspdValue
    {
        get => __userAFspdValue;
        set
        {
            if (__userAFspdValue == value) return;

            __userAFspdValue = value;

            OnRaisePropertyChanged(nameof(UserAFspdValue));
        }
    }

    [XmlElement("UserAFValue")]
    public ushort UserAFValue
    {
        get => __userAFValue;
        set
        {
            if (__userAFValue == value) return;

            __userAFValue = value;

            OnRaisePropertyChanged(nameof(UserAFValue));
        }
    }

    [XmlElement("UserAFSTSPDValue")]
    public ushort UserAFSTSPDValue
    {
        get => __userAFstspdValue;
        set
        {
            if (__userAFstspdValue == value) return;

            __userAFstspdValue = value;

            OnRaisePropertyChanged(nameof(UserAFSTSPDValue));
        }
    }

    [XmlElement("UserAFSTPos1Value")]
    public ushort UserAFSTPos1Value
    {
        get => __userAFstPos1Value;
        set
        {
            if (__userAFstPos1Value == value) return;

            __userAFstPos1Value = value;

            OnRaisePropertyChanged(nameof(UserAFSTPos1Value));
        }
    }

    [XmlElement("UsermFocusLValue")]
    public ushort UsermFocusLValue
    {
        get => __usermFocusLValue;
        set
        {
            if (__usermFocusLValue == value) return;

            __usermFocusLValue = value;

            OnRaisePropertyChanged(nameof(UsermFocusLValue));
        }
    }

    [XmlElement("UsermFocusMValue")]
    public ushort UsermFocusMValue
    {
        get => __usermFocusMValue;
        set
        {
            if (__usermFocusMValue == value) return;

            __usermFocusMValue = value;

            OnRaisePropertyChanged(nameof(UsermFocusMValue));
        }
    }

    [XmlElement("UsermFocusHValue")]
    public ushort UsermFocusHValue
    {
        get => __usermFocusHValue;
        set
        {
            if (__usermFocusHValue == value) return;

            __usermFocusHValue = value;

            OnRaisePropertyChanged(nameof(UsermFocusHValue));
        }
    }

    [XmlElement("GResponseValue")]
    public ushort GResponseValue
    {
        get => __gresponseValue;
        set
        {
            if (__gresponseValue == value) return;

            __gresponseValue = value;

            OnRaisePropertyChanged(nameof(GResponseValue));
        }
    }

    public override tpioOSDValue Clone()
    {
        throw new NotImplementedException();
    }
    public override void CopyFrom(tpioOSDValue source)
    {
        throw new NotImplementedException();
    }
    public override bool Equals(tpioOSDValue compare)
    {
        throw new NotImplementedException();
    }
}

//[XmlRoot("Preset")]
//[DataContract(Name = "Preset")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioPreset : tpIO<tpioPreset>
{
    [XmlElement("Number")] public string Number { get; set; }
    [XmlElement("Location")] public string Location { get; set; }

    public override tpioPreset Clone() => throw new NotImplementedException();
    public override void CopyFrom(tpioPreset source) => throw new NotImplementedException();
    public override bool Equals(tpioPreset compare) => throw new NotImplementedException();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioPresetListItem
{
    [XmlElement("Index")] public string Key { get; set; }
    [XmlElement("Preset")] public tpioPreset Value { get; set; }

}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 아래것 나중에 지울것임!!!
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public static class tpUtility
{
    public static string ExecutablePath()
    {
        try
        {
            return Environment.CurrentDirectory;
        }
        catch (Exception)
        {

        }
        return string.Empty;
    }

    public static string CombinePath(string directory, string filename = null)
    {
        try
        {
            string path = Path.Combine(Environment.CurrentDirectory, directory);

            CheckPath(path);

            if (!string.IsNullOrEmpty(filename)) path = Path.Combine(path, filename);

            return path;
        }
        catch (Exception)
        {

        }
        return string.Empty;
    }

    public static void CheckPath(string path)
    {
        try
        {
            DirectoryInfo di = new(path);

            if (!di.Exists) di.Create();
        }
        catch (Exception)
        {

        }
    }

    public static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
    }

    public static string ToHexString(byte[] data)
    {
        if (data is null || 0 == data.Length) return "Empty or null data";

        var sb = new StringBuilder();

        foreach (byte item in data)
        {
            sb.AppendFormat("0x{0:X2} ", item);
        }
        return sb.ToString().Trim();
    }
}
