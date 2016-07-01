using UnityEngine;

public class PCLPoint  {

    public Vector3 position;

    public bool isBody;
    public bool isValid;
    public bool isInROI;

    public PCLPoint(Vector3 position, bool isBody, bool isValid, bool isInROI)
    {
        this.position = position;
        this.isBody = isBody;
        this.isValid = isValid;
        this.isInROI = isInROI;
    }
}
