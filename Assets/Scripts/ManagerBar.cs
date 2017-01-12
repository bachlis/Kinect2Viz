using UnityEngine;
using System.Collections;

public class ManagerBar : OSCControllable
{
    public GameObject barman1;
    public GameObject barman2;
    public GameObject pea1;
    public GameObject pea2;
    [OSCProperty("barman")]
    public int barman_chooser = 0;
    int barman_chooser_old = 0;
    [OSCProperty("pea")]
    public int pea_chooser = 0;
    int pea_chooser_old = 0;

    public override void Start()
    {
        
    }

    public override void Update()
    {
        if (barman_chooser_old != barman_chooser)
        {
            barman_chooser_old = barman_chooser;

            if (barman_chooser == 0)
            {
                barman1.SetActive(true);
                barman2.SetActive(false);
            }
            else
            {
                barman2.SetActive(true);
                barman1.SetActive(false);
            }
        }

        if (pea_chooser_old != pea_chooser)
        {
            pea_chooser_old = pea_chooser;

            if (pea_chooser == 0)
            {
                pea1.SetActive(true);
                pea2.SetActive(false);
            }
            else
            {
                pea2.SetActive(true);
                pea1.SetActive(false);
            }
        }
    }
}
