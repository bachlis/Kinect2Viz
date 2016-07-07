using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
[RequireComponent(typeof(KinectManager))]
public class KinectPCLK1 : OSCControllable {

    const float CENTRALPOINT_X = 314.649173f;
    const float CENTRALPOINT_Y = 240.160459f;
    const float FOCAL_X = 572.882768f;
    const float FOCAL_Y = 542.739980f;
    const float FOCAL_X_INV = 1f / FOCAL_X;
    const float FOCAL_Y_INV = 1f / FOCAL_Y;

    public const int DEPTH_WIDTH = 640;
    public const int DEPTH_HEIGHT = 480;

    public static KinectPCLK1 instance;

    private KinectManager manager;

    public bool debug;
    public bool debugBodyOnly;

    public bool loadConfigOnStart;
    public bool saveConfigOnExit;


    [HideInInspector]
    public PCLPoint[] points;
    [HideInInspector]
    public PointOctree<PCLPoint> roiTree;
    [HideInInspector]
    public List<PCLPoint> roiPoints;

    [Range(0, 1)]
    [OSCProperty("bodyRandom")]
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


    [Range(1, 50)]
    [OSCProperty("downSample")]
    public int downSample = 1;
    private int lastDownSample = -1;


    [Range(0, 1)]
    [OSCProperty("randomX")]
    public float downSampleRandomXOffset;

    [Range(0, 1)]
    [OSCProperty("randomY")]
    public float downSampleRandomYOffset;

    float lastDSXOffset, lastDSYOffset;

    [Range(0,100)]
    [OSCProperty("randomAmplitude")]
    public float randomAnimationAmplitude;
    [Range(0,100)]
    [OSCProperty("randomSpeed")]
    public float randomAnimationSpeed;

    [HideInInspector]
    public Vector2[] dsOffsetRandoms;
    [HideInInspector]
    public Vector2[] dsOffsetRandomsAnimated;

    [OSCProperty("regenerate")]
    public bool regenerateRandom;

    [OSCProperty("minZ")]
    [Range(0, 10)]
    public float minDepth = 0;

    [OSCProperty("maxZ")]
    [Range(0, 10)]
    public float maxDepth = 10;

    [OSCProperty("minX")]
    [Range(-5, 5)]
    public float leftLimit = -5;

    [OSCProperty("maxX")]
    [Range(-5, 5)]
    public float rightLimit = 5;

    [OSCProperty("minY")]
    [Range(-5, 5)]
    public float bottomLimit = -5;

    [OSCProperty("maxY")]
    [Range(-5, 5)]
    public float topLimit = 5;

    /*
    [OSCProperty("position")]
    public Vector3 pos;

    [OSCProperty("rotation")]
    public Vector3 rot;//*/

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    public override void Start() {
        manager = GetComponent<KinectManager>();

        bodyCenter = Vector3.zero;
        bodyPoints = new List<PCLPoint>();
        points = new PCLPoint[0];
        roiPoints = new List<PCLPoint>();

        dsOffsetRandoms = new Vector2[0];
        dsOffsetRandomsAnimated = new Vector2[0];
        numPoints = 0;

        if (loadConfigOnStart) loadConfig();
    }

    // Update is called once per frame
    public override void Update()
    {
        //transform.localPosition = pos;
        //transform.localRotation = Quaternion.Euler(rot);

        if (Input.GetKeyDown(KeyCode.G)) regenerateRandom = !regenerateRandom;
        if (Input.GetKeyDown(KeyCode.H)) regenerateRandom = true;
        if (Input.GetKeyUp(KeyCode.H)) regenerateRandom = false;

        if(lastDSXOffset != downSampleRandomXOffset || lastDSYOffset != downSampleRandomYOffset || regenerateRandom)
        {
            generateOffsetRandoms();
            lastDSXOffset = downSampleRandomXOffset;
            lastDSYOffset = downSampleRandomYOffset;
        }
        

        animateOffsetRandoms();


        if (lastDownSample != downSample)
        {
            updateDownSample();
            lastDownSample = downSample;
            return;
        }


        ushort[] depthMap = manager.GetRawDepthMap();

        bodyTree = new PointOctree<PCLPoint>(10, bodyCenter, .01f); //take previous frame
        roiTree = new PointOctree<PCLPoint>(10, Vector3.zero, .01f); //take previous frame
        roiPoints.Clear();

        bodyPoints.Clear();
        bodyCenter = new Vector3();

        
        for (int ix = 0; ix < pointsWidth; ix++)
        {
            for (int iy = 0; iy < pointsHeight; iy++)
            {
                int dsIndex = iy * pointsWidth + ix;

                Vector2 drIndex = dsOffsetRandomsAnimated[dsIndex];
                int dIndexX = (int)(drIndex.x * DEPTH_WIDTH);
                int dIndexY = (int)(drIndex.y * DEPTH_HEIGHT);

                int index = dIndexY * DEPTH_WIDTH + dIndexX;

                //Debug.DrawRay(new Vector3(dsOffsetRandomsAnimated[dsIndex].x, dsOffsetRandomsAnimated[dsIndex].y) * 10, Vector3.forward * .5f,Color.red) ;

                if (index >= depthMap.Length) continue;

                ushort um = (ushort)(depthMap[index] & 7);


                if (um == 0) um = 255;
                else um = (byte)(um % 4);
                ushort d = (ushort)(depthMap[index] >> 3);

                bool isValid = d > 0;
                Vector3 p;

                if (isValid)
                {
                    float z_metric = d * 0.001f;

                    float tx = z_metric * ((dIndexX - CENTRALPOINT_X) * FOCAL_X_INV);
                    float ty = z_metric * ((dIndexY - CENTRALPOINT_Y) * FOCAL_Y_INV);

                    p = new Vector3(tx, -ty, z_metric);
                }
                else
                {
                    p = Vector3.zero;
                }

                bool isInROI = true;
                if (!isValid || p.z < minDepth || p.z > maxDepth
                    || p.x < leftLimit || p.x > rightLimit
                    || p.y < bottomLimit || p.y > topLimit) isInROI = false;


                Vector3 tPoint = transform.TransformPoint(p);

                int tIndex = iy * pointsWidth + ix;
                bodyMask[tIndex] = um != 255;

                bool isBody = bodyMask[tIndex];

                if (isBody)
                {
                    if (bodyRandomProba < 1)
                    {
                        isBody = Random.value <= bodyRandomProba;
                    }
                }


                PCLPoint pp = new PCLPoint(tPoint, isBody, isValid, isInROI,manager.usersMapColors[index]);
                points[tIndex] = pp;


                if (isValid)
                {
                    roiTree.Add(pp, tPoint);
                    roiPoints.Add(pp);
                    if (isBody)
                    {
                        bodyPoints.Add(pp);

                        bodyTree.Add(pp, tPoint);
                        bodyCenter += tPoint;
                    }
                }

                if (debug && isValid)
                {

                    if (isBody || !debugBodyOnly)
                    {

                        Color c = isInROI ? (isBody ? Color.yellow : Color.white) : Color.gray;
                        Debug.DrawRay(tPoint, Vector3.forward * .1f, c);
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
        if (saveConfigOnExit) saveConfig();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.TransformPoint((rightLimit + leftLimit) / 2, (bottomLimit + topLimit) / 2, (minDepth + maxDepth) / 2),
                            Vector3.Scale(new Vector3(rightLimit - leftLimit, topLimit - bottomLimit, maxDepth - minDepth), transform.localScale));
        //Gizmos.DrawWireCube(transform.position,
        //                    Vector3.Scale(new Vector3(rightLimit - leftLimit, topLimit - bottomLimit, maxDepth - minDepth), transform.localScale));
    }


    void OnGUI()
    {
        Event e = Event.current;

        if (e.type == EventType.keyDown)
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
        pointsWidth = Mathf.FloorToInt(DEPTH_WIDTH / downSample);
        pointsHeight = Mathf.FloorToInt(DEPTH_HEIGHT / downSample);
        numPoints = pointsWidth * pointsHeight;

        points = new PCLPoint[numPoints];
        bodyMask = new bool[numPoints];

        generateOffsetRandoms();
    }

    void generateOffsetRandoms()
    {
        dsOffsetRandoms = new Vector2[numPoints];
        dsOffsetRandomsAnimated = new Vector2[numPoints];

        for (int ix = 0; ix < pointsWidth; ix++)
        {
            for (int iy = 0; iy < pointsHeight; iy++)
            {
                int dsIndex = iy * pointsWidth + ix;

                if (dsIndex >= numPoints) return;

                float itx = ix * 1f / pointsWidth;// * k1Provider.DepthWidth;
                float ity = iy * 1f / pointsHeight;// * k1Provider.DepthHeight / pointsHeight;

                float downSampleRandomFactor = 1f / downSample;
                dsOffsetRandoms[dsIndex].x = Mathf.Clamp01(Random.Range(itx - downSampleRandomXOffset * downSampleRandomFactor, itx + downSampleRandomXOffset * downSampleRandomFactor));
                dsOffsetRandoms[dsIndex].y = Mathf.Clamp01(Random.Range(ity - downSampleRandomYOffset * downSampleRandomFactor, ity + downSampleRandomYOffset * downSampleRandomFactor));

                dsOffsetRandomsAnimated[dsIndex].x = dsOffsetRandoms[dsIndex].x;
                dsOffsetRandomsAnimated[dsIndex].y = dsOffsetRandoms[dsIndex].y;
            }
        }
    }

    void animateOffsetRandoms()
    {
        for (int i=0;i< numPoints;i++)
        { 
            float bound = 1f / downSample;
            Vector2 speed = Random.insideUnitCircle * randomAnimationAmplitude*.001f;
            Vector2 basePoint = dsOffsetRandoms[i];

            Vector2 targetPoint = new Vector2(Mathf.Clamp(dsOffsetRandomsAnimated[i].x + speed.x, basePoint.x - bound / 2, basePoint.x + bound / 2),
                                              Mathf.Clamp(dsOffsetRandomsAnimated[i].y + speed.y, basePoint.y - bound / 2, basePoint.y + bound / 2)
                                            );
            Vector2 targetSmoothed = Vector2.Lerp(dsOffsetRandomsAnimated[i], targetPoint, randomAnimationSpeed * Time.deltaTime);
            dsOffsetRandomsAnimated[i].x = Mathf.Clamp01(targetSmoothed.x);
            dsOffsetRandomsAnimated[i].y = Mathf.Clamp01(targetSmoothed.y);

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
