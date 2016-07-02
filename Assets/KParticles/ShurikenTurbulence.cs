using UnityEngine;
using System.Collections;



[ExecuteInEditMode]
public class ShurikenTurbulence : MonoBehaviour {

	ParticleSystem.Particle[] particles;
	
	public float amplitude = 1;
	public float frequency = 1;
	public float evolutionSpeed = 0;
	float evolution = 0;
	
	public float sizeFactor = 1;
	public float positionFactor = 1;
	
	public bool drawFields;
	
	PerlinNoise perlinX;
	PerlinNoise perlinY;
	PerlinNoise perlinZ;
	// Use this for initialization
	void Start () {
		perlinX = new PerlinNoise(0);
		perlinY = new PerlinNoise(1);
		perlinZ = new PerlinNoise(2);
		
		particles = new ParticleSystem.Particle[GetComponent<ParticleSystem>().maxParticles];
	}
	
	// Update is called once per frame
	void LateUpdate () {
		int numParticles = GetComponent<ParticleSystem>().GetParticles(particles);
		
		for(int i=0;i<numParticles;i++)
		{
			
			Vector3 p3d = getAffectedPerlin3D(particles[i].position);
			
			//if(i < 10) Debug.Log (p3d);
			particles[i].velocity += p3d  * positionFactor;
			particles[i].startSize = GetComponent<ParticleSystem>().startSize + p3d.magnitude * amplitude * sizeFactor / 100;
		
		}
		
		evolution += (evolutionSpeed * Time.deltaTime);
		
		GetComponent<ParticleSystem>().SetParticles(particles,numParticles);
	}
	
	void Update()
	{
		
		if(drawFields)
		{
			float step = .05f;
			for(int i = -150;i<150;i++)
			{
				float curStep = step*i;
				float nextStep = step*(i+1);
				Vector3 x1 = Vector3.right*curStep+getAffectedPerlin3D(Vector3.right*curStep);
				Vector3 x2 = Vector3.right*nextStep+getAffectedPerlin3D(Vector3.right*nextStep);
				Debug.DrawRay(x1,x2-x1,Color.red);
				Vector3 y1 = Vector3.up*curStep+getAffectedPerlin3D(Vector3.up*curStep);
				Vector3 y2 = Vector3.up*nextStep+getAffectedPerlin3D(Vector3.up*nextStep);
				Debug.DrawRay(y1,y2-y1,Color.green);
				Vector3 z1 = Vector3.forward*curStep+getAffectedPerlin3D(Vector3.forward*curStep);
				Vector3 z2 = Vector3.forward*nextStep+getAffectedPerlin3D(Vector3.forward*nextStep);
				Debug.DrawRay(z1,z2-z1,Color.blue);
			}
		}
	}
	
	Vector3 getPerlin3D(Vector3 pos)
	{
		float f1 = perlinX.FractalNoise3D(pos.x,pos.y,pos.z,1,1/frequency,amplitude);//Mathf.PerlinNoise(pos.x,pos.y) -.5f;
		float f2 = perlinY.FractalNoise3D(pos.x,pos.y,pos.z,1,1/frequency,amplitude);//Mathf.PerlinNoise(pos.x,pos.y) -.5f;
		float f3 = perlinZ.FractalNoise3D(pos.x,pos.y,pos.z,1,1/frequency,amplitude);//Mathf.PerlinNoise(pos.x,pos.y) -.5f;
		
		return new Vector3(f1,f2,f3) ;
	}
	
	Vector3 getAffectedPerlin3D(Vector3 pos)
	{
		return getPerlin3D(pos + Vector3.one*evolution);
	}
}
