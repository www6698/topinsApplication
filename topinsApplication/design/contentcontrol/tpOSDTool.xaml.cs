using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class tpOSDTool : UserControl
{
    public tpOSDTool()
    {
        InitializeComponent();
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpOSDToolMvvm : tpMvvm
{
    private readonly tpioCAMLens __camLens = tpWorkspace.Workspace.IOSetting.CAMLens;
    private readonly topinsControlPadMvvm __controlPad = tpWorkspace.Workspace.Content.ControlPad;

    private ObservableCollection<tpControlItem> __items = [];
    private bool __isDefaultEnabled;

    private bool __isFieldEnabled = true;

    #region tpRelayCommand
    private tpRelayCommand __transmitCommand;
    private tpRelayCommand __defaultValue;
    private tpRelayCommand __closeWindowCommand;
    #endregion

    public tpOSDToolMvvm() => InitializeProperties();

    #region ICommand
    public ICommand Transmit => __transmitCommand ??= new tpRelayCommand(OnTransmit, CanTransmit);
    public ICommand DefaultValue => __defaultValue ??= new tpRelayCommand(OnDefaultValue, CanDefaultValue);
    public ICommand CloseWindow => __closeWindowCommand ??= new tpRelayCommand(OnCloseWindow, CanCloseWindow);
    #endregion

    public ObservableCollection<tpControlItem> Items
    {
        get => __items;
        set => Set(ref __items, value);
    }

    public bool IsDefaultEnabled
    {
        get => __isDefaultEnabled;
        set => Set(ref __isDefaultEnabled, value);
    }

    public bool IsFieldEnabled
    {
        get => __isFieldEnabled;
        set => Set(ref __isFieldEnabled, value);
    }

    private void InitializeProperties()
    {
        foreach (var item in __camLens.LENS)
        {
            List<tpioOSData> items = null;

            var selectedItem = (tpioOSData)null;
            var currentValue = string.Empty;
            var value = GetValueByCommandKey(__camLens.OSDValue, item.Value.ItemName);

            switch (item.Value.InputType)
            {
                case eINPUTTYPE.COMBOBOX:
                    items = [.. item.Value.Data.Select(x => new tpioOSData
                    {
                        Description = x.Description,
                        Data = x.Data
                    })];
                    selectedItem = value.HasValue ? items?.FirstOrDefault(x => x.Data == value.Value) : null;

                    if (selectedItem is null) break;

                    currentValue = $"0x{selectedItem.Data:X4}";

                    break;
                case eINPUTTYPE.TEXTBOX:
                    selectedItem = new tpioOSData
                    {
                        Description = value.ToString(),
                        Data = value ?? 0
                    };
                    currentValue = value.ToString();

                    break;
                default:
                    break;
            }
            var controlItem = new tpControlItem
            {
                Address = item.Value.Address,

                Key = item.Key,
                CommandKey = item.Value.CommandKey,

                Name = item.Value.Name,
                ItemName = item.Value.ItemName,

                InputType = item.Value.InputType,
                Items = items,
                IsEnabled = item.Value.Block,

                OldSelectedItem = selectedItem,
                SelectedItem = selectedItem,
                CurrentValue = currentValue,

                RebootIcon = item.Value.Reboot,
                BlockIcon = !item.Value.Block,

                Range = item.Value.InputType == eINPUTTYPE.TEXTBOX ? $"{item.Value.Data[0].Data,6} ~ {item.Value.Data[1].Data,6}" : string.Empty,
                Min = item.Value.InputType == eINPUTTYPE.TEXTBOX ? item.Value.Data[0].Data : (ushort)0,
                Max = item.Value.InputType == eINPUTTYPE.TEXTBOX ? item.Value.Data[1].Data : (ushort)0,

                Command = Transmit,

                ItemChanged = OnItemChanged
            };
            controlItem.CommandParameter = controlItem;

            __items.Add(controlItem);
            //__items.Add(new tpControlItem
            //{
            //    Key = item.Key,
            //    CommandKey = item.Value.CommandKey,
            //    Title = item.Value.Name,

            //    InputType = item.Value.InputType,
            //    Items = items,
            //    IsEnabled = item.Value.Block,

            //    OldSelectedItem = selectedItem,
            //    SelectedItem = selectedItem,
            //    CurrentValue = currentValue,

            //    RebootIcon = item.Value.Reboot,
            //    BlockIcon = !item.Value.Block,

            //    Range = item.Value.InputType == eINPUTTYPE.TEXTBOX ? $"{item.Value.Data[0].Data,6} ~ {item.Value.Data[1].Data,6}" : string.Empty,
            //    Min = item.Value.InputType == eINPUTTYPE.TEXTBOX ? item.Value.Data[0].Data : (ushort)0,
            //    Max = item.Value.InputType == eINPUTTYPE.TEXTBOX ? item.Value.Data[1].Data : (ushort)0,

            //    Command = Transmit,
            //    CommandParameter = ,

            //    ItemChanged = OnItemChanged
            //});
        }
        if (!__camLens.OSDValue.IsSync)
        {

        }
    }

    private static ushort? GetValueByCommandKey(tpioOSDValue osdValue, string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName)) return null;

        if (typeof(tpioOSDValue).GetProperty($"{itemName}Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) is var property && property.PropertyType == typeof(ushort))
        {
            return (ushort)property.GetValue(osdValue);
        }
        return null;
    }

    private static void SetValueByCommandKey(tpioOSDValue osdValue, string itemName, ushort value)
    {
        if (string.IsNullOrWhiteSpace(itemName)) return;

        if (typeof(tpioOSDValue).GetProperty($"{itemName}Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) is var property && property.PropertyType == typeof(ushort))
        {
            property.SetValue(osdValue, value);
        }
    }

    private tpControlItem GetControlItem(string itemName) => string.IsNullOrWhiteSpace(itemName) ? null : Items.FirstOrDefault(x => x.ItemName == itemName);
    private void SetSeletedItem(string itemName, tpioOSData selectedItem)
    {
        if (selectedItem is null) return;

        var item = GetControlItem(itemName);

        if (item is not null)
        {
            item.SelectedItem = selectedItem;
            item.CurrentValue = selectedItem?.Description ?? string.Empty;

            SetValueByCommandKey(__camLens.OSDValue, itemName, selectedItem?.Data ?? 0);
        }
    }

    private void SetSelctedItem(string commandKey, ushort value)
    {
        if (string.IsNullOrWhiteSpace(commandKey)) return;

        var item = GetControlItem(commandKey);

        if (item is not null)
        {
            item.SelectedItem = new tpioOSData
            {
                Description = $"0x{value:X4}",
                Data = value
            };
            item.CurrentValue = $"0x{value:X4}";

            SetValueByCommandKey(__camLens.OSDValue, commandKey, value);
        }
    }

    private void ComboBoxItemChanged(tpControlItem item)
    {
        switch (item.ItemName)
        {
            case tpOSDKeys.COMMANDKEY_MODEL:
            case tpOSDKeys.COMMANDKEY_BAUDRATE:
            case tpOSDKeys.COMMANDKEY_AMPGAIN:
            case tpOSDKeys.COMMANDKEY_ZOOMAF:
            case tpOSDKeys.COMMANDKEY_PTAF:
            case tpOSDKeys.COMMANDKEY_AFAREASIZE:
            case tpOSDKeys.COMMANDKEY_AFAREAFRAME:
            case tpOSDKeys.COMMANDKEY_AFSEARCH:
            case tpOSDKeys.COMMANDKEY_ZOOMPOSINV:
            case tpOSDKeys.COMMANDKEY_FOCUSPOSINV:
            case tpOSDKeys.COMMANDKEY_IRISPOSINV:
            case tpOSDKeys.COMMANDKEY_CMDQINGAF:
            case tpOSDKeys.COMMANDKEY_CMDQINGPRST:
            case tpOSDKeys.COMMANDKEY_GRESPONSE:

                break;
        }
        if (ushort.TryParse(item.CurrentValue.Replace("0x", ""), NumberStyles.HexNumber, null, out ushort result))
        {
            if (result == item.SelectedItem.Data)
            {
                if (item.IsCommandEnabled) item.IsCommandEnabled = false;
            }
            else
            {
                if (!item.IsCommandEnabled) item.IsCommandEnabled = true;
            }
        }
    }

    private void TextBoxItemChanged(tpControlItem item)
    {
        switch (item.ItemName)
        {
            case tpOSDKeys.COMMANDKEY_ZOOMAFDELAY:
            case tpOSDKeys.COMMANDKEY_PTAFDELAY:
            case tpOSDKeys.COMMANDKEY_PTID:
            case tpOSDKeys.COMMANDKEY_AFTIMEOUT:
            case tpOSDKeys.COMMANDKEY_PWMFREQ:
            case tpOSDKeys.COMMANDKEY_ZOOMLSPOS:
            case tpOSDKeys.COMMANDKEY_USERAFSPD:
            case tpOSDKeys.COMMANDKEY_USERAF:
            case tpOSDKeys.COMMANDKEY_USERAFSTSPD:
            case tpOSDKeys.COMMANDKEY_USERAFSTPOS1:
            case tpOSDKeys.COMMANDKEY_USERMFOCUSL:
            case tpOSDKeys.COMMANDKEY_USERMFOCUSM:
            case tpOSDKeys.COMMANDKEY_USERMFOCUSH:

                break;
        }
        if (item.CurrentValue.Equals(item.SelectedItem.Data.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            if (item.IsCommandEnabled) item.IsCommandEnabled = false;
        }
        else
        {
            if (!item.IsCommandEnabled) item.IsCommandEnabled = true;
        }
    }

    private void DispatchCommand(tpControlItem item)
    {
        if (__camLens.GetCommand(item.CommandKey) is tpioLensCommand command && item.SelectedItem is tpioOSData osd)
        {
            for (int i = tpCONST.OSDTXCOUNT - 1; 0 <= i; i--)
            {
                byte[] data = command.Data;

                data[command.TXHIGH - 2] = (byte)(item.Address - i);
                //data[command.TXHIGH - 1] = osd.Data;

#if DEBUG && !FORDEBUG
                System.Diagnostics.Debug.WriteLine($"       >>>>>   {tpUtility.ToHexString(data)}");
#endif
            }
        }
    }

    private void OnItemChanged(tpControlItem item)
    {
        switch (item.InputType)
        {
            case eINPUTTYPE.COMBOBOX:
                ComboBoxItemChanged(item);

                break;
            case eINPUTTYPE.TEXTBOX:
                TextBoxItemChanged(item);

                break;
        }
    }

    private bool CanTransmit(object parameter) => true;
    private void OnTransmit(object parameter)
    {
        if (parameter is tpControlItem item)
        {
            DispatchCommand(item);
        }
    }

    private bool CanDefaultValue(object parameter) => true;
    private void OnDefaultValue(object parameter)
    {

    }

    private bool CanCloseWindow(object parameter) => true;
    private void OnCloseWindow(object parameter) => (parameter as Window)?.Close();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpControlItem : tpMvvm
{
    private bool __isCommandEnabled;

    private tpioOSData __selectedItem;
    private string __currentValue;

    public tpControlItem()
    {
        LostFocus = new tpRelayCommand(OnLostFocus, null);
        TextChanged = new tpRelayCommand(OnTextChanged, null);
    }

    #region ICommand
    public ICommand LostFocus { get; }
    public ICommand TextChanged { get; }
    #endregion

    public byte Address { get; set; }

    public string Key { get; set; }
    public string CommandKey { get; set; }

    public string Name { get; set; }
    public string ItemName { get; set; }

    public eINPUTTYPE InputType { get; set; }
    public IEnumerable<object> Items { get; set; }

    public bool IsEnabled { get; set; }
    public bool IsCommandEnabled
    {
        get => __isCommandEnabled;
        set => Set(ref __isCommandEnabled, value);
    }

    public tpioOSData SelectedItem
    {
        get => __selectedItem;
        set
        {
            switch (InputType)
            {
                case eINPUTTYPE.COMBOBOX:
                    if (Set(ref __selectedItem, value))
                    {
                        ItemChanged?.Invoke(this);
                    }
                    break;
                case eINPUTTYPE.TEXTBOX:
                    Set(ref __selectedItem, value);

                    ItemChanged?.Invoke(this);

                    break;
            }
        }
    }

    public object OldSelectedItem { get; set; }
    public string CurrentValue
    {
        get => __currentValue;
        set => Set(ref __currentValue, value);
    }

    public bool RebootIcon { get; set; }
    public bool BlockIcon { get; set; }

    public string Range { get; set; }
    public ushort Min { get; set; }
    public ushort Max { get; set; }

    public ICommand Command { get; set; }
    public object CommandParameter { get; set; }

    public Action<tpControlItem> ItemChanged { get; set; }

    private void OnLostFocus(object parameter)
    {
        switch (InputType)
        {
            case eINPUTTYPE.COMBOBOX:
                break;
            case eINPUTTYPE.TEXTBOX:
                SelectedItem.Description = SelectedItem.Data.ToString();

                break;
        }
    }

    private void OnTextChanged(object parameter)
    {
#if DEBUG && FORDEBUG
        System.Diagnostics.Debug.WriteLine($"tpControlItem::OnTextChanged     +++     Description : {SelectedItem.Description}, data : {SelectedItem.Data} ... min : {Min}, max : {Max}");
#endif
        if (parameter is ValueTuple<Key?, string> p && SelectedItem is tpioOSData data)
        {
            if (string.IsNullOrEmpty(p.Item2)) return;

            if (!tpInputValidator.IsIntInRange(p.Item2, Min, Max))
            {
                data.Description = CurrentValue;
            }
            if (ushort.TryParse(data.Description, out ushort result))
            {
                data.Data = result;
            }
            SelectedItem = data;
#if DEBUG && FORDEBUG
            System.Diagnostics.Debug.WriteLine($"tpControlItem::OnTextChanged     ---     Description : {SelectedItem.Description}, data : {SelectedItem.Data} ... min : {Min}, max : {Max}");
#endif
        }
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public static class tpInputValidator
{
    public static bool IsIntInRange(string input, ushort min, ushort max) => ushort.TryParse(input, out ushort result) && min <= result && result <= max;
}


//[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
//public class tpOSDToolMvvm : tpMvvm
//{
//    private readonly tpioCAMInformation __camInfo = tpWorkspace.Workspace.IOSetting.ControllerInfo;
//    private readonly topinsControlPadMvvm __controlPad = tpWorkspace.Workspace.Content.ControlPad;

//    private ObservableCollection<tpLensOSDItem> __modelItems;
//    private ObservableCollection<tpLensOSDItem> __baudRateItems;
//    private ObservableCollection<tpLensOSDItem> __ampGainItems;
//    private ObservableCollection<tpLensOSDItem> __zoomAFItems;
//    private ObservableCollection<tpLensOSDItem> __ptAFItems;
//    private ObservableCollection<tpLensOSDItem> __afAreaSizeItems;
//    private ObservableCollection<tpLensOSDItem> __afAreaFrameItems;
//    private ObservableCollection<tpLensOSDItem> __afSearchItems;
//    private ObservableCollection<tpLensOSDItem> __zoomPosInvItems;
//    private ObservableCollection<tpLensOSDItem> __focusPosInvItems;
//    private ObservableCollection<tpLensOSDItem> __irisPosInvItems;
//    private ObservableCollection<tpLensOSDItem> __cmdQingAFItems;
//    private ObservableCollection<tpLensOSDItem> __cmdQingPrstItems;
//    private ObservableCollection<tpLensOSDItem> __gresponseItems;

//    private tpLensOSDItem __selectedModelItem;
//    private tpLensOSDItem __selectedBaudRateItem;
//    private tpLensOSDItem __selectedAmpGainItem;
//    private tpLensOSDItem __selectedZoomAFItem;
//    private tpLensOSDItem __selectedPtAFItem;
//    private tpLensOSDItem __selectedAFAreaSizeItem;
//    private tpLensOSDItem __selectedAFAreaFrameItem;
//    private tpLensOSDItem __selectedAFSearchItem;
//    private tpLensOSDItem __selectedZoomPosInvItem;
//    private tpLensOSDItem __selectedFocusPosInvItem;
//    private tpLensOSDItem __selectedIrisPosInvItem;
//    private tpLensOSDItem __selectedCmdQingAFItem;
//    private tpLensOSDItem __selectedCmdQingPrstItem;
//    private tpLensOSDItem __selectedGResponseItem;

//    private tpLensOSDItem __zoomAFDelayItem;
//    private tpLensOSDItem __ptAFDelayItem;
//    private tpLensOSDItem __ptIDItem;
//    private tpLensOSDItem __afTimeoutItem;
//    private tpLensOSDItem __pwmFreqItem;
//    private tpLensOSDItem __zoomLSPosItem;
//    private tpLensOSDItem __userAFspdItem;
//    private tpLensOSDItem __userAFItem;
//    private tpLensOSDItem __userAFSTSPItem;
//    private tpLensOSDItem __userAFSTPos1Item;
//    private tpLensOSDItem __usermFocusLItem;
//    private tpLensOSDItem __usermFocusMItem;
//    private tpLensOSDItem __usermFocusHItem;

//    private string __modelName;
//    private string __baudRateName;
//    private string __ampgainName;
//    private string __zoomAFName;
//    private string __zoomAFDelayName;
//    private string __ptAFName;
//    private string __ptAFDelayName;
//    private string __ptIDName;
//    private string __afareaSizeName;
//    private string __afareaframeName;
//    private string __afsearchName;
//    private string __zoomPosInvName;
//    private string __focusPosInvName;
//    private string __irisPosInvName;
//    private string __afTimeoutName;
//    private string __cmdqingAFName;
//    private string __cmdqingprstName;
//    private string __pwmFreqName;
//    private string __zoomLSPosName;
//    private string __userAFspdName;
//    private string __userAFRangeName;
//    private string __userAFstspdName;
//    private string __userAFstPos1Name;
//    private string __usermFocusLName;
//    private string __usermFocusMName;
//    private string __usermFocusHName;
//    private string __gresponseName;

//    private string __modelValue;
//    private string __baudRateValue;
//    private string __ampgainValue;
//    private string __zoomAFValue;
//    private string __zoomAFDelayValue;
//    private string __ptAFValue;
//    private string __ptAFDelayValue;
//    private string __ptIDValue;
//    private string __afareaSizeValue;
//    private string __afareaframeValue;
//    private string __afsearchValue;
//    private string __zoomPosInvValue;
//    private string __focusPosInvValue;
//    private string __irisPosInvValue;
//    private string __afTimeoutValue;
//    private string __cmdqingAFValue;
//    private string __cmdqingprstValue;
//    private string __pwmFreqValue;
//    private string __zoomLSPosValue;
//    private string __userAFspdValue;
//    private string __userAFValueValue;
//    private string __userAFstspdValue;
//    private string __userAFstPos1Value;
//    private string __usermFocusLValue;
//    private string __usermFocusMValue;
//    private string __usermFocusHValue;
//    private string __gresponseValue;

//    //private string __modelRange;
//    //private string __baudRateRange;
//    //private string __ampgainRange;
//    //private string __zoomAFRange;
//    private string __zoomAFDelayRange;
//    //private string __ptAFRange;
//    private string __ptAFDelayRange;
//    private string __ptIDRange;
//    //private string __afareaSizeRange;
//    //private string __afareaframeRange;
//    //private string __afsearchRange;
//    //private string __zoomPosInvRange;
//    //private string __focusPosInvRange;
//    //private string __irisPosInvRange;
//    private string __afTimeoutRange;
//    //private string __cmdqingAFRange;
//    //private string __cmdqingprstRange;
//    private string __pwmFreqRange;
//    private string __zoomLSPosRange;
//    private string __userAFspdRange;
//    private string __userAFRangeRange;
//    private string __userAFstspdRange;
//    private string __userAFstPos1Range;
//    private string __usermFocusLRange;
//    private string __usermFocusMRange;
//    private string __usermFocusHRange;
//    //private string __gresponseRange;

//    private bool __modelRebootVisible;
//    private bool __baudRateRebootVisible;
//    private bool __ampgainRebootVisible;
//    private bool __zoomAFRebootVisible;
//    private bool __zoomAFDelayRebootVisible;
//    private bool __ptAFRebootVisible;
//    private bool __ptAFDelayRebootVisible;
//    private bool __ptIDRebootVisible;
//    private bool __afareaSizeRebootVisible;
//    private bool __afareaframeRebootVisible;
//    private bool __afsearchRebootVisible;
//    private bool __zoomPosInvRebootVisible;
//    private bool __focusPosInvRebootVisible;
//    private bool __irisPosInvRebootVisible;
//    private bool __afTimeoutRebootVisible;
//    private bool __cmdqingAFRebootVisible;
//    private bool __cmdqingprstRebootVisible;
//    private bool __pwmFreqRebootVisible;
//    private bool __zoomLSPosRebootVisible;
//    private bool __userAFspdRebootVisible;
//    private bool __userAFRebootVisible;
//    private bool __userAFstspdRebootVisible;
//    private bool __userAFstPos1RebootVisible;
//    private bool __usermFocusLRebootVisible;
//    private bool __usermFocusMRebootVisible;
//    private bool __usermFocusHRebootVisible;
//    private bool __gresponseRebootVisible;

//    private bool __modelBlockVisible;
//    private bool __baudRateBlockVisible;
//    private bool __ampgainBlockVisible;
//    private bool __zoomAFBlockVisible;
//    private bool __zoomAFDelayBlockVisible;
//    private bool __ptAFBlockVisible;
//    private bool __ptAFDelayBlockVisible;
//    private bool __ptIDBlockVisible;
//    private bool __afareaSizeBlockVisible;
//    private bool __afareaframeBlockVisible;
//    private bool __afsearchBlockVisible;
//    private bool __zoomPosInvBlockVisible;
//    private bool __focusPosInvBlockVisible;
//    private bool __irisPosInvBlockVisible;
//    private bool __afTimeoutBlockVisible;
//    private bool __cmdqingAFBlockVisible;
//    private bool __cmdqingprstBlockVisible;
//    private bool __pwmFreqBlockVisible;
//    private bool __zoomLSPosBlockVisible;
//    private bool __userAFspdBlockVisible;
//    private bool __userAFBlockVisible;
//    private bool __userAFstspdBlockVisible;
//    private bool __userAFstPos1BlockVisible;
//    private bool __usermFocusLBlockVisible;
//    private bool __usermFocusMBlockVisible;
//    private bool __usermFocusHBlockVisible;
//    private bool __gresponseBlockVisible;

//    private bool __isModelEnabled;
//    private bool __isBaudRateEnabled;
//    private bool __isAmpgainEnabled;
//    private bool __isZoomAFEnabled;
//    private bool __isZoomAFDelayEnabled;
//    private bool __isPtAFEnabled;
//    private bool __isPtAFDelayEnabled;
//    private bool __isPtIDEnabled;
//    private bool __isAFAreaSizeEnabled;
//    private bool __isAFAreaframeEnabled;
//    private bool __isAFSearchEnabled;
//    private bool __isZoomPosInvEnabled;
//    private bool __isFocusPosInvEnabled;
//    private bool __isIrisPosInvEnabled;
//    private bool __isAFTimeoutEnabled;
//    private bool __isCmdqingAFEnabled;
//    private bool __isCmdqingprstEnabled;
//    private bool __isPwmFreqEnabled;
//    private bool __isZoomLSPosEnabled;
//    private bool __isUserAFspdEnabled;
//    private bool __isUserAFEnabled;
//    private bool __isUserAFstspdEnabled;
//    private bool __isUserAFstPos1Enabled;
//    private bool __isUsermFocusLEnabled;
//    private bool __isUsermFocusMEnabled;
//    private bool __isUsermFocusHEnabled;
//    private bool __isGresponseEnabled;

//    #region tpRelayCommand
//    private tpRelayCommand __sendCommand;
//    private tpRelayCommand __closeWindowCommand;
//    #endregion

//    public tpOSDToolMvvm()
//    {
//        SetProperties();
//    }

//    #region ICommand
//    public ICommand SendCommand => __sendCommand ??= new tpRelayCommand(OnSendCommand, CanSendCommand);
//    public ICommand CloseWindow => __closeWindowCommand ??= new tpRelayCommand(OnCloseWindow, CanCloseWindow);
//    #endregion

//    public string ModelName
//    {
//        get => __modelName;
//        set => Set(ref __modelName, value);
//    }
//    public string BaudRateName
//    {
//        get => __baudRateName;
//        set => Set(ref __baudRateName, value);
//    }
//    public string AmpGainName
//    {
//        get => __ampgainName;
//        set => Set(ref __ampgainName, value);
//    }
//    public string ZoomAFName
//    {
//        get => __zoomAFName;
//        set => Set(ref __zoomAFName, value);
//    }
//    public string ZoomAFDelayName
//    {
//        get => __zoomAFDelayName;
//        set => Set(ref __zoomAFDelayName, value);
//    }
//    public string PtAFName
//    {
//        get => __ptAFName;
//        set => Set(ref __ptAFName, value);
//    }
//    public string PtAFDelayName
//    {
//        get => __ptAFDelayName;
//        set => Set(ref __ptAFDelayName, value);
//    }
//    public string PtIDName
//    {
//        get => __ptIDName;
//        set => Set(ref __ptIDName, value);
//    }
//    public string AFAreaSizeName
//    {
//        get => __afareaSizeName;
//        set => Set(ref __afareaSizeName, value);
//    }
//    public string AFAreaFrameName
//    {
//        get => __afareaframeName;
//        set => Set(ref __afareaframeName, value);
//    }
//    public string AFSearchName
//    {
//        get => __afsearchName;
//        set => Set(ref __afsearchName, value);
//    }
//    public string ZoomPosInvName
//    {
//        get => __zoomPosInvName;
//        set => Set(ref __zoomPosInvName, value);
//    }
//    public string FocusPosInvName
//    {
//        get => __focusPosInvName;
//        set => Set(ref __focusPosInvName, value);
//    }
//    public string IrisPosInvName
//    {
//        get => __irisPosInvName;
//        set => Set(ref __irisPosInvName, value);
//    }
//    public string AFTimeoutName
//    {
//        get => __afTimeoutName;
//        set => Set(ref __afTimeoutName, value);
//    }
//    public string CmdQingAFName
//    {
//        get => __cmdqingAFName;
//        set => Set(ref __cmdqingAFName, value);
//    }
//    public string CmdQingPrstName
//    {
//        get => __cmdqingprstName;
//        set => Set(ref __cmdqingprstName, value);
//    }
//    public string PwmFreqName
//    {
//        get => __pwmFreqName;
//        set => Set(ref __pwmFreqName, value);
//    }
//    public string ZoomLSPosName
//    {
//        get => __zoomLSPosName;
//        set => Set(ref __zoomLSPosName, value);
//    }
//    public string UserAFspdName
//    {
//        get => __userAFspdName;
//        set => Set(ref __userAFspdName, value);
//    }
//    public string UserAFRangeName
//    {
//        get => __userAFRangeName;
//        set => Set(ref __userAFRangeName, value);
//    }
//    public string UserAFSTSPDName
//    {
//        get => __userAFstspdName;
//        set => Set(ref __userAFstspdName, value);
//    }
//    public string UserAFSTPos1Name
//    {
//        get => __userAFstPos1Name;
//        set => Set(ref __userAFstPos1Name, value);
//    }
//    public string UsermFocusLName
//    {
//        get => __usermFocusLName;
//        set => Set(ref __usermFocusLName, value);
//    }
//    public string UsermFocusMName
//    {
//        get => __usermFocusMName;
//        set => Set(ref __usermFocusMName, value);
//    }
//    public string UsermFocusHName
//    {
//        get => __usermFocusHName;
//        set => Set(ref __usermFocusHName, value);
//    }
//    public string GResponseName
//    {
//        get => __gresponseName;
//        set => Set(ref __gresponseName, value);
//    }
//    public bool ModelRebootVisible
//    {
//        get => __modelRebootVisible;
//        set => Set(ref __modelRebootVisible, value);
//    }
//    public bool BaudRateRebootVisible
//    {
//        get => __baudRateRebootVisible;
//        set => Set(ref __baudRateRebootVisible, value);
//    }
//    public bool AmpGainRebootVisible
//    {
//        get => __ampgainRebootVisible;
//        set => Set(ref __ampgainRebootVisible, value);
//    }
//    public bool ZoomAFRebootVisible
//    {
//        get => __zoomAFRebootVisible;
//        set => Set(ref __zoomAFRebootVisible, value);
//    }
//    public bool ZoomAFDelayRebootVisible
//    {
//        get => __zoomAFDelayRebootVisible;
//        set => Set(ref __zoomAFDelayRebootVisible, value);
//    }
//    public bool PtAFRebootVisible
//    {
//        get => __ptAFRebootVisible;
//        set => Set(ref __ptAFRebootVisible, value);
//    }
//    public bool PtAFDelayRebootVisible
//    {
//        get => __ptAFDelayRebootVisible;
//        set => Set(ref __ptAFDelayRebootVisible, value);
//    }
//    public bool PtIDRebootVisible
//    {
//        get => __ptIDRebootVisible;
//        set => Set(ref __ptIDRebootVisible, value);
//    }
//    public bool AFAreaSizeRebootVisible
//    {
//        get => __afareaSizeRebootVisible;
//        set => Set(ref __afareaSizeRebootVisible, value);
//    }
//    public bool AFAreaFrameRebootVisible
//    {
//        get => __afareaframeRebootVisible;
//        set => Set(ref __afareaframeRebootVisible, value);
//    }
//    public bool AFSearchRebootVisible
//    {
//        get => __afsearchRebootVisible;
//        set => Set(ref __afsearchRebootVisible, value);
//    }
//    public bool ZoomPosInvRebootVisible
//    {
//        get => __zoomPosInvRebootVisible;
//        set => Set(ref __zoomPosInvRebootVisible, value);
//    }
//    public bool FocusPosInvRebootVisible
//    {
//        get => __focusPosInvRebootVisible;
//        set => Set(ref __focusPosInvRebootVisible, value);
//    }
//    public bool IrisPosInvRebootVisible
//    {
//        get => __irisPosInvRebootVisible;
//        set => Set(ref __irisPosInvRebootVisible, value);
//    }
//    public bool AFTimeoutRebootVisible
//    {
//        get => __afTimeoutRebootVisible;
//        set => Set(ref __afTimeoutRebootVisible, value);
//    }
//    public bool CmdQingAFRebootVisible
//    {
//        get => __cmdqingAFRebootVisible;
//        set => Set(ref __cmdqingAFRebootVisible, value);
//    }
//    public bool CmdQingPrstRebootVisible
//    {
//        get => __cmdqingprstRebootVisible;
//        set => Set(ref __cmdqingprstRebootVisible, value);
//    }
//    public bool PwmFreqRebootVisible
//    {
//        get => __pwmFreqRebootVisible;
//        set => Set(ref __pwmFreqRebootVisible, value);
//    }
//    public bool ZoomLSPosRebootVisible
//    {
//        get => __zoomLSPosRebootVisible;
//        set => Set(ref __zoomLSPosRebootVisible, value);
//    }
//    public bool UserAFspdRebootVisible
//    {
//        get => __userAFspdRebootVisible;
//        set => Set(ref __userAFspdRebootVisible, value);
//    }
//    public bool UserAFRebootVisible
//    {
//        get => __userAFRebootVisible;
//        set => Set(ref __userAFRebootVisible, value);
//    }
//    public bool UserAFSTSPDRebootVisible
//    {
//        get => __userAFstspdRebootVisible;
//        set => Set(ref __userAFstspdRebootVisible, value);
//    }
//    public bool UserAFSTPos1RebootVisible
//    {
//        get => __userAFstPos1RebootVisible;
//        set => Set(ref __userAFstPos1RebootVisible, value);
//    }
//    public bool UsermFocusLRebootVisible
//    {
//        get => __usermFocusLRebootVisible;
//        set => Set(ref __usermFocusLRebootVisible, value);
//    }
//    public bool UsermFocusMRebootVisible
//    {
//        get => __usermFocusMRebootVisible;
//        set => Set(ref __usermFocusMRebootVisible, value);
//    }
//    public bool UsermFocusHRebootVisible
//    {
//        get => __usermFocusHRebootVisible;
//        set => Set(ref __usermFocusHRebootVisible, value);
//    }
//    public bool GResponseRebootVisible
//    {
//        get => __gresponseRebootVisible;
//        set => Set(ref __gresponseRebootVisible, value);
//    }

//    public bool ModelBlockVisible
//    {
//        get => __modelBlockVisible;
//        set => Set(ref __modelBlockVisible, value);
//    }
//    public bool BaudRateBlockVisible
//    {
//        get => __baudRateBlockVisible;
//        set => Set(ref __baudRateBlockVisible, value);
//    }
//    public bool AmpGainBlockVisible
//    {
//        get => __ampgainBlockVisible;
//        set => Set(ref __ampgainBlockVisible, value);
//    }
//    public bool ZoomAFBlockVisible
//    {
//        get => __zoomAFBlockVisible;
//        set => Set(ref __zoomAFBlockVisible, value);
//    }
//    public bool ZoomAFDelayBlockVisible
//    {
//        get => __zoomAFDelayBlockVisible;
//        set => Set(ref __zoomAFDelayBlockVisible, value);
//    }
//    public bool PtAFBlockVisible
//    {
//        get => __ptAFBlockVisible;
//        set => Set(ref __ptAFBlockVisible, value);
//    }
//    public bool PtAFDelayBlockVisible
//    {
//        get => __ptAFDelayBlockVisible;
//        set => Set(ref __ptAFDelayBlockVisible, value);
//    }
//    public bool PtIDBlockVisible
//    {
//        get => __ptIDBlockVisible;
//        set => Set(ref __ptIDBlockVisible, value);
//    }
//    public bool AFAreaSizeBlockVisible
//    {
//        get => __afareaSizeBlockVisible;
//        set => Set(ref __afareaSizeBlockVisible, value);
//    }
//    public bool AFAreaFrameBlockVisible
//    {
//        get => __afareaframeBlockVisible;
//        set => Set(ref __afareaframeBlockVisible, value);
//    }
//    public bool AFSearchBlockVisible
//    {
//        get => __afsearchBlockVisible;
//        set => Set(ref __afsearchBlockVisible, value);
//    }
//    public bool ZoomPosInvBlockVisible
//    {
//        get => __zoomPosInvBlockVisible;
//        set => Set(ref __zoomPosInvBlockVisible, value);
//    }
//    public bool FocusPosInvBlockVisible
//    {
//        get => __focusPosInvBlockVisible;
//        set => Set(ref __focusPosInvBlockVisible, value);
//    }
//    public bool IrisPosInvBlockVisible
//    {
//        get => __irisPosInvBlockVisible;
//        set => Set(ref __irisPosInvBlockVisible, value);
//    }
//    public bool AFTimeoutBlockVisible
//    {
//        get => __afTimeoutBlockVisible;
//        set => Set(ref __afTimeoutBlockVisible, value);
//    }
//    public bool CmdQingAFBlockVisible
//    {
//        get => __cmdqingAFBlockVisible;
//        set => Set(ref __cmdqingAFBlockVisible, value);
//    }
//    public bool CmdQingPrstBlockVisible
//    {
//        get => __cmdqingprstBlockVisible;
//        set => Set(ref __cmdqingprstBlockVisible, value);
//    }
//    public bool PwmFreqBlockVisible
//    {
//        get => __pwmFreqBlockVisible;
//        set => Set(ref __pwmFreqBlockVisible, value);
//    }
//    public bool ZoomLSPosBlockVisible
//    {
//        get => __zoomLSPosBlockVisible;
//        set => Set(ref __zoomLSPosBlockVisible, value);
//    }
//    public bool UserAFspdBlockVisible
//    {
//        get => __userAFspdBlockVisible;
//        set => Set(ref __userAFspdBlockVisible, value);
//    }
//    public bool UserAFBlockVisible
//    {
//        get => __userAFBlockVisible;
//        set => Set(ref __userAFBlockVisible, value);
//    }
//    public bool UserAFSTSPDBlockVisible
//    {
//        get => __userAFstspdBlockVisible;
//        set => Set(ref __userAFstspdBlockVisible, value);
//    }
//    public bool UserAFSTPos1BlockVisible
//    {
//        get => __userAFstPos1BlockVisible;
//        set => Set(ref __userAFstPos1BlockVisible, value);
//    }
//    public bool UsermFocusLBlockVisible
//    {
//        get => __usermFocusLBlockVisible;
//        set => Set(ref __usermFocusLBlockVisible, value);
//    }
//    public bool UsermFocusMBlockVisible
//    {
//        get => __usermFocusMBlockVisible;
//        set => Set(ref __usermFocusMBlockVisible, value);
//    }
//    public bool UsermFocusHBlockVisible
//    {
//        get => __usermFocusHBlockVisible;
//        set => Set(ref __usermFocusHBlockVisible, value);
//    }
//    public bool GResponseBlockVisible
//    {
//        get => __gresponseBlockVisible;
//        set => Set(ref __gresponseBlockVisible, value);
//    }

//    public string ModelValue
//    {
//        get => __modelValue;
//        set => Set(ref __modelValue, value);
//    }
//    public string BaudRateValue
//    {
//        get => __baudRateValue;
//        set => Set(ref __baudRateValue, value);
//    }
//    public string AmpGainValue
//    {
//        get => __ampgainValue;
//        set => Set(ref __ampgainValue, value);
//    }
//    public string ZoomAFValue
//    {
//        get => __zoomAFValue;
//        set => Set(ref __zoomAFValue, value);
//    }
//    public string ZoomAFDelayValue
//    {
//        get => __zoomAFDelayValue;
//        set => Set(ref __zoomAFDelayValue, value);
//    }
//    public string PtAFValue
//    {
//        get => __ptAFValue;
//        set => Set(ref __ptAFValue, value);
//    }
//    public string PtAFDelayValue
//    {
//        get => __ptAFDelayValue;
//        set => Set(ref __ptAFDelayValue, value);
//    }
//    public string PtIDValue
//    {
//        get => __ptIDValue;
//        set => Set(ref __ptIDValue, value);
//    }
//    public string AFAreaSizeValue
//    {
//        get => __afareaSizeValue;
//        set => Set(ref __afareaSizeValue, value);
//    }
//    public string AFAreaFrameValue
//    {
//        get => __afareaframeValue;
//        set => Set(ref __afareaframeValue, value);
//    }
//    public string AFSearchValue
//    {
//        get => __afsearchValue;
//        set => Set(ref __afsearchValue, value);
//    }
//    public string ZoomPosInvValue
//    {
//        get => __zoomPosInvValue;
//        set => Set(ref __zoomPosInvValue, value);
//    }
//    public string FocusPosInvValue
//    {
//        get => __focusPosInvValue;
//        set => Set(ref __focusPosInvValue, value);
//    }
//    public string IrisPosInvValue
//    {
//        get => __irisPosInvValue;
//        set => Set(ref __irisPosInvValue, value);
//    }
//    public string AFTimeoutValue
//    {
//        get => __afTimeoutValue;
//        set => Set(ref __afTimeoutValue, value);
//    }
//    public string CmdQingAFValue
//    {
//        get => __cmdqingAFValue;
//        set => Set(ref __cmdqingAFValue, value);
//    }
//    public string CmdQingPrstValue
//    {
//        get => __cmdqingprstValue;
//        set => Set(ref __cmdqingprstValue, value);
//    }
//    public string PwmFreqValue
//    {
//        get => __pwmFreqValue;
//        set => Set(ref __pwmFreqValue, value);
//    }
//    public string ZoomLSPosValue
//    {
//        get => __zoomLSPosValue;
//        set => Set(ref __zoomLSPosValue, value);
//    }
//    public string UserAFspdValue
//    {
//        get => __userAFspdValue;
//        set => Set(ref __userAFspdValue, value);
//    }
//    public string UserAFRangeValue
//    {
//        get => __userAFValueValue;
//        set => Set(ref __userAFValueValue, value);
//    }
//    public string UserAFSTSPValue
//    {
//        get => __userAFstspdValue;
//        set => Set(ref __userAFstspdValue, value);
//    }
//    public string UserAFSTPos1Value
//    {
//        get => __userAFstPos1Value;
//        set => Set(ref __userAFstPos1Value, value);
//    }
//    public string UsermFocusLValue
//    {
//        get => __usermFocusLValue;
//        set => Set(ref __usermFocusLValue, value);
//    }
//    public string UsermFocusMValue
//    {
//        get => __usermFocusMValue;
//        set => Set(ref __usermFocusMValue, value);
//    }
//    public string UsermFocusHValue
//    {
//        get => __usermFocusHValue;
//        set => Set(ref __usermFocusHValue, value);
//    }
//    public string GResponseValue
//    {
//        get => __gresponseValue;
//        set => Set(ref __gresponseValue, value);
//    }
//    //public string ModelRange
//    //{
//    //    get => __modelRange;
//    //    set => Set(ref __modelRange, value);
//    //}
//    //public string BaudRateRange
//    //{
//    //    get => __baudRateRange;
//    //    set => Set(ref __baudRateRange, value);
//    //}
//    //public string AmpGainRange
//    //{
//    //    get => __ampgainRange;
//    //    set => Set(ref __ampgainRange, value);
//    //}
//    //public string ZoomAFRange
//    //{
//    //    get => __zoomAFRange;
//    //    set => Set(ref __zoomAFRange, value);
//    //}
//    public string ZoomAFDelayRange
//    {
//        get => __zoomAFDelayRange;
//        set => Set(ref __zoomAFDelayRange, value);
//    }
//    //public string PtAFRange
//    //{
//    //    get => __ptAFRange;
//    //    set => Set(ref __ptAFRange, value);
//    //}
//    public string PtAFDelayRange
//    {
//        get => __ptAFDelayRange;
//        set => Set(ref __ptAFDelayRange, value);
//    }
//    public string PtIDRange
//    {
//        get => __ptIDRange;
//        set => Set(ref __ptIDRange, value);
//    }
//    //public string AFAreaSizeRange
//    //{
//    //    get => __afareaSizeRange;
//    //    set => Set(ref __afareaSizeRange, value);
//    //}
//    //public string AfAreaFrameRange
//    //{
//    //    get => __afareaframeRange;
//    //    set => Set(ref __afareaframeRange, value);
//    //}
//    //public string AfSearchRange
//    //{
//    //    get => __afsearchRange;
//    //    set => Set(ref __afsearchRange, value);
//    //}
//    //public string ZoomPosInvRange
//    //{
//    //    get => __zoomPosInvRange;
//    //    set => Set(ref __zoomPosInvRange, value);
//    //}
//    //public string FocusPosInvRange
//    //{
//    //    get => __focusPosInvRange;
//    //    set => Set(ref __focusPosInvRange, value);
//    //}
//    //public string IrisPosInvRange
//    //{
//    //    get => __irisPosInvRange;
//    //    set => Set(ref __irisPosInvRange, value);
//    //}
//    public string AFTimeoutRange
//    {
//        get => __afTimeoutRange;
//        set => Set(ref __afTimeoutRange, value);
//    }
//    //public string CmdQingAFRange
//    //{
//    //    get => __cmdqingAFRange;
//    //    set => Set(ref __cmdqingAFRange, value);
//    //}
//    //public string CmdQingPrstRange
//    //{
//    //    get => __cmdqingprstRange;
//    //    set => Set(ref __cmdqingprstRange, value);
//    //}
//    public string PwmFreqRange
//    {
//        get => __pwmFreqRange;
//        set => Set(ref __pwmFreqRange, value);
//    }
//    public string ZoomLSPosRange
//    {
//        get => __zoomLSPosRange;
//        set => Set(ref __zoomLSPosRange, value);
//    }
//    public string UserAFspdRange
//    {
//        get => __userAFspdRange;
//        set => Set(ref __userAFspdRange, value);
//    }
//    public string UserAFRange
//    {
//        get => __userAFRangeRange;
//        set => Set(ref __userAFRangeRange, value);
//    }
//    public string UserAFSTSPRange
//    {
//        get => __userAFstspdRange;
//        set => Set(ref __userAFstspdRange, value);
//    }
//    public string UserAFSTPos1Range
//    {
//        get => __userAFstPos1Range;
//        set => Set(ref __userAFstPos1Range, value);
//    }
//    public string UsermFocusLRange
//    {
//        get => __usermFocusLRange;
//        set => Set(ref __usermFocusLRange, value);
//    }
//    public string UsermFocusMRange
//    {
//        get => __usermFocusMRange;
//        set => Set(ref __usermFocusMRange, value);
//    }
//    public string UsermFocusHRange
//    {
//        get => __usermFocusHRange;
//        set => Set(ref __usermFocusHRange, value);
//    }
//    //public string GResponseRange
//    //{
//    //    get => __gresponseRange;
//    //    set => Set(ref __gresponseRange, value);
//    //}

//    public bool IsModelEnabled
//    {
//        get => __isModelEnabled;
//        set => Set(ref __isModelEnabled, value);
//    }
//    public bool IsBaudRateEnabled
//    {
//        get => __isBaudRateEnabled;
//        set => Set(ref __isBaudRateEnabled, value);
//    }
//    public bool IsAmpGainEnabled
//    {
//        get => __isAmpgainEnabled;
//        set => Set(ref __isAmpgainEnabled, value);
//    }
//    public bool IsZoomAFEnabled
//    {
//        get => __isZoomAFEnabled;
//        set => Set(ref __isZoomAFEnabled, value);
//    }
//    public bool IsZoomAFDelayEnabled
//    {
//        get => __isZoomAFDelayEnabled;
//        set => Set(ref __isZoomAFDelayEnabled, value);
//    }
//    public bool IsPtAFEnabled
//    {
//        get => __isPtAFEnabled;
//        set => Set(ref __isPtAFEnabled, value);
//    }
//    public bool IsPtAFDelayEnabled
//    {
//        get => __isPtAFDelayEnabled;
//        set => Set(ref __isPtAFDelayEnabled, value);
//    }
//    public bool IsPtIDEnabled
//    {
//        get => __isPtIDEnabled;
//        set => Set(ref __isPtIDEnabled, value);
//    }
//    public bool IsAFAreaSizeEnabled
//    {
//        get => __isAFAreaSizeEnabled;
//        set => Set(ref __isAFAreaSizeEnabled, value);
//    }
//    public bool IsAFAreaframeEnabled
//    {
//        get => __isAFAreaframeEnabled;
//        set => Set(ref __isAFAreaframeEnabled, value);
//    }
//    public bool IsAFSearchEnabled
//    {
//        get => __isAFSearchEnabled;
//        set => Set(ref __isAFSearchEnabled, value);
//    }   
//    public bool IsZoomPosInvEnabled
//    {
//        get => __isZoomPosInvEnabled;
//        set => Set(ref __isZoomPosInvEnabled, value);
//    }
//    public bool IsFocusPosInvEnabled
//    {
//        get => __isFocusPosInvEnabled;
//        set => Set(ref __isFocusPosInvEnabled, value);
//    }
//    public bool IsIrisPosInvEnabled
//    {
//        get => __isIrisPosInvEnabled;
//        set => Set(ref __isIrisPosInvEnabled, value);
//    }
//    public bool IsAFTimeoutEnabled
//    {
//        get => __isAFTimeoutEnabled;
//        set => Set(ref __isAFTimeoutEnabled, value);
//    }
//    public bool IsCmdqingAFEnabled
//    {
//        get => __isCmdqingAFEnabled;
//        set => Set(ref __isCmdqingAFEnabled, value);
//    }
//    public bool IsCmdqingprstEnabled
//    {
//        get => __isCmdqingprstEnabled;
//        set => Set(ref __isCmdqingprstEnabled, value);
//    }
//    public bool IsPwmFreqEnabled
//    {
//        get => __isPwmFreqEnabled;
//        set => Set(ref __isPwmFreqEnabled, value);
//    }
//    public bool IsZoomLSPosEnabled
//    {
//        get => __isZoomLSPosEnabled;
//        set => Set(ref __isZoomLSPosEnabled, value);
//    }
//    public bool IsUserAFspdEnabled
//    {
//        get => __isUserAFspdEnabled;
//        set => Set(ref __isUserAFspdEnabled, value);
//    }   
//    public bool IsUserAFEnabled
//    {
//        get => __isUserAFEnabled;
//        set => Set(ref __isUserAFEnabled, value);
//    }
//    public bool IsUserAFstspdEnabled
//    {
//        get => __isUserAFstspdEnabled;
//        set => Set(ref __isUserAFstspdEnabled, value);
//    }
//    public bool IsUserAFstPos1Enabled
//    {
//        get => __isUserAFstPos1Enabled;
//        set => Set(ref __isUserAFstPos1Enabled, value);
//    }
//    public bool IsUsermFocusLEnabled
//    {
//        get => __isUsermFocusLEnabled;
//        set => Set(ref __isUsermFocusLEnabled, value);
//    }
//    public bool IsUsermFocusMEnabled
//    {
//        get => __isUsermFocusMEnabled;
//        set => Set(ref __isUsermFocusMEnabled, value);
//    }
//    public bool IsUsermFocusHEnabled
//    {
//        get => __isUsermFocusHEnabled;
//        set => Set(ref __isUsermFocusHEnabled, value);
//    }
//    public bool IsGresponseEnabled
//    {
//        get => __isGresponseEnabled;
//        set => Set(ref __isGresponseEnabled, value);
//    }


//    public ObservableCollection<tpLensOSDItem> ModelItems => __modelItems;
//    public tpLensOSDItem SelectedModelItem
//    {
//        get => __selectedModelItem;
//        set => Set(ref __selectedModelItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> BaudRateItems => __baudRateItems;
//    public tpLensOSDItem SelectedBaudRateItem
//    {
//        get => __selectedBaudRateItem;
//        set => Set(ref __selectedBaudRateItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> AmpGainItems => __ampGainItems;
//    public tpLensOSDItem SelectedAmpGainItem
//    {
//        get => __selectedAmpGainItem;
//        set => Set(ref __selectedAmpGainItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> ZoomAFItems => __zoomAFItems;
//    public tpLensOSDItem SelectedZoomAFItem
//    {
//        get => __selectedZoomAFItem;
//        set => Set(ref __selectedZoomAFItem, value);
//    }

//    public tpLensOSDItem ZoomAFDelayItem
//    {
//        get => __zoomAFDelayItem;
//        set
//        {
//            Set(ref __zoomAFDelayItem, value);
//        }
//    }

//    public ObservableCollection<tpLensOSDItem> PtAFItems => __ptAFItems;
//    public tpLensOSDItem SelectedPtAFItem
//    {
//        get => __selectedPtAFItem;
//        set => Set(ref __selectedPtAFItem, value);
//    }

//    public tpLensOSDItem PTAFDelayItem
//    {
//        get => __ptAFDelayItem;
//        set
//        {
//            Set(ref __ptAFDelayItem, value);
//        }
//    }
//    public tpLensOSDItem PTIDItem
//    {
//        get => __ptIDItem;
//        set
//        {
//            Set(ref __ptIDItem, value);
//        }
//    }

//    public ObservableCollection<tpLensOSDItem> AFAreaSizeItems => __afAreaSizeItems;
//    public tpLensOSDItem SelectedAFAreaSizeItem
//    {
//        get => __selectedAFAreaSizeItem;
//        set => Set(ref __selectedAFAreaSizeItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> AFAreaFrameItems => __afAreaFrameItems;
//    public tpLensOSDItem SelectedAFAreaFrameItem
//    {
//        get => __selectedAFAreaFrameItem;
//        set => Set(ref __selectedAFAreaFrameItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> AFSearchItems => __afSearchItems;
//    public tpLensOSDItem SelectedAFSearchItem
//    {
//        get => __selectedAFSearchItem;
//        set => Set(ref __selectedAFSearchItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> ZoomPosInvItems => __zoomPosInvItems;
//    public tpLensOSDItem SelectedZoomPosInvItem
//    {
//        get => __selectedZoomPosInvItem;
//        set => Set(ref __selectedZoomPosInvItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> FocusPosInvItems => __focusPosInvItems;
//    public tpLensOSDItem SelectedFocusPosInvItem
//    {
//        get => __selectedFocusPosInvItem;
//        set => Set(ref __selectedFocusPosInvItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> IrisPosInvItems => __irisPosInvItems;
//    public tpLensOSDItem SelectedIrisPosInvItem
//    {
//        get => __selectedIrisPosInvItem;
//        set => Set(ref __selectedIrisPosInvItem, value);
//    }

//    public tpLensOSDItem AFTimeoutItem
//    {
//        get => __afTimeoutItem;
//        set
//        {
//            Set(ref __afTimeoutItem, value);
//        }
//    }

//    public ObservableCollection<tpLensOSDItem> CmdQingAFItems => __cmdQingAFItems;
//    public tpLensOSDItem SelectedCmdQingAFItem
//    {
//        get => __selectedCmdQingAFItem;
//        set => Set(ref __selectedCmdQingAFItem, value);
//    }
//    public ObservableCollection<tpLensOSDItem> CmdQingPrstItems => __cmdQingPrstItems;
//    public tpLensOSDItem SelectedCmdQingPrstItem
//    {
//        get => __selectedCmdQingPrstItem;
//        set => Set(ref __selectedCmdQingPrstItem, value);
//    }

//    public tpLensOSDItem PwmFreqItem
//    {
//        get => __pwmFreqItem;
//        set
//        {
//            Set(ref __pwmFreqItem, value);
//        }
//    }
//    public tpLensOSDItem ZoomLSPosItem
//    {
//        get => __zoomLSPosItem;
//        set
//        {
//            Set(ref __zoomLSPosItem, value);
//        }
//    }
//    public tpLensOSDItem UserAFspdItem
//    {
//        get => __userAFspdItem;
//        set
//        {
//            Set(ref __userAFspdItem, value);
//        }
//    }
//    public tpLensOSDItem UserAFItem
//    {
//        get => __userAFItem;
//        set
//        {
//            Set(ref __userAFItem, value);
//        }
//    }
//    public tpLensOSDItem UserAFSTSPItem
//    {
//        get => __userAFSTSPItem;
//        set
//        {
//            Set(ref __userAFSTSPItem, value);
//        }
//    }
//    public tpLensOSDItem UserAFSTPos1Item
//    {
//        get => __userAFSTPos1Item;
//        set
//        {
//            Set(ref __userAFSTPos1Item, value);
//        }
//    }
//    public tpLensOSDItem UsermFocusLItem
//    {
//        get => __usermFocusLItem;
//        set
//        {
//            Set(ref __usermFocusLItem, value);
//        }
//    }
//    public tpLensOSDItem UsermFocusMItem
//    {
//        get => __usermFocusMItem;
//        set
//        {
//            Set(ref __usermFocusMItem, value);
//        }
//    }
//    public tpLensOSDItem UsermFocusHItem
//    {
//        get => __usermFocusHItem;
//        set
//        {
//            Set(ref __usermFocusHItem, value);
//        }
//    }

//    public ObservableCollection<tpLensOSDItem> GResponseItems => __gresponseItems;
//    public tpLensOSDItem SelectedGResponseItem
//    {
//        get => __selectedGResponseItem;
//        set => Set(ref __selectedGResponseItem, value);
//    }

//    private string GetLensOSDItemName(ObservableCollection<tpLensOSDItem> items, ushort data)
//    {
//        foreach (var item in items)
//        {
//            if (item.Data == data)
//            {
//                return item.Name;
//            }
//        }
//        return string.Empty;
//    }

//    private ObservableCollection<tpLensOSDItem> AddLensOSDItems(ref tpLensOSDItem item, tpioLensInfo lens, ushort data)
//    {
//        ObservableCollection<tpLensOSDItem> combobox = [];

//        for (int i = 0; i < lens.Data.Count; i++)
//        {
//            tpLensOSDItem lensItem = new()
//            {
//                Address = lens.Address,
//                Name = lens.Data[i].Description,
//                Data = lens.Data[i].Data,
//                PrevData = lens.Data[i].Data
//            };
//            if (data.Equals(lensItem.Data))
//            {
//                item = lensItem;
//            }
//            combobox.Add(lensItem);
//        }
//        return combobox;
//    }

//    private void SetComboBoxRange(tpioLensInfo lens)
//    {
//        tpLensOSDItem item = new();

//        switch (lens.Name)
//        {
//            case tpOSDKeys.MODEL:
//                __modelItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.ModelValue);

//                SelectedModelItem = item;

//                break;
//            case tpOSDKeys.BAUDRATE:
//                __baudRateItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.BaudRateValue);

//                SelectedBaudRateItem = item;
//                break;
//            case tpOSDKeys.AMPGAIN:
//                __ampGainItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.AmpGainValue);

//                SelectedAmpGainItem = item;
//                break;
//            case tpOSDKeys.ZOOMAF:
//                __zoomAFItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.ZoomAFValue);

//                SelectedZoomAFItem = item;

//                break;
//            case tpOSDKeys.PTAF:
//                __ptAFItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.PtAFValue);

//                SelectedPtAFItem = item;

//                break;
//            case tpOSDKeys.AFAREASIZE:
//                __afAreaSizeItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.AFAreaSizeValue);

//                SelectedAFAreaSizeItem = item;

//                break;
//            case tpOSDKeys.AFAREAFRAME:
//                __afAreaFrameItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.AFAreaFrameValue);

//                SelectedAFAreaFrameItem = item;

//                break;
//            case tpOSDKeys.AFSEARCH:
//                __afSearchItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.AFSearchValue);

//                SelectedAFSearchItem = item;

//                break;
//            case tpOSDKeys.ZOOMPOSINV:
//                __zoomPosInvItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.ZoomPosInvValue);

//                SelectedZoomPosInvItem = item;

//                break;
//            case tpOSDKeys.FOCUSPOSINV:
//                __focusPosInvItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.FocusPosInvValue);

//                SelectedFocusPosInvItem = item;

//                break;
//            case tpOSDKeys.IRISPOSINV:
//                __irisPosInvItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.IrisPosInvValue);

//                SelectedIrisPosInvItem = item;

//                break;
//            case tpOSDKeys.CMDQINGAF:
//                __cmdQingAFItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.CmdQingAFValue);

//                SelectedCmdQingAFItem = item;

//                break;
//            case tpOSDKeys.CMDQINGPRST:
//                __cmdQingPrstItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.CmdQingPrstValue);

//                SelectedCmdQingPrstItem = item;

//                break;
//            case tpOSDKeys.GRESPONSE:
//                __gresponseItems = AddLensOSDItems(ref item, lens, __camInfo.OSDValue.GResponseValue);

//                SelectedGResponseItem = item;

//                break;
//        }
//    }

//    private string SetTextBoxRange(ref tpLensOSDItem item, tpioLensInfo lens, ushort data)
//    {
//        item = new tpLensOSDItem
//        {
//            Address = lens.Address,
//            Name = data.ToString(),
//            Data = data,
//            PrevData = data,

//            RangeMin = lens.Data[0].Data,
//            RangeMax = lens.Data[1].Data
//        };
//        if (2 == lens.Data.Count)        // 2 < ???
//        {
//            return $"{lens.Data[0].Data,6} ~ {lens.Data[1].Data,6}";
//        }
//        return string.Empty;
//    }

//    private void SetProperties()
//    {
//        tpioLensInfo lens;
//        tpLensOSDItem item = new();

//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.MODEL_0X01)) is not null)
//        {
//            SetComboBoxRange(lens);

//            ModelName = lens.Name;
//            ModelRebootVisible = lens.NeetToReboot;
//            ModelBlockVisible = lens.Block;
//            ModelValue = GetLensOSDItemName(ModelItems, __camInfo.OSDValue.ModelValue);
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.BAUDRATE_0X05)) is not null)
//        {
//            SetComboBoxRange(lens);

//            BaudRateName = lens.Name;
//            BaudRateRebootVisible = lens.NeetToReboot;
//            BaudRateBlockVisible = lens.Block;
//            BaudRateValue = GetLensOSDItemName(BaudRateItems, __camInfo.OSDValue.BaudRateValue);
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.AMPGAIN_0X07)) is not null)
//        {
//            SetComboBoxRange(lens);

//            AmpGainName = lens.Name;
//            AmpGainRebootVisible = lens.NeetToReboot;
//            AmpGainBlockVisible = lens.Block;
//            AmpGainValue = GetLensOSDItemName(AmpGainItems, __camInfo.OSDValue.AmpGainValue);
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.ZOOMAF_0X09)) is not null)
//        {
//            SetComboBoxRange(lens);

//            ZoomAFName = lens.Name;
//            ZoomAFRebootVisible = lens.NeetToReboot;
//            ZoomAFBlockVisible = lens.Block;
//            ZoomAFValue = __camInfo.OSDValue.ZoomAFValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.ZOOMAFDELAY_0X0B)) is not null)
//        {
//            ZoomAFDelayRange = SetTextBoxRange(ref item, lens, __camInfo.OSDValue.ZoomAFDelayValue);
//            ZoomAFDelayName = lens.Name;
//            ZoomAFDelayRebootVisible = lens.NeetToReboot;
//            ZoomAFDelayBlockVisible = lens.Block;
//            ZoomAFDelayItem = item;
//            ZoomAFDelayValue = __camInfo.OSDValue.ZoomAFDelayValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.PTAF_0X0D)) is not null)
//        {
//            SetComboBoxRange(lens);

//            PtAFName = lens.Name;
//            PtAFRebootVisible = lens.NeetToReboot;
//            PtAFBlockVisible = lens.Block;
//            PtAFValue = __camInfo.OSDValue.PtAFValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.PTAFDELAY_0X0F)) is not null)
//        {
//            PtAFDelayName = lens.Name;
//            PtAFDelayRebootVisible = lens.NeetToReboot;
//            PtAFDelayBlockVisible = lens.Block;

//            PtAFDelayRange = SetTextBoxRange(ref __ptAFDelayItem, lens, __camInfo.OSDValue.PtAFDelayValue);
//            PtAFDelayValue = __camInfo.OSDValue.PtAFDelayValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.PTID_0X15)) is not null)
//        {
//            PtIDName = lens.Name;
//            PtIDRebootVisible = lens.NeetToReboot;
//            PtIDBlockVisible = lens.Block;

//            PtIDRange = SetTextBoxRange(ref __ptIDItem, lens, __camInfo.OSDValue.PtIDValue);
//            PtIDValue = __camInfo.OSDValue.PtIDValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.AFAREASIZE_0X19)) is not null)
//        {
//            SetComboBoxRange(lens);

//            AFAreaSizeName = lens.Name;
//            AFAreaSizeRebootVisible = lens.NeetToReboot;
//            AFAreaSizeBlockVisible = lens.Block;
//            AFAreaSizeValue = __camInfo.OSDValue.AFAreaSizeValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.AFAREAFRAME_0X1B)) is not null)
//        {
//            SetComboBoxRange(lens);

//            AFAreaFrameName = lens.Name;
//            AFAreaFrameRebootVisible = lens.NeetToReboot;
//            AFAreaFrameBlockVisible = lens.Block;
//            AFAreaFrameValue = __camInfo.OSDValue.AFAreaFrameValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.AFSEARCH_0X1D)) is not null)
//        {
//            SetComboBoxRange(lens);

//            AFSearchName = lens.Name;
//            AFSearchRebootVisible = lens.NeetToReboot;
//            AFSearchBlockVisible = lens.Block;
//            AFSearchValue = __camInfo.OSDValue.AFSearchValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.ZOOMPOSINV_0X25)) is not null)
//        {
//            SetComboBoxRange(lens);

//            ZoomPosInvName = lens.Name;
//            ZoomPosInvRebootVisible = lens.NeetToReboot;
//            ZoomPosInvBlockVisible = lens.Block;
//            ZoomPosInvValue = __camInfo.OSDValue.ZoomPosInvValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.FOCUSPOSINV_0X27)) is not null)
//        {
//            SetComboBoxRange(lens);

//            FocusPosInvName = lens.Name;
//            FocusPosInvRebootVisible = lens.NeetToReboot;
//            FocusPosInvBlockVisible = lens.Block;
//            FocusPosInvValue = __camInfo.OSDValue.FocusPosInvValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.IRISPOSINV_0X29)) is not null)
//        {
//            SetComboBoxRange(lens);

//            IrisPosInvName = lens.Name;
//            IrisPosInvRebootVisible = lens.NeetToReboot;
//            IrisPosInvBlockVisible = lens.Block;
//            IrisPosInvValue = __camInfo.OSDValue.IrisPosInvValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.AFTIMEOUT_0X33)) is not null)
//        {
//            AFTimeoutName = lens.Name;
//            AFTimeoutRebootVisible = lens.NeetToReboot;
//            AFTimeoutBlockVisible = lens.Block;

//            AFTimeoutRange = SetTextBoxRange(ref __afTimeoutItem, lens, __camInfo.OSDValue.AFTimeoutValue);
//            AFTimeoutValue = __camInfo.OSDValue.AFTimeoutValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.CMDQINGAF_0X35)) is not null)
//        {
//            SetComboBoxRange(lens);

//            CmdQingAFName = lens.Name;
//            CmdQingAFRebootVisible = lens.NeetToReboot;
//            CmdQingAFBlockVisible = lens.Block;
//            CmdQingAFValue = __camInfo.OSDValue.CmdQingAFValue.ToString();

//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.CMDQINGPRST_0X37)) is not null)
//        {
//            SetComboBoxRange(lens);

//            CmdQingPrstName = lens.Name;
//            CmdQingPrstRebootVisible = lens.NeetToReboot;
//            CmdQingPrstBlockVisible = lens.NeetToReboot;
//            CmdQingPrstValue = __camInfo.OSDValue.CmdQingPrstValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.PWMFREQ_0X39)) is not null)
//        {
//            PwmFreqName = lens.Name;
//            PwmFreqRebootVisible = lens.NeetToReboot;
//            PwmFreqBlockVisible = lens.Block;

//            PwmFreqRange = SetTextBoxRange(ref __pwmFreqItem, lens, __camInfo.OSDValue.PwmFreqValue);
//            PwmFreqValue = __camInfo.OSDValue.PwmFreqValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.ZOOMLSPOS_0X3B)) is not null)
//        {
//            ZoomLSPosName = lens.Name;
//            ZoomLSPosRebootVisible = lens.NeetToReboot;
//            ZoomLSPosBlockVisible = lens.Block;

//            ZoomLSPosRange = SetTextBoxRange(ref __zoomLSPosItem, lens, __camInfo.OSDValue.ZoomLSPosValue);
//            ZoomLSPosValue = __camInfo.OSDValue.ZoomLSPosValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.USERAFSPD_0X43)) is not null)
//        {
//            UserAFspdName = lens.Name;
//            UserAFspdRebootVisible = lens.NeetToReboot;
//            UserAFspdBlockVisible = lens.Block;

//            UserAFspdRange = SetTextBoxRange(ref __userAFspdItem, lens, __camInfo.OSDValue.UserAFspdValue);
//            UserAFspdValue = __camInfo.OSDValue.UserAFspdValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.USERAFRANGE_0X45)) is not null)
//        {
//            UserAFRangeName = lens.Name;
//            UserAFRebootVisible = lens.NeetToReboot;
//            UserAFBlockVisible = lens.Block;

//            UserAFRange = SetTextBoxRange(ref __userAFItem, lens, __camInfo.OSDValue.UserAFRangeValue);
//            UserAFRangeValue = __camInfo.OSDValue.UserAFRangeValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.USERAFSTSPD_0X47)) is not null)
//        {
//            UserAFSTSPDName = lens.Name;
//            UserAFSTSPDRebootVisible = lens.NeetToReboot;
//            UserAFSTSPDBlockVisible = lens.Block;

//            UserAFSTSPRange = SetTextBoxRange(ref __userAFSTSPItem, lens, __camInfo.OSDValue.UserAFSTSPValue);
//            UserAFSTSPValue = __camInfo.OSDValue.UserAFSTSPValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.USERAFSTPOS1_0X49)) is not null)
//        {
//            UserAFSTPos1Name = lens.Name;
//            UserAFSTPos1RebootVisible = lens.NeetToReboot;
//            UserAFSTPos1BlockVisible = lens.Block;

//            UserAFSTPos1Range = SetTextBoxRange(ref __userAFSTPos1Item, lens, __camInfo.OSDValue.UserAFSTPos1Value);
//            UserAFSTPos1Value = __camInfo.OSDValue.UserAFSTPos1Value.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.USERMFOCUSL_0X4D)) is not null)
//        {
//            UsermFocusLName = lens.Name;
//            UsermFocusLRebootVisible = lens.NeetToReboot;
//            UsermFocusLBlockVisible = lens.Block;

//            UsermFocusLRange = SetTextBoxRange(ref __usermFocusLItem, lens, __camInfo.OSDValue.UsermFocusLValue);
//            UsermFocusLValue = __camInfo.OSDValue.UsermFocusLValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.USERMFOCUSM_0X4F)) is not null)
//        {
//            UsermFocusMName = lens.Name;
//            UsermFocusMRebootVisible = lens.NeetToReboot;
//            UsermFocusMBlockVisible = lens.Block;

//            UsermFocusMRange = SetTextBoxRange(ref __usermFocusMItem, lens, __camInfo.OSDValue.UsermFocusMValue);
//            UsermFocusMValue = __camInfo.OSDValue.UsermFocusMValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.USERMFOCUSH_0X51)) is not null)
//        {
//            UsermFocusHName = lens.Name;
//            UsermFocusHRebootVisible = lens.NeetToReboot;
//            UsermFocusHBlockVisible = lens.Block;

//            UsermFocusHRange = SetTextBoxRange(ref __usermFocusHItem, lens, __camInfo.OSDValue.UsermFocusHValue);
//            UsermFocusHValue = __camInfo.OSDValue.UsermFocusHValue.ToString();
//        }
//        if ((lens = __camInfo.GetOSDInfo(tpOSDKeys.GRESPONSE_0X61)) is not null)
//        {
//            SetComboBoxRange(lens);

//            GResponseName = lens.Name;
//            GResponseRebootVisible = lens.NeetToReboot;
//            GResponseBlockVisible = lens.Block;
//            GResponseValue = __camInfo.OSDValue.GResponseValue.ToString();
//        }
//    }

//    private void RxAllValue()
//    {

//    }

//    private void TxCommand(byte address, ushort data, bool ioType)
//    {
//        tpioLensCommand command;

//        if (ioType)
//        {
//            command = __camInfo.GetCommand(tpCommandKeys.OSDSETTINGSAVE);
//        }
//        else
//        {
//            command = __camInfo.GetCommand(tpCommandKeys.OSDSETTINGLOAD);
//        }
//    }

//    public void OnRxReceived(byte[] data)
//    {

//    }

//    private bool CanSendCommand(object parameter) => true;
//    private void OnSendCommand(object parameter)
//    {

//        if (parameter is tpLensOSDItem item)
//        {
//            TxCommand(item.Address, item.Data, true);
//        }
//    }

//    private bool CanCloseWindow(object parameter) => true;
//    private void OnCloseWindow(object parameter) => (parameter as Window)?.Close();
//}

//[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
//public class tpLensOSDItem
//{
//    public string Name { get; set; }
//    public byte Address { get; set; }
//    public ushort Data { get; set; }
//    public ushort PrevData { get; set; }
//    public ushort RangeMin { get; set; }
//    public ushort RangeMax { get; set; }
//}
