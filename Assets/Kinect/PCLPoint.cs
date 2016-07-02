using UnityEngine;

public class PCLPoint  {

    public Vector3 position;

    public bool isBody;
    public bool isValid;
    public bool isInROI;
    public Color color;

    public PCLPoint(Vector3 position, bool isBody, bool isValid, bool isInROI, Color color)
    {
        this.position = position;
        this.isBody = isBody;
        this.isValid = isValid;
        this.isInROI = isInROI;
        this.color = color;
    }
}
