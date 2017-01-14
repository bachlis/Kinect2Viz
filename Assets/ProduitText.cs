using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduitText : OSCControllable {

    public string baseText;

	// Use this for initialization
	override public void Start () {
		
	}

    // Update is called once per frame
    override public void Update () {
		
	}

    [OSCMethod("defaultText")]
    public void setText(string value)
    {
        baseText = value;
    }
}
