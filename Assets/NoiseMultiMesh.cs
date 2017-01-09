using UnityEngine;
using System.Collections;

public class NoiseMultiMesh : OSCControllable
{
    MeshFilter[] mm;
    Mesh m;
    int[] numVertices;
    Vector3[][] initVertices;
    float[][] seeds;

    [OSCProperty("speed")]
    [Range(0, 10)]
    public float speed = 1f;

    [OSCProperty("amplitude")]
    [Range(0, 2)]
    public float amplitude = .1f;

    // Use this for initialization
    public override void Start()
    {
     /*   mm = GetComponentsInChildren<MeshFilter>();
        for(int j = 0; j < mm.Length; j++)
        {
            m = mm[j].mesh;
            numVertices[j] = m.vertexCount;
            initVertices[j] = new Vector3[numVertices[j]];
            seeds[j] = new float[numVertices[j]];

            for (int i = 0; i < numVertices[j]; i++)
            {
                initVertices[j][i] = m.vertices[i];
                seeds[j][i] = Random.value;
            }
        }
        //*/
    }

    // Update is called once per frame
    public override void Update()
    {
      /*  for (int j = 0; j < mm.Length; j++)
        {
            Vector3[] vertices = mm[j].mesh.vertices;

            for (int i = 0; i < numVertices[j]; i++)
            {
                Vector3 target = initVertices[j][i] + Random.insideUnitSphere * amplitude;
                vertices[i] = Vector3.Lerp(vertices[i], target, Time.deltaTime * seeds[j][i] * speed);
            }

            m.vertices = vertices;
        }//*/
    }
}