using UnityEngine;
using System.Collections;

public class DebugOrigin : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        if(isActiveAndEnabled) Gizmos.DrawWireSphere(transform.position, .1f);
    }
}
