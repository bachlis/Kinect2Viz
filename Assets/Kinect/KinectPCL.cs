using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;


[System.Serializable]
[RequireComponent(typeof(MultiSourceManager))]
public class KinectPCL : MonoBehaviour {

    public static KinectPCL instance;
    private MultiSourceManager multiSourceManager;


    public bool debug;
    public bool debugBodyOnly;

    public bool loadConfigOnStart;
    public bool saveConfigOnExit;
    

    [HideInInspector]
    public PCLPoint[] points;


    [Range(0,1)]
    public float bodyRandomProba;

    [HideInInspector]
    public bool[] bodyMask;

    [HideInInspector]
    public PointOctree<PCLPoint> bodyTree;
    [HideInInspector]
    public List<PCLPoint> bodyPoints;

    public Vector3 bodyCenter;


    [HideInInspector]
    public int numPoints;
    [HideInInspector]
    public int pointsWidth;
    [HideInInspector]
    public int pointsHeight;


    [Range(1,50)]
    public int downSample = 1;
    private int lastDownSample = -1;

    
    [Range(0,1)]
    public float downSampleRandomXOffset;
    [Range(0, 1)]
    public float downSampleRandomYOffset;

    [HideInInspector]
    public Vector2[] dsOffsetRandoms;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start () {
        multiSourceManager = GetComponent<MultiSourceManager>();
        bodyCenter = Vector3.zero;
        bodyPoints = new List<PCLPoint>();
        points = new PCLPoint[0];
        dsOffsetRandoms = new Vector2[0];
        numPoints = 0;

        if (loadConfigOnStart) loadConfig();
    }

    // Update is called once per frame
    void Update()
    {
        //TMP
        if (Input.GetKey(KeyCode.G)) generateOffsetRandoms();

        if (lastDownSample != downSample)
        {
            updateDownSample();
            lastDownSample = downSample;
            return;
        }

        CameraSpacePoint[] realWorldMap = multiSourceManager.GetRealWorldData();
        byte[] bodyIndexMap = multiSourceManager.GetBodyIndexData();
        //Texture2D tex = multiSourceManager.GetColorTexture();


        bodyTree = new PointOctree<PCLPoint>(10,bodyCenter,.01f); //take previous frame
        bodyPoints.Clear();
        bodyCenter = new Vector3();



        for(int ix = 0; ix < pointsWidth; ix++)
        {
            for (int iy = 0; iy < pointsHeight; iy++)
            {
                int dsIndex = iy * pointsWidth + ix;

                Vector2 rIndex = dsOffsetRandoms[dsIndex];

                int index = Mathf.RoundToInt(rIndex.x *multiSourceManager.DepthHeight * multiSourceManager.DepthWidth) + Mathf.RoundToInt(rIndex.y * multiSourceManager.DepthWidth);

                //Vector3 dv = new Vector3(rIndex.x * 10, rIndex.y * 10);

                //Debug.DrawLine(dv, dv + Vector3.forward * .2f, Color.red);
                

                if (index >= realWorldMap.Length) continue;

                
                CameraSpacePoint csp = realWorldMap[index];
                Vector3 p = new Vector3(csp.X, csp.Y, csp.Z);

                Vector3 tPoint = transform.TransformPoint(p);

                int tIndex = iy * pointsWidth + ix;
                bodyMask[tIndex] = bodyIndexMap[index] != 255;

                bool isBody = bodyMask[tIndex];
                bool isValid = true;

                if(isBody)
                {
                    if (bodyRandomProba < 1)
                    {
                        isBody = Random.value <= bodyRandomProba;
                    }
                }

                if (float.IsNaN(tPoint.x) || float.IsInfinity(tPoint.x))
                {
                    isValid = false;
                    tPoint = Vector3.zero;
                }

                PCLPoint pp = new PCLPoint(tPoint, isBody, isValid,true, isBody ? Color.yellow : Color.white);
                points[tIndex] = pp;

                
                if(isBody && isValid) 
                {
                    bodyPoints.Add(pp);

                    bodyTree.Add(pp, tPoint);
                    bodyCenter += tPoint;
                }

                if (debug && isValid)
                {
                   
                    if (isBody || !debugBodyOnly)
                    {
                        Color c = isBody ? Color.yellow : Color.white;
                        Debug.DrawLine(tPoint, tPoint + Vector3.forward * .05f, c);
                    }
                }

            }
        }


        if (bodyPoints.Count > 0)
        {
            bodyCenter /= bodyPoints.Count;
        }
    }
    

    void OnDestroy()
    {
        if(saveConfigOnExit) saveConfig();
    }

    void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(bodyCenter,Vector3.one*2);
    }


    void OnGUI()
    {
        Event e = Event.current;

        if(e.type == EventType.keyDown)
        {
            /*
            switch(e.keyCode)
            {
               //
            }
            */
        }
    }
    
    void updateDownSample()
    {
        pointsWidth = Mathf.FloorToInt(multiSourceManager.DepthWidth / downSample);
        pointsHeight = Mathf.FloorToInt(multiSourceManager.DepthHeight / downSample);
        numPoints = pointsWidth * pointsHeight;

        points = new PCLPoint[numPoints];
        bodyMask = new bool[numPoints];
        
        generateOffsetRandoms();
    }

    void generateOffsetRandoms()
    {
        dsOffsetRandoms = new Vector2[numPoints];

        for (int ix = 0; ix < pointsWidth; ix++)
        {
            for (int iy = 0; iy < pointsHeight; iy++)
            {
                int dsIndex = iy * pointsWidth + ix;

                float itx = ix * 1f / pointsWidth;// * multiSourceManager.DepthWidth;
                float ity = iy * 1f / pointsHeight;// * multiSourceManager.DepthHeight / pointsHeight;

                float downSampleRandomFactor = 1f / downSample;
                dsOffsetRandoms[dsIndex].x = Mathf.Clamp01(Random.Range(itx - downSampleRandomXOffset * downSampleRandomFactor, itx + downSampleRandomXOffset * downSampleRandomFactor));
                dsOffsetRandoms[dsIndex].y = Mathf.Clamp01(Random.Range(ity - downSampleRandomYOffset * downSampleRandomFactor, ity + downSampleRandomYOffset * downSampleRandomFactor));
            }
        }
    }

    public void saveConfig()
    {
        /*
        SaveContext saveContext = SaveContext.ToFile("kinect");
        saveContext.Save<bool>(mirror, "mirror");
        saveContext.Save<int>(downSample, "downSample");
        saveContext.Save<Vector3>(transform.position, "position");
        saveContext.Save<Quaternion>(transform.rotation, "rotation");
        saveContext.Flush();
        */
    }

    public void loadConfig()
    {
        /*
        LoadContext loadContext = LoadContext.FromFile("kinect");
        mirror = loadContext.Load<bool>("mirror");
        downSample = loadContext.Load<int>("downSample");
        transform.position = loadContext.Load<Vector3>("position");
        transform.rotation = loadContext.Load<Quaternion>("rotation");
        */
    }
}
