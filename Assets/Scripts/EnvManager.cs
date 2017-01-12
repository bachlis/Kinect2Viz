using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnvManager : OSCControllable {

    public Material skybox;
    public Color32[] colors;

    [OSCProperty("colorIndex")]
    public int colorIndex;
    int _lastIndex;

    public bool permanentUpdate;
    
	// Use this for initialization
	override public void Start () {
        _lastIndex = -1;
	}
	
	// Update is called once per frame
	override public void Update () {
		if(colorIndex != _lastIndex || permanentUpdate)
        {
            _lastIndex = colorIndex;
            skybox.SetColor("_Tint", colors[Mathf.Clamp(colorIndex, 0, colors.Length - 1)]);
            DynamicGI.UpdateEnvironment();
        }
	}
}
