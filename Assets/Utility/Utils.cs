using UnityEngine;

public class Utils {
	
	public static void RandomizeBuiltinArray(int[] arr)
	{
		for (var i = arr.Length - 1; i > 0; i--) {
			var r = Random.Range(0,i);
			var tmp = arr[i];
			arr[i] = arr[r];
			arr[r] = tmp;
		}
	}
	
	public static Vector3 getRandomVector(float min,float max, float elevation = 1f)
	{
		return new Vector3(Random.Range(min,max),Random.Range(min,max)*elevation,Random.Range(min,max));
	}
}
