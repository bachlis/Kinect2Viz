using UnityEngine;
using System.Collections;

public class PCLSkeleton : OSCControllable {

    public GameObject lineTextPrefab;

    public bool debugJoints;
    PCLHandler handler;

    public Vector3[] joints;
    public Transform lookAtTarget;

    [OSCProperty("circleSize")]
    [Range(0.01f, 1f)]
    public float circleSize;

    [OSCProperty("textSmooth")]
    [Range(0f,1f)]
    public float textSmooth;

    // Use this for initialization
    public override void Start () {
        
        handler = GetComponent<PCLHandler>();
	}
	 
    [OSCMethod("showText")]
    public void showText(string text, float time, int jointIndex, Vector3 dir, float fontSize)
    {
        LineText lt = Instantiate(lineTextPrefab).GetComponent<LineText>();
        lt.setProps(this, lookAtTarget, text, time, jointIndex, dir,fontSize,textSmooth,circleSize);
        //lt.transform.parent = transform;
        
    }

    [OSCMethod("showTextFixed")]
    public void showTextFixed(string text, float time, int jointIndex, Vector3 dir, float fontSize, Vector3 pos)
    {
        LineText lt = Instantiate(lineTextPrefab).GetComponent<LineText>();
        lt.setPropsWithFixedPos(pos, lookAtTarget, text, time, jointIndex, dir, fontSize, textSmooth, circleSize);
        //lt.transform.parent = transform;

    }

    // Update is called once per frame
    public override void Update () {
        if(handler.joints != null)
        {
            if (joints.Length == 0)
            {
                joints = new Vector3[handler.joints.Length];
            }
            for (int i = 0; i < handler.joints.Length; i++)
            {
                joints[i] = transform.TransformPoint(handler.joints[i]);
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            showText("Trop de la balle", 2, Random.Range(0, 4), Random.insideUnitSphere*.1f,2f);
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
