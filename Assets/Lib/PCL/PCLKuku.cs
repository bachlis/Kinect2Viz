using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CielaSpike;

[RequireComponent(typeof(PCLHandler))]
public class PCLKuku : OSCControllable
{

    PCLHandler handler;
    public BoxCollider roiCollider;

    [OSCProperty("active")]
    public bool processLines;

    [Header("Debug")]
    public bool drawLines;
    public bool debugBounds;
    public bool debugObjects;

    [Header("Cameras")]
    public bool gameCams;
    public bool sceneCam;

    [Header("Rendering")]
    public AnimationCurve alphaDecay;
    public Material lineMat;

    [Range(0.005f, 1)]
    [OSCProperty("maxDistance")]
    public float maxDistance;
    [OSCProperty("nearColor")]
    public Color nearColor;
    [OSCProperty("farColor")]
    public Color farColor;

    [Header("Texture")]
    [Range(0, 5)]
    [OSCProperty("textureOffset")]
    public float texOffset = 0;
    [OSCProperty("textureSpeed")]
    public float texSpeed = 0;
    [Range(-2, 2)]
    [OSCProperty("textureSpeedDiff")]
    public float texSpeedDiff = 0;
    [Range(0, 5)]
    [OSCProperty("textureTile")]
    public float textureTile = 0;

    [Header("ROI")]
    Vector3[] points;
    public int numROIPoints;
    List<KukuLink> links;

    Vector3[] randomVectors;

    [Header("Randomize")]
    [Range(0, 1f)]
    [OSCProperty("randomFactor")]
    public float randomFactor;
    [OSCProperty("shuffleRandom")]
    public bool shuffleRandom;

    [Range(0, 2)]
    public int sortComponent;


    //Multithread
    Task linesTask;
    bool linesReady;

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
    public override void Start() {
        base.Start();

        links = new List<KukuLink>();
        handler = GetComponent<PCLHandler>();
        randomVectors = new Vector3[handler.numTotalPoints];

        linesReady = true;
    }

    // Update is called once per frame
    public override void Update() {
        base.Update();

        if (!processLines) return;
        if (linesReady)
        {
            Debug.Log("Lines ready !");
            this.StartCoroutineAsync(processLinesAsync(), out linesTask);
        }
        else
        {
            //Debug.Log("skip update, wait for task to finish");
        }

    }

    IEnumerator processLinesAsync()
    {
        linesReady = false;

        
        List<Vector3> roiPoints = new List<Vector3>();

        if (randomVectors.Length == 0)
        {
            randomVectors = new Vector3[handler.numTotalPoints];
            Debug.Log("Here");

            yield return Ninja.JumpToUnity; //=> here's our ninja, following code runs on main thread.
            for (int i = 0; i < handler.numTotalPoints; i++)
            {
                randomVectors[i] = UnityEngine.Random.insideUnitSphere;

            }
            yield return Ninja.JumpBack;
            Debug.Log("Finish random");
        }

        if (shuffleRandom)
        {
            yield return Ninja.JumpToUnity;
            for (int i = 0; i < handler.numGoodPoints; i++)
            {
                int pIndex = handler.goodPointIndices[i];
                randomVectors[pIndex] = UnityEngine.Random.insideUnitSphere;
            }
            yield return Ninja.JumpBack;
        }

        yield return Ninja.JumpToUnity; //=> here's our ninja, following code runs on main thread.
        for (int i = 0; i < handler.numGoodPoints; i++)
        {

            int pIndex = handler.goodPointIndices[i];

            Vector3 realPos = transform.TransformPoint(handler.points[pIndex]);
           
            realPos += randomVectors[pIndex] * randomFactor;

            if (handler.numBodiesTracked == 0)
            {
                if (PointInOABB(realPos, roiCollider)) roiPoints.Add(realPos);
            }
            else
            {
                roiPoints.Add(realPos);
            }
        }
        yield return Ninja.JumpBack;

        points = roiPoints.ToArray();
        numROIPoints = points.Length;

        Array.Sort(points, delegate (Vector3 p1, Vector3 p2) {
            return p1[sortComponent].CompareTo(p2[sortComponent]);
        });

        List<KukuLink> newLinks = new List<KukuLink>();


        for (int i = 0; i < numROIPoints - 1; i++)
        {

            int j = i + 1;
            Vector3 pt = points[i];

            while (j < numROIPoints && points[j][sortComponent] - pt[sortComponent] < maxDistance)
            {
                float d = Vector3.Distance(pt, points[j]);
                if (d < maxDistance)
                {
                    newLinks.Add(new KukuLink(points[i], points[j], d));
                }
                j++;
            }
        }


        links = newLinks;
        linesReady = true;

        yield return true;
    }



    void postRender(Camera cam)
    {
        bool camCheck = true;
        if (cam.cameraType == CameraType.Game && !gameCams) camCheck = false;
        if (cam.cameraType == CameraType.SceneView && !sceneCam) camCheck = false;
        if (!camCheck) return;
        if (links == null) return;


        lineMat.SetPass(0);
        GL.PushMatrix();
        //GL.MultMatrix(Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale));
        GL.Begin(GL.LINES);

        if (drawLines)
        {
            foreach (KukuLink kl in links)
            {
                //if (Vector3.Distance(np, p) > maxDistance) continue;

                float lerpDist = alphaDecay.Evaluate(kl.distance / maxDistance);
                GL.Color(Color.Lerp(nearColor, farColor, lerpDist));

                float baseU = Time.time * (texSpeed + texSpeedDiff * lerpDist) + texOffset;
                GL.TexCoord(new Vector3(baseU, 0, 0));
                GL.Vertex(kl.start);

                GL.TexCoord(new Vector3(baseU + textureTile, 0, 0));
                GL.Vertex(kl.end);
            }
        }

        GL.End();
        GL.PopMatrix();

    }


    bool PointInOABB(Vector3 point, BoxCollider box)
    {
        point = box.transform.InverseTransformPoint(point) - box.center;

        float halfX = (box.size.x * 0.5f);
        float halfY = (box.size.y * 0.5f);
        float halfZ = (box.size.z * 0.5f);
        if (point.x < halfX && point.x > -halfX &&
           point.y < halfY && point.y > -halfY &&
           point.z < halfZ && point.z > -halfZ)
            return true;
        else
            return false;
    }
}
