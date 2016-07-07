using UnityEngine;
using System.Collections;

public class Manager : OSCControllable
{
    public GameObject[] decor;
    [OSCProperty("decor")]
    public int decor_chooser = 0;
    
    public override void Start()
    {
    }

    public override void Update()
    {
        for (int i = 0; i < decor.Length; i++)
        {
            GameObject o = decor[i];
            if (o != null)
                o.SetActive(decor_chooser == i + 1);
        }
    }
}
