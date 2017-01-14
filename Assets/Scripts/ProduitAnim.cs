using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProduitAnim : OSCControllable {

    public Transform[] groups;
    List<Transform> objects;
    List<Vector3> initPos;
    List<Vector3> initRot;

    public int currentIndex = 0;

    [Header("Animation")]
    [OSCProperty("upFactor")]
    public float upFactor = .1f;
    [OSCProperty("rotateSpeed")]
    public float rotateSpeed = 100;
    [OSCProperty("rotateRandom")]
    public float rotateRandom = 10;

    [Header("Target")]
    public Transform target;
    [OSCProperty("minDist")]
    public float minDist = 1;
    [OSCProperty("detachDistOffset")]
    public float detachDistOffset = .1f;

    [Header("Details")]
    Transform details;
    ProduitText detailsTF;
    Vector3 initDetailsScale;
    [OSCProperty("detailsUpFactor")]
    public float detailsUpFactor = 1;
    [OSCProperty("detailsLookAtYOffset")]
    public float detailsLookAtYOffset = 1;

	// Use this for initialization
	override public void Start () {
        objects = new List<Transform>();
        initPos = new List<Vector3>();
        initRot = new List<Vector3>();

        foreach (Transform g in groups)
        {
            for(int i=0;i<g.childCount;i++)
            {
                if (g.GetChild(i).name.Contains("socle")) continue;
                objects.Add(g.GetChild(i));
                initPos.Add(g.GetChild(i).transform.position);
                initRot.Add(g.GetChild(i).transform.rotation.eulerAngles);
            }
        }

        details = transform.FindChild("ProduitDetails").GetComponent<Transform>();
        detailsTF = details.FindChild("TF").GetComponent<ProduitText>();
        initDetailsScale = details.localScale;
        details.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	override public void Update () {
        if(currentIndex != -1)
        {
            if (Vector3.Distance(objects[currentIndex].position, target.position) > minDist + detachDistOffset)
            {
                setCurrentIndex(-1);
            }else
            {
                objects[currentIndex].Rotate(Vector3.up, Time.deltaTime*rotateSpeed);
                details.LookAt(target.position+Vector3.up*detailsLookAtYOffset);
            }
        }else
        {
            int closest = getClosestObject();
            if(Vector3.Distance(objects[closest].position, target.position) < minDist)
            {
                setCurrentIndex(closest);
            }
        }
        
	}

    int getClosestObject()
    {
        int index = -1;
        float mDist = 1000;
        int numObjects = objects.Count;
        for(int i =0;i<numObjects;i++)
        {
            Transform t = objects[i];
            float dist = Vector3.Distance(t.position, target.position);
            if(dist < mDist)
            {
                mDist = dist;
                index = i;
            }
        }

        return index;
    }

    void setCurrentIndex(int index)
    {
        if (currentIndex == index) return;
        if(currentIndex != -1)
        {
            objects[currentIndex].DOKill();
            objects[currentIndex].DOMove(initPos[currentIndex], .5f);
            objects[currentIndex].DORotate(initRot[currentIndex], .5f);

        }

        currentIndex = index;

        if (currentIndex != -1)
        {
            details.DOScale(Vector3.zero, .3f).SetEase(Ease.InOutQuad).OnComplete(detailsZeroComplete);
            objects[currentIndex].DOMove(initPos[currentIndex] + Vector3.up * upFactor, .5f).OnComplete(upTweenComplete);
            objects[currentIndex].DORotate(Random.onUnitSphere*rotateRandom, .5f).OnComplete(upTweenComplete);
            ProduitDescription desc = objects[currentIndex].GetComponent<ProduitDescription>();
            detailsTF.GetComponent<TextMesh>().text = desc != null ? desc.text : detailsTF.baseText;
        } else
        {
            details.DOScale(Vector3.zero, .3f).SetEase(Ease.InOutQuad);
        }
    }

    void upTweenComplete()
    {
        objects[currentIndex].DOMove(initPos[currentIndex] + Vector3.up * upFactor / 2, 1).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void detailsZeroComplete()
    {
        details.position = objects[currentIndex].position + Vector3.up * detailsUpFactor;
        details.DOScale(initDetailsScale, .3f);
    }
}
