using UnityEngine;
using System.Collections;

public class Manager : OSCControllable
{
    public GameObject[] decor;
    [OSCProperty("decor")]
    public int decor_chooser = 0;
    int decor_chooser_old = 0;
    public EnvManager envManager;


    

    
    public override void Start()
    {
        
    }

    public override void Update()
    {
        if (decor_chooser_old != decor_chooser)
        {
            decor_chooser_old = decor_chooser;

            for (int i = 0; i < decor.Length; i++)
            {
                GameObject o = decor[i];
                if (o != null)
                    o.SetActive(decor_chooser == i + 1);
            }

            envManager.colorIndex = Mathf.Clamp(decor_chooser - 1, 0, envManager.colors.Length - 1);
        }
    }

    [OSCMethod("envColor")]
    public void setColorAt(int index, Color col)
    {
        if (index < 0 && index >= envManager.colors.Length) return;
        envManager.colors[index] = col;
    }
}
