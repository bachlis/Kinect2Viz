using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class CamEffectManager : OSCControllable {

    BloomOptimized bloom;
    DepthOfField dof;
    Antialiasing aa; 
    ScreenSpaceAmbientOcclusion ssao;

    public override void Start()
    {
        bloom = gameObject.GetComponent<BloomOptimized>();
        dof = gameObject.GetComponent<DepthOfField>();
        ssao = gameObject.GetComponent<ScreenSpaceAmbientOcclusion>();
    }


    
    [OSCMethod("setBloom")]
    public void setBloom(int i)
    {
        bloom.enabled = i > 0;
    }

    [OSCMethod("setDof")]
    public void setDof(int i)
    {
        dof.enabled = i > 0;
    }

    [OSCMethod("setOcclusion")]
    public void setAO(int i)
    {
        ssao.enabled = i > 0;
    }


    [OSCMethod("bloomIntensity")]
    public void bloomIntensity(float value) { bloom.intensity = value; }
    [OSCMethod("bloomThreshold")]
    public void bloomThreshold(float value) { bloom.threshold = value; }
    [OSCMethod("dofDistance")]
    public void dofDistance(float value) { dof.focalLength = value; }
    [OSCMethod("dofSize")]
    public void dofSize(float value) { dof.focalSize = value; }
    [OSCMethod("dofAperture")]
    public void dofAperture(float value) { dof.aperture = value; }
    [OSCMethod("dofDebug")]
    public void dofDebug(bool value) { dof.visualizeFocus = value; }

}
