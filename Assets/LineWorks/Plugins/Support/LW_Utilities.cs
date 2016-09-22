// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LineWorks {
    /// <summary>
    /// Static utility class that contains a collection of extension methods, helper methods, debugging toggles, and a static material handler.
    /// </summary>
	public static class LW_Utilities {

		//Packing Extensions
		public const int c_Precision = 4096;
		public static float PackVector2(this Vector2 input, float min, float max) {
			float range = max - min;
			float offset = 0 - min;
			input.x = (input.x + offset) / range;
			input.y = (input.y + offset) / range;

			return input.PackVector2();
		}
		public static float PackVector2(this Vector2 input) {
			Vector2 output = input;
			output.x = Mathf.Floor(output.x * (c_Precision - 1));
			output.y = Mathf.Floor(output.y * (c_Precision - 1));

			return (output.x * c_Precision) + output.y;
		}
		public static Vector2 UnpackVector2(this float input, float min, float max) {
			Vector2 output = input.UnpackVector2();

			float range = max - min;
			float offset = 0 - min;
			output.x = (output.x * range) - offset;
			output.y = (output.y * range) - offset;

			return output;
		}
		public static Vector2 UnpackVector2(this float input) {
			Vector2 output = Vector2.zero;
			output.y = input % c_Precision;
			output.x = Mathf.Floor(input / c_Precision);
			output = output / (c_Precision - 1);

			return output;
		}

		public static float PackVector3(this Vector3 normal) {
		    // Scale  Bias values to 8 bit bytes in range of 0 to 255
		    byte x = ConvertByte(normal.x);
		    byte y = ConvertByte(normal.y);
		    byte z = ConvertByte(normal.z);
		 
		    uint  packedByte  = (uint)((x << 16) | (y << 8) | z);
		    float packedFloat = (float)(((double)packedByte) / ((double) (1 << 24)));
		 
		    return packedFloat;
		}
		public static Vector3 UnPackVector3(this float src) {
		 
		    Vector3 output = Vector3.zero;
		 
		    // Unpack to 0...1 range
		    output.x = Frac(src);
		    output.y = Frac(src * 256.0f);
		    output.z = Frac(src * 65536.0f);
		 
		    // Bias to -1..1 range
		    output = (output * 2f) - Vector3.one;
		 
		    return output;
		 
		}
		public static void TestPackVector3(Vector3 normal) {
		 
		    normal.Normalize();
		 
		    float f = PackVector3(normal);
		    Vector3 result = UnPackVector3(f).normalized;
		    Vector3 error  = normal - result;
		 
		    string s = "";
		 
		    s += "Normal: (" + normal.x + ", " + normal.y + ", " + normal.z + ")\n";
		    s += "Result : (" + result.x + ", " + result.y + ", " + result.z + ")\n";
		    s += "Error  : (" + error.x  + ", " + error.y  + ", " + error.z  + ")\n";
		 
		    Debug.Log(s);
		 
		}
		
		public static float DecodeFloatRGBA(this Color c ) {
			Vector4 enc = new Vector4(c.r, c.g, c.b, c.a);
			Vector4 kDecodeDot = new Vector4(1.0f, 1/255.0f, 1/65025.0f, 1/16581375.0f);
			return Vector4.Dot( enc, kDecodeDot );
		}
		public static Color EncodeFloatRGBA(this float v ) {
			Vector4 kEncodeMul = new Vector4(1.0f, 255.0f, 65025.0f, 16581375.0f);
			float kEncodeBit = 1.0f/255.0f;
			//Vector4 kEncodeBit = new Vector4(1.0f/255.0f, 1.0f/255.0f, 1.0f/255.0f, 0.0f);
			Vector4 enc = kEncodeMul * v;
			enc = Frac (enc);
			enc -= new Vector4(enc.y, enc.z, enc.w, enc.w) * kEncodeBit;
			return new Color(enc.x, enc.y, enc.z, enc.w);
		}
		public static void TestPackColor(Vector3 vector) {
			vector.Normalize();
			Color color = new Color(vector.x, vector.y, vector.z, 0);
		 
		    float f = DecodeFloatRGBA(color);
		    Color resultColor = EncodeFloatRGBA(f);
			Vector3 result = new Vector3(resultColor.r, resultColor.g, resultColor.b).normalized;
		    Vector3 error  = vector - result;
		 
		    string s = "";
		 
		    s += "Color: (" + vector.x + ", " + vector.y + ", " + vector.z + ")\n";
		    s += "Result : (" + result.x + ", " + result.y + ", " + result.z + ")\n";
		    s += "Error  : (" + error.x  + ", " + error.y  + ", " + error.z  + ")\n";
		 
		    Debug.Log(s);
		 
		}
		
		public static Color Vector4ToColor(this Vector4 v, float multiplier) {
			return new Color(v.x/multiplier, v.y/multiplier, v.z/multiplier, v.w/multiplier);
		}
		public static Vector4 ColorToVector4(this Color c, float multiplier) {
			return new Vector4(c.r*multiplier, c.g*multiplier, c.b*multiplier, c.a*multiplier);
		}
		
		private static Vector4 Frac(Vector4 v) {
			v.x = Frac(v.x);
			v.y = Frac(v.y);
			v.z = Frac(v.z);
			v.w = Frac(v.w);
			return v;
		}
		private static byte ConvertByte(float x) {
		    x = (x + 1.0f) * 0.5f;   // Bias
		    return (byte)(x*255.0f); // Scale
		}
		private static float Frac(float x) {
		    return x - Mathf.Floor(x);
		}
		
		// Vector Extensions
		public static Vector2[] ToVector2Array(this Vector3[] value) {
			Vector2[] array = new Vector2[value.Length];
			for (int i=0; i<value.Length; i++ ) array[i] = (Vector2)value[i];
			return array;
		}
		public static Vector3[] ToVector3Array(this Vector2[] value) {
			Vector3[] array = new Vector3[value.Length];
			for (int i=0; i<value.Length; i++ ) array[i] = (Vector3)value[i];
			return array;
		}
		
		// Matrix Extensions
		public static Vector3 right(this Matrix4x4 matrix) {
			Vector3 right;
			right.x = (matrix.m00 == matrix.m11) ? matrix.m00 : Mathf.Abs(matrix.m00);
		    right.y = matrix.m10;
		    right.z = matrix.m20;
			return right;
			//return (Vector3)source.GetColumn(0);
		}
		public static Vector3 up(this Matrix4x4 matrix) {
			Vector3 upwards;
		    upwards.x = matrix.m01;
		    upwards.y = (matrix.m00 == matrix.m11) ? matrix.m11 : Mathf.Abs(matrix.m11);
		    upwards.z = matrix.m21;
			return upwards;
			//return (Vector3)source.GetColumn(1);
		}
		public static Vector3 forward(this Matrix4x4 matrix) {
			Vector3 forward;
		    forward.x = matrix.m02;
		    forward.y = matrix.m12;
		    forward.z = matrix.m22;
			return forward;
			//return (Vector3)source.GetColumn(2);
		}
		public static Vector3 position(this Matrix4x4 source) {
			return (Vector3)source.GetColumn(3);
		}
			
		public static string ToString(this Matrix4x4 matrix) {
			string matString = "";
			for (int row=0; row<4; row++) {
				for (int c=0; c<4; c++) {
					matString += matrix[row, c] + " : ";
				}
				matString += "\n";
			}
			return matString;
		}

		public static Vector3 ExtractTranslationFromMatrix(this Matrix4x4 matrix) {
		    Vector3 translate;
		    translate.x = matrix.m03;
		    translate.y = matrix.m13;
		    translate.z = matrix.m23;
		    return translate;
		}
		public static Quaternion ExtractRotationFromMatrix(this Matrix4x4 matrix) {

			// Version 1
			//float angle1 = Mathf.Atan2(-matrix.m01, matrix.m00);
			//float angle2 = Mathf.Atan2(matrix.m10, matrix.m11);
			//Debug.Log("rotation: " + angle1 + " : " + angle2);
			//Quaternion rotation1 = Quaternion.Euler(new Vector3(0,0, angle1));
			//Quaternion rotation2 = Quaternion.Euler(new Vector3(0,0, angle2));

			// Version 2
			//var qw = Mathf.Sqrt(1f + matrix.m00 + matrix.m11 + matrix.m22) / 2;
			//var w = 4 * qw;
			//var qx = (matrix.m21 - matrix.m12) / w;
			//var qy = (matrix.m02 - matrix.m20) / w;
			//var qz = (matrix.m10 - matrix.m01) / w;

			//return new Quaternion(qx, qy, qz, qw);

			// Version 3
			Vector3 forward;
			forward.x = matrix.m02;
			forward.y = matrix.m12;
			forward.z = matrix.m22;

			Vector3 upwards;
			upwards.x = matrix.m01;
			upwards.y = matrix.m11;
			upwards.z = matrix.m21;

			Quaternion rotation = Quaternion.LookRotation(forward, upwards);

		    return rotation;
		}
		public static Vector3 ExtractScaleFromMatrix(this Matrix4x4 matrix) {
		    Vector3 scale;
			// Version 1
			//scale.x = matrix.GetColumn(0).magnitude * Mathf.Sign(matrix.m00);
			//scale.y = matrix.GetColumn(1).magnitude * Mathf.Sign(matrix.m11);
			//scale.z = matrix.GetColumn(2).magnitude * Mathf.Sign(matrix.m22);

			// Version 2
		    //scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude * Mathf.Sign(matrix.m00);
		    //scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude * Mathf.Sign(matrix.m11);
		    //scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude * Mathf.Sign(matrix.m22);

			// Version 3
			scale = new Vector3(
				matrix.GetColumn(0).magnitude,
				matrix.GetColumn(1).magnitude,
				matrix.GetColumn(2).magnitude
			);
			if (Vector3.Cross (matrix.GetColumn (0), matrix.GetColumn (1)).normalized != (Vector3)matrix.GetColumn (2).normalized) {
				scale.x *= -1;
			}
			return scale;

		}

		public static void DecomposeMatrix(this Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale) {
		    localPosition = matrix.ExtractTranslationFromMatrix();
		    localRotation = matrix.ExtractRotationFromMatrix();
		    localScale = matrix.ExtractScaleFromMatrix();
		}
		public static void SetTransformFromMatrix(this Matrix4x4 matrix, Transform transform) {
		    transform.localPosition = matrix.ExtractTranslationFromMatrix();
		    transform.localRotation = matrix.ExtractRotationFromMatrix();
		    transform.localScale = matrix.ExtractScaleFromMatrix();
		}
		 
		public static Matrix4x4 TranslationMatrix(Vector3 offset) {
		    Matrix4x4 matrix = Matrix4x4.identity;
		    matrix.m03 = offset.x;
		    matrix.m13 = offset.y;
		    matrix.m23 = offset.z;
		    return matrix;
		}

		public static float CalcArea(Vector3 a, Vector3 b, Vector3 c) {
			// This is the 2D area
			return Mathf.Abs((a.x * (b.y-c.y) + b.x * (c.y - a.y) + c.x * ( a.y - b.y)) * 0.5f);

			// this is the 3D area
			//float _a = Vector3.Distance(a,b);
			//float _b = Vector3.Distance(b,c);
			//float _c = Vector3.Distance(c,a);
			//float _s = (_a + _b + _c) * 0.5f;
			//return Mathf.Abs(Mathf.Sqrt(_s * (_s-_a) * (_s-_b) * (_s-_c)));
		}
			
		//Destroy
		public static T SafeDestroy<T>(T obj, bool allowDestroyingAssets = false) where T : UnityEngine.Object{
			if(obj != null)	{
				#if UNITY_EDITOR
				if(!AssetDatabase.Contains(obj) || allowDestroyingAssets) {
					//if (obj is LW_Element) (obj as LW_Element).UnregisterChildren();
					//else if (obj is LW_Canvas) (obj as LW_Canvas).UnregisterChildren();
					if(Application.isPlaying) {
						UnityEngine.Object.Destroy(obj);
					}
					else {
						UnityEngine.Object.DestroyImmediate(obj, allowDestroyingAssets);
					}
					return null;
				}
				else return obj;

				#else
				//if(PrefabUtility.GetPrefabType(obj) == PrefabType.None || allowDestroyingAssets) {
					//if (obj is LW_Element) (obj as LW_Element).UnregisterChildren();
					//else if (obj is LW_Canvas) (obj as LW_Canvas).UnregisterChildren();
					UnityEngine.Object.Destroy(obj);
					return null;
				//}
				//else return obj;
				#endif
			}
			else return obj;
		}
	}
}
