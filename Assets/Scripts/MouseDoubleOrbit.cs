using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class MouseDoubleOrbit : OSCControllable {

    public Transform t1;
    public Transform t2;

    [OSCProperty("distance")]
    public float distance;
    [OSCProperty("x")]
    public float x;
    [OSCProperty("y")]
    public float y;

    [OSCProperty("posOffset")]
    public Vector3 posOffset;
    [OSCProperty("lookAtOffset")]
    public Vector3 lookAtOffset;

    [OSCProperty("posLerpFactor")]
    public float posLerpFactor;
    [OSCProperty("lookAtLerpFactor")]
    public float lookAtLerpFactor;

    [OSCProperty("posSmooth")]
    public float posSmooth;
    [OSCProperty("lookAtSmooth")]
    public float lookAtSmooth;

    Vector3 posVelocity;
    Vector3 previousLookAtPos;
    Vector3 lookAtVelocity;

    //For debug
    Vector3 targetPos;
    Vector3 targetLookAtPos;
    Vector3 targetPosCenter;
	
    [OSCMethod("setCamera")]
    public void setCamera(float x, float y,float distance, Vector3 posOffset, Vector3 lookAtOffset, float posSmooth, float lookAtSmooth, float posLerpFactor, float lookAtLerpFactor)
    {
        Debug.Log("Set camera ("+oscName+") :" + x + "," + y + "," + distance);
        this.posSmooth = posSmooth;
        this.lookAtSmooth = lookAtSmooth;
        this.x = x;
        this.y = y;
        this.distance = distance;
        this.posOffset = posOffset;
        this.lookAtOffset = lookAtOffset;
        this.posLerpFactor = posLerpFactor;
        this.lookAtLerpFactor = lookAtLerpFactor;
    }

    [OSCMethod("setBloom")]
    public void setBloom(int i)
    {
        gameObject.GetComponent<BloomOptimized>().enabled = i > 0;
    }

    [OSCMethod("setDof")]
    public void setDof(int i)
    {
        gameObject.GetComponent<DepthOfField>().enabled = i > 0;
    }

    // Update is called once per frame
    public override void Update () {
        if (t1 == null || t2 == null) return;

        if (Input.GetMouseButton(0))
        {

            x += Input.GetAxis("Mouse X") * 10;
            y -= Input.GetAxis("Mouse Y") * 10;

        }


        previousLookAtPos = new Vector3(targetLookAtPos.x, targetLookAtPos.y, targetLookAtPos.z);


        //Position
        targetPosCenter = Vector3.Lerp(t1.position, t2.position, posLerpFactor);
        targetPos = Quaternion.Euler(y, x, 0) * (Vector3.back * distance) + targetPosCenter + posOffset;
        targetPos = Vector3.SmoothDamp(transform.position, targetPos, ref posVelocity, posSmooth);
        transform.position = targetPos;

        //Rotation
        targetLookAtPos = Vector3.Lerp(t1.position, t2.position, lookAtLerpFactor) + lookAtOffset;
        targetLookAtPos = Vector3.SmoothDamp(previousLookAtPos, targetLookAtPos, ref lookAtVelocity, lookAtSmooth);
        transform.LookAt(targetLookAtPos);
	}

    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(t1.position,.2f);
        Gizmos.DrawWireSphere(t2.position, .2f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosCenter, .2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetLookAtPos, .2f);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, targetPosCenter);
        Gizmos.DrawLine(transform.position, targetLookAtPos);
        Gizmos.DrawWireSphere(targetPosCenter, distance);
    }
}

