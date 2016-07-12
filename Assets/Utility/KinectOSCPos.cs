using UnityEngine;
using System.Collections;

public class KinectOSCPos : OSCControllable
{
    [OSCProperty("position")]
    public Vector3 pos;

    [OSCProperty("rotation")]
    public Vector3 rot;

    // Use this for initialization
    public override void Start () {
	
	}

    // Update is called once per frame
    public override void Update () {
        transform.localPosition = pos;
        transform.rotation = Quaternion.Euler(rot);
    }
}
