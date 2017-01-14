using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProduitAnim : OSCControllable {

    public Transform[] groups;
    List<Transform> objects;
    List<Vector3> initPos;
    List<Vector3> initRot;
    List<Vector3> initScales;

    
    [Header("Animation")]
    [OSCProperty("upFactor")]
    public float upFactor = .1f;
    [OSCProperty("rotateSpeed")]
    public float rotateSpeed = 100;
    [OSCProperty("rotateRandom")]
    public float rotateRandom = 10;
    [OSCProperty("clicUpY")]
    public float clicUpY = 3;

    [Header("Target")]
    public Transform target;
    [OSCProperty("minDist")]
    public float minDist = 1;
   
    [Header("Details")] 
    [OSCProperty("detachDistOffset")]
    public float detachDistOffset = .1f;
    [OSCProperty("detailsUpFactor")]
    public float detailsUpFactor = 1;
    [OSCProperty("detailsLookAtYOffset")]
    public float detailsLookAtYOffset = 1;

    Transform details;
    ProduitText detailsTF;
    Transform detailsBG;
    Material detailsMat;

    Vector3 initDetailsScale;

    PanierControl panier;
    public int currentIndex = 0;
    public int clickingIndex = -1;
    bool clicking;

    // Use this for initialization
    override public void Start () {
        objects = new List<Transform>();
        initPos = new List<Vector3>();
        initRot = new List<Vector3>();
        initScales = new List<Vector3>();

        foreach (Transform g in groups)
        {
            for(int i=0;i<g.childCount;i++)
            {
                if (g.GetChild(i).name.Contains("socle")) continue;
                objects.Add(g.GetChild(i));
                initPos.Add(g.GetChild(i).transform.position);
                initRot.Add(g.GetChild(i).transform.rotation.eulerAngles);
                initScales.Add(g.GetChild(i).transform.localScale);
            }
        }

        details = transform.FindChild("ProduitDetails").GetComponent<Transform>();
        detailsTF = details.FindChild("TF").GetComponent<ProduitText>();
        detailsBG = details.FindChild("BG");
        detailsMat = detailsBG.GetComponent<MeshRenderer>().sharedMaterial;
        panier = GetComponent<PanierControl>();

        initDetailsScale = details.localScale;
        details.localScale = Vector3.zero;

	}
	
	// Update is called once per frame
	override public void Update () {
        if(!clicking)
        {
            if (currentIndex != -1)
            {
                if (Vector3.Distance(objects[currentIndex].position, target.position) > minDist + detachDistOffset)
                {
                    setCurrentIndex(-1);
                }
                else
                {
                    objects[currentIndex].Rotate(Vector3.up, Time.deltaTime * rotateSpeed);
                    details.LookAt(target.position + Vector3.up * detailsLookAtYOffset);
                }
            }
            else
            {
                int closest = getClosestObject();
                if (Vector3.Distance(objects[closest].position, target.position) < minDist)
                {
                    setCurrentIndex(closest);
                }
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
            objects[currentIndex].DOScale(initScales[currentIndex], .5f);

        }

        currentIndex = index;

        if (currentIndex != -1)
        {
            objects[currentIndex].DOMove(initPos[currentIndex] + Vector3.up * upFactor, .5f).OnComplete(upTweenComplete);
            objects[currentIndex].DORotate(Random.onUnitSphere*rotateRandom, .5f).OnComplete(upTweenComplete);
            details.position = objects[currentIndex].position + Vector3.up * detailsUpFactor;
        }
        else
        {
            showDetails(false);
        }
    }

    [OSCMethod("showDetails")]
    public void showDetails(bool value)
    {
              if (currentIndex == -1) return;
  if (value)
        {
            details.DOScale(Vector3.zero, .3f).SetEase(Ease.InOutQuad).OnComplete(detailsZeroComplete);
            ProduitDescription desc = objects[currentIndex].GetComponent<ProduitDescription>();
            detailsTF.GetComponent<TextMesh>().text = desc != null ? desc.text : detailsTF.baseText;

        }
        else
        {
            details.DOScale(Vector3.zero, .3f).SetEase(Ease.InOutQuad);
        }
        
    }

    public void hideDetails()
    {
        showDetails(false);
    }


    void upTweenComplete()
    {
        objects[currentIndex].DOMove(initPos[currentIndex] + Vector3.up * upFactor / 2, 1).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    void detailsZeroComplete()
    {
        details.DOScale(initDetailsScale, .3f);
    }
    
    [OSCMethod("clicProduit")]
    public void clicProduit(Color startColor, float startScale, float time)
    {
        if (currentIndex == -1 || clicking) return;
        details.localScale = initDetailsScale * startScale;
        details.DOScale(initDetailsScale, time);
        detailsMat.SetColor("_EmissionColor", startColor);
        detailsMat.DOColor(Color.black, "_EmissionColor", time);

        clicking = true;
        clickingIndex = currentIndex;
        Invoke("hideDetails", .5f);
        objects[currentIndex].DOKill();
        objects[clickingIndex].DOMove(objects[clickingIndex].position + Vector3.up * clicUpY,1f).SetDelay(1).OnComplete(clicAnimComplete);
    }

    void clicAnimComplete()
    {
        objects[clickingIndex].position = initPos[clickingIndex];
        objects[clickingIndex].localScale = Vector3.zero;
        objects[clickingIndex].DOScale(initScales[clickingIndex], 1f).OnComplete(finishClicking);

        panier.numItems++;
        clicking = false;
    }

    void finishClicking()
    {
        setCurrentIndex(-1);
    }
}
