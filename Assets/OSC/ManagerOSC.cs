using UnityEngine;
using System.Collections;

public class ManagerOSC : OSCControllable
{
    public GameObject[] decor;
    [OSCProperty("decor")]
    public int decor_chooser = 0;
    /*
    public GameObject appart;
    [OSCProperty("appart")]
    public bool appart_visible = true;
    public GameObject ascenseur;
    [OSCProperty("ascenseur")]
    public bool ascenseur_visible = true;
    public GameObject bar;
    [OSCProperty("bar")]
    public bool bar_visible = true;
    public GameObject bureau;
    [OSCProperty("bureau")]
    public bool bureau_visible = true;
    public GameObject restaurant;
    [OSCProperty("restaurant")]
    public bool restaurant_visible = true;//*/

    public override void Start () {
	}

    public override void Update () {

        for (int i = 0; i < decor.Length; i++)
        {
            GameObject o = decor[i];
            if (o != null)
                o.SetActive(decor_chooser == i+1);
        }
        /*appart.SetActive(appart_visible);
        ascenseur.SetActive(ascenseur_visible);
        bar.SetActive(bar_visible);
        bureau.SetActive(bureau_visible);
        restaurant.SetActive(restaurant_visible);//*/

    }
}
