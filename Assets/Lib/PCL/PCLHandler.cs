using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
public class PCLHandler : MonoBehaviour {

   
    public enum DataTarget { Kinect1_1, Kinect1_2, Kinect1_3, Kinect2, RealSense };
    public DataTarget dataTarget;

    [Range(1, 50)]
    public int steps = 4;
    public Color color;


    public bool drawMesh;

    Mesh m;
    MeshFilter mf;
    PCLDataReceiver receiver;

    public Vector3[] points { get; private set; }
    public int[] positions { get; private set; }
    public int numGoodPoints;
    public int numTotalPoints;

    // Use this for initialization
    void Start () {
        m = new Mesh();
        mf = GetComponent<MeshFilter>();
        mf.mesh = m;

        receiver = GetComponentInParent<PCLDataReceiver>();
        switch (dataTarget)
        {
            case DataTarget.Kinect1_1:
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                break;

            case DataTarget.Kinect1_2:
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                break;

            case DataTarget.Kinect1_3:
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                break;

            case DataTarget.Kinect2:
                numTotalPoints = PCLConstants.NUM_K2_PIXELS;
                break;

            case DataTarget.RealSense:
                numTotalPoints = PCLConstants.NUM_RS_PIXELS;
                break;
        }

    }

    // Update is called once per frame
    void Update () {

       

        if (receiver.data.k1Clouds == null) return;
        switch (dataTarget)
        {
            case DataTarget.Kinect1_1:
                points = receiver.data.k1Clouds[0].points;
                positions = receiver.data.k1Clouds[0].positions;
                numGoodPoints = receiver.data.k1Clouds[0].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                break;
            case DataTarget.Kinect1_2:
                points = receiver.data.k1Clouds[1].points;
                positions = receiver.data.k1Clouds[1].positions;
                numGoodPoints = receiver.data.k1Clouds[1].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                break;
            case DataTarget.Kinect1_3:
                points = receiver.data.k1Clouds[2].points;
                positions = receiver.data.k1Clouds[2].positions;
                numGoodPoints = receiver.data.k1Clouds[2].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                break;

            case DataTarget.Kinect2:
                points = receiver.data.k2Cloud.points;
                positions = receiver.data.k2Cloud.positions;
                numGoodPoints = receiver.data.k2Cloud.numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K2_PIXELS;
                break;
            case DataTarget.RealSense:
                points = receiver.data.rsCloud.points;
                positions = receiver.data.rsCloud.positions;
                numGoodPoints = receiver.data.rsCloud.numGoodPoints;
                numTotalPoints = PCLConstants.NUM_RS_PIXELS;
                break;
        }
    }
}
