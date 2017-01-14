using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spout;

[ExecuteInEditMode]
public class PanierControl : OSCControllable {

    public Texture panierTex;

    [OSCProperty("position")]
    public Vector2 panierPos;
    [OSCProperty("size")]
    public Vector2 panierSize;
    [OSCProperty("textPosition")]
    public Vector2 textPos;
    [OSCProperty("fontSize")]
    public float fontSize;

    [OSCProperty("numItems")]
    public int numItems;

    public SpoutCamSender spoutCam;

    // Use this for initialization
    public override void Start () {

	}

    // Update is called once per frame
    public override void Update () {
		
	}

    
    void OnGUI()
    {
        if(Application.isPlaying) BeginRenderTextureGUI(spoutCam.texture);

        GUI.DrawTexture(new Rect(panierPos.x, panierPos.y, panierSize.x, panierSize.y), panierTex);
        GUI.color = Color.black;
        GUIStyle style = new GUIStyle();
        style.fontSize = (int)fontSize;
        style.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(panierPos.x+textPos.x, panierPos.y+textPos.y, panierSize.x, panierSize.y), numItems.ToString(),style);


        if (Application.isPlaying) EndRenderTextureGUI();
    }
    
    
    RenderTexture m_PreviousActiveTexture = null;
    protected void BeginRenderTextureGUI(RenderTexture targetTexture)
    {
        if (Event.current.type == EventType.Repaint)
        {
            m_PreviousActiveTexture = RenderTexture.active;
            if (targetTexture != null)
            {
                RenderTexture.active = targetTexture;
                //GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            }
        }
    }

    protected void EndRenderTextureGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            RenderTexture.active = m_PreviousActiveTexture;
        }
    }
}
