// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MouseOrbitImproved : OSCControllable
{
    public Transform target;

    [OSCProperty("target")]
    public Vector3 targetOffset;

    [OSCProperty("distance")]
    public float distance = 5.0f;

    public float xSpeed = 20.0f;
    public float ySpeed = 20.0f;

    float yMinLimit = -90;
    float yMaxLimit = 90;

    float distanceMin = 1;
    float distanceMax = 500;

    [OSCProperty("x")]
    public float x = 0.0f;
    [OSCProperty("y")]
    public float y = 0.0f;

    public float smoothTime = 0.2f;

    private float xSmooth = 0.0f;
    private float ySmooth = 0.0f;
    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;

    //private FIXME_VAR_TYPE posSmooth= Vector3.zero;
    //private FIXME_VAR_TYPE posVelocity= Vector3.zero;


    public override void Start()
    {
    }

    void LateUpdate()
    {
        if (target)
        {
            if (Input.GetMouseButton(0))
            {

                x += Input.GetAxis("Mouse X") * xSpeed;
                y -= Input.GetAxis("Mouse Y") * ySpeed;

                y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

            }

            if (Application.isPlaying)
            {
                xSmooth = Mathf.SmoothDamp(xSmooth, x, ref xVelocity, smoothTime);
                ySmooth = Mathf.SmoothDamp(ySmooth, y, ref yVelocity, smoothTime);
            }else
            {
                xSmooth = x;
                ySmooth = y;
            }

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * distance, distanceMin, distanceMax);


            transform.localRotation = Quaternion.Euler(ySmooth, xSmooth, 0);
            transform.position = transform.rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + targetOffset;

        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(target.transform.position+targetOffset, Vector3.one * 1f);
    }
}