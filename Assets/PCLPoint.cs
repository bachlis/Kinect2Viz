using UnityEngine;

public class PCLPoint  {

    public Vector3 position;

    public bool isBody;
    public bool isValid;

    public PCLPoint(Vector3 position, bool isBody, bool isValid)
    {
        this.position = position;
        this.isBody = isBody;
        this.isValid = isValid;
    }
}
