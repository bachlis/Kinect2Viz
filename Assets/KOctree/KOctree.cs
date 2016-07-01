using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KOctree : MonoBehaviour {
    
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

        List<PCLPoint> bodyPoints = KinectPCL.instance.bodyPoints;
        PointOctree<PCLPoint> po = KinectPCL.instance.bodyTree;

        if (po.Count == 0 || po.Count == KinectPCL.instance.numPoints) return;

        if (!processLines) return;

        GL.Begin(GL.LINES);
        lineMat.SetPass(0);

        foreach (PCLPoint p in bodyPoints)
        {
            Ray r = new Ray(p.position, Vector3.forward);
            PCLPoint[] np = po.GetNearby(r, maxDistance);
            
            foreach (PCLPoint npp in np)
            {
                if (npp.position.x < p.position.x || Vector3.Distance(npp.position,p.position) > maxDistance) continue;

                if (drawLines)
                {
                    float dist = Vector3.Distance(npp.position, p.position);
                    float targetAlpha = alphaDecay.Evaluate(dist / maxDistance);
                    
                    GL.Color(Color.Lerp(nearColor,farColor,targetAlpha));
                    GL.TexCoord(new Vector3(Time.time*targetAlpha, 0, 0));
                    GL.Vertex(npp.position);
                    GL.TexCoord(new Vector3(Time.time* targetAlpha+1, 0, 0));
                    GL.Vertex(p.position);
                }
            }
            
        }
        
        GL.End();

    }

    void OnDrawGizmos()
    {
        if (KinectPCL.instance == null) return;

        PointOctree<PCLPoint> po = KinectPCL.instance.bodyTree;
        if (po == null) return;

        if (debugPoints) po.DrawAllObjects();
        if (debugBounds) po.DrawAllBounds();
    }
}
