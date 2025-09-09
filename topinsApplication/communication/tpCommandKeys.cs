using System.Diagnostics.CodeAnalysis;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpCommandKeys
{
    public const string STOP = "Stop";
    public const string MOVE = "Move";

    public const string ZOOM = "Zoom";
    public const string FOCUS = "Focus";
    public const string IRIS = "Iris";

    public const string SAVE = "Save";
    public const string CALL = "Call";
    public const string CLEAR = "Clear";

    public const string STARTZOOMTELE = "StartZoomTele";
    public const string STOPZOOMTELE = "StopZoomTele";
    public const string STARTZOOMWIDE = "StartZoomWide";
    public const string STOPZOOMWIDE = "StopZoomWide";

    public const string FOCUSMANUAL = "FocusManual";
    public const string FOCUSAUTO = "FocusAuto";
    public const string FOCUSFULL = "FocusFull";
    public const string STARTFOCUSNEAR = "StartFocusNear";
    public const string STOPFOCUSNEAR = "StopFocusNear";
    public const string STARTFOCUSFAR = "StartFocusFar";
    public const string STOPFOCUSFAR = "StopFocusFar";

    public const string IRISON = "IrisOn";
    public const string IRISMANUAL = "IrisManual";
    public const string STARTIRISOPEN = "StartIrisOpen";
    public const string STOPIRISOPEN = "StopIrisOpen";
    public const string STARTIRISCLOSE = "StartIrisClose";
    public const string STOPIRISCLOSE = "StopIrisClose";

    public const string PRESETSAVE = "PresetSave";
    public const string PRESETCALL = "PresetCall";
    public const string PRESETCLEAR = "PresetClear";
    public const string PRESETSETPRESET = "PresetSetPreset";
    public const string PRESETGOTOPRESET = "PresetGotoPreset";
    public const string PRESETCLEARPRESET = "PresetClearPreset";

    public const string FOCUSPOSITIONREAD = "FocusPositionRead";
    public const string FOCUSPOSITIONMOVE = "FocusPositionMove";
    public const string FOCUSPOSITIONSTOP = "FocusPositionStop";

    //public const string QUARYZOOMPOSITIONREAD = "QuaryZoomPositionRead";
    //public const string QUARYZOOMPOSITIONMOVE = "QuaryZoomPositionMove";

    public const string ZOOMPOSITIONREAD = "ZoomPositionRead";
    public const string ZOOMPOSITIONMOVE = "ZoomPositionMove";
    public const string ZOOMPOSITIONStop = "ZoomPositionStop";

    public const string IRISPOSITIONREAD = "IrisPositionRead";
    public const string IRISPOSITIONMOVE = "IrisPositionMove";
    public const string IRISPOSITIONSTOP = "IrisPositionStop";

    public const string STARTIRFILTERIN = "IRFilterIn";
    public const string STOPIRFILTERIN = "StopIRFilterIn";
    public const string STARTIRFILTEROUT = "StartIRFilterOut";
    public const string STOPIRFILTEROUT = "StopIRFilterOut";

    public const string VERSION = "Version";

    public const string OSDSETTINGSAVE = "OSDSettingSave";
    public const string OSDSETTINGLOAD = "OSDSettingLoad";
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpOSDKeys
{
    public const string MODEL = "MODEL";
    public const string BAUDRATE = "BAUDRATE";
    public const string AMPGAIN = "AMPGAIN";
    public const string ZOOMAF = "ZOOM AF";
    public const string ZOOMAFDELAY = "ZOOMAF DELAY";
    public const string PTAF = "PT AF";
    public const string PTAFDELAY = "PTAF DELAY";
    public const string PTID = "PT ID";
    public const string AFAREASIZE = "AFAREASIZE";
    public const string AFAREAFRAME = "AFAREAFRAME";
    public const string AFSEARCH = "AFSEARCH";
    public const string ZOOMPOSINV = "ZOOM POS INV";
    public const string FOCUSPOSINV = "FOCUS POS INV";
    public const string IRISPOSINV = "IRIS POS INV";
    public const string AFTIMEOUT = "AF TIMEOUT";
    public const string CMDQINGAF = "CMDQING AF";
    public const string CMDQINGPRST = "CMDQING PRST";
    public const string PWMFREQ = "PWM FREQ";
    public const string ZOOMLSPOS = "ZOOM LS POS";
    public const string USERAFSPD = "USER AF SPD";
    public const string USERAF = "USERAF";
    public const string USERAFSTSPD = "USER AF ST SPD";
    public const string USERAFSTPOS1 = "USER AF ST POS1";
    public const string USERMFOCUSL = "USER M FOCUS L";
    public const string USERMFOCUSM = "USER M FOCUS M";
    public const string USERMFOCUSH = "USER M FOCUS H";
    public const string GRESPONSE = "G.RESPONSE";

    public const string COMMANDKEY_MODEL = "MODEL";
    public const string COMMANDKEY_BAUDRATE = "BAUDRATE";
    public const string COMMANDKEY_AMPGAIN = "AMPGAIN";
    public const string COMMANDKEY_ZOOMAF = "ZOOMAF";
    public const string COMMANDKEY_ZOOMAFDELAY = "ZOOMAFDELAY";
    public const string COMMANDKEY_PTAF = "PTAF";
    public const string COMMANDKEY_PTAFDELAY = "PTAFDELAY";
    public const string COMMANDKEY_PTID = "PTID";
    public const string COMMANDKEY_AFAREASIZE = "AFAREASIZE";
    public const string COMMANDKEY_AFAREAFRAME = "AFAREAFRAME";
    public const string COMMANDKEY_AFSEARCH = "AFSEARCH";
    public const string COMMANDKEY_ZOOMPOSINV = "ZOOMPOSINV";
    public const string COMMANDKEY_FOCUSPOSINV = "FOCUSPOSINV";
    public const string COMMANDKEY_IRISPOSINV = "IRISPOSINV";
    public const string COMMANDKEY_AFTIMEOUT = "AFTIMEOUT";
    public const string COMMANDKEY_CMDQINGAF = "CMDQINGAF";
    public const string COMMANDKEY_CMDQINGPRST = "CMDQINGPRST";
    public const string COMMANDKEY_PWMFREQ = "PWMFREQ";
    public const string COMMANDKEY_ZOOMLSPOS = "ZOOMLSPOS";
    public const string COMMANDKEY_USERAFSPD = "USERAFSPD";
    public const string COMMANDKEY_USERAF = "USERAF";
    public const string COMMANDKEY_USERAFSTSPD = "USERAFSTSPD";
    public const string COMMANDKEY_USERAFSTPOS1 = "USERAFSTPOS1";
    public const string COMMANDKEY_USERMFOCUSL = "USERMFOCUSL";
    public const string COMMANDKEY_USERMFOCUSM = "USERMFOCUSM";
    public const string COMMANDKEY_USERMFOCUSH = "USERMFOCUSH";
    public const string COMMANDKEY_GRESPONSE = "GRESPONSE";

    //public const string MODEL_0X01 = "MODEL_0X01";
    //public const string BAUDRATE_0X05 = "BAUDRATE_0X05";
    //public const string AMPGAIN_0X07 = "AMPGAIN_0X07";
    //public const string ZOOMAF_0X09 = "ZOOMAF_0X09";
    //public const string ZOOMAFDELAY_0X0B = "ZOOMAFDELAY_0X0B";
    //public const string PTAF_0X0D = "PTAF_0X0D";
    //public const string PTAFDELAY_0X0F = "PTAFDELAY_0X0F";
    //public const string PTID_0X15 = "PTID_0X15";
    //public const string AFAREASIZE_0X19 = "AFAREASIZE_0X19";
    //public const string AFAREAFRAME_0X1B = "AFAREAFRAME_0X1B";
    //public const string AFSEARCH_0X1D = "AFSEARCH_0X1D";
    //public const string ZOOMPOSINV_0X25 = "ZOOMPOSINV_0X25";
    //public const string FOCUSPOSINV_0X27 = "FOCUSPOSINV_0X27";
    //public const string IRISPOSINV_0X29 = "IRISPOSINV_0X29";
    //public const string AFTIMEOUT_0X33 = "AFTIMEOUT_0X33";
    //public const string CMDQINGAF_0X35 = "CMDQINGAF_0X35";
    //public const string CMDQINGPRST_0X37 = "CMDQINGPRST_0X37";
    //public const string PWMFREQ_0X39 = "PWMFREQ_0X39";
    //public const string ZOOMLSPOS_0X3B = "ZOOMLSPOS_0X3B";
    //public const string USERAFSPD_0X43 = "USERAFSPD_0X43";
    //public const string USERAFRANGE_0X45 = "USERAFRANGE_0X45";
    //public const string USERAFSTSPD_0X47 = "USERAFSTSPD_0X47";
    //public const string USERAFSTPOS1_0X49 = "USERAFSTPOS1_0X49";
    //public const string USERMFOCUSL_0X4D = "USERMFOCUSL_0X4D";
    //public const string USERMFOCUSM_0X4F = "USERMFOCUSM_0X4F";
    //public const string USERMFOCUSH_0X51 = "USERMFOCUSH_0X51";
    //public const string GRESPONSE_0X61 = "G.RESPONSE_0X61";
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public enum eINPUTTYPE
{
    NONE = 0,
    COMBOBOX,
    TEXTBOX
}
