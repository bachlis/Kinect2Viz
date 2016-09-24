using UnityEngine;
using System.Collections;

public class ROIFromBody : OSCControllable {

    public PCLHandler handler;

    [OSCProperty("yBottom")]
    public float yBottom;
    [OSCProperty("yTop")]
    public float yTop;
    [OSCProperty("xzSize")]
    public float xzSize;

    [OSCProperty("offsetX")]
    public float offsetX;
    [OSCProperty("offsetZ")]
    public float offsetZ;
    

	// Update is called once per frame
	public override void Update () {
        base.Update();

	    if(handler.numBodiesTracked > 0)
        {
            Vector3 localPos = transform.parent.InverseTransformPoint(handler.pclCenter);
            offsetX = localPos.x;
            offsetZ = localPos.z;
        }

        transform.localPosition = new Vector3(offsetX, (yTop + yBottom) / 2, offsetZ);
        transform.localScale = new Vector3(xzSize,  yTop - yBottom, xzSize);
	}
}
