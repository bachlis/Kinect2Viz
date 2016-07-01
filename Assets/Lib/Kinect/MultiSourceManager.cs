using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MultiSourceManager : MonoBehaviour {


    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }

    public int DepthWidth { get; private set; }
    public int DepthHeight { get; private set; }


    public bool useColor = true;
    public bool useDepth = true;
    public bool useBodyIndex = true;

    private KinectSensor _Sensor;
    private MultiSourceFrameReader _Reader;
    private Texture2D _ColorTexture;

    private ushort[] _DepthData;
    private CameraSpacePoint[] _RealWorldPoints;

    private byte[] _ColorData;
    private byte[] _BodyIndexData;

    private CoordinateMapper mapper;
    
    public Texture2D GetColorTexture()
    {
        return _ColorTexture;
    }
    
    public ushort[] GetDepthData()
    {
        return _DepthData;
    }

    public CameraSpacePoint[] GetRealWorldData()
    {
        return _RealWorldPoints;
    }

    public byte[] GetBodyIndexData()
    {
        return _BodyIndexData;
    }

    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null) 
        {
            
            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);
            
            var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;
            
            _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];
            
            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
            _DepthData = new ushort[depthFrameDesc.LengthInPixels];
            DepthWidth = depthFrameDesc.Width;
            DepthHeight = depthFrameDesc.Height;

            var bodyIndexFrameDesc = _Sensor.BodyIndexFrameSource.FrameDescription;
            _BodyIndexData = new byte[bodyIndexFrameDesc.LengthInPixels];

            mapper = _Sensor.CoordinateMapper;
            _RealWorldPoints = new CameraSpacePoint[depthFrameDesc.LengthInPixels];

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }
    
    void Update () 
    {
        if (_Reader != null) 
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (useColor)
                {
                    var colorFrame = frame.ColorFrameReference.AcquireFrame();
                    if (colorFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);
                        _ColorTexture.LoadRawTextureData(_ColorData);
                        _ColorTexture.Apply();

                        colorFrame.Dispose();
                        colorFrame = null;
                    }
                }

                if(useDepth)
                {
                    var depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null)
                    {
                        depthFrame.CopyFrameDataToArray(_DepthData);

                        depthFrame.Dispose();
                        depthFrame = null;

                        mapper.MapDepthFrameToCameraSpace(_DepthData, _RealWorldPoints);

                    }

                }

                if (useBodyIndex)
                {
                    var bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame();
                    if (bodyIndexFrame != null)
                    {
                        bodyIndexFrame.CopyFrameDataToArray(_BodyIndexData);

                        bodyIndexFrame.Dispose();
                        bodyIndexFrame = null;
                    }
                }

                frame = null;
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}
