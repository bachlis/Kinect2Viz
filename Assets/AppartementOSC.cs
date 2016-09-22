using UnityEngine;
using System.Collections;

public class AppartementOSC : OSCControllable {

    public Material appartMat;
    //Renderer[] renderers;

    [OSCProperty("wireColor")]
    public Color wireColor = Color.white;
    [OSCProperty("diffColor")]
    public Color diffColor = Color.white;

    Color lastWireColor = Color.white;
    Color lastDiffColor = Color.white;

    public override void Start()
    {
        base.Start();
        //renderers = GetComponentsInChildren<Renderer>();
        
    }

    // Update is called once per frame
    public override void Update () {
        base.Update();
	    if(wireColor.a != lastWireColor.a 
            || wireColor.r != lastWireColor.r
            || wireColor.g != lastWireColor.g
            || wireColor.b != lastWireColor.b)
        {
            appartMat.SetColor("_WireframeColor", wireColor);
            lastWireColor = wireColor;
        }

        if (diffColor.a != lastDiffColor.a
            || diffColor.r != lastDiffColor.r
            || diffColor.g != lastDiffColor.g
            || diffColor.b != lastDiffColor.b)
        {
            appartMat.SetColor("_DiffColor", diffColor);
            lastDiffColor = diffColor;
        }
    }
}
