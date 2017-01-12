using UnityEngine;
using System.Collections;

public class TextPosScript : OSCControllable {

    [OSCProperty("position")]
    public Vector3 pos;

    [OSCProperty("rotation")]
    public Vector3 rot;

    [OSCProperty("log")]
    public string text;

    private TextMesh textMesh;

    // Use this for initialization
    public override void Start () {
        textMesh = transform.FindChild("Log1").GetComponent<TextMesh>();
	}

    // Update is called once per frame
    public override void Update () {
        transform.localPosition = pos;
        transform.rotation = Quaternion.Euler(rot);
        //Debug.Log(text);
        textMesh.text = text;
    }
}
