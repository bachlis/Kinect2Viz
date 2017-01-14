using UnityEngine;
using System.Collections;
using UTJ;

public class ManagerBar : OSCControllable
{
    public AlembicStream barmanStream;

    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    [OSCMethod("time")]
    public void setTime(float time)
    {
        barmanStream.m_time = time;
    }

    [OSCMethod("playAt")]
    public void playAt(float time)
    {
        barmanStream.setTimeAndPlay(time);
    }

    [OSCMethod("manualPlay")]
    public void manualPlay(bool value)
    {
        barmanStream.setManualPlay(value);
    }
}
