using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PCLHandler))]
public class PCLOctree : OSCControllable
{

    PCLHandler handler;
    public Collider roiCollider { get; private set; }

    PointOctree<PCLPointObject> po;

    public bool processLines;
    public bool drawLines;
    public bool debugBounds;
    public bool debugObjects;
    public bool gameCams;
    public bool sceneCam;

    public AnimationCurve alphaDecay;
    public Material lineMat;

    [Range(0.005f, 1)]
    [OSCProperty("maxDistance")]
    public float maxDistance;
    [OSCProperty("nearColor")]
    public Color nearColor;
    [OSCProperty("farColor")]
    public Color farColor;
    [OSCProperty("textureSpeed")]
    public float texSpeed = 0;

    PCLPointObject[] pointObjects;
    int numValidPoints;

    public void OnEnable()
    {
        // register the callback when enabling object
        Camera.onPostRender += postRender;
    }
    public void OnDisable()
    {
        // remove the callback when disabling object
        Camera.onPostRender -= postRender;
    }

    // Use this for initialization
    public override void Start () {
        base.Start();

        handler = GetComponent<PCLHandler>();
        roiCollider = transform.parent.FindChild("ROI").GetComponent<Collider>();
        
	}

    // Update is called once per frame
    public override void Update () {
        base.Update();

        po = new PointOctree<PCLPointObject>(.1f, roiCollider.transform.position, .05f);
        pointObjects = new PCLPointObject[handler.numGoodPoints];
        numValidPoints = 0;
        for (int i = 0; i < handler.numGoodPoints; i++)
        {
            Vector3 realPos = transform.TransformPoint(handler.points[i]);
            if (roiCollider.bounds.Contains(realPos))
            {
                pointObjects[numValidPoints] = new PCLPointObject(realPos, i);
                po.Add(pointObjects[numValidPoints], realPos);
                numValidPoints++;
            }
        }
    }

    void postRender(Camera cam)
    {
        bool camCheck = true;
        if (cam.cameraType == CameraType.Game && !gameCams) camCheck = false;
        if(cam.cameraType == CameraType.SceneView && !sceneCam) camCheck = false;
        if (!camCheck) return;

        if (po == null) return;
        if (pointObjects == null) return;
         
        if (po.Count == 0 ) return;
        if (!processLines) return;


        lineMat.SetPass(0);
        GL.PushMatrix();
        //GL.MultMatrix(Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale));
        GL.Begin(GL.LINES);

        for(int i=0;i<numValidPoints;i++)
        {
            Vector3 p = pointObjects[i].point;
            PCLPointObject[] neighbours = po.GetNearbyAndPositive(p, maxDistance);
            
            foreach (PCLPointObject ppo in neighbours)
            {
                Vector3 np = ppo.point;

                //if (Vector3.Distance(np, p) > maxDistance) continue;

                if (drawLines)
                {
                    float dist = Vector3.Distance(np, p);
                    float targetAlpha = alphaDecay.Evaluate(dist / maxDistance);


                    GL.Color(Color.Lerp(nearColor, farColor, targetAlpha));
                    
                    //GL.TexCoord(Vector3.zero);
                    GL.TexCoord(new Vector3(Time.time*texSpeed*targetAlpha, 0, 0));
                    GL.Vertex(np);

                    //GL.TexCoord(Vector3.one);
                    GL.TexCoord(new Vector3(Time.time*texSpeed*targetAlpha+1, 0, 0));
                    GL.Vertex(p);
                    
                }
            }
            
        }

        GL.End();
        GL.PopMatrix();

    }

    void OnDrawGizmos()
    {
        if (po == null) return;
        if (debugBounds) po.DrawAllBounds();
        if (debugObjects) po.DrawAllObjects();
    }
}
