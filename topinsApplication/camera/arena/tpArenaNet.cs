using ArenaNET;
using System.Diagnostics.CodeAnalysis;

namespace topinsApplication.camera.lucid;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpArenaNet : IDisposable
{
    private readonly ISystem __system;
    private List<IDeviceInfo> __devices;
    private IDevice __device;
    private IImage __converted;

    private IImage __image;

    private string __acquisitionModeInitial;

    private bool __frameRateEnableInitial = false;

    private double __frameRateInitial = 0;
    private long __imageWidthInitial = 0;
    private long __imageHeightInitial = 0;

    private double __fps = tpCONST.FPS;

    private List<IImage> __iimages;

    public tpArenaNet() => __system = ArenaNET.Arena.OpenSystem();
    public void Dispose() => ArenaNET.Arena.CloseSystem(__system);

    public List<IDeviceInfo> Devices => __devices;
    public double FPS => __fps;
    public List<IImage> IImages
    {
        get => __iimages;
        set
        {
            if (value is null)
            {
                for (int i = 0; i < __iimages.Count; i++)
                {
                    if (__iimages[i] is not null)
                    {
                        ImageFactory.Destroy(__iimages[i]);         // 클론이면??
                    }
                    __iimages.RemoveAt(0);
                }
                __iimages = null;

                return;
            }
            __iimages = value;
        }
    }
    public bool Connected => __device != null;

    public List<IDeviceInfo> RefreshDevices()
    {
        __system.UpdateDevices(100);

        return __devices = __system.Devices;
    }

    public bool Connect(IDeviceInfo device)
    {
        try
        {
            __device = __system.CreateDevice(device);

            uint width = 1920;
            uint height = 1080;

            __acquisitionModeInitial = (__device.NodeMap.GetNode("AcquisitionMode") as IEnumeration).Entry.Symbolic;
            __frameRateEnableInitial = (__device.NodeMap.GetNode("AcquisitionFrameRateEnable") as IBoolean).Value;

            if (__frameRateEnableInitial)
            {
                __frameRateInitial = (__device.NodeMap.GetNode("AcquisitionFrameRate") as IFloat).Value;
            }
            __imageWidthInitial = (__device.NodeMap.GetNode("Width") as IInteger).Value;
            __imageHeightInitial = (__device.NodeMap.GetNode("Height") as IInteger).Value;

            (__device.NodeMap.GetNode("AcquisitionMode") as IEnumeration).FromString("Continuous");

            SetIntValue(__device.NodeMap, "Width", width);
            SetIntValue(__device.NodeMap, "Height", height);

            (__device.NodeMap.GetNode("AcquisitionFrameRateEnable") as IBoolean).Value = true;

            __fps = SetFloatValue(__device.NodeMap, "AcquisitionFrameRate", __fps);

#if DEBUG && !FORDEBUG
            System.Diagnostics.Debug.WriteLine($"Information: \nwidth: {(__device.NodeMap.GetNode("Width") as IInteger).Value}\nheight: {(__device.NodeMap.GetNode("Height") as IInteger).Value}\nfps: {__fps}\n");
#endif
            var streamAutoNegotiatePacketSizeNode = (IBoolean)__device.TLStreamNodeMap.GetNode("StreamAutoNegotiatePacketSize");

            streamAutoNegotiatePacketSizeNode.Value = true;

            var streamPacketResendEnableNode = (IBoolean)__device.TLStreamNodeMap.GetNode("StreamPacketResendEnable");

            streamPacketResendEnableNode.Value = true;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
        return __device != null;
    }

    public void Disconnect()
    {
        RestoreValues();

        __system.DestroyDevice(__device);
        __device = null;
    }

    private void RestoreValues()
    {
        SetIntValue(__device.NodeMap, "Width", __imageWidthInitial);
        SetIntValue(__device.NodeMap, "Height", __imageHeightInitial);

        (__device.NodeMap.GetNode("AcquisitionMode") as IEnumeration).FromString(__acquisitionModeInitial);
        (__device.NodeMap.GetNode("AcquisitionFrameRateEnable") as IBoolean).Value = __frameRateEnableInitial;

        if (__frameRateEnableInitial)
        {
            SetFloatValue(__device.NodeMap, "AcquisitionFrameRate", __frameRateInitial);
        }
        __frameRateEnableInitial = false;
    }

    public string ConnectedDeviceUId() => ((IString)__device.NodeMap.GetNode("DeviceSerialNumber")).Value;

    //public List<string> GetDeviceUIds()
    //{
    //    UpdateDevices();

    //    List<string> uids = [];

    //    for (int i = 0; i < __devices.Count; i++)
    //    {
    //        uids.Add(__devices[i].SerialNumber);
    //    }
    //    return uids;
    //}

    //public string GetDeviceName(string UId, string node)
    //{
    //    for (int i = 0; i < __devices.Count; i++)
    //    {
    //        if (__devices[i].SerialNumber == UId)
    //        {
    //            switch (node)
    //            {
    //                case tpCONST.DEVICEUSERID:
    //                case tpCONST.USERDEFINEDNAME:
    //                    return __devices[i].UserDefinedName;
    //                case tpCONST.DEVICEMODELNAME:
    //                case tpCONST.MODELNAME:
    //                    return __devices[i].ModelName;
    //            }
    //        }
    //    }
    //    return tpCONST.INVALIDDEVICENAME;
    //}

    //public bool IsNetworkValid(string UId)
    //{
    //    UpdateDevices();

    //    for (int i = 0; i < __devices.Count; i++)
    //    {
    //        if (__devices[i].SerialNumber == UId)
    //        {
    //            uint ip = (uint)__devices[i].IpAddress;
    //            uint subnet = (uint)__devices[i].SubnetMask;

    //            IInteger ifipNode = (IInteger)__system.GetTLInterfaceNodeMap(__devices[i]).GetNode("GevInterfaceSubnetIPAddress");
    //            IInteger ifsubnetNode = (IInteger)__system.GetTLInterfaceNodeMap(__devices[i]).GetNode("GevInterfaceSubnetMask");

    //            uint ifip = (uint)ifipNode.Value;
    //            uint ifsubnet = (uint)ifsubnetNode.Value;

    //            if (subnet != ifsubnet) return false;
    //            if ((ip & subnet) != (ifip & ifsubnet)) return false;

    //            return true;
    //        }
    //    }
    //    throw new Exception();
    //}

    //public System.Drawing.Bitmap GetImage(uint timeout = 2000)
    //{
    //    if (__converted is not null)
    //    {
    //        ImageFactory.Destroy(__converted);

    //        __converted = null;
    //    }
    //    IImage image = __device.GetImage(timeout);

    //    __converted = ImageFactory.Convert(image, (EPfncFormat)0x02200017);
    //    __device.RequeueBuffer(image);

    //    return __converted.Bitmap;
    //}

    //public IImage GetImage(uint timeout = 2000)
    //{
    //    if (__converted is not null)
    //    {
    //        ImageFactory.Destroy(__converted);

    //        __converted = null;
    //    }
    //    IImage image = __device.GetImage(timeout);

    //    if (false)
    //    {

    //    }
    //    __converted = ImageFactory.Convert(image, (EPfncFormat)0x02200017);
    //    __device.RequeueBuffer(image);

    //    return __converted;
    //}

    public IImage GetImage(bool recording, uint timeout = 2000)
    {
        if (__converted is not null)
        {
            ImageFactory.Destroy(__converted);

            __converted = null;
        }
        IImage image = __device.GetImage(timeout);


        if (recording)
        {
            __iimages.Add(new tpIImage(ImageFactory.Convert(image, EPfncFormat.BGR8)));
        }
        __converted = ImageFactory.Convert(image, (EPfncFormat)0x02200017);
        __device.RequeueBuffer(image);

        return __converted;
    }

    //public IImage GetIImage(uint timeout = 2000)
    //{
    //    if (__converted != null)
    //    {
    //        ImageFactory.Destroy(__converted);

    //        __converted = null;
    //    }
    //    if (__image != null)
    //    {
    //        __device.RequeueBuffer(__image);

    //        __image = null;
    //    }
    //    return __image = __device.GetImage(timeout);
    //}

    public bool StartStream()
    {
        if (__device is null) return false;

        if (!(__device.TLStreamNodeMap.GetNode("StreamIsGrabbing") as IBoolean).Value)
        {
            (__device.TLStreamNodeMap.GetNode("StreamBufferHandlingMode") as IEnumeration).Symbolic = "NewestOnly";

            __device.StartStream();

            return true;
        }
        return false;
    }

    public void StopStream()
    {
        if (__device is null) return;

        if ((__device.TLStreamNodeMap.GetNode("StreamIsGrabbing") as IBoolean).Value)
        {
            __device.StopStream();
        }
    }

    private void UpdateDevices()
    {
        __system.UpdateDevices(100);

        __devices = __system.Devices;
    }

    private long SetIntValue(INodeMap nodeMap, string nodeName, long value)
    {
        IInteger integer = (IInteger)nodeMap.GetNode(nodeName);

        value = ((value - integer.Min) / integer.Inc * integer.Inc) + integer.Min;

        if (value < integer.Min) value = integer.Min;
        if (value > integer.Max) value = integer.Max;

        integer.Value = value;

        return value;
    }

    private double SetFloatValue(INodeMap nodeMap, string nodeName, double value)
    {
        IFloat floatNode = (IFloat)nodeMap.GetNode(nodeName);

        if (value < floatNode.Min) value = floatNode.Min;
        if (value > floatNode.Max) value = floatNode.Max;

        floatNode.Value = value;

        return value;
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpIImage(IImage image) : IImage
{
    private readonly System.Drawing.Bitmap __bitmap = image.Bitmap is not null ? CloneBMP(image.Bitmap) : null;
    private readonly byte[] __dataArray = (byte[])image.DataArray.Clone();

    public System.Drawing.Bitmap Bitmap => __bitmap;

    public ulong TimestampNs { get; } = image.TimestampNs;
    public ulong Timestamp { get; } = image.Timestamp;
    public EPixelEndianness PixelEndianness { get; } = image.PixelEndianness;
    public uint BitsPerPixel { get; } = image.BitsPerPixel;
    public EPfncFormat PixelFormat { get; } = image.PixelFormat;
    public uint PaddingY { get; } = image.PaddingY;
    public uint PaddingX { get; } = image.PaddingX;
    public uint OffsetY { get; } = image.OffsetY;
    public uint OffsetX { get; } = image.OffsetX;
    public uint Height { get; } = image.Height;
    public uint Width { get; } = image.Width;
    public bool DataIsLargerThanBuffer => image.DataIsLargerThanBuffer;
    public bool IsIncomplete => image.IsIncomplete;
    public bool IsCompressedImage { get; } = image.IsCompressedImage;
    public bool HasChunkData { get; } = image.HasChunkData;
    public bool HasImageData { get; } = image.HasImageData;
    public EBufferPayloadType PayloadType { get; } = image.PayloadType;
    public ulong FrameId { get; } = image.FrameId;
    public uint SizeOfBuffer { get; } = image.SizeOfBuffer;
    public uint PayloadSize { get; } = image.PayloadSize;
    public uint SizeFilled { get; } = image.SizeFilled;
    public byte[] DataArray => __dataArray;
    public nint NativePtr => image.NativePtr;

    // Implement methods
    public IChunkData AsChunkData() => throw new NotImplementedException();
    public ICompressedImage AsCompressedImage() => throw new NotImplementedException();
    public IImage AsImage() => throw new NotImplementedException();
    public bool VerifyCRC() => throw new NotImplementedException();

    public static System.Drawing.Bitmap CloneBMP(System.Drawing.Bitmap source)
    {
        System.Drawing.Bitmap clone = new(source.Width, source.Height, source.PixelFormat);

        using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(clone))
        {
            graphic.DrawImage(source, new System.Drawing.Rectangle(0, 0, source.Width, source.Height));
        }
        return clone;
    }
    public static void SaveBMP(System.Drawing.Bitmap source, string fileName)
    {
        if (source is null) return;
        try
        {
            source.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to save image: {e.Message}");
        }
    }
}