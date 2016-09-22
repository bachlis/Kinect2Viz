using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class KukuTree {

    public Vector3[] points;
    //public KukuNeighbours[] neighArray;
    public List<KukuLink> links;

    public KukuTree()
    {
        
    }

    public void generateNeighbours(Vector3[] sourcePoints, float distance)
    {

        points = sourcePoints;
        Array.Sort(points, delegate (Vector3 p1, Vector3 p2) {
            return p1.x.CompareTo(p2.x);
        });

        //neighArray = new KukuNeighbours[points.Length];

        links = new List<KukuLink>();

        for (int i=0;i<points.Length-1; i++)
        {

            int j = i+1;
            Vector3 pt = points[i];

            //if(neighArray[i] == null) neighArray[i] = new KukuNeighbours();

            while (points[j].x - pt.x <distance){
                float dist = Vector3.Distance(pt, points[j]);
                if (dist < distance)
                {

                    //neighArray[i].addNeigh(j);
                    links.Add(new KukuLink(points[i], points[j],dist));

                    // if (neighArray[j] == null) neighArray[j] = new KukuNeighbours();
                    // neighArray[j].addNeigh(i);
                }
                j++;
            }
        }
    }

    

}

public class KukuLink
{
    public Vector3 start;
    public Vector3 end;
    public float distance;
    public KukuLink(Vector3 _start, Vector3 _end, float dist)
    {
        start = _start;
        end = _end;
        distance = dist;
    }
}

/*
public class KukuNeighbours
{
    List<int> neighList;

    public KukuNeighbours()
    {
        neighList = new List<int>();
    }

    public void addNeigh(int val)
    {
        neighList.Add(val);
    }
}*/