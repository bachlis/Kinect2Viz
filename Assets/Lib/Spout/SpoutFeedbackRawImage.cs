using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpoutFeedbackRawImage : MonoBehaviour {

    RawImage image;
    public Camera cam;

	// Use this for initialization
	void Start () {
        image = GetComponent<RawImage>();
        image.texture = cam.targetTexture;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
