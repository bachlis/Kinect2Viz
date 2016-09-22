using UnityEngine;
using System.Collections;

public class TransformOSCPos : OSCControllable
{
    [OSCProperty("position")]
    public Vector3 pos;

    [OSCProperty("rotation")]
    public Vector3 rot;

    [OSCProperty("scale")]
    public Vector3 scale = Vector3.one;

    [OSCProperty("smooth")]
    public float smoothTime = 0.2f;

    private Vector3 posSmooth = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 rotSmooth = new Vector3(0.0f, 0.0f, 0.0f);

    private Vector3 posVelocity = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 rotVelocity = new Vector3(0.0f, 0.0f, 0.0f);

    // Use this for initialization
    public override void Start () {
	
	}

    // Update is called once per frame
    public override void Update () {

        if (Application.isPlaying)
        {
            posSmooth.x = Mathf.SmoothDamp(posSmooth.x, pos.x, ref posVelocity.x, smoothTime);
            posSmooth.y = Mathf.SmoothDamp(posSmooth.y, pos.y, ref posVelocity.y, smoothTime);
            posSmooth.z = Mathf.SmoothDamp(posSmooth.z, pos.z, ref posVelocity.z, smoothTime);

            rotSmooth.x = Mathf.SmoothDamp(rotSmooth.x, rot.x, ref rotVelocity.x, smoothTime);
            rotSmooth.y = Mathf.SmoothDamp(rotSmooth.y, rot.y, ref rotVelocity.y, smoothTime);
            rotSmooth.z = Mathf.SmoothDamp(rotSmooth.z, rot.z, ref rotVelocity.z, smoothTime);
        }
        else
        {
            // posSmooth = pos ???
            posSmooth.x = pos.x;
            posSmooth.y = pos.y;
            posSmooth.z = pos.z;

            // rotSmooth = rot ???
            rotSmooth.x = rot.x;
            rotSmooth.y = rot.y;
            rotSmooth.z = rot.z;
        }

        transform.localPosition = posSmooth;
        transform.localRotation = Quaternion.Euler(rotSmooth);
        transform.localScale = scale;
    }
}
