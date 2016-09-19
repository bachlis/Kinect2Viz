using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class AssignMaterial : ScriptableWizard
{
    public Material mat;

    void OnWizardUpdate()
    {
        helpString = "Select Game Obects";
        isValid = (mat != null);
    }

    void OnWizardCreate()
    {

        GameObject[] gos = Selection.gameObjects;
   
        foreach (GameObject go in gos)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(); ;
            foreach(Renderer r in renderers)
            {
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    r.materials[i] = null;
                    r.material = null;
                    r.sharedMaterial = mat;
                    r.sharedMaterials[i] = mat;
                }
            }
            
        }
    }

    [MenuItem("Custom/Assign Material", false, 4)]
    static void assignMaterial()
    {
        ScriptableWizard.DisplayWizard<AssignMaterial>("Assign Material", "Assign");
    }


}

#endif
