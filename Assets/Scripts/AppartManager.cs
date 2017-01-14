using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppartManager : OSCControllable {

    public ParticleSystem waterParticles;

    [Header("Water")]
    [OSCProperty("water")]
    public bool water;
    bool lastWater;

	// Use this for initialization
	override public void Start () {

        //init
        lastWater = true;
        water = false;
	}
	
	// Update is called once per frame
	override public void Update () {
		if(lastWater != water)
        {
            lastWater = water;
            if (water) waterParticles.Play();
            else waterParticles.Stop();
        }
	}


}
