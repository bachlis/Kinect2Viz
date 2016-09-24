using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PCLHandler : MonoBehaviour {

   
    public enum DataTarget { Kinect1_1, Kinect1_2, Kinect1_3, Kinect2, RealSense };
    public DataTarget dataTarget;
    
    PCLDataReceiver receiver;

    public Vector3[] points { get; private set; }
    public int[] positions { get; private set; }
    public Vector3[] quads { get; private set; }
    public Vector2[] uvs { get; private set; }
    public int numQuads;

    public int numGoodPoints;
    public int numTotalPoints;
    public Vector3 pclCenter;
    public int numBodiesTracked;

    public bool debugCenter;

    public Vector3[] joints;

    // Use this for initialization
    void Start () {
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

        numBodiesTracked = 0;
        numQuads = 0;

        joints = new Vector3[14];
        //For debug
        for (int i = 0; i < joints.Length; i++) joints[i] = Random.insideUnitSphere * 3;
    }

    // Update is called once per frame
    void Update () {
        if (receiver.data.k1Clouds == null) return;
        //if (receiver.data.isReady == 0) return;

        switch (dataTarget)
        {
            case DataTarget.Kinect1_1:
                points = receiver.data.k1Clouds[0].points;
                positions = receiver.data.k1Clouds[0].positions;
                numGoodPoints = receiver.data.k1Clouds[0].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                pclCenter = receiver.data.k1Clouds[0].pclCenter;
                break;

            case DataTarget.Kinect1_2:
                points = receiver.data.k1Clouds[1].points;
                positions = receiver.data.k1Clouds[1].positions;
                numGoodPoints = receiver.data.k1Clouds[1].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                pclCenter = receiver.data.k1Clouds[1].pclCenter;
                break;

            case DataTarget.Kinect1_3:
                points = receiver.data.k1Clouds[2].points;
                positions = receiver.data.k1Clouds[2].positions;
                numGoodPoints = receiver.data.k1Clouds[2].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                pclCenter = receiver.data.k1Clouds[2].pclCenter;
                break;

            case DataTarget.Kinect2:
                points = receiver.data.k2Cloud.points;
                positions = receiver.data.k2Cloud.positions;
                numGoodPoints = receiver.data.k2Cloud.numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K2_PIXELS;
                pclCenter = transform.TransformPoint(receiver.data.k2Cloud.pclCenter);
                numBodiesTracked = receiver.data.k2Cloud.numBodiesTracked;

                joints[0] = receiver.data.k2Cloud.headPos;
                joints[1] = receiver.data.k2Cloud.neckPos;
                joints[2] = receiver.data.k2Cloud.torsoPos;
                joints[3] = receiver.data.k2Cloud.leftHandPos;
                joints[4] = receiver.data.k2Cloud.rightHandPos;

                quads = (Vector3[])receiver.data.k2Cloud.quads.Clone();
                uvs = (Vector2[])receiver.data.k2Cloud.uvs.Clone();
                numQuads = receiver.data.k2Cloud.numQuads;
                break;

            case DataTarget.RealSense:
                points = receiver.data.rsCloud.points;
                positions = receiver.data.rsCloud.positions;
                numGoodPoints = receiver.data.rsCloud.numGoodPoints;
                numTotalPoints = PCLConstants.NUM_RS_PIXELS;
                pclCenter = receiver.data.rsCloud.pclCenter;
                break;
        }

        
    }

    void OnDrawGizmos()
    {
        if (debugCenter)
        {
            Gizmos.color = new Color(1, .5f, 0);
            Gizmos.DrawWireSphere(pclCenter, .1f);
        }
    }
}
