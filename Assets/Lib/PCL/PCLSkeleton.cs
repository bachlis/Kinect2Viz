using UnityEngine;
using System.Collections;

public class PCLSkeleton : MonoBehaviour {

    public GameObject lineTextPrefab;

    public bool debugJoints;
    PCLHandler handler;

    public Vector3[] joints;

	// Use this for initialization
	void Start () {
        
        handler = GetComponent<PCLHandler>();
	}
	 

    void showText(string text, float time, int jointIndex, Vector3 dir)
    {
        LineText lt = Instantiate(lineTextPrefab).GetComponent<LineText>();
        lt.setProps(this, text, time, jointIndex, dir);
        //lt.transform.parent = transform;
        
    }

	// Update is called once per frame
	void Update () {
        joints = handler.joints;

        if(Input.GetMouseButtonDown(0))
        {
            showText("Trop de la balle", 1 + Random.value * 4, Random.Range(0, 4), Random.insideUnitSphere*2);
        }
	}

    void OnDrawGizmos() 
    {
        if (handler == null) return;
        if(debugJoints && handler.numBodiesTracked > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.TransformPoint(joints[0]), .05f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.TransformPoint(joints[0]), .05f);
            Gizmos.DrawWireSphere(transform.TransformPoint(joints[0]), .05f);
            Gizmos.DrawWireSphere(transform.TransformPoint(joints[0]), .05f);
            Gizmos.DrawWireSphere(transform.TransformPoint(joints[0]), .05f);
        }
    }
}
