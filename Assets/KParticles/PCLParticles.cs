using UnityEngine;
using System.Collections;

public class PCLParticles : MonoBehaviour {

    ParticleSystem ps;
    ParticleSystem.Particle[] particles;

    int[] shuffleIndices;

    public Color bodyColor = Color.white;

    [Range(0,.5f)]
    public float bodySize = .05f;

    public Color bgColor = Color.gray;

    [Range(0,.5f)]
    public float bgSize = .01f;


    public bool doSmooth = true;
    [Range(.01f,10)]
    public float smoothFactor = 2;
    // Use this for initialization

    void Start () {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[0];
        shuffleIndices = new int[0];
    }

    // Update is called once per frame
    void Update () {
        
        int numPoints = KinectPCL.instance.numPoints; 

        PCLPoint[] points = KinectPCL.instance.points;

        if (numPoints != particles.Length)
        {
            particles = new ParticleSystem.Particle[numPoints];
            shuffleIndices = new int[numPoints];
            RandomUnique(shuffleIndices,numPoints);
        }

        int numParticles = ps.GetParticles(particles);

        if (numPoints == 0) return;

        
        if(numPoints > numParticles)
        {
            ParticleSystem.EmitParams parms = new ParticleSystem.EmitParams();
            parms.position = Vector3.zero;
            parms.startSize = 0;
            //  parms.startLifetime = 1;

            ps.Emit(parms,numPoints-numParticles);
            numParticles = ps.GetParticles(particles);
        }
        

        for (int i=0;i<numParticles;i++)
        {
            int index = shuffleIndices[i];

            ParticleSystem.Particle p = particles[index];

            PCLPoint pp = points[i];
            if(pp == null)
            {
                Debug.Log("PP Null !");
                continue;
            }
            if (pp.isValid) continue;

            Vector3 point = transform.TransformPoint(pp.position);
            if (point.magnitude == 0) continue;


            //float age = p.startLifetime-p.lifetime;


            bool pointIsValid = !float.IsNaN(point.x);

           
            Color targetColor = pp.isBody ? bodyColor : bgColor;
            float targetSize = pp.isBody ? bodySize : bgSize;

            if (pointIsValid)
            {
               
                if(doSmooth)
                {
                    float lerpFactor = smoothFactor * Time.deltaTime;

                    p.position = Vector3.Lerp(p.position, point, lerpFactor);
                    p.startColor = Color.Lerp(p.startColor, targetColor, lerpFactor);
                    p.startSize = p.startSize + (targetSize - p.startSize) * lerpFactor;

                }
                else
                {
                    p.position = point;
                    p.startColor = targetColor;
                    p.startSize = targetSize ;
                }
                
                
            }

            
            

            particles[index] = p;
            
        }

        
        ps.SetParticles(particles, numParticles);
        
	}



    private void RandomUnique(int[] obj, int maxCount)
    {
        for (int i = 0; i < maxCount; i++)
        {
            obj[i] = i; 
        }

        Shuffle(shuffleIndices);
    }

    public void Shuffle(int[] obj)
    {
        for (int i = 0; i < obj.Length; i++)
        {
            int temp = obj[i];
            int objIndex = Random.Range(0, obj.Length);
            obj[i] = obj[objIndex];
            obj[objIndex] = temp;
        }
    }
}
