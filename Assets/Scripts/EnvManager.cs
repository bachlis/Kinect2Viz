using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnvManager : MonoBehaviour {

    public Material skybox;
    public Color32[] colors;

   // [OSCProperty("colorIndex")]
    public int colorIndex;
    int _lastIndex;

    public bool permanentUpdate;

    
     
	// Use this for initialization
	void Start () {
        _lastIndex = -1;
	}
	
	// Update is called once per frame
	void Update () {
		if(colorIndex != _lastIndex || permanentUpdate)
        {
            _lastIndex = colorIndex;
            skybox.SetColor("_Tint", colors[Mathf.Clamp(colorIndex, 0, colors.Length - 1)]);
            DynamicGI.UpdateEnvironment();
        }
	}
}
