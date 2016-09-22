using UnityEngine;
using System.Collections;

public class ROIFromBody : MonoBehaviour {

    public PCLHandler handler;
    public float yBottom;
    public float yTop;
    public float xzSize;

    public float targetX;
    public float targetZ;
    
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    if(handler.numBodiesTracked > 0)
        {
            Vector3 localPos = transform.parent.InverseTransformPoint(handler.pclCenter);
            targetX = localPos.x;
            targetZ = localPos.z;
        }

        transform.localPosition = new Vector3(targetX, (yTop + yBottom) / 2, targetZ);
        transform.localScale = new Vector3(xzSize,  yTop - yBottom, xzSize);
	}
}
