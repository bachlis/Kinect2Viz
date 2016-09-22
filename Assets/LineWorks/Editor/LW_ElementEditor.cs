// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LineWorks {

	public class LW_MenuEditor {

		[MenuItem("Help/LineWorks Documentation", false, 2000)]
		static void LoadLineWorksDocumentation () {
			Application.OpenURL("http://plp3d.com/LineWorks/Documentation/1.1.1/html/index.html");
		}
	}
		
	[CustomEditor(typeof(LW_Canvas))] public class LW_CanvasEditor : LW_ElementEditor {

		protected static bool s_DebugEditor = false;

		protected static float m_HandleSize = 0.04f;
		protected static Color[] m_ModeColors = {Color.magenta, Color.green, Color.cyan};

		protected LW_Canvas m_VectorCanvas;	
		protected Transform m_Transform;
		protected Vector3[] polylinePoints;
		protected SerializedProperty m_SelectedPoint;
		protected int m_SelectedShapeIndex = -1;
		protected int m_SelectedPointIndex = -1;
		protected bool m_JustEnabled = false;
		protected Rect m_SceneBoxRect = new Rect(4, 4, 200, Screen.height);

		private bool shiftModifier = false;
		private bool controlModifier = false;
		private Event currEvent;
		private Vector3 currMousePos;

		protected override void OnEnable() {
			m_VectorCanvas = target as LW_Canvas;
			m_Transform = m_VectorCanvas.transform;
			m_JustEnabled = true;
			base.OnEnable();

			//SceneView.RepaintAll();
		}

		public override void OnInspectorGUI() {
			if (!(serializedObject.targetObject is LW_Canvas)) return;

			serializedObject.Update ();

			m_Indent = 0;
			m_SelectedGraphic = m_VectorCanvas.m_SelectedGraphic;
			m_SelectedAppearance = m_VectorCanvas.m_SelectedAppearance;

			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Component Properties", titleStyle);
			EditorGUILayout.BeginVertical(footerStyle);{
				EditorGUILayout.BeginVertical(subGroupStyle); {
					EditorGUILayout.BeginHorizontal();
					m_VectorCanvas.m_CanvasExpanded = EditorGUILayout.Toggle(GUIContent.none, m_VectorCanvas.m_CanvasExpanded, foldoutToggleStyle, toggleButtonWidth);

					EditorGUILayout.LabelField("Canvas Properties", labelLargeStyle);
					EditorGUILayout.EndHorizontal();

					if (m_VectorCanvas.m_CanvasExpanded) {
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UseAccurateRaycasting"));

						EditorGUILayout.Separator();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SortingLayerID"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SortingOrder"));

						EditorGUILayout.Separator();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ViewBox"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ScaleMode"));
						if ((ScaleMode)serializedObject.FindProperty("m_ScaleMode").enumValueIndex == ScaleMode.NineSlice) {
							EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Border"), true);
						}

						GUILayoutOption buttonWidth = GUILayout.Width((Screen.width)/2f-30);
						EditorGUILayout.LabelField("Set Functions");
						EditorGUILayout.BeginHorizontal();{
							if (GUILayout.Button("Set Rect Size To ViewBox", miniButtonStyle, buttonWidth))
								m_VectorCanvas.SetRectSizeToViewBox();
							if (GUILayout.Button("Set Pivot To ViewBox", miniButtonStyle)) 
								m_VectorCanvas.SetRectPivotToViewBox();
						}EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();{
							if (GUILayout.Button("Set ViewBox To Rect", miniButtonStyle, buttonWidth)) 
										m_VectorCanvas.SetViewBoxToRect();
							if (GUILayout.Button("Set ViewBox To Bounds", miniButtonStyle)) 
								m_VectorCanvas.SetViewBoxToBounds();
						}EditorGUILayout.EndHorizontal();
					}
				}EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(subGroupStyle); {
					//expandedProp = serializedObject.FindProperty("m_RendererExpanded");
					EditorGUILayout.BeginHorizontal();{
						//expandedProp.boolValue = EditorGUILayout.Toggle(GUIContent.none, expandedProp.boolValue, foldoutToggleStyle, toggleButtonWidth);
						m_VectorCanvas.m_RendererExpanded = EditorGUILayout.Toggle(GUIContent.none, m_VectorCanvas.m_RendererExpanded, foldoutToggleStyle, toggleButtonWidth);
						EditorGUILayout.LabelField("Material Properties", labelLargeStyle);
					}EditorGUILayout.EndHorizontal();

					if (m_VectorCanvas.m_RendererExpanded) {
					//if (expandedProp.boolValue) {
						EditorGUILayout.Separator();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Color"), new GUIContent("Vertex Color"));

						EditorGUI.BeginChangeCheck();
						EditorGUILayout.Separator();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Material"), new GUIContent("Custom Material"));
						if (serializedObject.FindProperty("m_Material").objectReferenceValue == null) {
							SerializedProperty textureProp = serializedObject.FindProperty("m_MainTexture");
							EditorGUILayout.PropertyField(textureProp);
							//if (textureProp.objectReferenceValue != null || m_VectorCanvas.material != null) {
								EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UvTiling"), new GUIContent("UV Tiling"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UvOffset"), new GUIContent("UV Offset"));
							//}
							EditorGUILayout.Separator();
							EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BlendMode"), new GUIContent("Blend Mode"));
							EditorGUILayout.PropertyField(serializedObject.FindProperty("m_FeatureMode"), new GUIContent("Feature Mode"));
							if (serializedObject.FindProperty("m_FeatureMode").enumValueIndex > 0) {
								EditorGUILayout.Separator();
								EditorGUILayout.LabelField("Advanced Shader Features", labelLargeStyle);
								EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StrokeDrawMode"), new GUIContent("Stroke Draw Mode"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StrokeScaleMode"), new GUIContent("Stroke Scale Mode"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("m_JoinsAndCapsMode"), new GUIContent("Joins and Caps Mode"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("m_GradientsMode"), new GUIContent("Gradients Mode"));
								EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AntiAliasingMode"), new GUIContent("Anti-Aliasing Mode"));
							}
							else EditorGUILayout.HelpBox("Feature Mode must be set to Advanced for more options.", MessageType.Info);
						}
						if (EditorGUI.EndChangeCheck()) {
							m_VectorCanvas.ForceTotalRebuild();
						}
					}
				}EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(subGroupStyle); {
					//expandedProp = serializedObject.FindProperty("m_LODExpanded");
					EditorGUILayout.BeginHorizontal();
					//expandedProp.boolValue = EditorGUILayout.Toggle(GUIContent.none, expandedProp.boolValue, foldoutToggleStyle, toggleButtonWidth);
					m_VectorCanvas.m_LODExpanded = EditorGUILayout.Toggle(GUIContent.none, m_VectorCanvas.m_LODExpanded, foldoutToggleStyle, toggleButtonWidth);

					EditorGUILayout.LabelField("Level Of Detail (LOD) Properties", labelLargeStyle);
					EditorGUILayout.EndHorizontal();

					if (m_VectorCanvas.m_LODExpanded) {
					//if (expandedProp.boolValue) {
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Segmentation"));
						SerializedProperty optimizeProp = serializedObject.FindProperty("m_Optimize");
						EditorGUILayout.PropertyField(optimizeProp);
						GUI.enabled = optimizeProp.boolValue;
						EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Simplification"), new GUIContent("    Simplification"));
						GUI.enabled = true;

						if (EditorGUI.EndChangeCheck()) m_VectorCanvas.ForceTotalRebuild();
					}
				}EditorGUILayout.EndVertical();

				EditorGUILayout.LabelField("Graphics", labelLargeStyle);
				DrawElementHeader(serializedObject.targetObject, serializedObject.FindProperty("m_Graphic"));

			}EditorGUILayout.EndVertical();

			DrawSelectedElements();

			EditorGUILayout.Separator();

			m_VectorCanvas.m_SelectedGraphic = m_SelectedGraphic;
			m_VectorCanvas.m_SelectedAppearance = m_SelectedAppearance;

			serializedObject.ApplyModifiedProperties ();

			if (!s_DebugEditor) return;

			int index = 0;

			EditorGUILayout.LabelField("MaterialDict");
			Dictionary<Material, LW_MaterialBuffer> materialDict = LW_MaterialPool.materialDict;
			index = 0;
			foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in materialDict) {
				LW_MaterialBuffer materialBuffer = kvp.Value;
				List<LW_VertexBuffer> vertexBuffers = materialBuffer.vertexBuffers;
				int[] emptyIndices = materialBuffer.emptyIndices.ToArray();

				EditorGUILayout.BeginVertical(subGroupStyle);{
					EditorGUILayout.LabelField(index + " material: " + kvp.Key.name + " buffers: " + vertexBuffers.Count);

					EditorGUILayout.BeginVertical(subGroupStyle);{
						for (int i=0; i<vertexBuffers.Count; i++) {
							LW_VertexBuffer buffer = vertexBuffers[i];
							string label = i.ToString() + " ";
							if (buffer == null) label += "buffer == null";
							else {
								label += "styleIndex: " + buffer.styleDataIndex + " ";
								if (buffer.graphic == null) label += "graphic == null";
								else if (buffer.style == null) label += "style == null";
								else if (buffer.canvas == null) label += "canvas == null";
								else {
									label += "graphic: " + buffer.graphic.name + " style: " + buffer.style.name + " canvas: " + buffer.canvas.name;
								}
							}
							EditorGUILayout.LabelField(label);
						}
					}EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical(subGroupStyle);{
						for (int i=0; i<emptyIndices.Length; i++) {
							int emptyIndex = emptyIndices[i];
							string label = i.ToString() + " emptyIndex: " + emptyIndex;
							EditorGUILayout.LabelField(label);
						}
					}EditorGUILayout.EndVertical();

				}EditorGUILayout.EndVertical();
			}

			EditorGUILayout.LabelField("VertexBufferPool");
			EditorGUILayout.BeginVertical(subGroupStyle);{
				Dictionary<int, LW_VertexBuffer> vBufferDict = LW_BufferPool<LW_VertexBuffer>.bufferDict;
				index = 0;
				foreach(KeyValuePair<int, LW_VertexBuffer> kvp in vBufferDict) {
					int key = kvp.Key;
					LW_VertexBuffer vBuffer = kvp.Value;
					string label = "";
					if (vBuffer == null) label += "buffer == null";
					else {
						label += "styleIndex: " + vBuffer.styleDataIndex + " ";
						if (vBuffer.graphic == null) label += "graphic == null";
						else if (vBuffer.style == null) label += "style == null";
						else if (vBuffer.canvas == null) label += "canvas == null";
						else {
							label += "graphic: " + vBuffer.graphic.name + " style: " + vBuffer.style.name + " canvas: " + vBuffer.canvas.name;
						}

						label += " isEmpty: " + vBuffer.isEmpty;
					}
					EditorGUILayout.LabelField(index + " key: " + key + " buffer: " + label);
				}
			}EditorGUILayout.EndVertical();

			EditorGUILayout.LabelField("ContourBufferPool");
			EditorGUILayout.BeginVertical(subGroupStyle);{
				Dictionary<int, LW_ContourBuffer> cBufferDict = LW_BufferPool<LW_ContourBuffer>.bufferDict;

				EditorGUILayout.LabelField("count: " + cBufferDict.Count);
				/*
				index = 0;
				foreach(KeyValuePair<int, LW_ContourBuffer> kvp in cBufferDict) {
					int key = kvp.Key;
					LW_ContourBuffer cBuffer = kvp.Value;
					string label = "";
					if (cBuffer == null) label += "buffer == null";
					else {
						if (cBuffer.graphic == null) label += "graphic == null";
						else if (cBuffer.style == null) label += "style == null";
						else if (cBuffer.canvas == null) label += "canvas == null";
						else {
							label += "graphic: " + cBuffer.graphic.name + " style: " + cBuffer.style.name + " canvas: " + cBuffer.canvas.name;
						}
						label += " isEmpty: " + cBuffer.isEmpty;
					}
					EditorGUILayout.LabelField(index + " key: " + key + " buffer: " + label);
				}
				*/
			}EditorGUILayout.EndVertical();
		}

		public void OnSceneGUI() {
			if (m_JustEnabled) {
				SceneView.RepaintAll();
				m_JustEnabled = false;
			}
			Matrix4x4 matrix = m_Transform.localToWorldMatrix * m_VectorCanvas.scaler;

			if (m_VectorCanvas.viewBox.width > 0 && m_VectorCanvas.viewBox.height > 0) {
				Rect viewBox = m_VectorCanvas.viewBox;
				Vector2 lowerLeft = new Vector2(viewBox.x-viewBox.width*0.5f, viewBox.y-viewBox.height*0.5f);
				Vector2 lowerRight = new Vector2(viewBox.x+viewBox.width*0.5f, viewBox.y-viewBox.height*0.5f);
				Vector2 upperRight = new Vector2(viewBox.x+viewBox.width*0.5f, viewBox.y+viewBox.height*0.5f);
				Vector2 upperLeft = new Vector2(viewBox.x-viewBox.width*0.5f, viewBox.y+viewBox.height*0.5f);

				Handles.color = Color.green;
				Vector3[] viewBoxPos = new Vector3[5];
				
				viewBoxPos[0] = matrix.MultiplyPoint3x4(lowerLeft);
				viewBoxPos[1] = matrix.MultiplyPoint3x4(lowerRight);
				viewBoxPos[2] = matrix.MultiplyPoint3x4(upperRight);
				viewBoxPos[3] = matrix.MultiplyPoint3x4(upperLeft);
				viewBoxPos[4] = matrix.MultiplyPoint3x4(lowerLeft);
				Handles.DrawPolyLine(viewBoxPos);
				Vector3 basePointTransformedPosition = matrix.MultiplyPoint3x4(Vector3.zero);
				//Handles.Label(basePointTransformedPosition, "base point");
				float size = HandleUtility.GetHandleSize(basePointTransformedPosition)*0.08f;
				Handles.CircleCap(0, basePointTransformedPosition, Quaternion.identity, size);

				if (m_VectorCanvas.scaleMode == ScaleMode.NineSlice) {
					Handles.color = Color.red;
					Vector3[] line = new Vector3[2];
					Rect border = m_VectorCanvas.border;

					line[0] = matrix.MultiplyPoint3x4(upperLeft);
					line[1] = matrix.MultiplyPoint3x4(lowerLeft);
					line[0].x += border.x;
					line[1].x += border.x;
					Handles.DrawPolyLine(line);

					line[0] = matrix.MultiplyPoint3x4(upperRight);
					line[1] = matrix.MultiplyPoint3x4(lowerRight);
					line[0].x -= border.y;
					line[1].x -= border.y;
					Handles.DrawPolyLine(line);

					line[0] = matrix.MultiplyPoint3x4(upperLeft);
					line[1] = matrix.MultiplyPoint3x4(upperRight);
					line[0].y -= border.width;
					line[1].y -= border.width;
					Handles.DrawPolyLine(line);

					line[0] = matrix.MultiplyPoint3x4(lowerLeft);
					line[1] = matrix.MultiplyPoint3x4(lowerRight);
					line[0].y += border.height;
					line[1].y += border.height;
					Handles.DrawPolyLine(line);
				}
				/*
				else if (m_VectorCanvas.scaleMode == ScaleMode.FFD) {
					LW_Point2D[] ffdCorners = m_VectorCanvas.ffdCorners;
					LW_Point2D cp1, cp2;
					Vector2 corner1, corner2;
					Vector3 p1, p2, h1, h2;

					cp1 = ffdCorners[0];
					cp2 = ffdCorners[1];
					corner1 = upperLeft;
					corner2 = upperRight;

					p1 = matrix.MultiplyPoint3x4(corner1+cp1.position);
					p2 = matrix.MultiplyPoint3x4(corner2+cp2.position);
					h1 = cp1.hasHandleOut ? matrix.MultiplyPoint3x4(corner1+cp1.position+cp1.handleOut) : p1;
					h2 = cp2.hasHandleIn ? matrix.MultiplyPoint3x4(corner2+cp2.position+cp2.handleIn) : p2;

					//Debug.Log(p1 + ", " + p2 + ", " + h1 + ", " + h2);
					Handles.DrawBezier(p1, p2, h1, h2, Color.red, null, 2f);

					cp1 = ffdCorners[1];
					cp2 = ffdCorners[2];
					corner1 = upperRight;
					corner2 = lowerRight;

					p1 = matrix.MultiplyPoint3x4(corner1+cp1.position);
					p2 = matrix.MultiplyPoint3x4(corner2+cp2.position);
					h1 = cp1.hasHandleOut ? matrix.MultiplyPoint3x4(corner1+cp1.position+cp1.handleOut) : p1;
					h2 = cp2.hasHandleIn ? matrix.MultiplyPoint3x4(corner2+cp2.position+cp2.handleIn) : p2;

					//Debug.Log(p1 + ", " + p2 + ", " + h1 + ", " + h2);
					Handles.DrawBezier(p1, p2, h1, h2, Color.red, null, 2f);

					cp1 = ffdCorners[2];
					cp2 = ffdCorners[3];
					corner1 = lowerRight;
					corner2 = lowerLeft;

					p1 = matrix.MultiplyPoint3x4(corner1+cp1.position);
					p2 = matrix.MultiplyPoint3x4(corner2+cp2.position);
					h1 = cp1.hasHandleOut ? matrix.MultiplyPoint3x4(corner1+cp1.position+cp1.handleOut) : p1;
					h2 = cp2.hasHandleIn ? matrix.MultiplyPoint3x4(corner2+cp2.position+cp2.handleIn) : p2;

					//Debug.Log(p1 + ", " + p2 + ", " + h1 + ", " + h2);
					Handles.DrawBezier(p1, p2, h1, h2, Color.red, null, 2f);

					cp1 = ffdCorners[3];
					cp2 = ffdCorners[0];
					corner1 = lowerLeft;
					corner2 = upperLeft;

					p1 = matrix.MultiplyPoint3x4(corner1+cp1.position);
					p2 = matrix.MultiplyPoint3x4(corner2+cp2.position);
					h1 = cp1.hasHandleOut ? matrix.MultiplyPoint3x4(corner1+cp1.position+cp1.handleOut) : p1;
					h2 = cp2.hasHandleIn ? matrix.MultiplyPoint3x4(corner2+cp2.position+cp2.handleIn) : p2;

					//Debug.Log(p1 + ", " + p2 + ", " + h1 + ", " + h2);
					Handles.DrawBezier(p1, p2, h1, h2, Color.red, null, 2f);
				}
				*/
			}

			isUsed = false;

			currEvent = Event.current;
			if (currEvent.shift) shiftModifier = true;
			else shiftModifier = false;
			if (currEvent.control) controlModifier = true;
			else controlModifier = false;
				
			currMousePos = currEvent.mousePosition;
			if (m_SelectedGraphic != null) {
				DrawGraphicOnScene(m_VectorCanvas.graphic, matrix, false);
			}

		}	

		private void DrawGraphicOnScene(LW_Graphic graphic, Matrix4x4 parentMatrix, bool isSelected = false) {
			if (graphic == null && !graphic.isVisible) return;
			bool isCurrentGraphicSelection = false;
			if (m_SelectedGraphic == graphic) {
				isCurrentGraphicSelection = true;
				isSelected = true;
			}
			Matrix4x4 matrix = parentMatrix * graphic.transform;

			if (graphic is LW_Shape && isSelected) {
				if (graphic is LW_Vector2Shape) DrawPolylineOnScene(0, graphic as LW_Vector2Shape, matrix);
				else if (graphic is LW_Vector3Shape) DrawPolylineOnScene(0, graphic as LW_Vector3Shape, matrix);
				else if (graphic is LW_Point2DShape) DrawPolylineOnScene(0, graphic as LW_Point2DShape, matrix);
				else if (graphic is LW_Point3DShape) DrawPolylineOnScene(0, graphic as LW_Point3DShape, matrix);

				if (isCurrentGraphicSelection) {
					if (graphic is LW_Polyline2D) DrawPolylineOnScene(0, graphic as LW_Polyline2D, matrix);
					else if (graphic is LW_Polyline3D) DrawPolylineOnScene(0, graphic as LW_Polyline3D, matrix);
					else if (graphic is LW_Path2D) DrawPathOnScene(0, graphic as LW_Path2D, matrix);
					else if (graphic is LW_Path3D) DrawPathOnScene(0, graphic as LW_Path3D, matrix);
					else if (graphic is LW_Line) DrawLineOnScene(0, graphic as LW_Line, matrix);
					else if (graphic is LW_Polygon) DrawPolygonOnScene(0, graphic as LW_Polygon, matrix);
					else if (graphic is LW_Star) DrawStarOnScene(0, graphic as LW_Star, matrix);
					else if (graphic is LW_Circle) DrawCircleOnScene(0, graphic as LW_Circle, matrix);
					else if (graphic is LW_Ellipse) DrawEllipseOnScene(0, graphic as LW_Ellipse, matrix);
					else if (graphic is LW_Rectangle) DrawRectangleOnScene(0, graphic as LW_Rectangle, matrix);
				}
			}

			if (graphic is LW_Group) {
				LW_Group group = graphic as LW_Group;
				for (int i=0; i<group.Count; i++) {
					if (group[i] != null) DrawGraphicOnScene(group[i], matrix, isSelected);
				}
			}
		}
		private void DrawPolylineOnScene(int shapesIndex, LW_Vector2Shape shape, Matrix4x4 matrix) {
			if (shape.points != null && shape.points.Count > 1) {
				Handles.color = shape.editorColor;
				polylinePoints = new Vector3[shape.isClosed ? shape.points.Count+1 : shape.points.Count];
				for (int p=0; p<shape.points.Count; p++) {

					//Vector3 pos = transform.TransformPoint(matrix.MultiplyPoint3x4(shape.points[p]));
					Vector3 pos = matrix.MultiplyPoint3x4(shape.points[p]);

					polylinePoints[p] = pos;
				}
				if (shape.isClosed) polylinePoints[shape.points.Count] = polylinePoints[0];
				Handles.DrawPolyLine(polylinePoints);
			}
		}
		private void DrawPolylineOnScene(int shapesIndex, LW_Vector3Shape shape, Matrix4x4 matrix) {
			if (shape.points != null && shape.points.Count > 1) {
				Handles.color = shape.editorColor;
				polylinePoints = new Vector3[shape.isClosed ? shape.points.Count+1 : shape.points.Count];
				for (int p=0; p<shape.points.Count; p++) {

					//Vector3 pos = transform.TransformPoint(matrix.MultiplyPoint3x4(shape.points[p]));
					Vector3 pos = matrix.MultiplyPoint3x4(shape.points[p]);

					polylinePoints[p] = pos;
				}
				if (shape.isClosed) polylinePoints[shape.points.Count] = polylinePoints[0];
				Handles.DrawPolyLine(polylinePoints);
			}
		}
		private void DrawPolylineOnScene(int shapesIndex, LW_Point2DShape shape, Matrix4x4 matrix) {
			if (shape.points != null && shape.points.Count > 1) {
				int pointsCount = shape.isClosed ? shape.points.Count : shape.points.Count-1;
				for (int p=0; p<pointsCount; p++) {
					LW_Point2D cp1 = shape.points[p];
					LW_Point2D cp2 = p == shape.points.Count-1 ? shape.points[0] : shape.points[p+1];

					Vector3 p1 = matrix.MultiplyPoint3x4(cp1.position);
					Vector3 p2 = matrix.MultiplyPoint3x4(cp2.position);
					Vector3 h1 = cp1.hasHandleOut ? matrix.MultiplyPoint3x4(cp1.position+cp1.handleOut) : p1;
					Vector3 h2 = cp2.hasHandleIn ? matrix.MultiplyPoint3x4(cp2.position+cp2.handleIn) : p2;

					Handles.DrawBezier(p1, p2, h1, h2, shape.editorColor, null, 2f);
				}
			}
		}
		private void DrawPolylineOnScene(int shapesIndex, LW_Point3DShape shape, Matrix4x4 matrix) {
			if (shape.points != null && shape.points.Count > 1) {
				int pointsCount = shape.isClosed ? shape.points.Count : shape.points.Count-1;
				for (int p=0; p<pointsCount; p++) {
					LW_Point3D cp1 = shape.points[p];
					LW_Point3D cp2 = p == shape.points.Count-1 ? shape.points[0] : shape.points[p+1];

					Vector3 p1 = matrix.MultiplyPoint3x4(cp1.position);
					Vector3 p2 = matrix.MultiplyPoint3x4(cp2.position);
					Vector3 h1 = cp1.hasHandleOut ? matrix.MultiplyPoint3x4(cp1.position+cp1.handleOut) : p1;
					Vector3 h2 = cp2.hasHandleIn ? matrix.MultiplyPoint3x4(cp2.position+cp2.handleIn) : p2;

					Handles.DrawBezier(p1, p2, h1, h2, shape.editorColor, null, 2f);
				}
			}
		}

		private Color GetColor(LW_Shape shape) {
			Color c = Color.white;
			if (!shiftModifier && !controlModifier) c = shape.editorColor;
			else {
				if (shiftModifier) c *= Color.cyan;
				if (controlModifier) c*= Color.green;
			}
			return c;
		}

		private bool isUsed = false;
		private void DrawCircleOnScene(int shapesIndex, LW_Circle shape, Matrix4x4 matrix){
			Vector3 newPos = Vector3.zero;
			Vector2 center = shape.center;
			Vector2 radius = center + new Vector2(shape.radius,0);
			float r = shape.radius;


			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					// Center
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Center");
						EditorGUIUtility.labelWidth = 12;
						center.x = EditorGUILayout.FloatField ("X", center.x);
						center.y = EditorGUILayout.FloatField ("Y", center.y);
						if (center != shape.center) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.center = center;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Radius
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						r = EditorGUILayout.FloatField ("Radius", r);
						if (r != shape.radius) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radius = r;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			Handles.color = shape.editorColor;
			Handles.DrawPolyLine(new Vector3[]{
				matrix.MultiplyPoint3x4(radius), 
				matrix.MultiplyPoint3x4(center), 
			});

			// Center
			if (DrawPointMoveOnScene(ref newPos, center, shape, matrix, -1, "center", GetColor(shape), !isUsed)) {
				shape.center = (Vector2)newPos;
				isUsed = true;
			}

			// RadiusX
			if (DrawPointMoveOnScene(ref newPos, radius, shape, matrix, -1, "radius", GetColor(shape), !isUsed)) {
				shape.radius = newPos.x - shape.center.x;
				isUsed = true;
			}
		}
		private void DrawEllipseOnScene(int shapesIndex, LW_Ellipse shape, Matrix4x4 matrix){
			Vector3 newPos = Vector3.zero;
			Vector2 center = shape.center;
			Vector2 radiusX = shape.center + new Vector2(shape.radiusX,0);
			Vector2 radiusY = shape.center + new Vector2(0,shape.radiusY);
			float rx = shape.radiusX;
			float ry = shape.radiusY;


			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					// Center
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Center");
						EditorGUIUtility.labelWidth = 12;
						center.x = EditorGUILayout.FloatField ("X", center.x);
						center.y = EditorGUILayout.FloatField ("Y", center.y);
						if (center != shape.center) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.center = center;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Radius
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Radius");
						EditorGUIUtility.labelWidth = 12;
						rx = EditorGUILayout.FloatField ("X", rx);
						if (rx != shape.radiusX) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radiusX = rx;
							if (shiftModifier) shape.radiusY = ry = rx;
							isUsed = true;
						}
						ry = EditorGUILayout.FloatField ("Y", ry);
						if (ry != shape.radiusY) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radiusY = ry;
							if (shiftModifier) shape.radiusX = rx = ry;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					EditorGUILayout.HelpBox("Shift - Constrain Proportions", MessageType.None);
				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			Handles.color = shape.editorColor;
			Handles.DrawPolyLine(new Vector3[]{
				matrix.MultiplyPoint3x4(radiusX), 
				matrix.MultiplyPoint3x4(center), 
				matrix.MultiplyPoint3x4(radiusY)
			});

			// Center
			if (DrawPointMoveOnScene(ref newPos, center, shape, matrix, -1, "center", GetColor(shape), !isUsed)) {
				shape.center = (Vector2)newPos;
				isUsed = true;
			}

			// RadiusX
			if (DrawPointMoveOnScene(ref newPos, radiusX, shape, matrix, -1, "radiusX", GetColor(shape), !isUsed)) {
				shape.radiusX = newPos.x - shape.center.x;
				if (shiftModifier) shape.radiusY = shape.radiusX;
				isUsed = true;
			}

			// RadiusY
			if (DrawPointMoveOnScene(ref newPos, radiusY, shape, matrix, -1, "radiusY", GetColor(shape), !isUsed)) {
				shape.radiusY = newPos.y - shape.center.y;
				if (shiftModifier) shape.radiusX = shape.radiusY;
				isUsed = true;
			}
		}
		private void DrawRectangleOnScene(int shapesIndex, LW_Rectangle shape, Matrix4x4 matrix) {
			Vector3 newPos = Vector3.zero;
			Vector2 center = shape.center;
			Vector2 widthCorner = shape.center + new Vector2(shape.width/2f,0);
			Vector2 heightCorner = shape.center + new Vector2(0,shape.height/2f);
			Vector2 radiusCenter = shape.center + new Vector2(shape.width/2f, shape.height/2f) + new Vector2(-shape.radiusX,-shape.radiusY);
			float width = shape.width;
			float height = shape.height;
			float rx = shape.radiusX;
			float ry = shape.radiusY;

			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					// Center
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Center");
						EditorGUIUtility.labelWidth = 12;
						center.x = EditorGUILayout.FloatField ("X", center.x);
						center.y = EditorGUILayout.FloatField ("Y", center.y);
						if (center != shape.center) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.center = center;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Size
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Size");
						EditorGUIUtility.labelWidth = 12;
						width = EditorGUILayout.FloatField ("W", width);
						if (width != shape.width) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.width = width;
							if (shiftModifier) shape.height = height = width;
							isUsed = true;
						}
						height = EditorGUILayout.FloatField ("H", height);
						if (height != shape.height) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.height = height;
							if (shiftModifier) shape.width = width = height;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Radius
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Radius");
						EditorGUIUtility.labelWidth = 12;
						rx = EditorGUILayout.FloatField ("X", rx);
						if (rx != shape.radiusX) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radiusX = rx;
							if (shiftModifier) shape.radiusY = ry = rx;
							isUsed = true;
						}
						ry = EditorGUILayout.FloatField ("Y", ry);
						if (ry != shape.radiusY) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radiusY = ry;
							if (shiftModifier) shape.radiusX = rx = ry;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					EditorGUILayout.HelpBox("Shift - Constrain Proportions", MessageType.None);
				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			Handles.color = shape.editorColor;
			Handles.DrawPolyLine(new Vector3[]{
				matrix.MultiplyPoint3x4(widthCorner), 
				matrix.MultiplyPoint3x4(center), 
				matrix.MultiplyPoint3x4(heightCorner)
			});

			if (DrawPointMoveOnScene(ref newPos, center, shape, matrix, -1, "center", GetColor(shape), !isUsed)) {
				shape.center = (Vector2)newPos;
				isUsed = true;
			}
			if (DrawPointMoveOnScene(ref newPos, widthCorner, shape, matrix, -1, "width", GetColor(shape), !isUsed)) {
				shape.width = (newPos.x - shape.center.x)*2f;
				if (shiftModifier) shape.height = shape.width;
				isUsed = true;
			}

			if (DrawPointMoveOnScene(ref newPos, heightCorner, shape, matrix, -1, "height", GetColor(shape), !isUsed)) {
				shape.height = (newPos.y - shape.center.y)*2f;
				if (shiftModifier) shape.width = shape.height;
				isUsed = true;
			}

			if (DrawPointMoveOnScene(ref newPos, radiusCenter, shape, matrix, -1, "round", GetColor(shape), !isUsed)) {
				shape.radiusX = shape.center.x + shape.width*0.5f - newPos.x;
				shape.radiusY = shape.center.y + shape.height*0.5f - newPos.y;

				if (shiftModifier) shape.radiusX = shape.radiusY;
				isUsed = true;
			}
		}
		private void DrawPathOnScene(int shapesIndex, LW_Path2D shape, Matrix4x4 matrix) {

			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 70;
						EditorGUIUtility.fieldWidth = 30;
						m_SelectedPointIndex = EditorGUILayout.IntField("Selected", m_SelectedPointIndex);
						if (GUILayout.Button("<", EditorStyles.miniButtonLeft, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex > 0 ? m_SelectedPointIndex-1 : shape.points.Count-1;
						if (GUILayout.Button(">", EditorStyles.miniButtonRight, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex < shape.points.Count-1 ? m_SelectedPointIndex+1 : 0;

					}EditorGUILayout.EndHorizontal();

					if (m_SelectedPointIndex >= 0 && m_SelectedPointIndex < shape.points.Count) {
						
						LW_Point2D selectedPoint = shape.points[m_SelectedPointIndex];
						Vector2 position = selectedPoint.position;
						PointType type = selectedPoint.pointType;
						bool hasHandleIn = selectedPoint.hasHandleIn;
						Vector2 handleIn = selectedPoint.handleIn;
						bool hasHandleOut = selectedPoint.hasHandleOut;
						Vector2 handleOut = selectedPoint.handleOut;

						// Selected
						EditorGUILayout.LabelField ("Position");
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 20;
							EditorGUILayout.PrefixLabel(" ");
							EditorGUIUtility.labelWidth = 12;
							position.x = EditorGUILayout.FloatField ("X", position.x);
							position.y = EditorGUILayout.FloatField ("Y", position.y);
							if (position != selectedPoint.position) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.position = position;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

						// Type
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 70;
							type = (PointType)EditorGUILayout.EnumPopup("Point Type", type);
							if (type != selectedPoint.pointType && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.pointType = type;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

						// HandleIn
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 70;
							hasHandleIn = EditorGUILayout.Toggle("Handle-In", hasHandleIn);
							if (hasHandleIn != selectedPoint.hasHandleIn && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.hasHandleIn = hasHandleIn;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
						}EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 20;
							EditorGUILayout.PrefixLabel(" ");
							EditorGUIUtility.labelWidth = 12;

							handleIn.x = EditorGUILayout.FloatField ("X", handleIn.x);
							handleIn.y = EditorGUILayout.FloatField ("Y", handleIn.y);
							if (handleIn != selectedPoint.handleIn && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.handleIn = handleIn;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

						// HandleOut
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 70;
							hasHandleOut = EditorGUILayout.Toggle("Handle-Out", hasHandleOut);
							if (hasHandleOut != selectedPoint.hasHandleOut && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.hasHandleOut = hasHandleOut;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
						}EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 20;
							EditorGUILayout.PrefixLabel(" ");
							EditorGUIUtility.labelWidth = 12;

							handleOut.x = EditorGUILayout.FloatField ("X", handleOut.x);
							handleOut.y = EditorGUILayout.FloatField ("Y", handleOut.y);
							if (handleOut != selectedPoint.handleOut && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.handleOut = handleOut;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

					}
					EditorGUILayout.HelpBox("Control - Add/Remove Point", MessageType.None);

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			for (int i=0; i<shape.points.Count; i++) {
				DrawControlPointOnScene(shapesIndex, i, shape, matrix);
			}

			if (controlModifier) {
				DrawAddRemoveOnScreen(shape, matrix);
			}
		}
		private void DrawPathOnScene(int shapesIndex, LW_Path3D shape, Matrix4x4 matrix) {

			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 70;
						EditorGUIUtility.fieldWidth = 30;
						m_SelectedPointIndex = EditorGUILayout.IntField("Selected", m_SelectedPointIndex);
						if (GUILayout.Button("<", EditorStyles.miniButtonLeft, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex > 0 ? m_SelectedPointIndex-1 : shape.points.Count-1;
						if (GUILayout.Button(">", EditorStyles.miniButtonRight, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex < shape.points.Count-1 ? m_SelectedPointIndex+1 : 0;

					}EditorGUILayout.EndHorizontal();

					if (m_SelectedPointIndex >= 0 && m_SelectedPointIndex < shape.points.Count) {
						
						LW_Point3D selectedPoint = shape.points[m_SelectedPointIndex];

						Vector3 position = selectedPoint.position;
						PointType type = selectedPoint.pointType;
						bool hasHandleIn = selectedPoint.hasHandleIn;
						Vector3 handleIn = selectedPoint.handleIn;
						bool hasHandleOut = selectedPoint.hasHandleOut;
						Vector3 handleOut = selectedPoint.handleOut;

						// Selected
						EditorGUILayout.LabelField ("Position");
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 20;
							EditorGUILayout.PrefixLabel(" ");
							EditorGUIUtility.labelWidth = 12;
							EditorGUIUtility.fieldWidth = 30;
							position.x = EditorGUILayout.FloatField ("X", position.x);
							position.y = EditorGUILayout.FloatField ("Y", position.y);
							position.z = EditorGUILayout.FloatField ("Z", position.z);
							if (position != selectedPoint.position) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.position = position;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

						// Type
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 70;
							type = (PointType)EditorGUILayout.EnumPopup("Point Type", type);
							if (type != selectedPoint.pointType && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.pointType = type;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

						// HandleIn
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 70;
							hasHandleIn = EditorGUILayout.Toggle("Handle-In", hasHandleIn);
							if (hasHandleIn != selectedPoint.hasHandleIn && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.hasHandleIn = hasHandleIn;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
						}EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 20;
							EditorGUILayout.PrefixLabel(" ");
							EditorGUIUtility.labelWidth = 12;
							EditorGUIUtility.fieldWidth = 30;

							handleIn.x = EditorGUILayout.FloatField ("X", handleIn.x);
							handleIn.y = EditorGUILayout.FloatField ("Y", handleIn.y);
							handleIn.z = EditorGUILayout.FloatField ("Z", handleIn.z);
							if (handleIn != selectedPoint.handleIn && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.handleIn = handleIn;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

						// HandleOut
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 70;
							hasHandleOut = EditorGUILayout.Toggle("Handle-Out", hasHandleOut);
							if (hasHandleOut != selectedPoint.hasHandleOut && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.hasHandleOut = hasHandleOut;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
						}EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 20;
							EditorGUILayout.PrefixLabel(" ");
							EditorGUIUtility.labelWidth = 12;
							EditorGUIUtility.fieldWidth = 30;

							handleOut.x = EditorGUILayout.FloatField ("X", handleOut.x);
							handleOut.y = EditorGUILayout.FloatField ("Y", handleOut.y);
							handleOut.z = EditorGUILayout.FloatField ("Z", handleOut.z);
							if (handleOut != selectedPoint.handleOut && !isUsed) {
								RegisterUndo(shape, shape.SetElementDirty);
								selectedPoint.handleOut = handleOut;
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

					}
					EditorGUILayout.HelpBox("Shift - Move Point In/Out\nControl - Add/Remove Point", MessageType.None);

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			for (int i=0; i<shape.points.Count; i++) {
				DrawControlPointOnScene(shapesIndex, i, shape, matrix);
			}

			if (controlModifier) {
				DrawAddRemoveOnScreen(shape, matrix);
			}
		}
		private void DrawLineOnScene(int shapesIndex, LW_Line shape, Matrix4x4 matrix) {
			Vector3 newPos = Vector3.zero;
			Vector2 start = shape.start;
			Vector2 end = shape.end;

			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					// Start
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Start");
						EditorGUIUtility.labelWidth = 12;
						start.x = EditorGUILayout.FloatField ("X", start.x);
						start.y = EditorGUILayout.FloatField ("Y", start.y);
						if (start != shape.start) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.start = start;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// End
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("End");
						EditorGUIUtility.labelWidth = 12;
						end.x = EditorGUILayout.FloatField ("X", end.x);
						end.y = EditorGUILayout.FloatField ("Y", end.y);
						if (end != shape.end) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.end = end;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			if (DrawPointMoveOnScene(ref newPos, shape.start, shape, matrix, -1, "start", GetColor(shape), !isUsed)) {
				shape.start = (Vector2)newPos;
				isUsed = true;
			}

			if (DrawPointMoveOnScene(ref newPos, shape.end, shape, matrix, -1, "end", GetColor(shape), !isUsed)) {
				shape.end = (Vector2)newPos;
				isUsed = true;
			}
		}
		private void DrawPolygonOnScene(int shapesIndex, LW_Polygon shape, Matrix4x4 matrix){
			Vector3 newPos = Vector3.zero;
			Vector2 center = shape.center;
			Vector2 radius = shape.center + new Vector2(shape.radius,0);
			float r = shape.radius;
			int s = shape.sides;


			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					// Center
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Center");
						EditorGUIUtility.labelWidth = 12;
						center.x = EditorGUILayout.FloatField ("X", center.x);
						center.y = EditorGUILayout.FloatField ("Y", center.y);
						if (center != shape.center) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.center = center;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Radius
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						r = EditorGUILayout.FloatField ("Radius", r);
						if (r != shape.radius) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radius = r;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Sides
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						s = EditorGUILayout.IntField ("Sides", s);
						if (s != shape.sides) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.sides = s;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			Handles.color = shape.editorColor;
			Handles.DrawPolyLine(new Vector3[]{
				matrix.MultiplyPoint3x4(radius), 
				matrix.MultiplyPoint3x4(center)
			});

			// Center
			if (DrawPointMoveOnScene(ref newPos, center, shape, matrix, -1, "center", GetColor(shape), !isUsed)) {
				shape.center = (Vector2)newPos;
				isUsed = true;
			}

			// RadiusX
			if (DrawPointMoveOnScene(ref newPos, radius, shape, matrix, -1, "radius", GetColor(shape), !isUsed)) {
				shape.radius = newPos.x - shape.center.x;
				isUsed = true;
			}
		}
		private void DrawStarOnScene(int shapesIndex, LW_Star shape, Matrix4x4 matrix){
			Vector3 newPos = Vector3.zero;
			Vector2 center = shape.center;
			Vector2 radiusX = shape.center + new Vector2(shape.radiusX,0);
			Vector2 radiusY = shape.center + new Vector2(0,shape.radiusY);
			float rx = shape.radiusX;
			float ry = shape.radiusY;
			int s = shape.sides;

			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					// Center
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Center");
						EditorGUIUtility.labelWidth = 12;
						center.x = EditorGUILayout.FloatField ("X", center.x);
						center.y = EditorGUILayout.FloatField ("Y", center.y);
						if (center != shape.center) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.center = center;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Radius
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						EditorGUILayout.PrefixLabel("Radius");
						EditorGUIUtility.labelWidth = 12;
						rx = EditorGUILayout.FloatField ("X", rx);
						if (rx != shape.radiusX) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radiusX = rx;
							if (shiftModifier) shape.radiusY = ry = rx;
							isUsed = true;
						}
						ry = EditorGUILayout.FloatField ("Y", ry);
						if (ry != shape.radiusY) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.radiusY = ry;
							if (shiftModifier) shape.radiusX = rx = ry;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

					// Sides
					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 50;
						s = EditorGUILayout.IntField ("Sides", s);
						if (s != shape.sides) {
							RegisterUndo(shape, shape.SetElementDirty);
							shape.sides = s;
							isUsed = true;
						}
						EditorGUIUtility.labelWidth = 0;
					}EditorGUILayout.EndHorizontal();

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			Handles.color = shape.editorColor;
			Handles.DrawPolyLine(new Vector3[]{
				matrix.MultiplyPoint3x4(radiusX), 
				matrix.MultiplyPoint3x4(center), 
				matrix.MultiplyPoint3x4(radiusY)
			});

			// Center
			if (DrawPointMoveOnScene(ref newPos, center, shape, matrix, -1, "center", GetColor(shape), !isUsed)) {
				shape.center = (Vector2)newPos;
				isUsed = true;
			}

			// RadiusX
			if (DrawPointMoveOnScene(ref newPos, radiusX, shape, matrix, -1, "radiusX", GetColor(shape), !isUsed)) {
				shape.radiusX = newPos.x - shape.center.x;
				isUsed = true;
			}

			// RadiusY
			if (DrawPointMoveOnScene(ref newPos, radiusY, shape, matrix, -1, "radiusY", GetColor(shape), !isUsed)) {
				shape.radiusY = newPos.y - shape.center.y;
				isUsed = true;
			}
		}
		private void DrawPolylineOnScene(int shapesIndex, LW_Polyline2D shape, Matrix4x4 matrix) {
			Vector3 newPos = Vector3.zero;
			List<Vector2> points = shape.points;

			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 70;
						EditorGUIUtility.fieldWidth = 30;
						m_SelectedPointIndex = EditorGUILayout.IntField("Selected", m_SelectedPointIndex);
						if (GUILayout.Button("<", EditorStyles.miniButtonLeft, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex > 0 ? m_SelectedPointIndex-1 : shape.points.Count-1;
						if (GUILayout.Button(">", EditorStyles.miniButtonRight, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex < shape.points.Count-1 ? m_SelectedPointIndex+1 : 0;

					}EditorGUILayout.EndHorizontal();

					if (m_SelectedPointIndex >= 0 && m_SelectedPointIndex < shape.points.Count) {
						
						Vector2 selectedPoint = shape.points[m_SelectedPointIndex];

						// Selected
						EditorGUILayout.LabelField ("Position");
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 50;
							EditorGUILayout.PrefixLabel(m_SelectedPointIndex.ToString());
							EditorGUIUtility.labelWidth = 12;
							selectedPoint.x = EditorGUILayout.FloatField ("X", selectedPoint.x);
							selectedPoint.y = EditorGUILayout.FloatField ("Y", selectedPoint.y);
							if (selectedPoint != shape.points[m_SelectedPointIndex]) {
								RegisterUndo(shape, shape.SetElementDirty);
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

					}
					EditorGUILayout.HelpBox("Control - Add/Remove Point", MessageType.None);

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();

			for (int p=0; p<points.Count; p++) {
				bool isSelected = m_SelectedPointIndex == p;

				if (DrawPointMoveOnScene(ref newPos, points[p], shape, matrix, p, p.ToString(), isSelected ? Color.yellow : GetColor(shape), !isUsed)) {
					points[p] = (Vector2)newPos;
					shape.points = points;
					isUsed = true;
				}
			}

			if (controlModifier) {
				DrawAddRemoveOnScreen(shape, matrix);
			}	
		}
		private void DrawPolylineOnScene(int shapesIndex, LW_Polyline3D shape, Matrix4x4 matrix) {
			
			Vector3 newPos = Vector3.zero;
			List<Vector3> points = shape.points;

			Handles.BeginGUI ();
			GUILayout.BeginArea (m_SceneBoxRect);{
				EditorGUILayout.BeginVertical(subGroupStyle);{
					GUILayout.Label (shape.name, EditorStyles.boldLabel);

					EditorGUILayout.BeginHorizontal();{
						EditorGUIUtility.labelWidth = 70;
						EditorGUIUtility.fieldWidth = 30;
						m_SelectedPointIndex = EditorGUILayout.IntField("Selected", m_SelectedPointIndex);
						if (GUILayout.Button("<", EditorStyles.miniButtonLeft, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex > 0 ? m_SelectedPointIndex-1 : shape.points.Count-1;
						if (GUILayout.Button(">", EditorStyles.miniButtonRight, GUILayout.Width(20f))) m_SelectedPointIndex = m_SelectedPointIndex < shape.points.Count-1 ? m_SelectedPointIndex+1 : 0;

					}EditorGUILayout.EndHorizontal();

					if (m_SelectedPointIndex >= 0 && m_SelectedPointIndex < shape.points.Count) {
						
						Vector3 selectedPoint = shape.points[m_SelectedPointIndex];

						// Selected
						EditorGUILayout.LabelField ("Position");
						EditorGUILayout.BeginHorizontal();{
							EditorGUIUtility.labelWidth = 20;
							EditorGUILayout.PrefixLabel(" ");
							EditorGUIUtility.labelWidth = 12;
							EditorGUIUtility.fieldWidth = 30;
							selectedPoint.x = EditorGUILayout.FloatField ("X", selectedPoint.x);
							selectedPoint.y = EditorGUILayout.FloatField ("Y", selectedPoint.y);
							selectedPoint.z = EditorGUILayout.FloatField ("Z", selectedPoint.z);
							if (selectedPoint != shape.points[m_SelectedPointIndex]) {
								RegisterUndo(shape, shape.SetElementDirty);
								shape.points[m_SelectedPointIndex] = selectedPoint;
								isUsed = true;
							}
							EditorGUIUtility.labelWidth = 0;
						}EditorGUILayout.EndHorizontal();

					}
					EditorGUILayout.HelpBox("Shift - Move Point In/Out\nControl - Add/Remove Point", MessageType.None);

				}EditorGUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}GUILayout.EndArea();
			Handles.EndGUI();


			for (int p=0; p<points.Count; p++) {
				bool isSelected = m_SelectedPointIndex == p;

				if (DrawPointMoveOnScene(ref newPos, points[p], shape, matrix, p, p.ToString(), isSelected ? Color.yellow : GetColor(shape), !isUsed, shiftModifier)) {
					points[p] = newPos;
					shape.points = points;
					isUsed = true;
				}
			}

			if (controlModifier) {
				DrawAddRemoveOnScreen(shape, matrix);
			}
		}

		private void DrawControlPointOnScene (int shapesIndex, int pointIndex, LW_Point2DShape shape, Matrix4x4 matrix) {
			List<LW_Point2D> points = shape.points;
			LW_Point2D point = points[pointIndex];

			bool isSelected = m_SelectedPointIndex == pointIndex;
			Vector3 newPos = Vector3.zero;
			Vector3 pointPos = point.position;
			Vector3 inPos = point.position + point.handleIn;
			Vector3 outPos = point.position + point.handleOut;
			bool hasHandleIn = point.hasHandleIn;
			bool hasHandleOut = point.hasHandleOut;

			List<Vector3> m_HandleLine = new List<Vector3>();
			if (hasHandleIn) m_HandleLine.Add(inPos);
			if (hasHandleIn || hasHandleOut) m_HandleLine.Add(pointPos);
			if (hasHandleOut) m_HandleLine.Add(outPos);

			DrawPolylineOnScene(m_HandleLine.ToArray(), shape.editorColor, matrix);

			// HandleIn
			if (hasHandleIn) {
				if (DrawPointMoveOnScene(ref newPos, inPos, shape, matrix, pointIndex, null, Color.gray, !isUsed)) {					
					point.handleIn = (Vector2)newPos - point.position;
					points[pointIndex] = point;
					shape.points = points;
					isUsed = true;
				}
			}

			// HandleOut
			if (hasHandleOut) {
				if (DrawPointMoveOnScene(ref newPos, outPos, shape, matrix, pointIndex, null, Color.gray, !isUsed)) {
					point.handleOut = (Vector2)newPos - point.position;
					points[pointIndex] = point;
					shape.points = points;
					isUsed = true;
				}
			}

			// Point
			if (DrawPointMoveOnScene(ref newPos, pointPos, shape, matrix, pointIndex, pointIndex.ToString(), isSelected ? Color.yellow : GetColor(shape), !isUsed)) {
				point.position = newPos;
				points[pointIndex] = point;
				shape.points = points;
				isUsed = true;
			}
		}
		private void DrawControlPointOnScene (int shapesIndex, int pointIndex, LW_Point3DShape shape, Matrix4x4 matrix) {
			List<LW_Point3D> points = shape.points;
			LW_Point3D point = points[pointIndex];

			bool isSelected = m_SelectedPointIndex == pointIndex;
			Vector3 newPos = Vector3.zero;
			Vector3 pointPos = point.position;
			Vector3 inPos = point.position + point.handleIn;
			Vector3 outPos = point.position + point.handleOut;
			bool hasHandleIn = point.hasHandleIn;
			bool hasHandleOut = point.hasHandleOut;

			List<Vector3> m_HandleLine = new List<Vector3>();
			if (hasHandleIn) m_HandleLine.Add(inPos);
			if (hasHandleIn || hasHandleOut) m_HandleLine.Add(pointPos);
			if (hasHandleOut) m_HandleLine.Add(outPos);

			DrawPolylineOnScene(m_HandleLine.ToArray(), shape.editorColor, matrix);

			// HandleIn
			if (hasHandleIn) {
				if (DrawPointMoveOnScene(ref newPos, inPos, shape, matrix, pointIndex, null, Color.gray, !isUsed, shiftModifier)) {					
					point.handleIn = newPos - point.position;
					points[pointIndex] = point;
					shape.points = points;
					isUsed = true;
				}
			}

			// HandleOut
			if (hasHandleOut) {
				if (DrawPointMoveOnScene(ref newPos, outPos, shape, matrix, pointIndex, null, Color.gray, !isUsed, shiftModifier)) {
					point.handleOut = newPos - point.position;
					points[pointIndex] = point;
					shape.points = points;
					isUsed = true;
				}
			}

			// Point
			if (DrawPointMoveOnScene(ref newPos, pointPos, shape, matrix, pointIndex, pointIndex.ToString(), isSelected ? Color.yellow : GetColor(shape), !isUsed, shiftModifier)) {
				point.position = newPos;
				points[pointIndex] = point;
				shape.points = points;
				isUsed = true;
			}

		}

		private void DrawPolylineOnScene(Vector3[] points, Color color, Matrix4x4 matrix) {
			Handles.color = color;
			Vector3[] tranformedPoint = new Vector3[points.Length];
			for (int i=0; i<points.Length; i++) {
				//tranformedPoint[i] = transform.TransformPoint(m_Scaler.MultiplyPoint3x4(points[i]));
				tranformedPoint[i] = matrix.MultiplyPoint3x4(points[i]);
			}
			Handles.DrawPolyLine(tranformedPoint);
		}
			
		private bool DrawPointMoveOnScene(ref Vector3 newPosition, Vector3 oldPosition, LW_Element element, Matrix4x4 matrix, int pointIndex, string label, Color color, bool isSelectable, bool contrainVertical = false) {
			// Get the needed data before the handle
			int someHashCode = GetHashCode();
			int controlIDBeforeHandle = GUIUtility.GetControlID(someHashCode, FocusType.Passive);
			bool isEventUsedBeforeHandle = (Event.current.type == EventType.used);
			bool isSelected = m_SelectedPointIndex == pointIndex;

			Vector3 oldTransformedPosition = matrix.MultiplyPoint3x4(oldPosition);

			if (!string.IsNullOrEmpty(label)) Handles.Label(oldTransformedPosition, "   " + label);
			float innerSize = HandleUtility.GetHandleSize(oldTransformedPosition) * 0.12f;

			Vector3 newTransformedPosition = oldTransformedPosition;
			if (isSelectable) {
				if (contrainVertical) {
					Handles.color = color;
					Vector3 worldPos = Handles.FreeMoveHandle(oldTransformedPosition, Quaternion.identity, innerSize, Vector3.zero, Handles.SphereCap);
					if (isSelected) {
						Plane graphicPlane = new Plane(m_Transform.right, oldTransformedPosition);
						Vector3 camPos = Camera.current.transform.position;
						Ray ray = new Ray(camPos, worldPos - camPos);
						float rayDistance;
						if (graphicPlane.Raycast(ray, out rayDistance)) {
							newTransformedPosition = ray.GetPoint(rayDistance);
						}
					}
				}
				else {
					Handles.color = color;
					Vector3 worldPos = Handles.FreeMoveHandle(oldTransformedPosition, Quaternion.identity, innerSize, Vector3.zero, Handles.SphereCap);
					if (isSelected) {
						Plane graphicPlane = new Plane(m_Transform.forward, oldTransformedPosition);
						Vector3 camPos = Camera.current.transform.position;
						Ray ray = new Ray(camPos, worldPos - camPos);
						float rayDistance;
						if (graphicPlane.Raycast(ray, out rayDistance)) {
							newTransformedPosition = ray.GetPoint(rayDistance);
						}
					}
				}
			}
			else {
				Handles.SphereCap(0, oldTransformedPosition, Quaternion.identity, innerSize);
			}
			newPosition = matrix.inverse.MultiplyPoint3x4(newTransformedPosition);
			if (contrainVertical) {
				newPosition.x = oldPosition.x;
				newPosition.y = oldPosition.y;
			}

			// Get the needed data after the handle
			int controlIDAfterHandle = GUIUtility.GetControlID(someHashCode, FocusType.Passive);
			bool isEventUsedByHandle = !isEventUsedBeforeHandle && (Event.current.type == EventType.used);

			if ((controlIDBeforeHandle < GUIUtility.hotControl && GUIUtility.hotControl < controlIDAfterHandle) || isEventUsedByHandle) {
				m_SelectedPointIndex = pointIndex;
				SceneView.RepaintAll();
				EditorUtility.SetDirty (target);
			}

			if ((newPosition-oldPosition).sqrMagnitude > 0.1f) {
				RegisterUndo(element, element.SetElementDirty);
				return true;
			}
			else return false;
		}
		private bool DrawPointButtonOnScene(Vector3 oldPosition, LW_Element element, Matrix4x4 matrix, string label, Color color) {
			Vector3 oldTransformedPosition = matrix.MultiplyPoint3x4(oldPosition);

			if (!string.IsNullOrEmpty(label)) Handles.Label(oldTransformedPosition, "   " + label);
			float innerSize = HandleUtility.GetHandleSize(oldTransformedPosition) * 0.12f;

			Handles.color = color;
			if (Handles.Button(oldTransformedPosition, Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed ) {
				RegisterUndo(element, element.SetElementDirty);
				return true;
			}
			else return false;
		}

		protected void DrawAddRemoveOnScreen(LW_Vector2Shape shape, Matrix4x4 matrix) {
			float innerSize = HandleUtility.GetHandleSize(currMousePos) * 0.12f;

			Vector3 currWorldPos = Vector3.zero;
			Plane graphicPlane = new Plane(m_Transform.forward, m_Transform.position);
			Ray ray = HandleUtility.GUIPointToWorldRay(currEvent.mousePosition);
			float rayDistance;
			if (graphicPlane.Raycast(ray, out rayDistance)) {
				currWorldPos = ray.GetPoint(rayDistance);
			}

			Vector3 currLocalPos = (Vector2)matrix.inverse.MultiplyPoint3x4(currWorldPos);

			int addIndex = 0;
			int removeIndex = 0;

			float firstDistance = (currLocalPos - (Vector3)shape.points[0]).sqrMagnitude;
			float lastDistance = (currLocalPos - (Vector3)shape.points[shape.points.Count-1]).sqrMagnitude;

			if (firstDistance < lastDistance) addIndex = -1;
			else addIndex = shape.points.Count-1;

			float currAngle = 90;
			float currDistance = firstDistance;
			for (int i=1; i<shape.points.Count; i++) {
				Vector3 prevPoint = (Vector3)shape.points[i-1];
				Vector3 nextPoint = (Vector3)shape.points[i];

				float angle = Mathf.Abs(Vector3.Angle(currLocalPos - prevPoint, nextPoint - currLocalPos));
				if (angle < currAngle) {
					currAngle = angle;
					addIndex = i-1;
				}

				float distance = (currLocalPos - nextPoint).sqrMagnitude;
				if (distance < currDistance) {
					currDistance = distance;
					removeIndex = i;
				}
			}
			if (currDistance < innerSize*2) {
				Handles.color = Color.red;
				if (Handles.Button (matrix.MultiplyPoint3x4(shape.points[removeIndex]), Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					shape.RemoveAt(removeIndex);
				}
			}
			else {
				Handles.color = Color.red;
				if (addIndex < shape.points.Count-1) {
					Vector3 pointAfter = shape.points[addIndex+1];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointAfter), 4);
				}
				if (addIndex >= 0 && addIndex < shape.points.Count) {
					Vector3 pointBefore = shape.points[addIndex];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointBefore), 4);
				}
				Handles.color = Color.gray;
				if (Handles.Button (currWorldPos, Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					if (addIndex == -1) {
						shape.InsertAt(0);
						shape.points[0] = (Vector2)currLocalPos;
					}
					else {
						shape.InsertAt(addIndex);
						shape.points[addIndex+1] = (Vector2)currLocalPos;
					}
				}
			}
			SceneView.RepaintAll();
		}
		protected void DrawAddRemoveOnScreen(LW_Vector3Shape shape, Matrix4x4 matrix) {
			float innerSize = HandleUtility.GetHandleSize(currMousePos) * 0.12f;

			Vector3 currWorldPos = Vector3.zero;
			Plane graphicPlane = new Plane(m_Transform.forward, m_Transform.position);
			Ray ray = HandleUtility.GUIPointToWorldRay(currEvent.mousePosition);
			float rayDistance;
			if (graphicPlane.Raycast(ray, out rayDistance)) {
				currWorldPos = ray.GetPoint(rayDistance);
			}

			Vector3 currLocalPos = (Vector2)matrix.inverse.MultiplyPoint3x4(currWorldPos);

			int addIndex = 0;
			int removeIndex = 0;

			float firstDistance = (currLocalPos - shape.points[0]).sqrMagnitude;
			float lastDistance = (currLocalPos - shape.points[shape.points.Count-1]).sqrMagnitude;

			if (firstDistance < lastDistance) addIndex = -1;
			else addIndex = shape.points.Count-1;

			float currAngle = 90;
			float currDistance = firstDistance;
			for (int i=1; i<shape.points.Count; i++) {
				Vector3 prevPoint = shape.points[i-1];
				Vector3 nextPoint = shape.points[i];

				float angle = Mathf.Abs(Vector3.Angle(currLocalPos - prevPoint, nextPoint - currLocalPos));
				if (angle < currAngle) {
					currAngle = angle;
					addIndex = i-1;
				}

				float distance = (currLocalPos - nextPoint).sqrMagnitude;
				if (distance < currDistance) {
					currDistance = distance;
					removeIndex = i;
				}
			}
			if (currDistance < innerSize*2) {
				Handles.color = Color.red;
				if (Handles.Button (matrix.MultiplyPoint3x4(shape.points[removeIndex]), Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					shape.RemoveAt(removeIndex);
				}
			}
			else {
				Handles.color = Color.red;
				if (addIndex < shape.points.Count-1) {
					Vector3 pointAfter = shape.points[addIndex+1];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointAfter), 4);
				}
				if (addIndex >= 0 && addIndex < shape.points.Count) {
					Vector3 pointBefore = shape.points[addIndex];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointBefore), 4);
				}
				Handles.color = Color.gray;
				if (Handles.Button (currWorldPos, Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					if (addIndex == -1) {
						shape.InsertAt(0);
						shape.points[0] = currLocalPos;
					}
					else {
						shape.InsertAt(addIndex);
						shape.points[addIndex+1] = currLocalPos;
					}
				}
			}
			SceneView.RepaintAll();
		}
		protected void DrawAddRemoveOnScreen(LW_Point2DShape shape, Matrix4x4 matrix) {
			float innerSize = HandleUtility.GetHandleSize(currMousePos) * 0.12f;

			Vector3 currWorldPos = Vector3.zero;
			Plane graphicPlane = new Plane(m_Transform.forward, m_Transform.position);
			Ray ray = HandleUtility.GUIPointToWorldRay(currEvent.mousePosition);
			float rayDistance;
			if (graphicPlane.Raycast(ray, out rayDistance)) {
				currWorldPos = ray.GetPoint(rayDistance);
			}

			Vector3 currLocalPos = (Vector2)matrix.inverse.MultiplyPoint3x4(currWorldPos);

			int addIndex = 0;
			int removeIndex = 0;

			float firstDistance = (currLocalPos - (Vector3)shape.points[0].position).sqrMagnitude;
			float lastDistance = (currLocalPos - (Vector3)shape.points[shape.points.Count-1].position).sqrMagnitude;

			if (firstDistance < lastDistance) addIndex = -1;
			else addIndex = shape.points.Count-1;

			float currAngle = 90;
			float currDistance = firstDistance;
			for (int i=1; i<shape.points.Count; i++) {
				Vector3 prevPoint = (Vector3)shape.points[i-1].position;
				Vector3 nextPoint = (Vector3)shape.points[i].position;

				float angle = Mathf.Abs(Vector3.Angle(currLocalPos - prevPoint, nextPoint - currLocalPos));
				if (angle < currAngle) {
					currAngle = angle;
					addIndex = i-1;
				}

				float distance = (currLocalPos - nextPoint).sqrMagnitude;
				if (distance < currDistance) {
					currDistance = distance;
					removeIndex = i;
				}
			}
			if (currDistance < innerSize*2) {
				Handles.color = Color.red;
				if (Handles.Button (matrix.MultiplyPoint3x4(shape.points[removeIndex].position), Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					shape.RemoveAt(removeIndex);
				}
			}
			else {
				Handles.color = Color.red;
				if (addIndex < shape.points.Count-1) {
					LW_Point2D pointAfter = shape.points[addIndex+1];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointAfter.position), 4);
				}
				if (addIndex >= 0 && addIndex < shape.points.Count) {
					LW_Point2D pointBefore = shape.points[addIndex];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointBefore.position), 4);
				}
				Handles.color = Color.gray;
				if (Handles.Button (currWorldPos, Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					if (addIndex == -1) {
						shape.InsertAt(0);
						shape.points[0] = new LW_Point2D(currLocalPos);
					}
					else {
						shape.InsertAt(addIndex);
						shape.points[addIndex+1] = new LW_Point2D(currLocalPos);
					}
				}
			}
			SceneView.RepaintAll();
		}
		protected void DrawAddRemoveOnScreen(LW_Point3DShape shape, Matrix4x4 matrix) {
			float innerSize = HandleUtility.GetHandleSize(currMousePos) * 0.12f;

			Vector3 currWorldPos = Vector3.zero;
			Plane graphicPlane = new Plane(m_Transform.forward, m_Transform.position);
			Ray ray = HandleUtility.GUIPointToWorldRay(currEvent.mousePosition);
			float rayDistance;
			if (graphicPlane.Raycast(ray, out rayDistance)) {
				currWorldPos = ray.GetPoint(rayDistance);
			}

			Vector3 currLocalPos = (Vector2)matrix.inverse.MultiplyPoint3x4(currWorldPos);

			int addIndex = 0;
			int removeIndex = 0;

			float firstDistance = (currLocalPos - shape.points[0].position).sqrMagnitude;
			float lastDistance = (currLocalPos - shape.points[shape.points.Count-1].position).sqrMagnitude;

			if (firstDistance < lastDistance) addIndex = -1;
			else addIndex = shape.points.Count-1;

			float currAngle = 90;
			float currDistance = firstDistance;
			for (int i=1; i<shape.points.Count; i++) {
				Vector3 prevPoint = shape.points[i-1].position;
				Vector3 nextPoint = shape.points[i].position;

				float angle = Mathf.Abs(Vector3.Angle(currLocalPos - prevPoint, nextPoint - currLocalPos));
				if (angle < currAngle) {
					currAngle = angle;
					addIndex = i-1;
				}

				float distance = (currLocalPos - nextPoint).sqrMagnitude;
				if (distance < currDistance) {
					currDistance = distance;
					removeIndex = i;
				}
			}
			if (currDistance < innerSize*2) {
				Handles.color = Color.red;
				if (Handles.Button (matrix.MultiplyPoint3x4(shape.points[removeIndex]), Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					shape.RemoveAt(removeIndex);
				}
			}
			else {
				Handles.color = Color.red;
				if (addIndex < shape.points.Count-1) {
					LW_Point2D pointAfter = shape.points[addIndex+1];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointAfter.position), 4);
				}
				if (addIndex >= 0 && addIndex < shape.points.Count) {
					LW_Point2D pointBefore = shape.points[addIndex];
					Handles.DrawDottedLine (currWorldPos, matrix.MultiplyPoint3x4(pointBefore.position), 4);
				}
				Handles.color = Color.gray;
				if (Handles.Button (currWorldPos, Quaternion.identity, innerSize, innerSize, Handles.SphereCap) && !isUsed) {
					RegisterUndo(shape, shape.SetElementDirty);
					if (addIndex == -1) {
						shape.InsertAt(0);
						shape.points[0] = new LW_Point3D(currLocalPos);
					}
					else {
						shape.InsertAt(addIndex);
						shape.points[addIndex+1] = new LW_Point3D(currLocalPos);
					}
				}
			}
			SceneView.RepaintAll();
		}
	}

	[CustomEditor(typeof(LW_Group))] public class LW_GroupEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Shape))] public class LW_ShapeEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Circle))] public class LW_CircleEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Ellipse))] public class LW_EllipseEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Rectangle))] public class LW_RectangleEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Path2D))] public class LW_Path2DEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Path3D))] public class LW_Path3DEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Line))]	public class LW_LineEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Polygon))] public class LW_PolygonEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Star))] public class LW_StarEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Polyline2D))] public class LW_Polyline2DEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Polyline3D))] public class LW_Polyline3DEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Styles))] public class LW_StylesEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Fill))]	public class LW_FillEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Stroke))] public class LW_StrokeEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Marker))] public class LW_MarkerEditor : LW_ElementEditor {}
	[CustomEditor(typeof(LW_Collider))] public class LW_ColliderEditor : LW_ElementEditor {}

	public class LW_ElementEditor : Editor {

		protected static GUIStyle titleStyle {
			get {
				if (m_TitleStyle == null) {
					m_TitleStyle = new GUIStyle("OL Title");
					GUIStyleState state = m_TitleStyle.normal;
					state.textColor = Color.black;
					m_TitleStyle.normal = state;
					m_TitleStyle.alignment = TextAnchor.MiddleLeft;
					m_TitleStyle.fontSize = 12;
					m_TitleStyle.fontStyle = FontStyle.Bold;
					m_TitleStyle.margin = new RectOffset(0,0,0,0);
					m_TitleStyle.padding = new RectOffset(6,6,0,0);
					m_TitleStyle.overflow = new RectOffset(0,0,1,2);
				}
				return m_TitleStyle;
			}
		}
		private static GUIStyle m_TitleStyle;

		protected static GUIStyle headerStyle {
			get {
				if (m_HeaderStyle == null) {
					m_HeaderStyle = new GUIStyle("IN BigTitle");
					m_HeaderStyle.margin = new RectOffset(0,0,0,0);
					m_HeaderStyle.overflow = new RectOffset(5,5,0,1);
					m_HeaderStyle.padding = new RectOffset(0,0,6,5);
				}
				return m_HeaderStyle;
			}
		}
		private static GUIStyle m_HeaderStyle;

		protected static GUIStyle footerStyle {
			get {
				if (m_FooterStyle == null) {
					m_FooterStyle = new GUIStyle("CN Box");
					m_FooterStyle.fixedHeight = 0;
					m_FooterStyle.stretchHeight = false;
					m_FooterStyle.margin = new RectOffset(0,0,0,0);
					m_FooterStyle.padding = new RectOffset(6,6,6,1);
					m_FooterStyle.overflow = new RectOffset(0,0,0,0);
				}
				return m_FooterStyle;
			}
		}
		private static GUIStyle m_FooterStyle;

		protected static GUIStyle subGroupStyle {
			get {
				if (m_SubGroupStyle == null) {
					m_SubGroupStyle = new GUIStyle("HelpBox");
					m_SubGroupStyle.padding = new RectOffset(6,6,6,6);
					m_SubGroupStyle.margin = new RectOffset(4,4,4,5);
				}
				return m_SubGroupStyle;
			}
		}
		private static GUIStyle m_SubGroupStyle;

		protected static GUIStyle itemStyle {
			get {
				if (m_ItemStyle == null) {
					m_ItemStyle = new GUIStyle("HelpBox");
					m_ItemStyle.padding = new RectOffset(6,6,6,6);
					m_ItemStyle.margin = new RectOffset(4,4,4,5);
				}
				return m_ItemStyle;
			}
		}
		private static GUIStyle m_ItemStyle;

		protected static GUIStyle miniButtonStyle {
			get {
				if (m_MiniButtonStyle == null) {
					m_MiniButtonStyle = new GUIStyle(EditorStyles.miniButton);
					m_MiniButtonStyle.alignment = TextAnchor.MiddleCenter;
					m_MiniButtonStyle.padding = new RectOffset(0,0,2,2);
				}
				return m_MiniButtonStyle;
			}
		}
		private static GUIStyle m_MiniButtonStyle;

		protected static GUIStyle miniButtonLeftStyle {
			get {
				if (m_MiniButtonLeftStyle == null) {
					m_MiniButtonLeftStyle = new GUIStyle(EditorStyles.miniButtonLeft);
					m_MiniButtonLeftStyle.alignment = TextAnchor.MiddleCenter;
					m_MiniButtonLeftStyle.padding = new RectOffset(0,0,2,2);
				}
				return m_MiniButtonLeftStyle;
			}
		}
		private static GUIStyle m_MiniButtonLeftStyle;

		protected static GUIStyle miniButtonMidStyle {
			get {
				if (m_MiniButtonMidStyle == null) {
					m_MiniButtonMidStyle = new GUIStyle(EditorStyles.miniButtonMid);
					m_MiniButtonMidStyle.alignment = TextAnchor.MiddleCenter;
					m_MiniButtonMidStyle.padding = new RectOffset(0,0,2,2);
				}
				return m_MiniButtonMidStyle;
			}
		}
		private static GUIStyle m_MiniButtonMidStyle;

		protected static GUIStyle miniButtonRightStyle {
			get {
				if (m_MiniButtonRightStyle == null) {
					m_MiniButtonRightStyle = new GUIStyle(EditorStyles.miniButtonRight);
					m_MiniButtonRightStyle.alignment = TextAnchor.MiddleCenter;
					m_MiniButtonRightStyle.padding = new RectOffset(0,0,2,2);
				}
				return m_MiniButtonRightStyle;
			}
		}
		private static GUIStyle m_MiniButtonRightStyle;

		protected static GUIStyle labelLargeStyle {
			get {
				if (m_LabelLargeStyle == null) {
					m_LabelLargeStyle = new GUIStyle(EditorStyles.boldLabel);
					m_LabelLargeStyle.padding = new RectOffset(0,0,0,0);
					m_LabelLargeStyle.margin = new RectOffset(0,0,0,0);
				}
				return m_LabelLargeStyle;
			}
		}
		private static GUIStyle m_LabelLargeStyle;

		protected static GUIStyle labelBlankStyle {
			get {
				if (m_LabelBlankStyle == null) {
					m_LabelBlankStyle = new GUIStyle("label");
					m_LabelBlankStyle.border = new RectOffset(0,0,0,0);
					m_LabelBlankStyle.margin = new RectOffset(0,0,0,0);
					m_LabelBlankStyle.overflow = new RectOffset(0,0,0,0);
					m_LabelBlankStyle.padding = new RectOffset(0,0,0,0);
				}
				return m_LabelBlankStyle;
			}
		}
		private static GUIStyle m_LabelBlankStyle;

		protected static GUIStyle helpBoxStyle {
			get {
				if (m_HelpBoxStyle == null) {
					m_HelpBoxStyle = new GUIStyle("HelpBox");
					m_HelpBoxStyle.padding = new RectOffset(6,6,6,6);
				}
				return m_HelpBoxStyle;
			}
		}
		private static GUIStyle m_HelpBoxStyle;

		protected static GUIStyle foldoutStyle {
			get {
				if (m_FoldoutStyle == null) {
					m_FoldoutStyle = new GUIStyle("foldout");
					m_FoldoutStyle.fixedWidth = 0;
					m_FoldoutStyle.stretchWidth = false;
					m_FoldoutStyle.border = new RectOffset(14,0,14,0);
					m_FoldoutStyle.overflow = new RectOffset(0,0,0,0);
					m_FoldoutStyle.padding = new RectOffset(14,0,0,0);
					m_FoldoutStyle.margin = new RectOffset(16,0,2,0);
				}
				return m_FoldoutStyle;
			}
		}
		private static GUIStyle m_FoldoutStyle;

		protected static GUIStyle foldoutToggleStyle {
			get {
				if (m_FoldoutToggleStyle == null) {
					m_FoldoutToggleStyle = new GUIStyle("foldout");
					m_FoldoutToggleStyle.border = new RectOffset(14,0,13,0);
					m_FoldoutToggleStyle.overflow = new RectOffset(0,0,0,0);
					m_FoldoutToggleStyle.padding = new RectOffset(0,0,0,0);
					m_FoldoutToggleStyle.margin = new RectOffset(0,0,0,0);
				}
				return m_FoldoutToggleStyle;
			}
		}
		private static GUIStyle m_FoldoutToggleStyle;

		protected static int m_Indent = 0;
		protected static Color m_Tint = Color.white;
		protected static Color m_White = Color.white;
		protected static Color m_SelectedGraphicTint = new Color(0.8f, 0.8f, 1f, 1f);
		protected static Color m_SelectedStyleTint = new Color(1f, 0.8f, 0.8f, 1f);
		protected static Matrix4x4 m_IdentityMatrix = Matrix4x4.identity;

		protected static List<GUIContent> popupList = new List<GUIContent>();
		protected static GUIContent[] menuPopup = new GUIContent[] {
			new GUIContent("Menu")
		};
		protected static GUIContent[] elementPopup = new GUIContent[] {
			new GUIContent("Save to Assets"),
			new GUIContent("Copy and Replace")
		};
		protected static GUIContent[] copyPopup = new GUIContent[] {
			new GUIContent("Copy and Disconnect")
		};
		protected static GUIContent[] collectionPopup = new GUIContent[] {
			new GUIContent("Move Element Up"),
			new GUIContent("Move Element Down"),
			new GUIContent("Duplicate Element"),
			new GUIContent("Delete Element")
		};
		protected static GUIContent[] stylePopup = new GUIContent[] {
			new GUIContent("Add Fill"), 
			new GUIContent("Add Stroke"), 
			new GUIContent("Add Marker"), 
			new GUIContent("Add Collider"),
		};
		protected static GUIContent[] graphicPopup = new GUIContent[] {
			new GUIContent("Add Circle"), 
			new GUIContent("Add Ellipse"), 
			new GUIContent("Add Rectangle"), 
			new GUIContent("Add Line"), 
			new GUIContent("Add Polygon"), 
			new GUIContent("Add Star"), 
			new GUIContent("Add Polyline 2D"), 
			new GUIContent("Add Polyline 3D"), 
			new GUIContent("Add Path 2D"), 
			new GUIContent("Add Path 3D"),
			new GUIContent("Add Group"),
			new GUIContent("Add Empty"),
			new GUIContent("Extract to Child")
		};

		protected static GUIContent
		moveUpButtonContent = new GUIContent("^", "move up"),
		moveDownButtonContent = new GUIContent("v", "move down"),
		duplicateButtonContent = new GUIContent("+", "duplicate"),
		deleteButtonContent = new GUIContent("-", "delete");

		protected static GUILayoutOption 
		toggleButtonWidth = GUILayout.Width(14f),
		miniButtonWidth = GUILayout.Width(14f),
		saveButtonWidth = GUILayout.Width(45f),
		labelButtonWidth = GUILayout.Width(90f),
		dropDownButtonWidth = GUILayout.Width(56f);

		protected bool m_ShowEmpty = true;
		protected bool m_ShowInherited = true;
		protected bool m_ElementDeleted = false;
		protected UnityEngine.Object m_UndoObject = null;

		protected LW_Element m_ElementRef;
		protected LW_Graphic m_SelectedGraphic;
		protected LW_Appearance m_SelectedAppearance;

		protected static UnityAction m_OnUndoCallback;
		protected static Stack<UnityAction> m_OnUndoCallbacks;
		protected static Stack<UnityAction> m_OnRedoCallbacks;
		protected static UnityAction actionToFlush;

		protected virtual void OnEnable() {
			if (serializedObject.targetObject is LW_Element) m_ElementRef = serializedObject.targetObject as LW_Element;
			m_Indent = 0;
			toggleButtonWidth = GUILayout.Width(14f);
			miniButtonWidth = GUILayout.Width(14f);
			saveButtonWidth = GUILayout.Width(45f);
			labelButtonWidth = GUILayout.Width(90f);
			m_TitleStyle = null;
			m_HeaderStyle = null;
			m_MiniButtonStyle = null;
			m_MiniButtonLeftStyle = null;
			m_MiniButtonMidStyle = null;
			m_MiniButtonRightStyle = null;
			m_LabelLargeStyle = null;
			m_LabelBlankStyle = null;
			m_FooterStyle = null;
			m_HelpBoxStyle = null;
			m_FoldoutStyle = null;
			m_FoldoutToggleStyle = null;
			m_SubGroupStyle = null;
			m_OnUndoCallbacks = new Stack<UnityAction>();
			Undo.undoRedoPerformed += undoRedoPerformed;
			Undo.willFlushUndoRecord += willFlushUndoRecord;
		}
		protected virtual void OnDisable() {
			Undo.undoRedoPerformed -= undoRedoPerformed;
			Undo.willFlushUndoRecord -= willFlushUndoRecord;
		}

		protected void RegisterUndo(UnityEngine.Object obj, UnityAction action) {
			if (obj != null && !Application.isPlaying) {
				EditorUtility.SetDirty(obj);
				Undo.RecordObject(obj, "LineWorks - " + obj.name);
				SceneView.RepaintAll();
				if (action != null) actionToFlush = action;
			}
			if (action != null) action();
		}

		private void undoRedoPerformed() {
			if (m_OnUndoCallbacks != null && m_OnUndoCallbacks.Count > 0) {
				UnityAction action = m_OnUndoCallbacks.Pop();
				if (action != null) {
					action();
					if (m_OnRedoCallbacks == null) m_OnRedoCallbacks = new Stack<UnityAction>();
					m_OnRedoCallbacks.Push(action);
				}
			}
			else if (m_OnRedoCallbacks != null && m_OnRedoCallbacks.Count > 0) {
				UnityAction action = m_OnRedoCallbacks.Pop();
				if (action != null) {
					action();
					if (m_OnUndoCallbacks == null) m_OnUndoCallbacks = new Stack<UnityAction>();
					m_OnUndoCallbacks.Push(action);
				}
			}
		}
		private void willFlushUndoRecord() {
			if (actionToFlush != null) RegisterUndoAction(actionToFlush);
		}
		private void RegisterUndoAction(UnityAction action) {
			if (action != null && !Application.isPlaying) {
				if (m_OnUndoCallbacks == null) m_OnUndoCallbacks = new Stack<UnityAction>();
				m_OnUndoCallbacks.Push(action);
			}
		}

		public override void OnInspectorGUI(){
			if (!(serializedObject.targetObject is LW_Element)) return;

			serializedObject.Update();

			m_Indent = 0;

			m_SelectedGraphic = m_ElementRef.m_SelectedGraphic;
			m_SelectedAppearance = m_ElementRef.m_SelectedAppearance;

			DrawElementProperties(serializedObject, m_ElementRef);

			SerializedProperty elementCollectionProp = null;
			if (m_ElementRef is LW_Group) elementCollectionProp = serializedObject.FindProperty("m_GraphicsList");
			else if (m_ElementRef is LW_Styles) elementCollectionProp = serializedObject.FindProperty("m_StylesList");
			if (elementCollectionProp != null) {
				EditorGUILayout.BeginVertical(subGroupStyle);{
					m_Indent++;
					for (int i=0; i<elementCollectionProp.arraySize; i++) {
						DrawElementHeader(serializedObject.targetObject, elementCollectionProp.GetArrayElementAtIndex(i), elementCollectionProp, i);
						if (m_ElementDeleted) break;
					}
					m_Indent--;
				}EditorGUILayout.EndVertical();
			}
				
			DrawSelectedElements();

			m_ElementRef.m_SelectedGraphic = m_SelectedGraphic;
			m_ElementRef.m_SelectedAppearance = m_SelectedAppearance;

			serializedObject.ApplyModifiedProperties ();
		}

		// Component
		protected void DrawSelectedElements() {

			if (m_SelectedGraphic != null) {
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField(m_SelectedGraphic.name + " Properties", titleStyle);
				EditorGUILayout.BeginVertical(footerStyle);{
					DrawElementProperties(new SerializedObject(m_SelectedGraphic), m_SelectedGraphic);
				}EditorGUILayout.EndVertical();
			}
			else {
				EditorGUILayout.Separator();
				EditorGUILayout.BeginVertical(footerStyle);{
					EditorGUILayout.HelpBox("No Graphic element is currently selected. Select a graphic element by checking the toggle on the element.", MessageType.Info, true);
					GUILayout.Space(1f);
				}EditorGUILayout.EndVertical();
			}

			if (m_SelectedAppearance != null) {
				EditorGUILayout.Separator();
				EditorGUILayout.LabelField(m_SelectedAppearance.name + " Properties", titleStyle);
				EditorGUILayout.BeginVertical(footerStyle);{
					DrawElementProperties(new SerializedObject(m_SelectedAppearance), m_SelectedAppearance);
				}EditorGUILayout.EndVertical();
			}
			else {
				EditorGUILayout.Separator();
				EditorGUILayout.BeginVertical(footerStyle);{
					EditorGUILayout.HelpBox("No Style element is currently selected. Select a style element by checking the toggle on the element.", MessageType.Info, true);
					GUILayout.Space(1f);
				}EditorGUILayout.EndVertical();
			}
		}

		// Graphics
		protected void DrawElementHeader(UnityEngine.Object parentRef, SerializedProperty elementProp, SerializedProperty parentCollectionProp = null, int index = -1) {
			SerializedObject elementSO = null;
			LW_Element elementRef = null;
			if (elementProp.objectReferenceValue != null) {
				elementRef = elementProp.objectReferenceValue as LW_Element;
				elementSO = new SerializedObject(elementRef);
				elementSO.Update ();
			}

			SerializedProperty elementCollectionProp = null;
			if (elementRef is LW_Group) elementCollectionProp = elementSO.FindProperty("m_GraphicsList");
			else if (elementRef is LW_Styles) elementCollectionProp = elementSO.FindProperty("m_StylesList");

			m_ElementDeleted = false;
			bool isCollection = elementCollectionProp != null;
			bool isEmpty = elementRef == null;
			bool isSelectedBefore = (m_SelectedGraphic != null && elementRef == m_SelectedGraphic) || (m_SelectedAppearance != null && elementRef == m_SelectedAppearance);

			Color m_Selected = elementRef is LW_Appearance ? m_SelectedStyleTint : m_SelectedGraphicTint;
			m_Tint = isSelectedBefore ? m_Selected : m_White;

			if (isCollection) GUI.backgroundColor = m_Tint;
			else GUI.backgroundColor = new Color(m_Tint.r * 0.85f, m_Tint.g * 0.85f, m_Tint.b * 0.85f, 1);
			EditorGUILayout.BeginVertical(headerStyle);{
				GUI.backgroundColor = m_White;

				EditorGUILayout.BeginHorizontal ();{

					if (elementRef != null) {
						// Selection Toggle
						if ((elementRef is LW_Graphic && (elementRef as LW_Graphic).styles.Count > 0) || elementRef is LW_Style) GUI.backgroundColor = new Color(1f, 0.8f, 0.8f, 1f);
						bool isSelectedAfter = EditorGUILayout.Toggle (GUIContent.none, isSelectedBefore, toggleButtonWidth);
						GUI.backgroundColor = m_White;
						if (isSelectedBefore != isSelectedAfter) {
							if (elementRef is LW_Graphic) {
								if (isSelectedAfter) m_SelectedGraphic = elementRef as LW_Graphic;
								else m_SelectedGraphic = null;
								m_SelectedAppearance = null;
							}
							else if (elementRef is LW_Appearance) {
								if (isSelectedAfter) m_SelectedAppearance = elementRef as LW_Appearance;
								else m_SelectedAppearance = null;
							}
							SceneView.RepaintAll();
						}
					}

					// Foldout and Indents
					if (isCollection) {
						if (m_Indent > 0) EditorGUILayout.LabelField(GUIContent.none, labelBlankStyle, GUILayout.Width(14f*m_Indent));
						if (isCollection) elementRef.m_ElementHeaderExpanded = EditorGUILayout.Toggle(GUIContent.none, elementRef.m_ElementHeaderExpanded, foldoutToggleStyle, toggleButtonWidth);
					}
					else if (isEmpty) {
						if (m_Indent > -1) EditorGUILayout.LabelField(GUIContent.none, labelBlankStyle, GUILayout.Width(14f*(m_Indent+2)+8));
					}
					else {
						if (m_Indent > -1) EditorGUILayout.LabelField(GUIContent.none, labelBlankStyle, GUILayout.Width(14f*(m_Indent+1)+4));
					}

					if (elementProp != null) {
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(elementProp, GUIContent.none, true);
						if (EditorGUI.EndChangeCheck() && elementRef != null) {
							if (elementRef.hideFlags == HideFlags.None) elementRef = LW_Utilities.SafeDestroy(elementRef, false);
							else elementRef = LW_Utilities.SafeDestroy(elementRef, true);
							m_ElementDeleted = elementRef == null;
						}
					}
					else EditorGUILayout.LabelField(elementRef.name);


					// Object Menu
					if (elementRef is LW_Group) elementRef.m_ElementHeaderExpanded = DrawGroupMenu(parentRef, elementProp, elementCollectionProp, parentCollectionProp, index) || elementRef.m_ElementHeaderExpanded;
					else if (elementRef is LW_Styles) elementRef.m_ElementHeaderExpanded = DrawAppearanceMenu(parentRef, elementProp, elementCollectionProp, parentCollectionProp, index) || elementRef.m_ElementHeaderExpanded;
					else if (elementRef is LW_Element) DrawElementMenu(parentRef, elementProp, parentCollectionProp, index);
					else DrawEmptyMenu(parentRef, elementProp, parentCollectionProp, index);

				}EditorGUILayout.EndHorizontal();
			}EditorGUILayout.EndVertical();
				
			if (!m_ElementDeleted && isCollection && elementRef.m_ElementHeaderExpanded) {

				m_Indent++;
				for (int i=0; i<elementCollectionProp.arraySize; i++) {
					DrawElementHeader(elementRef, elementCollectionProp.GetArrayElementAtIndex(i), elementCollectionProp, i);
					if (m_ElementDeleted) break;
				}
				m_Indent--;
			}

			if (!m_ElementDeleted && elementSO != null) elementSO.ApplyModifiedProperties ();
			m_ElementDeleted = false;
		}

		protected bool DrawGroupMenu(UnityEngine.Object parentRef, SerializedProperty elementProp, SerializedProperty elementCollectionProp, SerializedProperty parentCollectionProp, int elementIndex) {
			bool hasAddedElement = false;

			if (popupList == null) popupList = new List<GUIContent>();
			else popupList.Clear();

			popupList.AddRange(menuPopup);
			popupList.AddRange(graphicPopup);
			popupList.AddRange(elementPopup);
			if (parentCollectionProp != null) popupList.AddRange(collectionPopup);

			int popupIndex = EditorGUILayout.Popup(GUIContent.none, 0, popupList.ToArray(), saveButtonWidth);

			if (popupIndex > 0 && popupIndex <= graphicPopup.Length) 
				hasAddedElement = AddGraphic(elementProp, elementCollectionProp, parentCollectionProp, elementIndex, popupIndex);
			else if (popupIndex-graphicPopup.Length > 0 && popupIndex-graphicPopup.Length <= elementPopup.Length) 
				ModifyElement(elementProp, popupIndex-graphicPopup.Length);
			else if (popupIndex-graphicPopup.Length-elementPopup.Length > 0 && popupIndex-graphicPopup.Length-elementPopup.Length <= collectionPopup.Length) 
				ModifyCollection(parentRef, elementProp, parentCollectionProp, elementIndex, popupIndex-graphicPopup.Length-elementPopup.Length);

			return hasAddedElement;
		}
		protected bool DrawAppearanceMenu(UnityEngine.Object parentRef, SerializedProperty elementProp, SerializedProperty elementCollectionProp, SerializedProperty parentCollectionProp, int elementIndex) {
			bool hasAddedElement = false;

			if (popupList == null) popupList = new List<GUIContent>();
			else popupList.Clear();

			popupList.AddRange(menuPopup);
			popupList.AddRange(stylePopup);
			popupList.AddRange(elementPopup);
			if (parentCollectionProp != null) popupList.AddRange(collectionPopup);

			int popupIndex = EditorGUILayout.Popup(GUIContent.none, 0, popupList.ToArray(), saveButtonWidth);

			if (popupIndex > 0 && popupIndex <= stylePopup.Length) 
				hasAddedElement = AddStyle(elementProp, elementCollectionProp, elementIndex, popupIndex);
			else if (popupIndex-stylePopup.Length > 0 && popupIndex-stylePopup.Length <= elementPopup.Length) 
				ModifyElement(elementProp, popupIndex-stylePopup.Length);
			else if (popupIndex-stylePopup.Length-elementPopup.Length > 0 && popupIndex-stylePopup.Length-elementPopup.Length <= collectionPopup.Length) 
				ModifyCollection(parentRef, elementProp, parentCollectionProp, elementIndex, popupIndex-stylePopup.Length-elementPopup.Length);

			return hasAddedElement;
		}
		protected void DrawElementMenu(UnityEngine.Object parentRef, SerializedProperty elementProp, SerializedProperty parentCollectionProp, int elementIndex) {
			if (popupList == null) popupList = new List<GUIContent>();
			else popupList.Clear();

			popupList.AddRange(menuPopup);
			popupList.AddRange(elementPopup);
			if (parentCollectionProp != null) popupList.AddRange(collectionPopup);

			int popupIndex = EditorGUILayout.Popup(GUIContent.none, 0, popupList.ToArray(), saveButtonWidth);

			if (popupIndex > 0 && popupIndex <= elementPopup.Length) 
				ModifyElement(elementProp, popupIndex);
			else if (popupIndex-elementPopup.Length > 0 && popupIndex-elementPopup.Length <= collectionPopup.Length) 
				ModifyCollection(parentRef, elementProp, parentCollectionProp, elementIndex, popupIndex-elementPopup.Length);
			
		}
		protected void DrawEmptyMenu(UnityEngine.Object parentRef, SerializedProperty elementProp, SerializedProperty parentCollectionProp, int elementIndex) {
			if (popupList == null) popupList = new List<GUIContent>();
			else popupList.Clear();

			popupList.AddRange(menuPopup);
			if (parentCollectionProp != null) popupList.AddRange(collectionPopup);

			int popupIndex = EditorGUILayout.Popup(GUIContent.none, 0, popupList.ToArray(), saveButtonWidth);

			if (popupIndex > 0 && popupIndex <= collectionPopup.Length) 
				ModifyCollection(parentRef, elementProp, parentCollectionProp, elementIndex, popupIndex);
		}

		protected bool AddGraphic(SerializedProperty elementProp, SerializedProperty elementCollectionProp, SerializedProperty parentCollectionProp, int elementIndex, int popupIndex) {
			LW_Element elementRef = elementProp.objectReferenceValue as LW_Element;
			if (elementRef != null) {
				float arcAprox = LW_Shape.c_ArcAprox;
				LW_Graphic newElement = null;
				switch(popupIndex) {
				case 1:
					newElement = ScriptableObject.CreateInstance<LW_Circle> ();
					break;
				case 2: 
					newElement = ScriptableObject.CreateInstance<LW_Ellipse> ();
					break;
				case 3:
					newElement = ScriptableObject.CreateInstance<LW_Rectangle> ();
					break;
				case 4:
					newElement = ScriptableObject.CreateInstance<LW_Line> ();
					break;
				case 5:
					newElement = ScriptableObject.CreateInstance<LW_Polygon> ();
					break;
				case 6:
					newElement = ScriptableObject.CreateInstance<LW_Star> ();
					break;
				case 7:
					newElement = ScriptableObject.CreateInstance<LW_Polyline2D> ();
					LW_Polyline2D polyline2d = newElement as LW_Polyline2D;
					List<Vector2> polyPoints2d = new List<Vector2>(3);
					polyPoints2d.Add(new Vector2(-40,40));
					polyPoints2d.Add(new Vector2(0,-40));
					polyPoints2d.Add(new Vector2(40,40));
					polyline2d.points = polyPoints2d;
					break;
				case 8:
					newElement = ScriptableObject.CreateInstance<LW_Polyline3D> ();
					LW_Polyline3D polyline3d = newElement as LW_Polyline3D;
					List<Vector3> polyPoints3d = new List<Vector3>(3);
					polyPoints3d.Add(new Vector3(-40,40,-40));
					polyPoints3d.Add(new Vector3(0,-40,40));
					polyPoints3d.Add(new Vector3(40,40,-40));
					polyline3d.points = polyPoints3d;
					break;
				case 9:
					newElement = ScriptableObject.CreateInstance<LW_Path2D> ();
					LW_Path2D path2d = newElement as LW_Path2D;
					List<LW_Point2D> pathPoints2d = new List<LW_Point2D>(3);
					pathPoints2d.Add(new LW_Point2D(new Vector2(-40,40), new Vector2(-40*arcAprox,0), new Vector3(40*arcAprox,0), PointType.Symetric));
					pathPoints2d.Add(new LW_Point2D(new Vector2(0,-40), new Vector2(-40*arcAprox,0), new Vector3(40*arcAprox,0), PointType.Symetric));
					pathPoints2d.Add(new LW_Point2D(new Vector2(40,40), new Vector2(-40*arcAprox,0), new Vector3(40*arcAprox,0), PointType.Symetric));
					path2d.points = pathPoints2d;
					break;
				case 10:
					newElement = ScriptableObject.CreateInstance<LW_Path3D> ();
					LW_Path3D path3d = newElement as LW_Path3D;
					List<LW_Point3D> pathPoints3d = new List<LW_Point3D>(3);
					pathPoints3d.Add(new LW_Point3D(new Vector3(-40,40,-40), new Vector3(-40*arcAprox,0,0), new Vector3(40*arcAprox,0,0), PointType.Symetric));
					pathPoints3d.Add(new LW_Point3D(new Vector3(0,-40,40), new Vector3(-40*arcAprox,0,0), new Vector3(40*arcAprox,0,0), PointType.Symetric));
					pathPoints3d.Add(new LW_Point3D(new Vector3(40,40,-40), new Vector3(-40*arcAprox,0,0), new Vector3(40*arcAprox,0,0), PointType.Symetric));
					path3d.points = pathPoints3d;
					break;
				case 11:  
					// New Group
					newElement = ScriptableObject.CreateInstance<LW_Group> ();
					break;
				case 12:
					// New Empty
					if (elementCollectionProp != null) {
						elementCollectionProp.arraySize++;
						SerializedProperty newElementProperty = elementCollectionProp.GetArrayElementAtIndex(elementCollectionProp.arraySize-1);
						newElementProperty.objectReferenceValue = null;
					}
					break;
				case 13:
					//Extract to Child
					LW_Canvas baseCanvas = serializedObject.targetObject as LW_Canvas;
					LW_Canvas newCanvas = LW_Canvas.Create(baseCanvas.gameObject, elementProp.objectReferenceValue.name);
					newCanvas.rectTransform.localPosition = -baseCanvas.viewBox.position;
					LW_Group groupRef = elementProp.objectReferenceValue as LW_Group;
					//newCanvas.graphic = groupRef.Copy() as LW_Group;
					newCanvas.graphic = groupRef;
					newCanvas.SetViewBoxToBounds();
					newCanvas.rectTransform.localPosition += (Vector3)newCanvas.viewBox.position;
					newCanvas.SetRectSizeToViewBox();

					if (m_SelectedGraphic == elementRef) {
						m_SelectedGraphic = null;
						m_SelectedAppearance = null;
					}
					if (m_SelectedAppearance == elementRef) {
						m_SelectedAppearance = null;
					}

					int oldSize = parentCollectionProp.arraySize;
					parentCollectionProp.DeleteArrayElementAtIndex(elementIndex);
					if (parentCollectionProp.arraySize == oldSize) {
						parentCollectionProp.DeleteArrayElementAtIndex(elementIndex);
					}

					m_ElementDeleted = true;
					break;
				}
				if (newElement != null && elementCollectionProp != null) {
					if (AssetDatabase.Contains(elementRef)) newElement.SaveToAssetDatabase(elementRef);

					elementCollectionProp.arraySize++;
					SerializedProperty newElementProperty = elementCollectionProp.GetArrayElementAtIndex(elementCollectionProp.arraySize-1);
					newElementProperty.objectReferenceValue = newElement;

					m_SelectedGraphic = newElement;
					return true;
				}
				else return false;
			}
			else return false;
		}
		protected bool AddStyle(SerializedProperty elementProp, SerializedProperty elementCollectionProp, int elementIndex, int popupIndex) {
			LW_Element elementRef = elementProp.objectReferenceValue as LW_Element;
			if (elementRef != null) {
				LW_Style newElement = null;
				switch(popupIndex) {
				case 1:
					newElement = ScriptableObject.CreateInstance<LW_Fill> ();
					break;
				case 2: 
					newElement = ScriptableObject.CreateInstance<LW_Stroke> ();
					break;
				case 3:
					newElement = ScriptableObject.CreateInstance<LW_Marker> ();
					break;
				case 4:
					newElement = ScriptableObject.CreateInstance<LW_Collider> ();
					break;
				}
				if (newElement != null && elementCollectionProp != null) {
					if (AssetDatabase.Contains(elementRef)) newElement.SaveToAssetDatabase(elementRef);

					elementCollectionProp.arraySize++;
					SerializedProperty newElementProperty = elementCollectionProp.GetArrayElementAtIndex(elementCollectionProp.arraySize-1);
					newElementProperty.objectReferenceValue = newElement;

					//m_SelectedAppearanceProp.objectReferenceValue = newElement;
					m_SelectedAppearance = newElement;
					return true;
				}
				else return false;
			}
			else return false;
		}
		protected void ModifyElement(SerializedProperty elementProp, int popupIndex) {
			LW_Element elementRef = elementProp.objectReferenceValue as LW_Element;
			if (elementRef != null) {
				switch(popupIndex) {
				case 1:
					// Save
					Save(elementProp);
					break;
				case 2: 
					// Copy
					Copy(elementProp);
					break;
				}
			}
		}
		protected void ModifyCollection(UnityEngine.Object parentRef, SerializedProperty elementProp, SerializedProperty parentCollectionProp, int elementIndex, int popupIndex) {
			LW_Element elementRef = elementProp.objectReferenceValue as LW_Element;
			switch(popupIndex) {
			case 1:
				// Move up
				parentCollectionProp.MoveArrayElement (elementIndex, elementIndex - 1);
				break;
			case 2:
				// Move down
				parentCollectionProp.MoveArrayElement (elementIndex, elementIndex + 1);
				break;
			case 3:
				// Duplicate
				parentCollectionProp.InsertArrayElementAtIndex (elementIndex);
				if (elementRef != null) {
					SerializedProperty newElementProp = parentCollectionProp.GetArrayElementAtIndex (elementIndex+1);
					LW_Element copiedElementRef = elementRef.Copy() as LW_Element;
					if (parentRef != null && AssetDatabase.Contains(parentRef)) copiedElementRef.SaveToAssetDatabase(parentRef);
					newElementProp.objectReferenceValue = copiedElementRef;
				}
				//Resources.UnloadUnusedAssets();
				break;
			case 4:
				// Delete

				if (m_SelectedGraphic == elementRef) {
					m_SelectedGraphic = null;
					m_SelectedAppearance = null;
				}
				if (m_SelectedAppearance == elementRef) {
					m_SelectedAppearance = null;
				}

				int oldSize = parentCollectionProp.arraySize;
				parentCollectionProp.DeleteArrayElementAtIndex(elementIndex);
				if (parentCollectionProp.arraySize == oldSize) {
					parentCollectionProp.DeleteArrayElementAtIndex(elementIndex);
				}
				m_ElementDeleted = true;
				break;
			default:
				Debug.LogError("Unrecognized Option");
				break;
			}
		}

		// Actions
		protected void Save(SerializedProperty elementProp) {
			LW_Element elementRef = elementProp.objectReferenceValue as LW_Element;

			if (AssetDatabase.Contains(elementRef)) {
				if (EditorUtility.DisplayDialog(
					"Copy then Save element?", 
					"The LW_Element is already saved to the project. Would you like to make a Copy and Save that?",
					"Copy and Save", 
					"Cancel")) {
					LW_Element copiedElementRef = elementRef.Copy() as LW_Element;
					copiedElementRef.SaveToAssetDatabase();

					if (m_SelectedGraphic == elementRef) {
						m_SelectedGraphic = null;
						m_SelectedAppearance = null;
					}
					if (m_SelectedAppearance == elementRef) {
						m_SelectedAppearance = null;
					}

					if (EditorUtility.DisplayDialog(
						"Replace original element?", 
						"Would you like to Replace the original element with the Copy and Destroy the original?",
						"Replace and Destroy", 
						"Keep Original")) {
						elementProp.objectReferenceValue = copiedElementRef;
						elementRef = LW_Utilities.SafeDestroy<LW_Element>(elementRef, true);
						m_ElementDeleted = (elementRef == null);
					}
				}
			}
			else {
				elementRef.SaveToAssetDatabase();
			}
		}
		protected void Copy(SerializedProperty elementProp) {
			LW_Element elementRef = elementProp.objectReferenceValue as LW_Element;
			LW_Element copiedElementRef = elementRef.Copy() as LW_Element;

			if (m_SelectedGraphic == elementRef) {
				m_SelectedGraphic = copiedElementRef as LW_Graphic;
				m_SelectedAppearance = null;
			}
			if (m_SelectedAppearance == elementRef) {
				m_SelectedAppearance = copiedElementRef as LW_Appearance;
			}
				
			//if (AssetDatabase.Contains(elementRef)) {
			//	copiedElementRef.SaveToAssetDatabase();
			//	elementProp.objectReferenceValue = copiedElementRef;
			//	elementRef = LW_Utilities.SafeDestroy<LW_Element>(elementRef, true);
			//	m_ElementDeleted = (elementRef == null);
			//}
			//else {
				elementProp.objectReferenceValue = copiedElementRef;
				elementRef = LW_Utilities.SafeDestroy<LW_Element>(elementRef, false);
				m_ElementDeleted = (elementRef == null);
			//}
		}

		// Draw Element Properties
		protected void DrawElementProperties(SerializedObject elementSO, LW_Element elementRef) {
			elementSO.Update();
			if (elementRef is LW_Graphic) DrawGraphicProperties(elementSO, elementRef as LW_Graphic);
			if (elementRef is LW_Appearance) DrawAppearanceProperties(elementSO, elementRef as LW_Appearance);
			elementSO.ApplyModifiedProperties();
		}
		protected void DrawGraphicProperties(SerializedObject graphicSO, LW_Graphic graphicRef) {
			EditorGUILayout.BeginVertical(subGroupStyle); {
				EditorGUILayout.BeginHorizontal();
				graphicRef.m_ElementPropertiesExpanded = EditorGUILayout.Toggle(GUIContent.none, graphicRef.m_ElementPropertiesExpanded, foldoutToggleStyle, toggleButtonWidth);
					
				EditorGUILayout.LabelField("Graphic Properties", labelLargeStyle);
				EditorGUILayout.EndHorizontal();

				if (graphicRef.m_ElementPropertiesExpanded) {
					// LW_Element Properties
					EditorGUILayout.LabelField("Element", labelLargeStyle);
					EditorGUILayout.PropertyField(graphicSO.FindProperty("m_Name"));
					EditorGUILayout.PropertyField(graphicSO.FindProperty("m_IsVisible"));
					EditorGUILayout.Separator();

					// LW_Graphic Properties
					EditorGUILayout.LabelField("Transform", labelLargeStyle);
					EditorGUILayout.PropertyField(graphicSO.FindProperty("m_Position"));
					EditorGUILayout.PropertyField(graphicSO.FindProperty("m_EulerRotation"));
					EditorGUILayout.PropertyField(graphicSO.FindProperty("m_Scale"));
					EditorGUILayout.Separator();

					// LW_Shape Properties
					if (graphicRef is LW_Shape) {
						EditorGUILayout.LabelField("Shape", labelLargeStyle);
						EditorGUILayout.PropertyField(graphicSO.FindProperty("m_ReverseDirection"));
						EditorGUILayout.PropertyField(graphicSO.FindProperty("m_EditorColor"));
					}
				}

			}EditorGUILayout.EndVertical();

			// Specific Shape Properties
			if (graphicRef is LW_Shape) DrawShapeProperties(graphicSO, graphicRef as LW_Shape);

			// Styles Collection
			EditorGUILayout.LabelField("Styles", labelLargeStyle);
			DrawElementHeader(graphicRef, graphicSO.FindProperty("m_Styles"));
		}

		// Draw Specific Shape Properties
		protected void DrawShapeProperties(SerializedObject shapeSO, LW_Shape shapeRef) {
			EditorGUILayout.BeginVertical(subGroupStyle); {
				if (shapeRef is LW_Circle) DrawCircleProperties(shapeSO, shapeRef as LW_Circle);
				else if (shapeRef is LW_Ellipse) DrawEllipseProperties(shapeSO, shapeRef as LW_Ellipse);
				else if (shapeRef is LW_Rectangle) DrawRectangleProperties(shapeSO, shapeRef as LW_Rectangle);
				else if (shapeRef is LW_Line) DrawLineProperties(shapeSO, shapeRef as LW_Line);
				else if (shapeRef is LW_Polygon) DrawPolygonProperties(shapeSO, shapeRef as LW_Polygon);
				else if (shapeRef is LW_Star) DrawStarProperties(shapeSO, shapeRef as LW_Star);
				else if (shapeRef is LW_Polyline2D) DrawPolylineProperties(shapeSO, shapeRef as LW_Polyline2D);
				else if (shapeRef is LW_Path2D) DrawPathProperties(shapeSO, shapeRef as LW_Path2D);
				else if (shapeRef is LW_Polyline3D) DrawPolylineProperties(shapeSO, shapeRef as LW_Polyline3D);
				else if (shapeRef is LW_Path3D) DrawPathProperties(shapeSO, shapeRef as LW_Path3D);
			}EditorGUILayout.EndVertical();
		}
		protected void DrawCircleProperties(SerializedObject shape, LW_Circle shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);

			EditorGUILayout.LabelField("Circle Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Circle", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_Center"));
				EditorGUILayout.PropertyField(shape.FindProperty("m_Radius"));
			}
		}
		protected void DrawEllipseProperties(SerializedObject shape, LW_Ellipse shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Ellipse Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Ellipse", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_Center"));

				bool smallWidth = Screen.width < 333;

				// Size
				if (smallWidth) EditorGUILayout.PrefixLabel("Radius");
				EditorGUILayout.BeginHorizontal();{
					if (!smallWidth) EditorGUILayout.PrefixLabel("Radius");
					else EditorGUILayout.LabelField("", GUILayout.Width(10));
					EditorGUIUtility.labelWidth = 12;
					EditorGUILayout.PropertyField(shape.FindProperty("m_RadiusX"), new GUIContent("X"));
					EditorGUILayout.PropertyField(shape.FindProperty("m_RadiusY"), new GUIContent("Y"));
					EditorGUIUtility.labelWidth = 0;
				}EditorGUILayout.EndHorizontal();
			}
		}
		protected void DrawRectangleProperties(SerializedObject shape, LW_Rectangle shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Rectangle Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Rectangle", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_Center"));

				bool smallWidth = Screen.width < 333;

				// Size
				if (smallWidth) EditorGUILayout.PrefixLabel("Size");
				EditorGUILayout.BeginHorizontal();{
					if (!smallWidth) EditorGUILayout.PrefixLabel("Size");
					else EditorGUILayout.LabelField("", GUILayout.Width(10));
					EditorGUIUtility.labelWidth = 12;
					EditorGUILayout.PropertyField(shape.FindProperty("m_Width"), new GUIContent("W"));
					EditorGUILayout.PropertyField(shape.FindProperty("m_Height"), new GUIContent("H"));
					EditorGUIUtility.labelWidth = 0;
				}EditorGUILayout.EndHorizontal();

				// Radius
				if (smallWidth) EditorGUILayout.PrefixLabel("Radius");
				EditorGUILayout.BeginHorizontal();{
					if (!smallWidth) EditorGUILayout.PrefixLabel("Radius");
					else EditorGUILayout.LabelField("", GUILayout.Width(10));
					EditorGUIUtility.labelWidth = 12;
					EditorGUILayout.PropertyField(shape.FindProperty("m_RadiusX"), new GUIContent("X"));
					EditorGUILayout.PropertyField(shape.FindProperty("m_RadiusY"), new GUIContent("Y"));
					EditorGUIUtility.labelWidth = 0;
				}EditorGUILayout.EndHorizontal();
			}
		}
		protected void DrawLineProperties(SerializedObject shape, LW_Line shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Line Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Line", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_Start"));
				EditorGUILayout.PropertyField(shape.FindProperty("m_End"));
			}
		}
		protected void DrawPolygonProperties(SerializedObject shape, LW_Polygon shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Polygon Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Polygon", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_Center"));
				EditorGUILayout.PropertyField(shape.FindProperty("m_Radius"));
				EditorGUILayout.PropertyField(shape.FindProperty("m_Sides"));
			}
		}
		protected void DrawStarProperties(SerializedObject shape, LW_Star shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Star Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Star", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_Center"));
				EditorGUILayout.PropertyField(shape.FindProperty("m_Sides"));

				// Radius XY
				bool smallWidth = Screen.width < 333;
				if (smallWidth) EditorGUILayout.PrefixLabel("Radius");
				EditorGUILayout.BeginHorizontal();{
					if (!smallWidth) EditorGUILayout.PrefixLabel("Radius");
					else EditorGUILayout.LabelField("", GUILayout.Width(10));
					EditorGUIUtility.labelWidth = 12;
					EditorGUILayout.PropertyField(shape.FindProperty("m_RadiusX"), new GUIContent("X"));
					EditorGUILayout.PropertyField(shape.FindProperty("m_RadiusY"), new GUIContent("Y"));
					EditorGUIUtility.labelWidth = 0;
				}EditorGUILayout.EndHorizontal();
			}
		}
		protected void DrawPolylineProperties(SerializedObject shape, LW_Shape shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Polyline Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Polyline", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_IsClosed"));

				SerializedProperty points = shape.FindProperty("m_Points");
				if (points != null && points.arraySize > 0) {
					DrawList(shape.targetObject as LW_Element, points, "Points");
				}
				else if (GUILayout.Button("Add Point")) {
					points.arraySize++;
					shape.ApplyModifiedProperties();
				}
			}
		}
		protected void DrawPathProperties(SerializedObject shape, LW_Shape shapeRef) {
			EditorGUILayout.BeginHorizontal();
			shapeRef.m_SpecificShapeExpanded = EditorGUILayout.Toggle(GUIContent.none, shapeRef.m_SpecificShapeExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Path Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (shapeRef.m_SpecificShapeExpanded) { 
				//EditorGUILayout.LabelField("Path", labelLargeStyle);
				EditorGUILayout.PropertyField(shape.FindProperty("m_IsClosed"));

				SerializedProperty points = shape.FindProperty("m_Points");
				if (points != null && points.arraySize > 0) {
					DrawList(shape.targetObject as LW_Element, points, "Points");
				}
				else if (GUILayout.Button("Add Point")) {
					points.arraySize++;
					shape.ApplyModifiedProperties();
				}
			}
		}

		// Draw Style Properties
		protected void DrawAppearanceProperties(SerializedObject appearanceSO, LW_Appearance appearanceRef) {
			EditorGUILayout.BeginVertical(subGroupStyle); {
				EditorGUILayout.BeginHorizontal();
				appearanceRef.m_ElementPropertiesExpanded = EditorGUILayout.Toggle(GUIContent.none, appearanceRef.m_ElementPropertiesExpanded, foldoutToggleStyle, toggleButtonWidth);

				EditorGUILayout.LabelField("Appearance Properties", labelLargeStyle);
				EditorGUILayout.EndHorizontal();

				if (appearanceRef.m_ElementPropertiesExpanded) {
					// LW_Element Properties
					EditorGUILayout.LabelField("Element", labelLargeStyle);
					EditorGUILayout.PropertyField(appearanceSO.FindProperty("m_Name"));
					EditorGUILayout.PropertyField(appearanceSO.FindProperty("m_IsVisible"));
					EditorGUILayout.Separator();

					// LW_Shape Properties
					if (appearanceRef is LW_Style) {
						//EditorGUI.BeginChangeCheck();
						EditorGUILayout.LabelField("Style", labelLargeStyle);
						EditorGUILayout.PropertyField(appearanceSO.FindProperty("m_LateralOffset"));
						EditorGUILayout.PropertyField(appearanceSO.FindProperty("m_VerticalOffset"));
						EditorGUILayout.PropertyField(appearanceSO.FindProperty("m_SegmentationMultiplier"));
						EditorGUILayout.PropertyField(appearanceSO.FindProperty("m_SimplificationMultiplier"));
						EditorGUILayout.PropertyField(appearanceSO.FindProperty("m_PresizeVBO"), new GUIContent("Pre-Size Buffer"));
						//EditorGUILayout.Separator();
						//if (EditorGUI.EndChangeCheck()) styleRef.isGeometryDirty = true;
					}
				}

			}EditorGUILayout.EndVertical();

			// Specific Shape Properties
			if (appearanceRef is LW_Style) DrawStyleProperties(appearanceSO, appearanceRef as LW_Style);
		}
		protected void DrawStyleProperties(SerializedObject styleSO, LW_Style styleRef) {
			if (styleRef is LW_PaintStyle) DrawColorStyleProperties(styleSO, styleRef as LW_PaintStyle);

			EditorGUILayout.BeginVertical(subGroupStyle); {
				if (styleRef is LW_Fill) DrawFillProperties(styleSO, styleRef as LW_Fill);
				else if (styleRef is LW_Stroke) DrawStrokeProperties(styleSO, styleRef as LW_Stroke);
				else if (styleRef is LW_Marker) DrawMarkerProperties(styleSO, styleRef as LW_Marker);
				else if (styleRef is LW_Collider) DrawCollider(styleSO, styleRef as LW_Collider);
			}EditorGUILayout.EndVertical();
		}

		protected void DrawColorStyleProperties(SerializedObject styleSO, LW_PaintStyle paintRef) {
			EditorGUILayout.BeginVertical(subGroupStyle); {
				EditorGUILayout.BeginHorizontal();
				paintRef.m_PaintExpanded = EditorGUILayout.Toggle(GUIContent.none, paintRef.m_PaintExpanded, foldoutToggleStyle, toggleButtonWidth);
				EditorGUILayout.LabelField("Paint Properties", labelLargeStyle);
				EditorGUILayout.EndHorizontal();

				if (paintRef.m_PaintExpanded) {
					//EditorGUILayout.LabelField("Appearance", labelLargeStyle);
					SerializedProperty materialProp = styleSO.FindProperty("m_Material");
					EditorGUILayout.PropertyField(materialProp, new GUIContent("Custom Material"));
					if (materialProp.objectReferenceValue == null) {
						SerializedProperty textureProp = styleSO.FindProperty("m_MainTexture");
						EditorGUILayout.PropertyField(textureProp, new GUIContent("Main Texture"));
						if (textureProp.objectReferenceValue != null || paintRef.material != null) {
							EditorGUILayout.PropertyField(styleSO.FindProperty("m_UvTiling"), new GUIContent("UV Tiling"));
							EditorGUILayout.PropertyField(styleSO.FindProperty("m_UvOffset"), new GUIContent("UV Offset"));
						}
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_UvMode"), new GUIContent("UV Mode"));
					}

					EditorGUILayout.Separator();
					EditorGUILayout.Slider(styleSO.FindProperty("m_Opacity"), 0, 1);

					EditorGUILayout.Separator();
					SerializedProperty paintModeProp = styleSO.FindProperty("m_PaintMode");
					EditorGUILayout.PropertyField(paintModeProp);
					SerializedProperty colorsProp = styleSO.FindProperty("m_GradientColors");
					if ((PaintMode)paintModeProp.enumValueIndex == PaintMode.Solid) {
						SerializedProperty stopProp = colorsProp.GetArrayElementAtIndex(0);
						if (stopProp != null) {
							SerializedProperty colorProp = stopProp.FindPropertyRelative("m_Value");
							if (colorProp != null) EditorGUILayout.PropertyField(colorProp);
						}
						else colorsProp.arraySize++;
					}
					else {
						DrawList(paintRef, colorsProp, "Gradient Colors");

						EditorGUILayout.LabelField("Gradient Transform", labelLargeStyle);
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_GradientPosition"));
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_GradientRotation"));
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_GradientScale"));
						EditorGUILayout.Separator();

						//EditorGUILayout.PropertyField(styleSO.FindProperty("m_GradientSpreadMethod"));
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_GradientUnits"));
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_GradientStart"));
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_GradientEnd"));
					}
				}
			}EditorGUILayout.EndVertical();
		}
		protected void DrawFillProperties(SerializedObject styleSO, LW_Fill fillRef) {
			EditorGUILayout.BeginHorizontal();
			fillRef.m_SpecificStyleExpanded = EditorGUILayout.Toggle(GUIContent.none, fillRef.m_SpecificStyleExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Fill Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (fillRef.m_SpecificStyleExpanded) {
				//EditorGUILayout.LabelField("Fill", labelLargeStyle);
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_FillRule"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_LandscapeDepth"));
			}
		}
		protected void DrawStrokeProperties(SerializedObject styleSO, LW_Stroke strokeRef) {
			EditorGUILayout.BeginHorizontal();
			strokeRef.m_SpecificStyleExpanded = EditorGUILayout.Toggle(GUIContent.none, strokeRef.m_SpecificStyleExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Stroke Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (strokeRef.m_SpecificStyleExpanded) {
				//EditorGUILayout.LabelField("Stroke", labelLargeStyle);
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_GlobalWidth"));
				DrawList(strokeRef, styleSO.FindProperty("m_VariableWidths"), "Variable Widths");
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_SpaceWidthsEvenly"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_SpaceColorsEvenly"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_ScreenSpace"));
				SerializedProperty linecapProp = styleSO.FindProperty("m_Linecap");
				EditorGUILayout.PropertyField(linecapProp);
				if (linecapProp.enumValueIndex == 3) {
					EditorGUILayout.PropertyField(styleSO.FindProperty("m_CapTexture"));
				}
				SerializedProperty linejoinProp = styleSO.FindProperty("m_Linejoin");
				EditorGUILayout.PropertyField(linejoinProp);
				if (linejoinProp.enumValueIndex == 3) {
					EditorGUILayout.PropertyField(styleSO.FindProperty("m_JoinTexture"));
				}
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_MiterLimit"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_Angle"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_Justification"));
			}
		}
		protected void DrawMarkerProperties(SerializedObject styleSO, LW_Marker markerRef) {
			EditorGUILayout.BeginHorizontal();
			markerRef.m_SpecificStyleExpanded = EditorGUILayout.Toggle(GUIContent.none, markerRef.m_SpecificStyleExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Marker Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (markerRef.m_SpecificStyleExpanded) {
				EditorGUILayout.LabelField("Marker", labelLargeStyle);
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_Graphic"));
				EditorGUILayout.Separator();

				EditorGUILayout.LabelField("Transform", labelLargeStyle);
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_Position"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_EulerRotation"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_Scale"));
				SerializedProperty scaleProp = styleSO.FindProperty("m_ScaleWithStroke");
				EditorGUILayout.PropertyField(scaleProp);
				if (scaleProp.boolValue) {
					EditorGUILayout.PropertyField(styleSO.FindProperty("m_Stroke"));
				}
				else {
					DrawList(markerRef, styleSO.FindProperty("m_VariableScales"), "Variable Scales");
				}

				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_FaceForward"));
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_Angle"));
				EditorGUILayout.Separator();

				//EditorGUILayout.LabelField("Start", labelLargeStyle);
				SerializedProperty atStartProp = styleSO.FindProperty("m_AtStart");
				EditorGUILayout.PropertyField(atStartProp);
				if (atStartProp.boolValue) {
					EditorGUILayout.PropertyField(styleSO.FindProperty("m_FlipStart"));
					EditorGUILayout.Separator();
				}

				//EditorGUILayout.LabelField("End", labelLargeStyle);
				SerializedProperty atEndProp = styleSO.FindProperty("m_AtEnd");
				EditorGUILayout.PropertyField(atEndProp);
				if (atEndProp.boolValue) {
					EditorGUILayout.PropertyField(styleSO.FindProperty("m_FlipEnd"));
					EditorGUILayout.Separator();
				}
					
				//EditorGUILayout.LabelField("Middle", labelLargeStyle);
				SerializedProperty atMiddleProp = styleSO.FindProperty("m_AtMiddle");
				EditorGUILayout.PropertyField(atMiddleProp);
				if (atMiddleProp.boolValue) {
					SerializedProperty placementProp = styleSO.FindProperty("m_PlacementMode");
					EditorGUILayout.PropertyField(placementProp);
					if ((PlacementMode)placementProp.enumValueIndex == PlacementMode.AtFixedLengths) {
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_FixedSpacingLength"));
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_FixedJustification"));
					}
					if ((PlacementMode)placementProp.enumValueIndex != PlacementMode.AtEveryPoint) {
						EditorGUILayout.PropertyField(styleSO.FindProperty("m_NumberOfMarkers"));
					}
				}
			}
		}
		protected void DrawCollider(SerializedObject styleSO, LW_Collider colliderRef) {
			EditorGUILayout.BeginHorizontal();
			colliderRef.m_SpecificStyleExpanded = EditorGUILayout.Toggle(GUIContent.none, colliderRef.m_SpecificStyleExpanded, foldoutToggleStyle, toggleButtonWidth);
			EditorGUILayout.LabelField("Collider Properties", labelLargeStyle);
			EditorGUILayout.EndHorizontal();

			if (colliderRef.m_SpecificStyleExpanded) {
				//EditorGUILayout.LabelField("Collider", labelLargeStyle);
				EditorGUILayout.PropertyField(styleSO.FindProperty("m_ColliderType"));
			}
		}

		protected void DrawList(LW_Element listElement, SerializedProperty list, string label) {
			bool wasEnabled = GUI.enabled;

			if (!list.isArray) {
				EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
				return;
			}

			EditorGUILayout.BeginVertical();{
				EditorGUILayout.BeginHorizontal();{
					EditorGUILayout.LabelField("", GUILayout.Width(7f));
					EditorGUILayout.PropertyField(list, false);
				}EditorGUILayout.EndHorizontal();

				if (list.isExpanded) {
					SerializedProperty size = list.FindPropertyRelative("Array.size");
					if (size.hasMultipleDifferentValues) {
						EditorGUILayout.HelpBox("Not showing lists with different sizes.", MessageType.Info);
					}
					else {
						for (int i = 0; i < list.arraySize; i++) {
							SerializedProperty prop = list.GetArrayElementAtIndex(i);
							EditorGUILayout.BeginHorizontal(subGroupStyle);{
								EditorGUILayout.PropertyField(prop);

								GUI.enabled = i > 0;
								if (GUILayout.Button (moveUpButtonContent, miniButtonLeftStyle, miniButtonWidth) && i > 0) {
									list.MoveArrayElement(i, i + 1);
								}
								GUI.enabled = i < list.arraySize - 1;
								if (GUILayout.Button (moveDownButtonContent, miniButtonMidStyle, miniButtonWidth) && i < list.arraySize - 1) {
									list.MoveArrayElement(i, i + 1);
								}
								GUI.enabled = wasEnabled;
								if (GUILayout.Button(duplicateButtonContent, miniButtonMidStyle, miniButtonWidth)) {
									list.InsertArrayElementAtIndex(i);
								}
								if (GUILayout.Button(deleteButtonContent, miniButtonRightStyle, miniButtonWidth)) {
									int oldSize = list.arraySize;
									list.DeleteArrayElementAtIndex(i);
									if (list.arraySize == oldSize) {
										list.DeleteArrayElementAtIndex(i);
									}
								}
							}EditorGUILayout.EndHorizontal();
							//EditorGUILayout.Separator();
						}
						if (list.arraySize == 0 && GUILayout.Button("Add", miniButtonStyle)) {
							list.arraySize += 1;
						}
					}
				}
			}EditorGUILayout.EndVertical();
		}

		/*
		protected Matrix4x4 DrawMatrix(Matrix4x4 oldMatrix) {
			Vector3 oldPosition = oldMatrix.GetPosition();
			Vector3 oldRotation = oldMatrix.GetRotation().eulerAngles;
			Vector3 oldScale = oldMatrix.GetScale();
			Vector3 newPosition = EditorGUILayout.Vector3Field("Position", oldPosition);
			Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation", oldRotation);
			Vector3 newScale = EditorGUILayout.Vector3Field("Scale", oldScale);
			EditorGUILayout.Separator();
			if (newPosition != oldPosition || newRotation != oldRotation || newScale != oldScale) {
				Matrix4x4 newMatrix = Matrix4x4.TRS(newPosition, Quaternion.Euler(newRotation), newScale);
				return newMatrix;
			}
			else return oldMatrix;
		}
		*/
	}
		
	[CustomPropertyDrawer(typeof(LW_Point2D))]
	public class LW_Point2DDrawer : LW_PointDrawer {
		protected override void EnforcePointType(SerializedProperty property, bool keepHandleIn = true) {
			SerializedProperty handleInProperty = property.FindPropertyRelative("m_HandleIn");
			SerializedProperty handleOutProperty = property.FindPropertyRelative("m_HandleOut");
			SerializedProperty pointTypeProperty = property.FindPropertyRelative("m_PointType");
			PointType type = (PointType)pointTypeProperty.enumValueIndex;
			if (keepHandleIn) {
				if (type == PointType.Smooth) {
					handleOutProperty.vector2Value = -handleInProperty.vector2Value.normalized * handleOutProperty.vector2Value.magnitude;
				}
				else if (type == PointType.Symetric) {
					handleOutProperty.vector2Value = -handleInProperty.vector2Value;
				}
			}
			else {
				if (type == PointType.Smooth) {
					handleInProperty.vector2Value = -handleOutProperty.vector2Value.normalized * handleInProperty.vector2Value.magnitude;
				}
				else if (type == PointType.Symetric) {
					handleInProperty.vector2Value = -handleOutProperty.vector2Value;
				}
			}
		}
	}
	[CustomPropertyDrawer(typeof(LW_Point3D))]
	public class LW_Point3DDrawer : LW_PointDrawer {
		protected override void EnforcePointType(SerializedProperty property, bool keepHandleIn = true) {
			SerializedProperty handleInProperty = property.FindPropertyRelative("m_HandleIn");
			SerializedProperty handleOutProperty = property.FindPropertyRelative("m_HandleOut");
			SerializedProperty pointTypeProperty = property.FindPropertyRelative("m_PointType");
			PointType type = (PointType)pointTypeProperty.enumValueIndex;
			if (keepHandleIn) {
				if (type == PointType.Smooth) {
					handleOutProperty.vector3Value = -handleInProperty.vector3Value.normalized * handleOutProperty.vector3Value.magnitude;
				}
				else if (type == PointType.Symetric) {
					handleOutProperty.vector3Value = -handleInProperty.vector3Value;
				}
			}
			else {
				if (type == PointType.Smooth) {
					handleInProperty.vector3Value = -handleOutProperty.vector3Value.normalized * handleInProperty.vector3Value.magnitude;
				}
				else if (type == PointType.Symetric) {
					handleInProperty.vector3Value = -handleOutProperty.vector3Value;
				}
			}
		}
	}
	public class LW_PointDrawer : PropertyDrawer {
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			float height = 0;
			if (!property.isExpanded) height = EditorGUIUtility.singleLineHeight;
			else if (Screen.width<333) height = EditorGUIUtility.singleLineHeight*7+9;
			else height = EditorGUIUtility.singleLineHeight*4+6;
			return height;
		}
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			label.text = label.text.Remove(0,8);
			SerializedProperty positionProperty = property.FindPropertyRelative("m_Position");
			SerializedProperty handleInProperty = property.FindPropertyRelative("m_HandleIn");
			SerializedProperty handleOutProperty = property.FindPropertyRelative("m_HandleOut");
			SerializedProperty hasHandleInProperty = property.FindPropertyRelative("m_HasHandleIn");
			SerializedProperty hasHandleOutProperty = property.FindPropertyRelative("m_HasHandleOut");
			SerializedProperty pointTypeProperty = property.FindPropertyRelative("m_PointType");

			float height = Screen.width<333 ? EditorGUIUtility.singleLineHeight*2+2 : EditorGUIUtility.singleLineHeight;
			float width = 60;
			//float indent = 18;
			float foldoutWidth = 10;
			float indexWidth = 18;

			Rect labelRect = new Rect(position.x+foldoutWidth, position.y, indexWidth, height);
			Rect postiionRect = new Rect(position.x+foldoutWidth+indexWidth, position.y, position.width-foldoutWidth-indexWidth, height);
			Rect typeRect = new Rect(position.x+foldoutWidth+indexWidth, position.y+height+2, position.width+width-foldoutWidth-indexWidth, EditorGUIUtility.singleLineHeight);
			Rect inRect = new Rect(position.x+foldoutWidth+indexWidth, position.y+height+EditorGUIUtility.singleLineHeight+4, position.width+width-foldoutWidth-indexWidth, height);
			Rect outRect = new Rect(position.x+foldoutWidth+indexWidth, position.y+height*2+EditorGUIUtility.singleLineHeight+6, position.width+width-foldoutWidth-indexWidth, height);

			property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, GUIContent.none, EditorStyles.foldout);
			EditorGUI.LabelField(labelRect, label);
			EditorGUI.PropertyField(postiionRect, positionProperty, GUIContent.none);

			if (property.isExpanded) {
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(typeRect, pointTypeProperty, new GUIContent("Point Type"));
				if (EditorGUI.EndChangeCheck()) EnforcePointType(property);
				EditorGUI.BeginChangeCheck();
				DrawHandleGUI(inRect, handleInProperty, hasHandleInProperty, new GUIContent("Handle In"), false);
				if (EditorGUI.EndChangeCheck()) EnforcePointType(property, true);
				EditorGUI.BeginChangeCheck();
				DrawHandleGUI(outRect, handleOutProperty, hasHandleOutProperty, new GUIContent("Handle Out"), false);
				if (EditorGUI.EndChangeCheck()) EnforcePointType(property, false);
			}
		}
		protected Rect DrawHandleGUI(Rect position, SerializedProperty valueProperty, SerializedProperty isDefinedProperty, GUIContent label, bool condenseLabel = false) {
			Rect activeRect = new Rect(position.x, position.y, 14, position.height);
			position.x += 14;
			position.width -= 14;

			isDefinedProperty.boolValue = EditorGUI.Toggle(activeRect, isDefinedProperty.boolValue);

			bool wasEnabled = GUI.enabled;
			GUI.enabled = (isDefinedProperty.boolValue);
			if (!isDefinedProperty.boolValue) valueProperty.isExpanded = false;

			EditorGUIUtility.labelWidth -= 14;

			Rect valueRect = new Rect(position.x, position.y, position.width, position.height);
			EditorGUI.PropertyField(valueRect, valueProperty, label, true);
			GUI.enabled = wasEnabled;

			EditorGUIUtility.labelWidth = 0;
			return position;
		}
		protected virtual void EnforcePointType(SerializedProperty property, bool keepHandleIn = true) {
		}
	}

	[CustomPropertyDrawer(typeof(LW_ColorStop))]
	public class LW_ColorStopDrawer : LW_StopDrawer {}
	[CustomPropertyDrawer(typeof(LW_WidthStop))]
	public class LW_WidthStopDrawer : LW_StopDrawer {}
	public class LW_StopDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			label.text = label.text.Remove(0,8);
			EditorGUIUtility.labelWidth = 12;
			SerializedProperty valueProperty = property.FindPropertyRelative("m_Value");
			SerializedProperty percentageProp = property.FindPropertyRelative("m_Percentage");
			Rect valueRect = new Rect(position.x, position.y, 46, position.height);
			Rect percentageRect = new Rect(position.x+valueRect.width, position.y, position.width-46, position.height);
			EditorGUI.PropertyField(valueRect, valueProperty, label);
			EditorGUI.Slider(percentageRect, percentageProp, 0, 1, GUIContent.none);
		}
	}
}
