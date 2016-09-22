using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PCLDataReceiver : MonoBehaviour
{

    public PCLData data { get; private set; }
    
    // Use this for initialization
    void Start() {
        data = new PCLData();
        
        int size = System.Runtime.InteropServices.Marshal.SizeOf(data);
        MemoryMapManager.instance.setupMemoryShare("SoftLovePCL", size, false);

    }
	
	// Update is called once per frame
	void Update () {
        MemoryMapManager.fillData(data);
    }
}

