using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduitDescription : MonoBehaviour {

    public string text;

    void Start()
    {
        text = text.Replace("\\n", "\n");
    }
}
