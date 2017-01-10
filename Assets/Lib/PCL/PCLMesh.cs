using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PCLMesh : OSCControllable {

    Mesh m;
    
    PCLHandler handler;


    public bool isActive = false;

    Vector3[] randomVectors;
    [OSCProperty("randomFactor")]
    public float randomFactor;
    [OSCProperty("shuffleRandom")]
    public bool shuffleRandom;

    public bool onlyWhenBodyDetected;
    public bool useROIWhenNobody;
    public BoxCollider roiCollider;

    // Use this for initialization
    public override void Start () {
        handler = GetComponent<PCLHandler>();
        m = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = m;
        randomVectors = new Vector3[0];

        Debug.Log("Start");
	}
	
    [OSCMethod("active")]
    public void setActive(bool value)
    {
        m.Clear();
        isActive = value;
    }

    [OSCMethod("color")]
    public void setColor(Color col)
    {
        GetComponent<Renderer>().sharedMaterial.color = col;
    }

    // Update is called once per frame
    public override void Update () {

        if (handler.numBodiesTracked == 0 && onlyWhenBodyDetected)
        {
            m.Clear();
            return;
        }

        if (!isActive) return;
        if (handler.numQuads == 0) return;
        if (handler.points.Length == 0) return;

        if (randomVectors.Length == 0)
        {
            randomVectors = new Vector3[handler.numTotalPoints];
            for (int i = 0; i < handler.numTotalPoints; i++)
            {
                randomVectors[i] = UnityEngine.Random.insideUnitSphere;
            }
        }

        Vector3[] vertices = new Vector3[handler.numGoodPoints];
        int[] indexRemap = new int[handler.numTotalPoints];
        int[] indices = new int[handler.numQuads * 4];
        Vector2[] uvs = new Vector2[handler.numGoodPoints];
        
        for(int i=0;i<handler.numGoodPoints;i++)
        {
            int pIndex = handler.goodPointIndices[i];

            if (shuffleRandom)
            {
                randomVectors[pIndex] = UnityEngine.Random.insideUnitSphere;
            }


            vertices[i] = handler.points[pIndex] + randomVectors[pIndex] * randomFactor;
            indexRemap[pIndex] = i;

            float uvX = (pIndex % handler.pclWidth) * 1.0f / handler.pclWidth; ;
            float uvY = Mathf.Floor(pIndex / handler.pclWidth) * 1.0f / handler.pclHeight;
            uvs[i] = new Vector2(uvX, uvY);
        }


        for(int i=0;i<handler.numQuads;i++)
        {
            int index1 = i * 4;
            int index2 = i * 4 + 1;
            int index3 = i * 4 + 2;
            int index4 = i * 4 + 3;

            int pIndex1 = handler.quadIndices[index1];
            int pIndex2 = handler.quadIndices[index2];
            int pIndex3 = handler.quadIndices[index3];
            int pIndex4 = handler.quadIndices[index4];

            if (pIndex1 >= handler.numTotalPoints || handler.points[pIndex1].x == 0) continue;
            if (pIndex2 >= handler.numTotalPoints || handler.points[pIndex2].x == 0) continue;
            if (pIndex3 >= handler.numTotalPoints || handler.points[pIndex3].x == 0) continue;
            if (pIndex4 >= handler.numTotalPoints || handler.points[pIndex4].x == 0) continue;

            indices[index1] = indexRemap[pIndex1];
            indices[index2] = indexRemap[pIndex2];
            indices[index3] = indexRemap[pIndex3];
            indices[index4] = indexRemap[pIndex4];
        }

        m.Clear();
        m.vertices = vertices;
        m.uv = uvs;
        m.SetIndices(indices, MeshTopology.Quads, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
       
    }

    void OnDisable()
    {
        m.Clear();
    }
    
    
}
