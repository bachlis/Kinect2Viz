using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KOctreeK1 : MonoBehaviour {
    
    public bool debugPoints;
    public bool debugBounds;

    public bool processLines;
    public bool drawLines;

    
    public AnimationCurve alphaDecay;

    [Range(0.005f,2)]
    public float maxDistance;

    public Color nearColor;
    public Color farColor;

    public Material lineMat;

    public bool bodyOnly;

    public float texSpeed = 0;

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
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
       
    }

    void postRender(Camera cam)
    {
        if (cam != Camera.main) return;

        List<PCLPoint> tPoints = bodyOnly ? KinectPCLK1.instance.bodyPoints : KinectPCLK1.instance.roiPoints;
        PointOctree<PCLPoint> po = bodyOnly ? KinectPCLK1.instance.bodyTree : KinectPCLK1.instance.roiTree;

        if (po.Count == 0 || (bodyOnly && po.Count == KinectPCLK1.instance.numPoints)) return;

        if (!processLines) return;

        lineMat.SetPass(0);

        foreach (PCLPoint p in tPoints)
        {
            if (!p.isInROI) continue;

            Ray r = new Ray(p.position, Vector3.forward);
            PCLPoint[] np = po.GetNearby(r, maxDistance);
            
            foreach (PCLPoint npp in np)
            {
                if (npp.position.x < p.position.x || Vector3.Distance(npp.position,p.position) > maxDistance) continue;

                if (drawLines)
                {
                    float dist = Vector3.Distance(npp.position, p.position);
                    float targetAlpha = alphaDecay.Evaluate(dist / maxDistance);

                    GL.Begin(GL.LINES);
                    GL.Color(Color.Lerp(nearColor,farColor,targetAlpha));
                    
                    GL.TexCoord(Vector3.zero);
                    GL.TexCoord(new Vector3(Time.time*texSpeed*targetAlpha, 0, 0));
                    GL.Vertex(npp.position);

                    GL.TexCoord(Vector3.one);
                    GL.TexCoord(new Vector3(Time.time*texSpeed*targetAlpha+1, 0, 0));
                    GL.Vertex(p.position);
                    GL.End();
                }
            }
            
        }
        
       // GL.End();

    }

    void OnDrawGizmos()
    {
        if (KinectPCLK1.instance == null) return;

        PointOctree<PCLPoint> po = bodyOnly?KinectPCLK1.instance.bodyTree:KinectPCLK1.instance.roiTree;
        if (po == null) return;

        if (debugPoints) po.DrawAllObjects();
        if (debugBounds) po.DrawAllBounds();
    }
}
