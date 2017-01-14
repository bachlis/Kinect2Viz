using UnityEngine;
using System.Collections;

public class NoiseMultiMesh : OSCControllable
{

    Mesh[] m;
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
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        m = new Mesh[filters.Length];
        initVertices = new Vector3[filters.Length][];
        seeds = new float[filters.Length][];
        numVertices = new int[filters.Length];

        for(int f=0;f<filters.Length;f++)
        {
            m[f] = filters[f].mesh;
            numVertices[f] = m[f].vertexCount;
            initVertices[f] = new Vector3[numVertices[f]];
            seeds[f] = new float[numVertices[f]];

            for (int i = 0; i < numVertices[f]; i++)
            {
                initVertices[f][i] = m[f].vertices[i];
                seeds[f][i] = Random.value;
            }
        }
        
    }

    // Update is called once per frame
    public override void Update()
    {
        for(int f=0;f<m.Length;f++)
        {
            Vector3[] vertices = m[f].vertices;

            for (int i = 0; i < numVertices[f]; i++)
            {
                Vector3 target = initVertices[f][i] + Random.insideUnitSphere * amplitude;
                vertices[i] = Vector3.Lerp(vertices[i], target, Time.deltaTime * seeds[f][i] * speed);
            }

            m[f].vertices = vertices;
        }
        
    }
}
