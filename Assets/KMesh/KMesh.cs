using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class KMesh : MonoBehaviour {

    Mesh mesh;

    public Color bodyColor = Color.white;
    public Color bgColor = Color.white;

    public bool doSmooth = true;
    [Range(.01f, 10)]
    public float smoothFactor = 2;

    Color[] colors;
    Vector3[] vertices;

    public Vector3 offset;

    public bool invertNormals;

    Material mat;

    /*
    public int resoX;
    public int resoZ;
    int lastResoX;
    int lastResoZ;
    */

    // Use this for initialization
    void Start () {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mat = GetComponent<Renderer>().material;   
	}
	
	// Update is called once per frame
	void Update () {
        int numPoints = KinectPCL.instance.numPoints;


        
        if (mesh.vertexCount != numPoints)
        {
            rebuildMesh(KinectPCL.instance.pointsWidth, KinectPCL.instance.pointsHeight);

            if (mesh.vertexCount == 0) return;
            colors = new Color[mesh.vertexCount];
            vertices = new Vector3[mesh.vertexCount];
            mat.SetInt("_ResX", KinectPCL.instance.pointsWidth);
            mat.SetInt("_ResY", KinectPCL.instance.pointsHeight);
        }
        
        

        //Debug.Log(mesh.vertexCount + " / " + numPoints);

        PCLPoint[] points = KinectPCL.instance.points;
        
        bool[] isBody = KinectPCL.instance.bodyMask;

        float lerpFactor = smoothFactor * Time.deltaTime;


        for (int i = 0; i < mesh.vertexCount; i++)
        {
            PCLPoint pp = points[i];

            if (!pp.isValid) continue;

            Color targetColor = isBody[i] ? bodyColor : bgColor;

            Vector3 targetPoint = pp.position + offset;

            if (doSmooth)
            {
                colors[i] = Color.Lerp(mesh.colors[i], targetColor, lerpFactor);
                vertices[i] = Vector3.Lerp(vertices[i], targetPoint, lerpFactor);
            }
            else
            {
                colors[i] = targetColor;
                vertices[i] = targetPoint;
            }

        }
        

        mesh.vertices = vertices;
        mesh.colors = colors;

        //mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }


    void rebuildMesh(int resX, int resZ)
    {
        if (resX <2 || resZ <2) return;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        

	
        Vector3[] vertices = new Vector3[resX * resZ];
        for (int z = 0; z < resZ; z++)
        {
            for (int x = 0; x < resX; x++)
            {
                vertices[x + z * resX] = new Vector3(x*1f/(resX-1),0,z*1f/(resZ-1));
            }
        }
	
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int v = 0; v < resZ; v++)
        {
            for (int u = 0; u < resX; u++)
            {
                uvs[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
            }
        }
        
        int nbFaces = (resX - 1) * (resZ - 1);
        int[] triangles = new int[nbFaces * 6];
        

        int t = 0;

        for (int ix = 0;ix<resX-1;ix++)
        {
            for(int iy=0;iy<resZ-1;iy++)
            {
                // Retrieve lower left corner from face ind
                int index = iy * resX + ix;

                if (invertNormals)
                {
                    triangles[t] = index;
                    triangles[t + 2] = index + resX;
                    triangles[t + 1] = index + 1;

                    triangles[t + 3] = index + 1;
                    triangles[t + 5] = index + resX;
                    triangles[t + 4] = index + resX + 1;
                }else
                {
                    triangles[t] = index;
                    triangles[t + 1] = index + resX;
                    triangles[t + 2] = index + 1;

                    triangles[t + 3] = index + 1;
                    triangles[t + 4] = index + resX;
                    triangles[t + 5] = index + resX + 1;
                }

                if(index+resX+1 >= vertices.Length)
                {
                    Debug.Log("Bug, out of vertices : " + ix+"::"+iy+" / index = "+index+", bottom rigth : "+(index + resX + 1));
                }
                t += 6;
            }
           
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.colors = new Color[mesh.vertices.Length];
        mesh.RecalculateBounds();
        ;
    }

}
