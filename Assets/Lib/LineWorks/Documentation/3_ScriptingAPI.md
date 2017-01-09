
# Scripting API {#scripttut}

# Introduction {#intro}

LineWorks comes with a robust and complete API for creating and modifying LineWork elements via scripts. For detailed documentation of the complete API, see the [Classes Documentation](annotated.html) on this webpage.

# Getting Started {#started}

To work in a Script, 
1.	Use the static `Create` methods to quickly instantiate new %LineWorks objects. 
2.	Use the `Add` instance methods to apply LineWorks elements to the appropriate collections.
3.	Use the `Set` instance methods to modify a LineWorks element or modify an elements properties directly through the property accessors.

> There should be no-need to call any SetDirty() or Rebuild() methods. This is all handled internally.  If for some reason your LineWorks elements are not updating as expected. you can try to call SetElementDirty() on the element and if that doesn't work, you can call ForceTotalRebuild() on the LW_Linework as a last resort. If you are experiencing this issue, please let us know.

Below is a super simple example which will draw a circle with a stroke that pulses in color and width. For more scripting examples see the "Examples" folder within the "LineWorks" folder.

~~~{.cs}
using UnityEngine;
using LineWorks;
using System.Collections.Generic;

public class LW_ExampleScript : MonoBehaviour {
	private LW_Canvas linework;
	private LW_Circle circle;
	private LW_Stroke stroke;
	private Material material;
	void Start () {
		// Create the LineWorks Components and Scriptable Objects.
		linework = LW_Canvas.Create(gameObject, "MyFirstLineWork");
		circle = LW_Circle.Create(Vector2.zero, 30f);
		stroke = LW_Stroke.Create(Color.white, 2f);
		// Adjust the segmenetation to get a smoother looking line.
		linework.segmentation = 20;
		// If you want to use any of the advanced shader features you have to set featureMode to Advanced.
		linework.featureMode = FeatureMode.Advanced;
		// If you would like the stroke to be 3D. ie. always face the camera.
		linework.strokeDrawMode = StrokeDrawMode.Draw3D;
		// If you would like to have the shader provide anti-aliasing
		linework.antiAliasingMode = AntiAliasingMode.On;
		// It is recommended for 3D lines to use the 'Round' Linejoin.
		linework.joinsAndCapsMode = JoinsAndCapsMode.Shader;
		stroke.linejoin = Linejoin.Round;
		// Apply the stroke to the circle and add the circle to the cavas.
		circle.styles.Add(stroke);
		linework.graphic.Add(circle);
	}
	void Update () {
		// While you can modify the width and color of the stroke directly on the LW_Stroke 
		// element, it would cause a Mesh Rebuild that comes with a CPU performance cost. 
		// By getting a reference to the auto-generated material and modify the material's 
		// values, <strong>no CPU overhead</strong> will be required for rebuilding the Mesh. 
		// This is great for dynamic situations where updating the Mesh every frame would 
		// be unreasonable.
		if (linework.materials != null && linework.materials.Count > 0) {
			// We can use the first Material because with only one shape and one style, we 
			// can be pretty sure there is only one Material. Don't confuse 'linework.materials' 
			// with 'linework.material'. 'linework.materials' is a list of all the materials 
			// applied to the MeshRenderer while 'linework.material' is used to override the 
			// auto-generated materials.
			material = linework.materials[0];
			float t = Mathf.PingPong(Time.time, 1f);
			// Ping-Pongs the stroke's Width between 2 and 4. This is multiplied by the 
			// stroke width ie. stroke width(2f) * material width(2f to 4f) = final width;
			material.SetFloat("_Width", Mathf.Lerp(2f, 4f, t));
			// Ping-Pongs the stroke's Color between blue and red. This is multiplied by the 
			// stroke color ie. stroke color(white) * material color(blue to red) = final color.
			material.SetColor("_Color", Color.Lerp(Color.blue, Color.red, t));
		}
		// Spins the LineWork to show the 3D Lines. This is not a LineWorks thing.
		transform.localRotation = Quaternion.Euler(new Vector3(40f,40f,0) * Time.time);
	}
}
~~~

> While Importing SVGs at runtime should be possible, there is a good chance that you might hit a bug or two, let us know if you need this and are having trouble with it. Also, one user has reported that the SVG importer allows for providing a web address as the path (Unintended feature, but pretty cool).
