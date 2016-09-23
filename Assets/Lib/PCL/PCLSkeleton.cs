using UnityEngine;
using System.Collections;

public class PCLSkeleton : OSCControllable {

    public bool debugJoints;
    PCLHandler handler;
	// Use this for initialization
	public override void Start () {
        
        handler = GetComponent<PCLHandler>();
	}

    // Update is called once per frame
    public override void Update () {
	
	}

    [OSCMethod("showText")]
    public void showText(string text, int skelIndex)
    {
        Debug.Log("Show text :" + text + " at " + skelIndex);
    }

    void OnDrawGizmos() 
    {
        if (handler == null) return;
        if(debugJoints && handler.numBodiesTracked > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.TransformPoint(handler.headPos), .05f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.TransformPoint(handler.neckPos), .05f);
            Gizmos.DrawWireSphere(transform.TransformPoint(handler.torsoPos), .05f);
            Gizmos.DrawWireSphere(transform.TransformPoint(handler.leftHandPos), .05f);
            Gizmos.DrawWireSphere(transform.TransformPoint(handler.rightHandPos), .05f);
        }
    }
}
