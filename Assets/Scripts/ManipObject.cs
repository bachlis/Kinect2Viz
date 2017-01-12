using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ManipObject : OSCControllable {

    public PCLSkeleton skel;


    public int joint;
    public bool attached;
    Vector3 posVelocity;

    Vector3 initPos;
    Vector3 targetPos;

    [OSCProperty("smooth")]
    public float smooth = .1f;

    [OSCMethod("attach")]
    public void setAttached(bool value, int targetJoint)
    {
        if (attached == value) return;
        attached = value;
        targetJoint = Mathf.Clamp(targetJoint, 0, skel.joints.Length);
    }

    override public void Start () {
        initPos = transform.position;
	}
	
	override public void Update () {
		if(attached)  targetPos = skel.joints[joint];
        else targetPos = initPos;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref posVelocity, smooth);
    }
}
