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
    Transform lookAtTarget;
    
    public float lineSizeFactor;

    private string text;

    private float blinkSpeed = 10;

    PCLSkeleton skel;
    int jointIndex;

    float posSmooth;
    Vector3 posVelocity;

    void Awake()
    {
        tm = GetComponentInChildren<TextMesh>();
        
    }
    public void setPropsWithFixedPos(Vector3 fixedPos, Transform lookAtTarget, string text, float time, int jointIndex, Vector3 dir, float fontSize, float posSmooth, float circleSize)
    {
        transform.position = fixedPos;
        setProps(null, lookAtTarget, text, time, jointIndex, dir, fontSize, posSmooth, circleSize);
    }

    public void setProps(PCLSkeleton skel, Transform lookAtTarget, string text, float time, int jointIndex, Vector3 dir, float fontSize, float posSmooth, float circleSize)
    {
        this.skel = skel;
        this.lookAtTarget = lookAtTarget;
        this.jointIndex = jointIndex;
        this.text = text;
        this.posSmooth = posSmooth;
        
        Invoke("kill", time);
    
        // Create the LineWorks Components and Scriptable Objects.
        linework = GetComponent<LW_Canvas>();
        linework.segmentation = 20;
        linework.featureMode = FeatureMode.Advanced;
        linework.strokeDrawMode = StrokeDrawMode.Draw3D;
        linework.joinsAndCapsMode = JoinsAndCapsMode.Shader;

        circle = LW_Circle.Create(Vector2.zero, .15f / transform.localScale.x);
        circleStroke = LW_Stroke.Create(Color.white, .2f);
        circleStroke.linejoin = Linejoin.Round;
        circle.styles.Add(circleStroke);
        linework.graphic.Add(circle);

        Vector3[] points = new Vector3[3];
        points[0] = Vector3.zero;
        points[1] = Vector3.Scale(Vector3.one, dir)/transform.localScale.x;
        points[2] = points[1] + (Vector3.right * dir.x * lineSizeFactor) / transform.localScale.x;
        line = LW_Polyline3D.Create(points);
        lineStroke = LW_Stroke.Create(Color.white, .2f);
        line.styles.Add(lineStroke);
        linework.graphic.Add(line);
        line.isVisible = false;

        tm.transform.localPosition = points[2];
        tm.anchor = dir.x > 0 ? TextAnchor.LowerRight : TextAnchor.LowerLeft;
        tm.alignment = dir.x > 0 ? TextAlignment.Right : TextAlignment.Left;
        tm.characterSize = fontSize/100;
        text = "Super cool dis-donc";
        
        seq = DOTween.Sequence();
        //seq.Pause();
        //blink circle
        seq.AppendCallback(()=> blinkShape(circle,.5f)).AppendInterval(.3f);
        // reduce circle
        seq.AppendCallback(()=>reduceCircle(circleSize/transform.localScale.x)).AppendInterval(.2f);
        //reveal line
        seq.AppendCallback(() => blinkShape(line,.5f)).AppendInterval(.3f);
        seq.AppendCallback(()=>revealText(1));

        if(skel != null) transform.position = skel.joints[jointIndex];
        transform.LookAt(lookAtTarget.position);
        
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

    void reduceCircle(float targetRadius)
    {
        DOTween.To(() => circle.radius, x => circle.radius = x, targetRadius, .4f);
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
            transform.position = Vector3.SmoothDamp(transform.position, skel.joints[jointIndex], ref posVelocity, posSmooth);
        }

        transform.LookAt(transform.position*2-lookAtTarget.position);
    }
}