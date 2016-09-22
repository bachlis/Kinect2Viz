using UnityEngine;
using System.Collections;

public class PCLSkeleton : MonoBehaviour {

    public bool debugJoints;
    PCLHandler handler;
	// Use this for initialization
	void Start () {
        
        handler = GetComponent<PCLHandler>();
	}
	 
	// Update is called once per frame
	void Update () {
	
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
