// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

#if UNITY_EDITOR || (!UNITY_WEBPLAYER && !UNITY_WEBGL)
using System.Xml;
using System.Xml.Linq;
#endif

namespace LineWorks {
	
	#if UNITY_EDITOR
	/// <summary>
	/// Asset import post processor for handling the importing of SVG assets.
	/// </summary>
	public class LW_SVGPostProcessor : AssetPostprocessor {
        static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

			List<string> svgPaths = new List<string>();
			
			for (int i=0; i<importedAssets.Length; i++) {
				string path = importedAssets[i]; 
				if (Path.GetExtension(path) == ".svg") {
					svgPaths.Add(path);
				}
            }

			if(svgPaths.Count > 0) {
				for (int i=0; i<svgPaths.Count; i++) {
					string path = svgPaths[i];                
	                float progress = (float)i/(float)svgPaths.Count;
					
					if(EditorUtility.DisplayCancelableProgressBar("LineWorks - Importing", "Importing " + path,	progress)) break;
					else {
						#if UNITY_EDITOR || (!UNITY_WEBPLAYER && !UNITY_WEBGL)
						LW_SVGParser.ImportSVGFromPath(path);
						#endif
					}
	            }
			}
			
            EditorUtility.ClearProgressBar();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Canvas.ForceUpdateCanvases();
			SceneView.RepaintAll();
        }
	}
	#endif
	
	#if UNITY_EDITOR || (!UNITY_WEBPLAYER && !UNITY_WEBGL)
	/// <summary>
	/// This is a static class for SVG Importing.
	/// </summary>
	public static class LW_SVGParser {
		#if UNITY_EDITOR || DEVELOPMENT
		private static bool s_DebugSVGImporter = false;
		#endif

		private static Dictionary<string, string> m_StyleTextDefs = new Dictionary<string, string>();
		private static Dictionary<string, XElement> m_SymbolDefs = new Dictionary<string, XElement>();
		private static Dictionary<string, XElement> m_GradientDefs = new Dictionary<string, XElement>();
		private static Dictionary<string, XElement> m_MarkerDefs = new Dictionary<string, XElement>();
		private static Color m_CurrentColor = Color.white;
		private static XNamespace xlinkNamespace = "http://www.w3.org/1999/xlink";
		//private static XNamespace svgNamespace = "http://www.w3.org/2000/svg";
		private static int m_SortOrder = 0;
		#if UNITY_EDITOR 
		private static string m_CurrAssetPath = "";
		#endif

		/// <summary>
		/// Imports the SVG from path.
		/// </summary>
		/// <param name="svgPath">Svg path.</param>
		public static void ImportSVGFromPath(string svgPath) {
			string extension = System.IO.Path.GetExtension(svgPath);
			if (extension == ".svg") {
				m_StyleTextDefs = new Dictionary<string, string>();
				m_SymbolDefs = new Dictionary<string, XElement>();
				m_GradientDefs = new Dictionary<string, XElement>();
				m_MarkerDefs = new Dictionary<string, XElement>();
				LW_Fill fill = ScriptableObject.CreateInstance<LW_Fill>();
				LW_Stroke stroke = ScriptableObject.CreateInstance<LW_Stroke>();
				LW_Marker marker = ScriptableObject.CreateInstance<LW_Marker>();
				fill.isVisible  = true;
				stroke.isVisible = false;
				marker.isVisible = false;
				fill.gradientColors = new List<LW_ColorStop>(){new LW_ColorStop(Color.black, 0)};
				stroke.gradientColors = null;

				string name = System.IO.Path.GetFileNameWithoutExtension(svgPath);
				#if UNITY_EDITOR 
				string path = Path.GetDirectoryName(svgPath);
				m_CurrAssetPath = path + "/" + name + ".prefab";
				#endif
				
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.ProhibitDtd = false;
				XmlReader reader = XmlReader.Create(svgPath, settings);
				XDocument xmlDoc = XDocument.Load(reader);
				XElement xRootElement = xmlDoc.Root;
				xlinkNamespace = xRootElement.GetNamespaceOfPrefix("xlink");
				//svgNamespace = xRootElement.GetNamespaceOfPrefix("svg");

				ParseElement(xRootElement, fill, stroke, marker, name);
				Resources.UnloadUnusedAssets();
			}
			else {
				Debug.Log("Provided Path does not have a SVG extension. File cannot be imported.");
			}
		}

		private static void ParseElement(XElement xElement, LW_Fill fill, LW_Stroke stroke, LW_Marker marker, string name = "", LW_Group container = null, GameObject context = null) {
			switch(xElement.Name.LocalName) {
			case "defs":
			case "style":
			case "symbol":
			case "marker":
			case "linearGradient":
			case "radialGradient":
				ParseDefsElement(xElement);
				break;
			case "svg":
			case "circle":
			case "ellipse":
			case "line":
			case "polygon":
			case "polyline":
			case "rect":
			case "path":
			case "g":
			case "use":
				ParseGraphicElement(xElement, fill, stroke, marker, name, container, context);
				break;
			case "mask":
			case "clipPath":
				Debug.Log("<" + xElement.Name.LocalName + "> Clipping elements are not currently implemented.");
				break;
			case "metadata":
			case "desc":
			case "title":
				Debug.Log("<" + xElement.Name.LocalName + "> Descriptive elements are not currently implemented.");
				break;
			case "animate":
			case "animateColor":
			case "animateMotion":
			case "animateTransform":
			case "mpath":
			case "set":
				Debug.Log("<" + xElement.Name.LocalName + "> Animation elements are not currently implemented.");
				break;
			case "feBlend":
			case "feColorMatrix":
			case "feComponentTransfer":
			case "feComposite":
			case "feConvolveMatrix":
			case "feDiffuseLighting":
			case "feDisplacementMap":
			case "feFlood":
			case "feFuncA":
			case "feFuncB":
			case "feFuncG":
			case "feFuncR":
			case "feGaussianBlur":
			case "feImage":
			case "feMerge":
			case "feMergeNode":
			case "feMorphology":
			case "feOffset":
			case "feSpecularLighting":
			case "feTile":
			case "feTurbulence":
			case "feDistantLight":
			case "fePointLight":
			case "feSpotLight":
			case "filter":
				Debug.Log("<" + xElement.Name.LocalName + "> Filter elements are not currently implemented.");
				break;
			case "font":
			case "font-face":
			case "font-face-format":
			case "font-face-name":
			case "font-face-src":
			case "font-face-uri":
			case "hkern":
			case "vkern":
				Debug.Log("<" + xElement.Name.LocalName + "> Font elements are not currently implemented.");
				break;
			case "altGlyph":
			case "altGlyphDef":
			case "altGlyphItem":
			case "glyph":
			case "glyphRef":
			case "missing-glyph":
			case "textPath":
			case "text":
			case "tref":
			case "tspan":
				Debug.Log("<" + xElement.Name.LocalName + "> Text elements are not currently implemented.");
				break;
			case "a":
				Debug.Log("<" + xElement.Name.LocalName + "> Hyperlink elements are not currently implemented.");
				break;
			case "pattern":
				Debug.Log("<" + xElement.Name.LocalName + "> Pattern elements are not currently implemented.");
				break;
			case "switch":
				Debug.Log("<" + xElement.Name.LocalName + "> Switch elements are not currently implemented.");
				break;
			case "image":
				Debug.Log("<" + xElement.Name.LocalName + "> Image elements are not currently implemented.");
				break;
			case "color-profile":
				Debug.Log("<" + xElement.Name.LocalName + "> Color Profile elements are not currently implemented.");
				break;
			case "cursor":
				Debug.Log("<" + xElement.Name.LocalName + "> Cursor elements are not currently implemented.");
				break;
			case "foreignObject":
				Debug.Log("<" + xElement.Name.LocalName + "> External object elements are not currently implemented.");
				break;
			case "script":
				Debug.Log("<" + xElement.Name.LocalName + "> Script elements are not currently implemented.");
				break;
			case "view":
				Debug.Log("<" + xElement.Name.LocalName + "> View elements are not currently implemented.");
				break;
			}
		}

		private static void ParseDefsElement(XElement xElement) {
			switch(xElement.Name.LocalName) {
			case "defs":
				foreach (XElement xChildElement in xElement.Elements()) ParseDefsElement(xChildElement);
				break;
			case "style":
				StoreStyleElement(xElement);
				break;
			case "symbol":
				StoreSymbolElement(xElement);
				break;
			case "marker":
				StoreMarkerElement(xElement);
				break;
			case "linearGradient":
			case "radialGradient":
				StoreGradientElement(xElement);
				break;
			}
		}
		private static void StoreStyleElement(XElement xElement) {
			if (xElement.HasAttributes && xElement.Attribute("type") != null && xElement.Attribute("type").Value == "text/css") {
				CssParser parser = new CssParser();
				var rules = parser.ParseAll(xElement.Value);
				foreach (var rule in rules) {
					StringBuilder builder = new StringBuilder();
					foreach (var declaration in rule.Declarations) {
						builder.Append(declaration.Property.Trim()).Append("=").Append(declaration.Value.Trim()).Append(" "); 
					}
					foreach (string selector in rule.Selectors) {
						m_StyleTextDefs.Add(selector.Trim('.').ToLower(), builder.ToString());
					}
				}
			}
			else Debug.Log("Non-CSS style defs are currently not supported.");
		}
		private static void StoreSymbolElement(XElement xElement) {
			string id = xElement.Attribute("id") != null ? xElement.Attribute("id").Value : xElement.Name.LocalName;
			m_SymbolDefs.Add(id.ToLower(), xElement);
		}
		private static void StoreMarkerElement(XElement xElement) {
			string id = xElement.Attribute("id") != null ? xElement.Attribute("id").Value : xElement.Name.LocalName;
			m_MarkerDefs.Add(id.ToLower(), xElement);
		}
		private static void StoreGradientElement(XElement xElement) {
			string id = xElement.Attribute("id") != null ? xElement.Attribute("id").Value : xElement.Name.LocalName;
			m_GradientDefs.Add(id.ToLower(), xElement);
		}

		private static void ParseGraphicElement(XElement xElement, LW_Fill fill, LW_Stroke stroke, LW_Marker marker, string name = "", LW_Group container = null, GameObject context = null) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugSVGImporter) Debug.Log("ParseGraphicElement: " + xElement.Name.LocalName);
			#endif

			LW_Graphic graphic = null;
			GameObject svgGO = context;
			LW_Canvas vectorCanvas = null;

			float width, height, x, y, rx, ry, radius;
			Vector2 center, start, end, size;
			bool isClosed = false;
			Vector2[] points;

			width = height = 100;
			x = y = rx = ry = radius = 0;
			center = start = end = size = Vector2.zero;

			switch(xElement.Name.LocalName) {
			case "svg":
				string goName = string.IsNullOrEmpty(name) ? "SVG" : name;
				svgGO = new GameObject(goName);
				vectorCanvas = svgGO.AddComponent<LW_Canvas>();
				//graphic = LW_Group.Create();
				//vectorCanvas.graphic = graphic as LW_Group;
				vectorCanvas.sortingOrder = m_SortOrder++;
				graphic = vectorCanvas.graphic;

				if (context != null) {
					svgGO.transform.SetParent(context.transform);
					vectorCanvas.rectTransform.anchoredPosition = Vector2.zero;
					vectorCanvas.rectTransform.anchorMax = Vector2.one;
					vectorCanvas.rectTransform.anchorMin = Vector2.zero;
					vectorCanvas.rectTransform.offsetMax = Vector2.zero;
					vectorCanvas.rectTransform.offsetMin = Vector2.zero;
					vectorCanvas.rectTransform.pivot = new Vector2(0.5f,0.5f);
					vectorCanvas.rectTransform.sizeDelta = Vector2.zero;
				}

				if (xElement.Attribute("width") != null && xElement.Attribute("height") != null) {
					width = xElement.Attribute("width").Value.ToFloat();
					height = xElement.Attribute("height").Value.ToFloat();
					vectorCanvas.rectTransform.sizeDelta = new Vector2(width, height);
					//Update this to set anchor positions if pecentages;
				}
				if (xElement.Attribute("x") != null && xElement.Attribute("y") != null) {
					x = xElement.Attribute("x").Value.ToFloat();
					y = -xElement.Attribute("y").Value.ToFloat();
					vectorCanvas.rectTransform.anchoredPosition = new Vector2(x,y);
					//Update this to set anchor positions if pecentages;
				}
				if (xElement.Attribute("viewBox") != null) {
					string numberRegex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
					string[] splitArgs = Regex.Matches(xElement.Attribute("viewBox").Value, numberRegex).OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
					float[] rectArguments = splitArgs.Select(arg => float.Parse(arg)).ToArray();
					center = new Vector2(rectArguments[0]+rectArguments[2]/2f, rectArguments[1]-rectArguments[3]/2f);
					size = new Vector2(rectArguments[2], rectArguments[3]);
					vectorCanvas.viewBox = new Rect(center.x, center.y, size.x, size.y);
				}
				else {
					center = new Vector2(x+width/2f, y-height/2f);
					size = new Vector2(width, height);
					vectorCanvas.viewBox = new Rect(center.x, center.y, size.x, size.y);
				}
				if (xElement.Attribute("preserveAspectRatio") != null) {
					string[] arguments = xElement.Attribute("preserveAspectRatio").Value.ToLower().Split(' ');
					if (arguments.Length == 2) {
						// Set Pivot Location
						string[] pivotAlignments = arguments[0].ToString().TrimStart('x').Split('y');
						if (pivotAlignments.Length == 2) {
							Vector2 pivot = new Vector2(0.5f, 0.5f);
							switch(pivotAlignments[0]) {
							case "min":
								pivot.x = 0;
								break;
							case "mid":
								pivot.x = 0.5f;
								break;
							case "max":
								pivot.x = 1;
								break;
							}
							switch(pivotAlignments[1]) {
							case "min":
								pivot.y = 1;
								break;
							case "mid":
								pivot.y = 0.5f;
								break;
							case "max":
								pivot.y = 0;
								break;
							}
							vectorCanvas.rectTransform.pivot = pivot;
						}
						// Set Scale Mode
						switch(arguments[1].ToString()) {
						case "meet":
							vectorCanvas.scaleMode = ScaleMode.ScaleToFit;
							break;
						case "slice":
							vectorCanvas.scaleMode = ScaleMode.ScaleToFill;
							break;
						case "none":
							vectorCanvas.scaleMode = ScaleMode.StretchToFill;
							break;
						}
					}
				}

				break;
			case "g":
				graphic = LW_Group.Create();
				break;
			case "use":
				break;
			case "rect" :
				width = (xElement.Attribute("width") != null) ? xElement.Attribute("width").Value.ToFloat() : 100;
				height = (xElement.Attribute("height") != null) ? xElement.Attribute("height").Value.ToFloat() : 100;
				rx = (xElement.Attribute("rx") != null) ? xElement.Attribute("rx").Value.ToFloat() : 0;
				ry = (xElement.Attribute("ry") != null) ? xElement.Attribute("ry").Value.ToFloat() : 0;
				center = (xElement.Attribute("x") != null && xElement.Attribute("y") != null) ? 
					new Vector2(xElement.Attribute("x").Value.ToFloat()+width/2f, -xElement.Attribute("y").Value.ToFloat()-height/2f) : 
					new Vector2(width/2f, -height/2f);
				graphic = LW_Rectangle.Create(center, width, height, rx, ry);
				break;
			case "circle" :
				center = (xElement.Attribute("cx") != null && xElement.Attribute("cy") != null) ?
					new Vector2(xElement.Attribute("cx").Value.ToFloat(), -xElement.Attribute("cy").Value.ToFloat()) :
					Vector2.zero;
				radius = (xElement.Attribute("r") != null) ? xElement.Attribute("r").Value.ToFloat() : 40;
				graphic = LW_Circle.Create(center, radius);
				break;
			case "ellipse" :
				center = (xElement.Attribute("cx") != null && xElement.Attribute("cy") != null) ?
					new Vector2(xElement.Attribute("cx").Value.ToFloat(), -xElement.Attribute("cy").Value.ToFloat()) :
					Vector2.zero;
				rx = (xElement.Attribute("rx") != null) ? xElement.Attribute("rx").Value.ToFloat() : 0;
				ry = (xElement.Attribute("ry") != null) ? xElement.Attribute("ry").Value.ToFloat() : 0;
				graphic = LW_Ellipse.Create(center, rx, ry);
				break;
			case "line" :
				start = (xElement.Attribute("p1") != null) ?
					new Vector2(xElement.Attribute("x1").Value.ToFloat(), -xElement.Attribute("y1").Value.ToFloat()) :
					Vector2.zero;
				end = (xElement.Attribute("p2") != null) ?
					new Vector2(xElement.Attribute("x2").Value.ToFloat(), -xElement.Attribute("y2").Value.ToFloat()) :
					Vector2.zero;
				graphic = LW_Line.Create(start, end);
				break;
			case "polygon" :
				isClosed = true;
				points = xElement.Attribute("points").Value.ToVector2Array();
				if (points.Length > 2 && points[0] == points[points.Length-1]) {
					System.Array.Resize(ref points, points.Length-1);
				}
				graphic = LW_Polyline2D.Create(points, isClosed);
				break;
			case "polyline" :
				isClosed = false;
				points = xElement.Attribute("points").Value.ToVector2Array();
				if (points.Length > 2 && points[0] == points[points.Length-1]) {
					System.Array.Resize(ref points, points.Length-1);
					isClosed = true;
				}
				graphic = LW_Polyline2D.Create(points, isClosed);
				break;
			case "path" :
				List<LW_Graphic> pathsList = new List<LW_Graphic>();
				if (xElement.HasAttributes && xElement.Attribute("d") != null) pathsList = ParsePaths(xElement, "d");
				if (pathsList.Count == 1) graphic = pathsList[0];
				else if (pathsList.Count > 1) graphic = LW_Group.Create(pathsList);
				break;
			}

			if (graphic != null) {
				graphic.name = ParseName(xElement);

				if (xElement.Attribute("transform") != null) graphic.transform = ParseTransform(xElement.Attribute("transform").Value); 

				ParseStyle(xElement, graphic, fill, stroke, marker);

				if (graphic is LW_Group) {
					LW_Group group = graphic as LW_Group;
					foreach (XElement xChildElement in xElement.Elements()) {
						LW_Fill childFill = fill.Copy() as LW_Fill;
						LW_Stroke childStroke = stroke.Copy() as LW_Stroke;
						LW_Marker childMarker = marker.Copy() as LW_Marker;
						ParseElement(xChildElement, childFill, childStroke, childMarker, "", group, svgGO);
					}
				}

				if (xElement.Name.LocalName != "svg") {
					if (container != null) {
						container.Add(graphic);
					}
				}
				else {
					if (context != null) {
						svgGO.transform.SetParent(context.transform);
					}
					else {
						#if UNITY_EDITOR
						if (!Application.isPlaying) {
							Object prefab = AssetDatabase.LoadAssetAtPath(m_CurrAssetPath, typeof(GameObject));
							if (prefab == null) {
								prefab = PrefabUtility.CreateEmptyPrefab(m_CurrAssetPath);
							}
							else {
								Object[] assets = AssetDatabase.LoadAllAssetsAtPath(m_CurrAssetPath) as Object[];
								for(int i=0; i<assets.Length; i++) {
									if (assets[i] is LW_Element) UnityEngine.Object.DestroyImmediate(assets[i], true);
								}
							}

							vectorCanvas.graphic.SaveToAssetDatabase(prefab);
							LW_Canvas[] lineworks = svgGO.GetComponentsInChildren<LW_Canvas>();
							for (int i=0; i<lineworks.Length; i++) {
								if (lineworks[i] != vectorCanvas) lineworks[i].graphic.SaveToAssetDatabase(prefab);
							}

							prefab = PrefabUtility.ReplacePrefab(svgGO, prefab, ReplacePrefabOptions.ReplaceNameBased);

							UnityEngine.Object.DestroyImmediate(svgGO);
						}
						#endif
					}
				}
			}
		}
		private static List<LW_Graphic> ParsePaths (XElement xElement, string attributeName) {
			List<LW_Graphic> pathsList = new List<LW_Graphic>();
			if (xElement.HasAttributes && xElement.Attribute(attributeName) != null) {
				bool isClosed = false;
				List<LW_Point2D> pointsList = new List<LW_Point2D>();

				Vector2 pt1 = Vector2.zero, cp1 = Vector2.zero, cp2 = Vector2.zero, pt2 = Vector2.zero;		
				string command = "", prevCommand = "";
				float[] arguments = new float[0];
				string path = xElement.Attribute(attributeName).Value;
				string separators = @"(?=[MZLHVCSQTAmzlhvcsqta])";

				var tokens = Regex.Split(path, separators).Where(t => !string.IsNullOrEmpty(t));
				//int firstPointIndex = 0;
				LW_Point2D firstPoint = new LW_Point2D();
				LW_Point2D lastPoint = new LW_Point2D();
				LW_Point2D currPoint = new LW_Point2D();

				foreach (string token in tokens){

					command = token.Take(1).Single().ToString();
					string remainingargs = token.Substring(1);
					string numberRegex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
					string[] splitArgs = Regex.Matches(remainingargs, numberRegex).OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
					arguments = splitArgs.Select(arg => float.Parse(arg)).ToArray();

					switch(command) {
					case "M" : 
						if (arguments.Length > 1) {
							if (pointsList.Count > 0) {
								if (!isClosed && (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint)) pointsList.Add(lastPoint);
								if (pointsList.Count > 1 && isClosed && (pointsList[0].position - pointsList[pointsList.Count-1].position).sqrMagnitude < 0.01f) {
									if (pointsList[pointsList.Count-1].hasHandleIn) {
										firstPoint = pointsList[0];
										firstPoint.handleIn = pointsList[pointsList.Count-1].handleIn;
										pointsList[0] = firstPoint;
									}
									pointsList.Remove(pointsList[pointsList.Count-1]);
								}
								LW_Path2D shape = ScriptableObject.CreateInstance<LW_Path2D>();
								shape.Set(pointsList.ToArray(), isClosed);
								pathsList.Add(shape);
							}
							pointsList.Clear();
							isClosed = false;

							currPoint = new LW_Point2D();

							pt2.x = arguments[0];
							pt2.y = -(arguments[1]);

							currPoint.position = pt2;

							firstPoint = currPoint;
							lastPoint = currPoint;

							pt1 = pt2;

							if (arguments.Length > 2 && arguments.Length % 2 == 0) {
								for(int i=2; i<arguments.Length; i+=2){
									currPoint = new LW_Point2D();

									pt2.x = arguments[i+0];
									pt2.y = -arguments[i+1];

									currPoint.position = pt2;
									if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
									lastPoint = currPoint;

									pt1 = pt2;
								}
							}
						}
						break;
					case "m" : 
						if (arguments.Length > 1) {
							if (pointsList.Count > 0) {
								if (!isClosed && (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint)) pointsList.Add(lastPoint);
								if (pointsList.Count > 1 && isClosed && (pointsList[0].position - pointsList[pointsList.Count-1].position).sqrMagnitude < 0.01f) {
									if (pointsList[pointsList.Count-1].hasHandleIn) {
										firstPoint = pointsList[0];
										firstPoint.handleIn = pointsList[pointsList.Count-1].handleIn;
										pointsList[0] = firstPoint;
									}
									pointsList.Remove(pointsList[pointsList.Count-1]);
								}
								LW_Path2D shape = ScriptableObject.CreateInstance<LW_Path2D>();
								shape.Set(pointsList.ToArray(), isClosed);
								pathsList.Add(shape);
							}
							pointsList.Clear();
							isClosed = false;

							currPoint = new LW_Point2D();

							pt2.x = pt1.x + arguments[0];
							pt2.y = pt1.y - arguments[1];

							currPoint.position = pt2;

							firstPoint = currPoint;
							lastPoint = currPoint;

							pt1 = pt2;

							if (arguments.Length > 2 && arguments.Length % 2 == 0) {
								for(int i=2; i<arguments.Length; i+=2){
									currPoint = new LW_Point2D();

									pt2.x = pt1.x + arguments[i+0];
									pt2.y = pt1.y - arguments[i+1];

									currPoint.position = pt2;
									if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
									lastPoint = currPoint;

									pt1 = pt2;

									prevCommand = command;
								}
							}
						}
						break;

					case "C" : 
						if (arguments.Length % 6 == 0) {
							for(int i=0; i<arguments.Length; i+=6){
								currPoint = new LW_Point2D();

								cp1.x = arguments[i+0];
								cp1.y = -arguments[i+1];
								cp2.x = arguments[i+2];
								cp2.y = -arguments[i+3];
								pt2.x = arguments[i+4];
								pt2.y = -arguments[i+5];

								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "c" : 
						if (arguments.Length % 6 == 0) {
							for(int i=0; i<arguments.Length; i+=6){
								currPoint = new LW_Point2D();

								cp1.x = pt1.x + arguments[i+0];
								cp1.y = pt1.y - arguments[i+1];
								cp2.x = pt1.x + arguments[i+2];
								cp2.y = pt1.y - arguments[i+3];
								pt2.x = pt1.x + arguments[i+4];
								pt2.y = pt1.y - arguments[i+5];

								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "L" : 
						if (arguments.Length % 2 == 0) {	
							for(int i=0; i<arguments.Length; i+=2){
								currPoint = new LW_Point2D();

								pt2.x = arguments[i+0];
								pt2.y = -arguments[i+1];

								currPoint.position = pt2;
								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "l" :
						if (arguments.Length % 2 == 0) {
							for(int i=0; i<arguments.Length; i+=2){
								currPoint = new LW_Point2D();

								pt2.x = pt1.x + arguments[i+0];
								pt2.y = pt1.y - arguments[i+1];

								currPoint.position = pt2;
								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "V" : 
						for(int i=0; i<arguments.Length; i++){
							currPoint = new LW_Point2D();

							pt2.y = -arguments[i];

							currPoint.position = pt2;
							if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
							lastPoint = currPoint;

							pt1 = pt2;

							prevCommand = command;
						}
						break;

					case "v" : 
						for(int i=0; i<arguments.Length; i++){
							currPoint = new LW_Point2D();

							pt2.y = pt1.y - arguments[i];

							currPoint.position = pt2;
							if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
							lastPoint = currPoint;

							pt1 = pt2;

							prevCommand = command;
						}
						break;

					case "H" : 
						for(int i=0; i<arguments.Length; i++){
							currPoint = new LW_Point2D();

							pt2.x = arguments[i];

							currPoint.position = pt2;
							if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
							lastPoint = currPoint;

							pt1 = pt2;

							prevCommand = command;
						}
						break;

					case "h" : 
						for(int i=0; i<arguments.Length; i++){
							currPoint = new LW_Point2D();

							pt2.x = pt1.x + arguments[i];

							currPoint.position = pt2;
							if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
							lastPoint = currPoint;

							pt1 = pt2;

							prevCommand = command;
						}
						break;

					case "S" : 
						if (arguments.Length % 4 == 0) {
							for(int i=0; i<arguments.Length; i+=4){
								currPoint = new LW_Point2D();

								cp2.x = arguments[i+0];
								cp2.y = -arguments[i+1];
								pt2.x = arguments[i+2];
								pt2.y = -arguments[i+3];

								if (prevCommand == "S" || prevCommand == "s" || prevCommand == "C" || prevCommand == "c") { 
									cp1 = -lastPoint.handleIn+lastPoint.position;
									lastPoint.pointType = PointType.Symetric;
								}
								else cp1 = lastPoint.position;

								//lastPoint.handleOut = -lastPoint.handleIn;
								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;
					case "s" : 
						if (arguments.Length % 4 == 0) {
							for(int i=0; i<arguments.Length; i+=4){
								currPoint = new LW_Point2D();

								cp2.x = pt1.x + arguments[i+0];
								cp2.y = pt1.y - arguments[i+1];
								pt2.x = pt1.x + arguments[i+2];
								pt2.y = pt1.y - arguments[i+3];

								if (prevCommand == "S" || prevCommand == "s" || prevCommand == "C" || prevCommand == "c") { 
									cp1 = -lastPoint.handleIn+lastPoint.position;
									lastPoint.pointType = PointType.Symetric;
								}
								else cp1 = lastPoint.position;

								//lastPoint.handleOut = -lastPoint.handleIn;
								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;
					case "Q" : 
						if (arguments.Length % 4 == 0) {
							for(int i=0; i<arguments.Length; i+=4){
								currPoint = new LW_Point2D();

								cp2.x = arguments[i+0];
								cp2.y = -arguments[i+1];
								pt2.x = arguments[i+2];
								pt2.y = -arguments[i+3];
								cp1 = cp2;

								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "q" : 
						if (arguments.Length % 4 == 0) {
							for(int i=0; i<arguments.Length; i+=4){
								currPoint = new LW_Point2D();

								cp1.x = pt1.x + arguments[i+0];
								cp1.y = pt1.y - arguments[i+1];
								pt2.x = pt1.x + arguments[i+2];
								pt2.y = pt1.y - arguments[i+3];
								cp2 = cp1;

								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "T" : 
						if (arguments.Length % 2 == 0) {
							for(int i=0; i<arguments.Length; i+=4){
								currPoint = new LW_Point2D();

								pt2.x = arguments[i+0];
								pt2.y = -arguments[i+1];
								if (prevCommand == "Q" || prevCommand == "q" || prevCommand == "T" || prevCommand == "t") {
									cp1.x = pt1.x + (pt1.x - cp1.x);
									cp1.y = pt1.y - (pt1.y - cp1.y);
								}
								else cp1 = pt1;
								cp2 = cp1;

								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "t" : 
						if (arguments.Length % 2 == 0) {
							for(int i=0; i<arguments.Length; i+=4){
								currPoint = new LW_Point2D();

								pt2.x = pt1.x + arguments[i+0];
								pt2.y = -(pt1.y + arguments[i+1]);
								if (prevCommand == "Q" || prevCommand == "q" || prevCommand == "T" || prevCommand == "t") {
									cp1.x = pt1.x + (pt1.x - cp1.x);
									cp1.y = pt1.y - (pt1.y - cp1.y);
								}
								else cp1 = pt1;
								cp2 = cp1;

								if (cp1 != lastPoint.position) lastPoint.handleOut = cp1-lastPoint.position;
								currPoint.position = pt2;
								if (cp2 != currPoint.position) currPoint.handleIn = cp2-currPoint.position;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "A" : 
						if (arguments.Length % 7 == 0) {
							for(int i=0; i<arguments.Length; i+=7){
								currPoint = new LW_Point2D();

								//float rx = arguments[i+0];
								//float ry = arguments[i+1];
								//float rotation = arguments[i+2];
								//int largeArcFlag = (int)arguments[i+3];
								//int sweepFlag = (int)arguments[i+4];

								pt2.x = arguments[i+5];
								pt2.y = -arguments[i+6];

								currPoint.position = pt2;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "a" : 
						if (arguments.Length % 7 == 0) {
							for(int i=0; i<arguments.Length; i+=7){
								currPoint = new LW_Point2D();

								//float rx = arguments[i+0];
								//float ry = arguments[i+1];
								//float rotation = arguments[i+2];
								//int largeArcFlag = (int)arguments[i+3];
								//int sweepFlag = (int)arguments[i+4];

								pt2.x = pt1.x + arguments[i+5];
								pt2.y = pt1.y - arguments[i+6];

								currPoint.position = pt2;

								if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
								lastPoint = currPoint;

								pt1 = pt2;

								prevCommand = command;
							}
						}
						break;

					case "Z" :
						isClosed = true;
						if ((lastPoint.position - firstPoint.position).sqrMagnitude < 0.01f) {
							if (lastPoint.hasHandleIn) {
								firstPoint = pointsList[0];
								firstPoint.handleIn = lastPoint.handleIn;
								pointsList[0] = firstPoint;
							}
						}
						else {
							if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
						}

						pt1 = pt2;

						prevCommand = command;
						break;

					case "z" :
						isClosed = true;
						if ((lastPoint.position - firstPoint.position).sqrMagnitude < 0.01f) {
							if (lastPoint.hasHandleIn) {
								firstPoint = pointsList[0];
								firstPoint.handleIn = lastPoint.handleIn;
								pointsList[0] = firstPoint;
							}
						}
						else {
							if (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint) pointsList.Add(lastPoint);
						}

						pt1 = pt2;

						prevCommand = command;
						break;

					default : 
						break;
					}
				}
				if (pointsList.Count > 0) {
					if (!isClosed && (pointsList.Count == 0 || pointsList[pointsList.Count-1] != lastPoint)) pointsList.Add(lastPoint);
					if (pointsList.Count > 1 && isClosed && (pointsList[0].position - pointsList[pointsList.Count-1].position).sqrMagnitude < 0.01f) {
						if (pointsList[pointsList.Count-1].hasHandleIn) {
							firstPoint = pointsList[0];
							firstPoint.handleIn = pointsList[pointsList.Count-1].handleIn;
							pointsList[0] = firstPoint;
						}
						pointsList.Remove(pointsList[pointsList.Count-1]);
					}
					LW_Path2D shape = ScriptableObject.CreateInstance<LW_Path2D>();
					shape.Set(pointsList.ToArray(), isClosed);
					pathsList.Add(shape);
				}
				pointsList.Clear();
				isClosed = false;
			}
			return pathsList;
		}
		private static string  ParseName (XElement xElement) {
			if (xElement.Attribute("id") != null) return xElement.Attribute("id").Value;
			return xElement.Name.LocalName;
		}
		private static Matrix4x4 ParseTransform (string transformString) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugSVGImporter) Debug.Log("ParseTransform: " + transformString);
			#endif
			
			Matrix4x4 transformMatrix = Matrix4x4.identity;
			Dictionary<string, float[]> argumentDictionary = new Dictionary<string, float[]>();

			string[] transformations = transformString.Trim().Split(')');
			if (transformations.Length > 0) {
				for (int i=0; i<transformations.Length; i++){
					string[] transformation = transformations[i].Trim().Split('(');
					if (transformation.Length == 2) {
						string name = transformation[0];

						string numberRegex = @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?";
						string[] splitArgs = Regex.Matches(transformation[1], numberRegex).OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
						float[] arguments = splitArgs.Select(arg => float.Parse(arg)).ToArray();
						if (arguments.Length > 0) {
							argumentDictionary.Add(name, arguments);
						}
					}
				}

			}
			/*
			//string[] matches = Regex.Matches(transformString, @"/(\w+\((\-?\d+\.?\d*e?\-?\d*,?)+\))+/g").OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
			string[] matches = Regex.Matches(transformString, @"(\w+\((\-?\d+\.?\d*[\s,]?)+\))+").OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
			Debug.Log("matches " + matches.Length);
			for (int i=0; i<matches.Length; i++) {
				string match = matches[i];
				string[] tokens = Regex.Matches(match, @"[\w\.\-]+").OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
				string name = "";
				float[] arguments = new float[0];
				if (tokens.Length > 0) {
					name = tokens[0];
					arguments = new float[tokens.Length-1];
					for (int a=0; a<arguments.Length; a++) arguments[a] = float.Parse(tokens[a+1]);
				}
				Debug.Log(name + " : " + match);
				argumentDictionary.Add(name, arguments);
			}
			*/

			for (int i=argumentDictionary.Count-1; i>-1; i--) {
				KeyValuePair<string, float[]> kvp = argumentDictionary.ElementAt(i);
				Matrix4x4 matrix = Matrix4x4.identity;
				switch(kvp.Key) {
				case "matrix":
					if (kvp.Value.Length > 0) matrix.m00 = kvp.Value[0];
					if (kvp.Value.Length > 1) matrix.m10 = -kvp.Value[1];
					if (kvp.Value.Length > 2) matrix.m01 = -kvp.Value[2];
					if (kvp.Value.Length > 3) matrix.m11 = kvp.Value[3];
					if (kvp.Value.Length > 4) matrix.m03 = kvp.Value[4];
					if (kvp.Value.Length > 5) matrix.m13 = -kvp.Value[5];
					transformMatrix *= matrix;
					break;
				case "translate":
					if (kvp.Value.Length > 0) matrix.m03 = kvp.Value[0];
					if (kvp.Value.Length > 1) matrix.m13 = -kvp.Value[1];
					transformMatrix *= matrix;
					break;
				case "rotate":
					Quaternion rot = Quaternion.identity;
					Vector3 pos = Vector3.zero;
					if (kvp.Value.Length > 0) rot = Quaternion.Euler(new Vector3(0,0, -kvp.Value[0]));
					if (kvp.Value.Length > 1) pos.x += kvp.Value[1];
					if (kvp.Value.Length > 2) pos.y -= kvp.Value[2];
					matrix *= Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
					matrix *= Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
					matrix *= Matrix4x4.TRS(-pos, Quaternion.identity, Vector3.one);
					transformMatrix *= matrix;
					break;
				case "scale":
					if (kvp.Value.Length > 0) matrix.m00 = matrix.m11 = kvp.Value[0];
					if (kvp.Value.Length == 2) matrix.m11 = kvp.Value[1];
					transformMatrix *= matrix;
					break;
				case "skewX":
					if (kvp.Value.Length == 1) matrix[0,1] = Mathf.Tan(Mathf.Deg2Rad * kvp.Value[1]);
					transformMatrix *= matrix;
					break;
				case "skewY":
					if (kvp.Value.Length == 1) matrix[1,0] = -Mathf.Tan(Mathf.Deg2Rad * kvp.Value[1]);
					transformMatrix *= matrix;
					break;
				}
			}
			return transformMatrix;
		}
		private static void ParseStyle(XElement xElement, LW_Graphic graphic, LW_Fill fill, LW_Stroke stroke, LW_Marker marker) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugSVGImporter) Debug.Log("ParseStyle: " + xElement.Name.LocalName);
			#endif

			// Apply Attribute Styles
			if (HasStyleAttribute(xElement)) {
				foreach (XAttribute attribute in xElement.Attributes()) {
					ParseStyleDeclaration(attribute.Name.LocalName, attribute.Value, fill, stroke, marker);
				}
			}

			// Apply Inline Styles
			if (xElement.Attribute("style") != null) {
				Dictionary<string, string> styleDictionary = new Dictionary<string, string>();
				string styleCSS = xElement.Attribute("style").Value.Replace(" ","");
				string[] ccs = styleCSS.ToLower().Split(';');
				foreach (string s in ccs) {
					string[] kvp = s.Split(':');
					if (kvp.Length == 2) styleDictionary.Add(kvp[0], kvp[1]);
				}
				foreach (KeyValuePair<string, string> declaration in styleDictionary) {
					ParseStyleDeclaration(declaration.Key, declaration.Value, fill, stroke, marker);
				}
			}

			// Apply CSS Styles
			if (xElement.Attribute("class") != null) {
				string[] classes = xElement.Attribute("class").Value.Trim().Split(' ');
				StringBuilder styleTextBuilder = new StringBuilder();
				foreach (string cls in classes) {
					string defName = cls.ToLower();
					if (m_StyleTextDefs.ContainsKey(defName)) styleTextBuilder.Append(m_StyleTextDefs[defName]);
				}
				if (styleTextBuilder.Length > 0) {
					string[] declarations = styleTextBuilder.ToString().Split(' ') as string[];
					foreach (string declarationString in declarations) {
						string[] declaration = declarationString.Split('=');
						if (declaration.Length == 2) {
							ParseStyleDeclaration(declaration[0], declaration[1], fill, stroke, marker);
						}
					}
				}
			}

			// Add Styles to Graphic
			switch(xElement.Name.LocalName) {
			case "circle":
			case "ellipse":
			case "line":
			case "polygon":
			case "polyline":
			case "rect":
			case "path":
				if (fill.isVisible) graphic.styles.Add(fill);
				if (stroke.isVisible) graphic.styles.Add(stroke);
				if (marker.isVisible) graphic.styles.Add(marker);
				break;
			}
		}
		private static bool HasStyleAttribute(XElement xElement) {
			bool hasStyle = false;
			foreach (XAttribute attribute in xElement.Attributes()) {
				switch (attribute.Name.LocalName) {
				case "opacity":
				case "fill":
				case "fill-opacity":
				case "fill-rule":
				case "stroke":
				case "stroke-opacity":
				case "stroke-width":
				case "stroke-linecap":
				case "stroke-linejoin":
				case "stroke-Miterlimit":
				case "marker":
				case "marker-start":
				case "marker-mid":
				case "marker-end":
					hasStyle = true;
					break;
				}
			}
			return hasStyle;
		}
		private static void ParseStyleDeclaration (string property, string value, LW_Fill fill, LW_Stroke stroke, LW_Marker marker) {
			switch (property) {
			case "color":
				m_CurrentColor = value.ToColor();
				break;
			case "opacity":
				fill.opacity = value.ToFloat();
				stroke.opacity = value.ToFloat();
				break;
			case "fill":
				if (value == "none") {
					fill.isVisible = false;
					//fill.gradientColors = null;
				}
				else if (value.StartsWith("currentColor")) {
					fill.isVisible = true;
					fill.color = m_CurrentColor;
				}
				else if (value.StartsWith("url(#")) {
					fill.isVisible = true;
					string defName = value.Replace("url(#","").TrimEnd(')').ToLower();
					if (m_GradientDefs.ContainsKey(defName)) ParseGradient(m_GradientDefs[defName], fill);
				}
				else {
					fill.isVisible = true;
					fill.color = value.ToColor();
				}
				break;
			case "fill-opacity":
				fill.opacity = value.ToFloat();
				break;
			case "fill-rule":
				fill.fillRule = value.ToEnum<FillRule>();
				break;
			case "stroke":
				if (value == "none") {
					stroke.isVisible = false;
					//stroke.gradientColors = null;
				}
				else if (value.StartsWith("currentColor")) {
					stroke.isVisible = true;
					stroke.color = m_CurrentColor;
				}
				else if (value.StartsWith("url(#")) {
					stroke.isVisible = true;
					string defName = value.TrimStart("url(#".ToCharArray()).TrimEnd(')');
					if (m_GradientDefs.ContainsKey(defName)) ParseGradient(m_GradientDefs[defName], stroke);
				}
				else {
					stroke.isVisible = true;
					stroke.color = value.ToColor();
				}
				break;
			case "stroke-opacity":
				stroke.opacity = value.ToFloat();
				break;
			case "stroke-width":
				stroke.globalWidth = value.ToFloat();
				break;
			case "stroke-linecap":
				stroke.linecap = value.ToEnum<Linecap>();
				break;
			case "stroke-linejoin":
				stroke.linejoin = value.ToEnum<Linejoin>();
				break;
			case "stroke-Miterlimit":
				stroke.miterLimit = value.ToFloat();
				break;
			case "marker":
				if (value == "none") {
					marker.graphic = null;
				}
				else if (value.StartsWith("url(#")) {
					string defName = value.TrimStart("url(#".ToCharArray()).TrimEnd(')').ToLower();
					if (m_MarkerDefs.ContainsKey(defName)) ParseMarker(m_MarkerDefs[defName], marker);
					marker.atStart = true;
					marker.atMiddle = true;
					marker.atEnd = true;
				}
				break;
			case "marker-start":
				if (value == "none") marker.graphic = null;
				else if (value.StartsWith("url(#")) {
					string defName = value.TrimStart("url(#".ToCharArray()).TrimEnd(')').ToLower();
					if (m_MarkerDefs.ContainsKey(defName)) ParseMarker(m_MarkerDefs[defName], marker);
					marker.atStart = true;;
				}
				break;
			case "marker-mid":
				if (value == "none") marker.graphic = null;
				else if (value.StartsWith("url(#")) {
					string defName = value.TrimStart("url(#".ToCharArray()).TrimEnd(')').ToLower();
					if (m_MarkerDefs.ContainsKey(defName)) ParseMarker(m_MarkerDefs[defName], marker);
					marker.atMiddle = true;
				}
				break;
			case "marker-end":
				if (value == "none") marker.graphic = null;
				else if (value.StartsWith("url(#")) {
					string defName = value.TrimStart("url(#".ToCharArray()).TrimEnd(')').ToLower();
					if (m_MarkerDefs.ContainsKey(defName)) ParseMarker(m_MarkerDefs[defName], marker);
					marker.atEnd = true;
				}
				break;
			}
		}
		private static void	ParseGradient(XElement xElement, LW_PaintStyle style) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugSVGImporter) Debug.Log("ParseGradient: " + xElement.Name.LocalName);
			#endif
			
			style.gradientColors = new List<LW_ColorStop>();
			if (xElement.Attribute(xlinkNamespace + "href") != null) {
				string link = xElement.Attribute(xlinkNamespace + "href").Value.TrimStart('#').ToLower();
				if (m_GradientDefs.ContainsKey(link)) ParseGradient(m_GradientDefs[link], style);
			}

			if (xElement.Attribute("gradientUnits") != null) style.gradientUnits = (GradientUnits)xElement.Attribute("gradientUnits").Value.ToEnum<GradientUnits>();
			if (xElement.Attribute("spreadMethod") != null) style.gradientSpreadMethod = (SpreadMethod)xElement.Attribute("spreadMethod").Value.ToEnum<SpreadMethod>();
			if (xElement.Attribute("gradientTransform") != null) style.gradientTransform = ParseTransform(xElement.Attribute("gradientTransform").Value);

			switch(xElement.Name.LocalName) {
			case "linearGradient":
				style.paintMode = PaintMode.LinearGradient;
				if (xElement.Attribute("x1") != null && xElement.Attribute("y1") != null) {
					style.gradientStart = new Vector2(xElement.Attribute("x1").Value.ToFloat(), -xElement.Attribute("y1").Value.ToFloat());
				}
				if (xElement.Attribute("x2") != null && xElement.Attribute("y2") != null) {
					style.gradientEnd = new Vector2(xElement.Attribute("x2").Value.ToFloat(), -xElement.Attribute("y2").Value.ToFloat());
				}
				break;
			case "radialGradient":
				style.paintMode = PaintMode.RadialGradient;
				if (xElement.Attribute("cx") != null && xElement.Attribute("cy") != null) style.gradientStart = new Vector2(xElement.Attribute("cx").Value.ToFloat(), style.gradientUnits == GradientUnits.userSpaceOnUse ? -xElement.Attribute("cy").Value.ToFloat() : 1-xElement.Attribute("cy").Value.ToFloat());
				if (xElement.Attribute("r") != null) style.gradientEnd = new Vector2(style.gradientStart.x + xElement.Attribute("r").Value.ToFloat(), style.gradientStart.y);
				//if (xElement.Attribute("fx") != null && xElement.Attribute("fy") != null) {}
				break;
			}

			foreach (XElement xChildElement in xElement.Elements()) {
				if (xChildElement.Name.LocalName == "stop") {
					LW_ColorStop stop = ParseGradientStop(xChildElement);
					//Debug.Log("Stop: " + stop.color + " : " + stop.percentage);
					style.AddStop(stop);
				}
			}
		}
		private static LW_ColorStop ParseGradientStop(XElement xElement) {
			//Debug.Log("ParseGradientStop: " + xElement.Name.LocalName);
			LW_ColorStop colorStop = new LW_ColorStop(Color.white, 0);

			foreach (XAttribute attribute in xElement.Attributes()) {
				ParseStopDeclaration(ref colorStop, attribute.Name.LocalName, attribute.Value);
			}

			return colorStop;
		}
		private static LW_ColorStop ParseStopDeclaration (ref LW_ColorStop colorStop, string property, string value) {
			//Debug.Log("ParseStopDeclaration: " + property + " : " + value);
			switch (property) {
			case "style":
				Dictionary<string, string> stopDictionary = new Dictionary<string, string>();
				string stopCSS = value.Replace(" ","");

				string[] ccs = stopCSS.ToLower().Split(';');
				foreach (string s in ccs) {
					string[] kvp = s.Split(':');
					if (kvp.Length == 2) stopDictionary.Add(kvp[0], kvp[1]);
				}
				foreach (KeyValuePair<string, string> declaration in stopDictionary) {
					colorStop = ParseStopDeclaration(ref colorStop, declaration.Key, declaration.Value);
				}
				break;
			case "offset":
				colorStop.percentage = value.ToFloat();
				break;
			case "stop-color":
				if (value == "none") colorStop.color = Color.clear;
				else {
					colorStop.color = value.ToColor();
				}
				break;
			case "stop-opacity":
				Color color = colorStop.color;
				color.a = value.ToFloat();
				colorStop.color = color;
				break;
			}
			return colorStop;
		}
		private static void ParseMarker(XElement xElement, LW_Marker marker) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugSVGImporter) Debug.Log("ParseMarker: " + xElement.Name.LocalName);
			#endif
//			Vector2 center = Vector2.zero, start = Vector2.zero, end = Vector2.zero;
//			float width = 0, height = 0, rx = 0, ry = 0, radius = 0;
//			
//			switch(xElement.Name.LocalName) {
//			case "marker" :
//				if (xElement.Attribute("width") != null) width = xElement.Attribute("width").Value.ToFloat();
//				if (xElement.Attribute("height") != null) height = xElement.Attribute("height").Value.ToFloat();
//				if (xElement.Attribute("rx") != null) rx = xElement.Attribute("rx").Value.ToFloat();
//				if (xElement.Attribute("ry") != null) ry = xElement.Attribute("ry").Value.ToFloat();
//				if (xElement.Attribute("x") != null && xElement.Attribute("y") != null) {
//					center = new Vector2(xElement.Attribute("x").Value.ToFloat()+width/2f, -xElement.Attribute("y").Value.ToFloat()-height/2f);
//				}
//				else {
//					center = new Vector2(width/2f, -height/2f);
//				}
//				break;
//			}
		}
	}
	#endif
	
	internal static class LW_SVGExtensions  {
		public static Vector2[] ToVector2Array(this string value) {
			List<Vector2> vectorList = new List<Vector2>();
			string[] stringArray = value.Replace("\n", " ").TrimEnd(' ').TrimStart(' ').Split(' ');
			if (stringArray.Length > 1) {
				for (int i=0; i<stringArray.Length; i++) {
					if (stringArray[i].Length > 2) {
						string[] splitPoint = stringArray[i].ToString().Trim().Split(',');
						if (splitPoint.Length == 2) {
							Vector2 newPoint = new Vector2(float.Parse(splitPoint[0]), -float.Parse(splitPoint[1]));
							if (vectorList.Count == 0 || vectorList[vectorList.Count-1] != newPoint) vectorList.Add(newPoint);
						}
					}
				}
			}
			return vectorList.ToArray();
		}
		public static Vector3[] ToVector3Array(this string value) {
			List<Vector3> vectorList = new List<Vector3>();
			string[] stringArray = value.Replace("\n", " ").TrimEnd(' ').TrimStart(' ').Split(' ');
			if (stringArray.Length > 1) {
				for (int i=0; i<stringArray.Length; i++) {
					if (stringArray[i].Length > 2) {
						string[] splitPoint = stringArray[i].ToString().Trim().Split(',');
						if (splitPoint.Length == 2) {
							Vector3 newPoint = new Vector3(float.Parse(splitPoint[0]), -float.Parse(splitPoint[1]));
							if (vectorList.Count == 0 || vectorList[vectorList.Count-1] != newPoint) vectorList.Add(newPoint);
						}
					}
				}
			}
			return vectorList.ToArray();
		}
		public static float ToFloat(this string value) {
			value = value.Replace(" ", "");
			if (value.EndsWith("%")) {
				return float.Parse(value.TrimEnd('%'))/100f;
			}
			else {
				value = value.Contains("e-") || value.Contains("e+") ? value : Regex.Replace(value, "[A-Za-z ]", "");
				return float.Parse(value);
			}
		}
		public static T ToEnum<T> (this string value) {
	    	return (T) System.Enum.Parse(typeof(T), value, true);
		}
		public static Matrix4x4 ToMatrix (this string value) {
			Matrix4x4 transform = Matrix4x4.identity;
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			Vector3 scale = Vector3.one;
		    Dictionary<string, float[]> argumentDictionary = new Dictionary<string, float[]>();
			string[] matches = Regex.Matches(value, @"(\w+\((\-?\d+\.?\d*[\s,]?)+\))+").OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
		    for (int i=0; i<matches.Length; i++) {
				string match = matches[i];
				string[] tokens = Regex.Matches(match, @"[\w\.\-]+").OfType<Match>().Select(m => m.Groups[0].Value).ToArray();
				string name = "";
				float[] arguments = new float[0];
				if (tokens.Length > 0) {
					name = tokens[0];
					arguments = new float[tokens.Length-1];
					for (int a=0; a<arguments.Length; a++) arguments[a] = float.Parse(tokens[a+1]);
				}
				argumentDictionary.Add(name, arguments);
		    }
			//foreach (KeyValuePair<string, float[]> kvp in argumentDictionary) {
			for (int i=0; i<argumentDictionary.Count; i++) {
			//for (int i=argumentDictionary.Count-1; i>=0; i--) {
				KeyValuePair<string, float[]> kvp = argumentDictionary.ElementAt(i);
				switch(kvp.Key) {
				case "matrix":
					////Debug.Log("matrix");
					Matrix4x4 matrix = Matrix4x4.identity;
					if (kvp.Value.Length > 0) matrix.m00 = kvp.Value[0];
					if (kvp.Value.Length > 1) matrix.m10 = -kvp.Value[1];
					if (kvp.Value.Length > 2) matrix.m01 = -kvp.Value[2];
					if (kvp.Value.Length > 3) matrix.m11 = kvp.Value[3];
					if (kvp.Value.Length > 4) matrix.m03 = kvp.Value[4];
					if (kvp.Value.Length > 5) matrix.m13 = -kvp.Value[5];
					transform *= matrix;
					break;
				case "translate":
					////Debug.Log("translate");
					position = Vector3.zero;
					if (kvp.Value.Length > 0) position.x += kvp.Value[0];
					if (kvp.Value.Length > 1) position.y -= kvp.Value[1];
					transform *= Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
					break;
				case "rotate":
					////Debug.Log("rotate");
					rotation = Quaternion.identity;
					position = Vector3.zero;
					if (kvp.Value.Length > 0) rotation = Quaternion.Euler(new Vector3(0,0, -kvp.Value[0]));
					if (kvp.Value.Length > 1) position.x += kvp.Value[1];
					if (kvp.Value.Length > 2) position.y -= kvp.Value[2];
					if (position != Vector3.zero) transform *= Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
					transform *= Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
					if (position != Vector3.zero) transform *= Matrix4x4.TRS(-position, Quaternion.identity, Vector3.one);
					break;
				case "scale":
					////Debug.Log("scale");
					scale = Vector3.one;
					if (kvp.Value.Length > 0) scale.x = scale.y = kvp.Value[0];
					if (kvp.Value.Length == 2) scale.y = kvp.Value[1];
					transform *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
					break;
				case "skewX":
					////Debug.Log("skewX");
					Matrix4x4 skewX = Matrix4x4.identity;
					if (kvp.Value.Length == 1) skewX[0,1] = Mathf.Tan(Mathf.Deg2Rad * kvp.Value[1]);
					transform *= skewX;
					break;
				case "skewY":
					////Debug.Log("skewY");
					Matrix4x4 skewY = Matrix4x4.identity;
					if (kvp.Value.Length == 1) skewY[1,0] = -Mathf.Tan(Mathf.Deg2Rad * kvp.Value[1]);
					transform *= skewY;
					break;
				}
			}
			return transform;
		}
		public static Color ToColor (this string value) {
			if (value.StartsWith("rgb")) return RgbToColor(value);
			else if (value.StartsWith("#")) return HexToColor(value);
			else {
				switch (value) {
				case "none" :
					return Color.clear;
				case "currentColor" :
					return Color.clear;
				case "inherit" :
					return Color.white;
				case "aliceblue" :
					return HexToColor("#f0f8ff");
				case "antiquewhite" :
					return HexToColor("#faebd7");
				case "aqua" :
					return HexToColor("#00ffff");
				case "aquamarine" :
					return HexToColor("#7fffd4");
				case "azure" :
					return HexToColor("#f0ffff");
				case "beige" :
					return HexToColor("#f5f5dc");
				case "bisque" :
					return HexToColor("#ffe4c4");
				case "black" :
					return HexToColor("#000000");
				case "blanchedalmond" :
					return HexToColor("#ffebcd");
				case "blue" :
					return HexToColor("#0000ff");
				case "blueviolet" :
					return HexToColor("#8a2be2");
				case "brown" :
					return HexToColor("#a52a2a");
				case "burlywood" :
					return HexToColor("#deb887");
				case "cadetblue" :
					return HexToColor("#5f9ea0");
				case "chartreuse" :
					return HexToColor("#7fff00");
				case "chocolate" :
					return HexToColor("#d2691e");
				case "coral" :
					return HexToColor("#ff7f50");
				case "cornflowerblue" :
					return HexToColor("#6495ed");
				case "cornsilk" :
					return HexToColor("#fff8dc");
				case "crimson" :
					return HexToColor("#dc143c");
				case "cyan" :
					return HexToColor("#00ffff");
				case "darkblue" :
					return HexToColor("#00008b");
				case "darkcyan" :
					return HexToColor("#008b8b");
				case "darkgoldenrod" :
					return HexToColor("#b8860b");
				case "darkgray" :
					return HexToColor("#a9a9a9");
				case "darkgrey" :
					return HexToColor("#a9a9a9");
				case "darkgreen" :
					return HexToColor("#006400");
				case "darkkhaki" :
					return HexToColor("#bdb76b");
				case "darkmagenta" :
					return HexToColor("#8b008b");
				case "darkolivegreen" :
					return HexToColor("#556b2f");
				case "darkorange" :
					return HexToColor("#ff8c00");
				case "darkorchid" :
					return HexToColor("#9932cc");
				case "darkred" :
					return HexToColor("#8b0000");
				case "darksalmon" :
					return HexToColor("#e9967a");
				case "darkseagreen" :
					return HexToColor("#8fbc8f");
				case "darkslateblue" :
					return HexToColor("#483d8b");
				case "darkslategray" :
					return HexToColor("#2f4f4f");
				case "darkslategrey" :
					return HexToColor("#2f4f4f");
				case "darkturquoise" :
					return HexToColor("#00ced1");
				case "darkviolet" :
					return HexToColor("#9400d3");
				case "deeppink" :
					return HexToColor("#ff1493");
				case "deepskyblue" :
					return HexToColor("#00bfff");
				case "dimgray" :
					return HexToColor("#696969");
				case "dimgrey" :
					return HexToColor("#696969");
				case "dodgerblue" :
					return HexToColor("#1e90ff");
				case "firebrick" :
					return HexToColor("#b22222");
				case "floralwhite" :
					return HexToColor("#fffaf0");
				case "forestgreen" :
					return HexToColor("#228b22");
				case "fuchsia" :
					return HexToColor("#ff00ff");
				case "gainsboro" :
					return HexToColor("#dcdcdc");
				case "ghostwhite" :
					return HexToColor("#f8f8ff");
				case "gold" :
					return HexToColor("#ffd700");
				case "goldenrod" :
					return HexToColor("#daa520");
				case "gray" :
					return HexToColor("#808080");
				case "grey" :
					return HexToColor("#808080");
				case "green" :
					return HexToColor("#008000");
				case "greenyellow" :
					return HexToColor("#adff2f");
				case "honeydew" :
					return HexToColor("#f0fff0");
				case "hotpink" :
					return HexToColor("#ff69b4");
				case "indianred" :
					return HexToColor("#cd5c5c");
				case "indigo" :
					return HexToColor("#4b0082");
				case "ivory" :
					return HexToColor("#fffff0");
				case "khaki" :
					return HexToColor("#f0e68c");
				case "lavender" :
					return HexToColor("#e6e6fa");
				case "lavenderblush" :
					return HexToColor("#fff0f5");
				case "lawngreen" :
					return HexToColor("#7cfc00");
				case "lemonchiffon" :
					return HexToColor("#fffacd");
				case "lightblue" :
					return HexToColor("#add8e6");
				case "lightcoral" :
					return HexToColor("#f08080");
				case "lightcyan" :
					return HexToColor("#e0ffff");
				case "lightgoldenrodyellow" :
					return HexToColor("#fafad2");
				case "lightgray" :
					return HexToColor("#d3d3d3");
				case "lightgreen" :
					return HexToColor("#90ee90");
				case "lightgrey" :
					return HexToColor("#d3d3d3");
				case "lightpink" :
					return HexToColor("#ffb6c1");
				case "lightsalmon" :
					return HexToColor("#ffa07a");
				case "lightseagreen" :
					return HexToColor("#20b2aa");
				case "lightskyblue" :
					return HexToColor("#87cefa");
				case "lightslategray" :
					return HexToColor("#778899");
				case "lightslategrey" :
					return HexToColor("#778899");
				case "lightsteelblue" :
					return HexToColor("#b0c4de");
				case "lightyellow" :
					return HexToColor("#ffffe0");
				case "lime" :
					return HexToColor("#00ff00");
				case "limegreen" :
					return HexToColor("#32cd32");
				case "linen" :
					return HexToColor("#faf0e6");
				case "magenta" :
					return HexToColor("#ff00ff");
				case "maroon" :
					return HexToColor("#800000");
				case "mediumaquamarine" :
					return HexToColor("#66cdaa");
				case "mediumblue" :
					return HexToColor("#0000cd");
				case "mediumorchid" :
					return HexToColor("#ba55d3");
				case "mediumpurple" :
					return HexToColor("#9370db");
				case "mediumseagreen" :
					return HexToColor("#3cb371");
				case "mediumslateblue" :
					return HexToColor("#7b68ee");
				case "mediumspringgreen" :
					return HexToColor("#00fa9a");
				case "mediumturquoise" :
					return HexToColor("#48d1cc");
				case "mediumvioletred" :
					return HexToColor("#c71585");
				case "midnightblue" :
					return HexToColor("#191970");
				case "mintcream" :
					return HexToColor("#f5fffa");
				case "mistyrose" :
					return HexToColor("#ffe4e1");
				case "moccasin" :
					return HexToColor("#ffe4b5");
				case "navajowhite" :
					return HexToColor("#ffdead");
				case "navy" :
					return HexToColor("#000080");
				case "oldlace" :
					return HexToColor("#fdf5e6");
				case "olive" :
					return HexToColor("#808000");
				case "olivedrab" :
					return HexToColor("#6b8e23");
				case "orange" :
					return HexToColor("#ffa500");
				case "orangered" :
					return HexToColor("#ff4500");
				case "orchid" :
					return HexToColor("#da70d6");
				case "palegoldenrod" :
					return HexToColor("#eee8aa");
				case "palegreen" :
					return HexToColor("#98fb98");
				case "paleturquoise" :
					return HexToColor("#afeeee");
				case "palevioletred" :
					return HexToColor("#db7093");
				case "papayawhip" :
					return HexToColor("#ffefd5");
				case "peachpuff" :
					return HexToColor("#ffdab9");
				case "peru" :
					return HexToColor("#cd853f");
				case "pink" :
					return HexToColor("#ffc0cb");
				case "plum" :
					return HexToColor("#dda0dd");
				case "powderblue" :
					return HexToColor("#b0e0e6");
				case "purple" :
					return HexToColor("#800080");
				case "red" :
					return HexToColor("#ff0000");
				case "rosybrown" :
					return HexToColor("#bc8f8f");
				case "royalblue" :
					return HexToColor("#4169e1");
				case "saddlebrown" :
					return HexToColor("#8b4513");
				case "salmon" :
					return HexToColor("#fa8072");
				case "sandybrown" :
					return HexToColor("#f4a460");
				case "seagreen" :
					return HexToColor("#2e8b57");
				case "seashell" :
					return HexToColor("#fff5ee");
				case "sienna" :
					return HexToColor("#a0522d");
				case "silver" :
					return HexToColor("#c0c0c0");
				case "skyblue" :
					return HexToColor("#87ceeb");
				case "slateblue" :
					return HexToColor("#6a5acd");
				case "slategray" :
					return HexToColor("#708090");
				case "slategrey" :
					return HexToColor("#708090");
				case "snow" :
					return HexToColor("#fffafa");
				case "springgreen" :
					return HexToColor("#00ff7f");
				case "steelblue" :
					return HexToColor("#4682b4");
				case "tan" :
					return HexToColor("#d2b48c");
				case "teal" :
					return HexToColor("#008080");
				case "thistle" :
					return HexToColor("#d8bfd8");
				case "tomato" :
					return HexToColor("#ff6347");
				case "turquoise" :
					return HexToColor("#40e0d0");
				case "violet" :
					return HexToColor("#ee82ee");
				case "wheat" :
					return HexToColor("#f5deb3");
				case "white" :
					return HexToColor("#ffffff");
				case "whitesmoke" :
					return HexToColor("#f5f5f5");
				case "yellow" :
					return HexToColor("#ffff00");
				case "yellowgreen" :
					return HexToColor("#9acd32");
				default :
					return HexToColor(value);
				}
			}
		}
		private	static Color RgbToColor (string value) {
			string[] rgbArray = value.TrimStart("rgb(".ToCharArray()).TrimEnd(')').Replace(", ", ",").Split(',');
			if (rgbArray.Length == 3) return new Color(rgbArray[0].ToString().ToFloat()/255f, rgbArray[1].ToString().ToFloat()/255f, rgbArray[2].ToString().ToFloat()/255f, 1);
			else if (rgbArray.Length == 4) return new Color(rgbArray[0].ToString().ToFloat()/255f, rgbArray[1].ToString().ToFloat()/255f, rgbArray[2].ToString().ToFloat()/255f, rgbArray[3].ToString().ToFloat()/255f);
			else return Color.clear;	
		}
		private	static Color HexToColor (string value) {
			float r=0,g=0,b=0,a=1;
			if (value[0].ToString() == "#") {
				if (value.Length == 9) { //8 digit hex (includes the # symbol when counting length)
					a = (float)(HexToInt(value.Substring(2, 1)) + HexToInt(value.Substring(1, 1)) * 16f)/255f;
					r = (float)(HexToInt(value.Substring(4, 1)) + HexToInt(value.Substring(3, 1)) * 16f)/255f;
					g = (float)(HexToInt(value.Substring(6, 1)) + HexToInt(value.Substring(5, 1)) * 16f)/255f;
					b = (float)(HexToInt(value.Substring(8, 1)) + HexToInt(value.Substring(7, 1)) * 16f)/255f;
				}
				else if (value.Length == 7) { //6 digit hex
					r = (float)(HexToInt(value.Substring(2, 1)) + HexToInt(value.Substring(1, 1)) * 16f)/255f;
					g = (float)(HexToInt(value.Substring(4, 1)) + HexToInt(value.Substring(3, 1)) * 16f)/255f;
					b = (float)(HexToInt(value.Substring(6, 1)) + HexToInt(value.Substring(5, 1)) * 16f)/255f;
					a = 1;
				}
				else if (value.Length == 4) { //3 digit hex
					r = (float)(HexToInt(value.Substring(1, 1)) + HexToInt(value.Substring(1, 1)) * 16f)/255f;
					g = (float)(HexToInt(value.Substring(2, 1)) + HexToInt(value.Substring(2, 1)) * 16f)/255f;
					b = (float)(HexToInt(value.Substring(3, 1)) + HexToInt(value.Substring(3, 1)) * 16f)/255f;
					a = 1;
				}
				else if (value.Length == 3) { //2 digit hex
					r = (float)(HexToInt(value.Substring(2, 1)) + HexToInt(value.Substring(1, 1)) * 16f)/255f;
					g = r;
					b = g;
					a = 1;
				}
			}
			//Debug.Log("Creating Color: " + new Color(r,g,b,a));
			return new Color(r, g, b, a);
		}
		private static int HexToInt (string hexChar) {
		    if (hexChar.ToUpper() == "0") return 0;
		    else if (hexChar.ToUpper() == "1") return 1;
		    else if (hexChar.ToUpper() == "2") return 2;
		    else if (hexChar.ToUpper() == "3") return 3;
		    else if (hexChar.ToUpper() == "4") return 4;
		    else if (hexChar.ToUpper() == "5") return 5;
		    else if (hexChar.ToUpper() == "6") return 6;
		    else if (hexChar.ToUpper() == "7") return 7;
		    else if (hexChar.ToUpper() == "8") return 8;
		    else if (hexChar.ToUpper() == "9") return 9;
		    else if (hexChar.ToUpper() == "A") return 10;
		    else if (hexChar.ToUpper() == "B") return 11;
		    else if (hexChar.ToUpper() == "C") return 12;
		    else if (hexChar.ToUpper() == "D") return 13;
		    else if (hexChar.ToUpper() == "E") return 14;
		    else if (hexChar.ToUpper() == "F") return 15;
		    else return 0;
		}
	}
}
