using UnityEngine;
using System.Collections;

public class OctreeTest : MonoBehaviour
{

    public bool debugPoints;
    public bool debugBounds;

    Transform[] gos;

    PointOctree<Transform> po;

    // Use this for initialization
    void Start()
    {

        gos = GetComponentsInChildren<Transform>();

    }

    // Update is called once per frame
    void Update()
    {
        po = new PointOctree<Transform>(10, Vector3.zero, .01f);
        foreach (Transform go in gos)
        {
            po.Add(go, go.transform.position);
        }

    }

    void OnDrawGizmos()
    {
        if (debugPoints) po.DrawAllObjects();
        if (debugBounds)
        {
            po.DrawAllBounds();
        }
    }
}
