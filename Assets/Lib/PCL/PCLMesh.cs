using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PCLMesh : MonoBehaviour {

    Mesh m;
    
    PCLHandler handler;

	// Use this for initialization
	void Start () {
        handler = GetComponent<PCLHandler>();
        m = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = m;
	}
	
	// Update is called once per frame
	void Update () {
        if (handler.numQuads == 0) return;

        Vector3[] vertices = new Vector3[handler.numQuads * 4];
        int[] indices = new int[handler.numQuads * 4];
        Vector2[] uvs = new Vector2[handler.numQuads * 4];
        for(int i=0;i<handler.numQuads;i++)
        {
            for(int j=0;j<4;j++)
            {
                int index = i * 4 + j;
                vertices[index] = handler.quads[index];
                indices[index] = index;
                uvs[index] = handler.uvs[index];
            }
        }

        m.Clear();
        m.vertices = vertices;
        m.uv = uvs;
        m.SetIndices(indices, MeshTopology.Quads, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
       
    }
}
