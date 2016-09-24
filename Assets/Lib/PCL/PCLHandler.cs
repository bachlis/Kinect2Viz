﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PCLHandler : MonoBehaviour {


    public enum DataTarget { Kinect1_1, Kinect1_2, Kinect1_3, Kinect2, RealSense };
    public DataTarget dataTarget;

    PCLDataReceiver receiver;

    public Vector3[] points { get; private set; }
    public int[] goodPointIndices { get; private set; }
    public int[] quadIndices { get; private set; }
    public int numQuads;

    public int numGoodPoints;

    public int numTotalPoints;
    public int pclWidth {get; private set;}
    public int pclHeight { get; private set; }

    public Vector3 pclCenter;
    public int numBodiesTracked;

    public bool debugCenter;

    public Vector3 headPos;
    public Vector3 neckPos;
    public Vector3 torsoPos;
    public Vector3 leftHandPos;
    public Vector3 rightHandPos;

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
    }

    // Update is called once per frame
    void Update () {
        if (receiver.data.k1Clouds == null) return;
        //if (receiver.data.isReady == 0) return;

        switch (dataTarget)
        {
            case DataTarget.Kinect1_1:
                points = receiver.data.k1Clouds[0].points;
                goodPointIndices = receiver.data.k1Clouds[0].goodPointIndices;
                numGoodPoints = receiver.data.k1Clouds[0].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                pclCenter = receiver.data.k1Clouds[0].pclCenter;
                pclWidth = PCLConstants.K1_PCL_WIDTH;
                pclHeight = PCLConstants.K1_PCL_HEIGHT;
                break;

            case DataTarget.Kinect1_2:
                points = receiver.data.k1Clouds[1].points;
                goodPointIndices = receiver.data.k1Clouds[1].goodPointIndices;
                numGoodPoints = receiver.data.k1Clouds[1].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                pclCenter = receiver.data.k1Clouds[1].pclCenter;
                pclWidth = PCLConstants.K1_PCL_WIDTH;
                pclHeight = PCLConstants.K1_PCL_HEIGHT;
                break;

            case DataTarget.Kinect1_3:
                points = receiver.data.k1Clouds[2].points;
                goodPointIndices = receiver.data.k1Clouds[2].goodPointIndices;
                numGoodPoints = receiver.data.k1Clouds[2].numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K1_PIXELS;
                pclCenter = receiver.data.k1Clouds[2].pclCenter;
                pclWidth = PCLConstants.K1_PCL_WIDTH;
                pclHeight = PCLConstants.K1_PCL_HEIGHT;
                break;

            case DataTarget.Kinect2:
                points = receiver.data.k2Cloud.points;
                goodPointIndices = receiver.data.k2Cloud.goodPointIndices;
                numGoodPoints = receiver.data.k2Cloud.numGoodPoints;
                numTotalPoints = PCLConstants.NUM_K2_PIXELS;
                pclCenter = transform.TransformPoint(receiver.data.k2Cloud.pclCenter);
                numBodiesTracked = receiver.data.k2Cloud.numBodiesTracked;

                headPos = receiver.data.k2Cloud.headPos;
                neckPos = receiver.data.k2Cloud.neckPos;
                torsoPos = receiver.data.k2Cloud.torsoPos;
                leftHandPos = receiver.data.k2Cloud.leftHandPos;
                rightHandPos = receiver.data.k2Cloud.rightHandPos;

                quadIndices = receiver.data.k2Cloud.quadIndices;
                numQuads = receiver.data.k2Cloud.numQuads;
                pclWidth = PCLConstants.K2_PCL_WIDTH;
                pclHeight = PCLConstants.K2_PCL_HEIGHT;
                break;

            case DataTarget.RealSense:
                points = receiver.data.rsCloud.points;
                goodPointIndices = receiver.data.rsCloud.goodPointIndices;
                numGoodPoints = receiver.data.rsCloud.numGoodPoints;
                numTotalPoints = PCLConstants.NUM_RS_PIXELS;
                pclCenter = receiver.data.rsCloud.pclCenter;
                pclWidth = PCLConstants.RS_PCL_WIDTH;
                pclHeight = PCLConstants.RS_PCL_HEIGHT;
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
