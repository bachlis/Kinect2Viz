using UnityEngine;
using LineWorks;
using System.Collections.Generic;
using DG.Tweening;
using System;

public class LineText : MonoBehaviour
{
    private LW_Canvas linework;
    private LW_Circle circle;
    private LW_Stroke circleStroke;

    private LW_Polyline3D line;
    private LW_Stroke lineStroke;
    private bool matIsInit;
    private Sequence seq;
    TextMesh tm;

    Transform origin;

    private float lineSizeFactor = 5;

    private string text;

    private float blinkSpeed = 10;

    PCLSkeleton skel;
    int jointIndex;

    void Awake()
    {
        tm = GetComponentInChildren<TextMesh>();
    }

    public void setProps(PCLSkeleton skel, string text, float time, int jointIndex, Vector3 dir)
    {
        this.skel = skel;
        this.jointIndex = jointIndex;
        this.text = text;
        Invoke("kill", time);
    
        // Create the LineWorks Components and Scriptable Objects.
        linework = GetComponent<LW_Canvas>();
        linework.segmentation = 20;
        linework.featureMode = FeatureMode.Advanced;
        linework.strokeDrawMode = StrokeDrawMode.Draw3D;
        linework.joinsAndCapsMode = JoinsAndCapsMode.Shader;
        linework.material.SetFloat("_Width", .3f);

        circle = LW_Circle.Create(Vector2.zero, 3f);
        circleStroke = LW_Stroke.Create(Color.white, .2f);
        circleStroke.linejoin = Linejoin.Round;
        circle.styles.Add(circleStroke);
        linework.graphic.Add(circle);

        Vector3[] points = new Vector3[3];
        points[0] = Vector3.zero;
        points[1] = Vector3.Scale(Vector3.one * lineSizeFactor, dir);
        points[2] = points[1] + Vector3.right * dir.x * lineSizeFactor;
        line = LW_Polyline3D.Create(points);
        lineStroke = LW_Stroke.Create(Color.white, .2f);
        line.styles.Add(lineStroke);
        linework.graphic.Add(line);
        line.isVisible = false;

        tm.transform.localPosition = points[2];
        tm.anchor = dir.x > 0 ? TextAnchor.LowerRight : TextAnchor.LowerLeft;
        tm.alignment = dir.x > 0 ? TextAlignment.Right : TextAlignment.Left;

        text = "Super cool dis-donc";
        
        seq = DOTween.Sequence();
        //seq.Pause();
        //blink circle
        seq.AppendCallback(()=> blinkShape(circle,.5f)).AppendInterval(.3f);
        // reduce circle
        seq.AppendCallback(reduceCircle).AppendInterval(.2f);
        //reveal line
        seq.AppendCallback(() => blinkShape(line,.5f)).AppendInterval(.3f);
        seq.AppendCallback(()=>revealText(1));

        transform.position = skel.joints[jointIndex];
        transform.LookAt(Vector3.Scale(Camera.main.transform.position,new Vector3(-1,-1,-1)));
        
    }

    

    void kill()
    {
        seq.Kill();
        Sequence seq2 = DOTween.Sequence();
        //seq.Pause();
        //blink circle
        seq2.AppendCallback(() => blinkShape(circle, .5f)).AppendCallback(() => blinkShape(line, .5f)).AppendCallback(() => blinkText(.5f));
        Destroy(gameObject, .5f);
    }

    void revealText(float time)
    {
        DOTween.To(() => 0, x => tm.text = text.Substring(0, (int)(x)), text.Length, time).SetEase(Ease.Linear);

        tm.text = "Hello test";
    }

    void reduceCircle()
    {
        DOTween.To(() => circle.radius, x => circle.radius = x, .2f, .4f);
    }


    void blinkShape(LW_Shape shape ,float time)
    {
        DOTween.To(() => 0, x => shape.isVisible = Mathf.Floor(x % 2) == 0, blinkSpeed / time, time).SetEase(Ease.Linear).OnComplete(()=> shape.isVisible = true);
        
    }

    void blinkText(float time)
    {
        DOTween.To(() => 0, x => tm.GetComponent<Renderer>().enabled = Mathf.Floor(x % 2) == 0, blinkSpeed / time, time).SetEase(Ease.Linear).OnComplete(() => tm.GetComponent<Renderer>().enabled = true);

    }

    void Update()
    {
        if (skel != null)
        {
            Debug.Log(skel.joints[jointIndex]);
            transform.position = skel.joints[jointIndex];
        }

        transform.LookAt(Vector3.Scale(Camera.main.transform.position,new Vector3(-1,-1,-1)));
    }
}