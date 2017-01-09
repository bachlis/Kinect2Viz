	// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System;
using System.Collections.Generic;
//using LibTessDotNet;

namespace LineWorks {

	public abstract class LW_Buffer : IDisposable {

		#if UNITY_EDITOR || DEVELOPMENT
		protected static bool s_DebugBuffer = false;
		#endif

		public bool isValid {
			get {
				return style != null && graphic != null && canvas != null && !style.isOrphaned && !graphic.isOrphaned;
			}
		}

		public int id { get { return m_Id; } set { m_Id = value; } }
		protected int m_Id = -1;

		public bool isEmpty { get { return m_IsEmpty || !isValid; } set { m_IsEmpty = value; } }
		protected bool m_IsEmpty = true;

		public LW_Style style { get { return m_Style; } set { m_Style = value; } }
		protected LW_Style m_Style = null;

		public LW_Graphic graphic { get { return m_Graphic; } set { m_Graphic = value; } }
		protected LW_Graphic m_Graphic = null;

		public LW_Canvas canvas { get { return m_Canvas; } set { m_Canvas = value; } }
		protected LW_Canvas m_Canvas = null;

		public Material material { get { return m_Material; } set { m_Material = value; } }
		protected Material m_Material = null;

		public Bounds bounds { get { return m_Bounds; } set { m_Bounds = value; } }
		protected Bounds m_Bounds = new Bounds();

		public LW_Buffer(){}

		public virtual void Clear() {
			m_Id = -1;
			m_IsEmpty = true;
			m_Style = null;
			m_Graphic = null;
			m_Canvas = null;
			m_Material = null;
			m_Bounds = new Bounds();
		}
		public virtual void Dispose() {
			m_Id = -1;
			m_IsEmpty = true;
			m_Style = null;
			m_Graphic = null;
			m_Canvas = null;
			m_Material = null;
			m_Bounds = new Bounds();
		}

		public virtual void Rebuild(LW_Graphic graphic, LW_Style style, LW_Canvas canvas, bool forceRebuild) {
			Clear();
			graphic.id = style.id = canvas.id = -1;
			this.graphic = graphic;
			this.style = style;
			this.canvas = canvas;
			this.id = GetCacheKey(graphic.id, style.id, canvas.id);
		}
		public virtual void Presize(int capacity) {
		}

		public static int GetCacheKey(int obj1, int obj2) {
			int key = 1;
			int prime = 31;
			key = prime * key + obj1;
			key = prime * key + obj2;
			return key;
		}
		public static int GetCacheKey(int obj1, int obj2, int obj3) {
			int key = 1;
			int prime = 31;
			key = prime * key + obj1;
			key = prime * key + obj2;
			key = prime * key + obj3;
			return key;
		}

		// Utilities	
		protected static Vector2 CalcPerpendicular(Vector2 v0) {
			return new Vector2(v0.y, -v0.x);
		}
		protected static Vector3 CalcPerpendicular(Vector3 v0) {
			return new Vector3(v0.y, -v0.x, v0.z);
		}
	}

	public class LW_ContourBuffer : LW_Buffer {

		public LibTessDotNet.ContourVertex[] pointsForFill {
			get {
				if (m_ContourPoints == null) 
					return null;

				if (m_ContourPointsForFill == null || m_ContourPointsForFill.Length != m_ContourPoints.Count)
					m_ContourPointsForFill = new LibTessDotNet.ContourVertex[m_ContourPoints.Count];

				for (int i=0; i<m_ContourPoints.Count; i++) {
					Vector3 point = m_ContourPoints[i];
					LibTessDotNet.ContourVertex vert = m_ContourOffsets.Count > i 
						? new LibTessDotNet.ContourVertex(){Position= new LibTessDotNet.Vec3(){X=point.x, Y=point.y, Z=point.z}, Data = m_ContourOffsets[i]} 
						: new LibTessDotNet.ContourVertex(){Position= new LibTessDotNet.Vec3(){X=point.x, Y=point.y, Z=point.z}} ;
					m_ContourPointsForFill[i] = vert;
				}

				return m_ContourPointsForFill;
			}
		}
		protected LibTessDotNet.ContourVertex[] m_ContourPointsForFill;

		public List<Vector3> points {
			get {
				return m_ContourPoints;
			}
		}
		protected List<Vector3> m_ContourPoints = LW_ListPool<Vector3>.Get();

		public List<Vector3> offsets {
			get {
				return m_ContourOffsets;
			}
		}
		protected List<Vector3> m_ContourOffsets = LW_ListPool<Vector3>.Get();

		public List<float> lengths {
			get {
				return m_ContourLengths;
			}
		}
		protected List<float> m_ContourLengths = LW_ListPool<float>.Get();

		public bool isClosed {
			get {
				return m_IsClosed;
			}
		}
		protected bool m_IsClosed = false;

		public int pointCount {
			get {
				if (m_PointCount == 0) m_PointCount = points.Count;
				return m_PointCount;
			}
		}
		protected int m_PointCount = 0;

		public float totalLength {
			get {
				if (m_TotalLength == 0) m_TotalLength = lengths[lengths.Count-1];
				return m_TotalLength;
			}
		}
		protected float m_TotalLength = 0;

		public LW_ContourBuffer() {}

		public override void Clear() {
			base.Clear();

			if (m_ContourPoints != null) m_ContourPoints.Clear();
			if (m_ContourOffsets != null) m_ContourOffsets.Clear();
			if (m_ContourLengths != null) m_ContourLengths.Clear();
			m_IsClosed = false;
			m_PointCount = 0;
			m_TotalLength = 0;
		}
		public override void Dispose() {
			base.Dispose();

			//LW_ListPool<Vector3>.Release(m_ContourPoints);
			//LW_ListPool<Vector3>.Release(m_ContourOffsets);
			//LW_ListPool<float>.Release(m_ContourLengths);
			if (m_ContourPoints != null) m_ContourPoints.Clear();
			if (m_ContourOffsets != null) m_ContourOffsets.Clear();
			if (m_ContourLengths != null) m_ContourLengths.Clear();
			//m_ContourPoints = null;
			//m_ContourOffsets = null;
			//m_ContourLengths = null;
			m_IsClosed = false;
			m_PointCount = 0;
			m_TotalLength = 0;
		}	

		public override void Rebuild(LW_Graphic graphic, LW_Style style, LW_Canvas canvas, bool forceRebuild) {
			base.Rebuild(graphic, style, canvas, forceRebuild);

			if (!(graphic is LW_Shape)) return;
			LW_Shape shape = graphic as LW_Shape;
			m_IsClosed = shape.isClosed;

			shape.RebuildShape(forceRebuild);

			float landscapeDepth = (style is LW_Fill && !shape.isClosed && (style as LW_Fill).landscapeDepth != 0) ? (style as LW_Fill).landscapeDepth : 0;
			int segmentation = (int)Mathf.Max(canvas.segmentation * style.segmentationMultiplier, 1);
			float simplification = canvas.simplification * style.simplificationMultipiler;
			bool optimize = canvas.optimize || simplification > 0;
			float lateralOffset = style.lateralOffset;
			float verticalOffset = style.verticalOffset;
			Presize(style.presizeVBO);

			shape.RebuildContourPoints(ref m_ContourPoints, segmentation);

			// Calc Simplification
			if (optimize)
				LW_Simplifier.SimplifyPoints(ref m_ContourPoints, simplification, false);

			if (m_ContourPoints.Count < 2) return;

			// Get Offsets
			shape.RebuildContourOffsets(ref m_ContourOffsets, m_ContourPoints);

			// Get Lengths
			shape.RebuildContourLengths(ref m_ContourLengths, m_ContourPoints);

			// Offset Position
			float sign = Mathf.Sign(shape.SignedArea(m_ContourPoints));
			for (int p=0; p<m_ContourPoints.Count; p++) {
				m_ContourPoints[p] = m_ContourPoints[p] + sign * m_ContourOffsets[p] * lateralOffset + Vector3.back * verticalOffset;
			}

			// Calc Bounds
			Bounds _bounds = bounds;
			bool isFirstPoint = true;
			for (int p=0; p<m_ContourPoints.Count; p++) {
				Vector3 vert = m_ContourPoints[p];
				if (isFirstPoint) {
					_bounds = new Bounds(vert, Vector3.zero);
					isFirstPoint = false;
				}
				else _bounds.Encapsulate(vert);
			}

			// Add Landscape Fill Point
			if (!shape.isClosed && landscapeDepth != 0) {
				Vector3 upperFirstPoint = m_ContourPoints[0];
				Vector3 upperLastPoint = m_ContourPoints[m_ContourPoints.Count-1];
				Vector3 newPoint1 = new Vector3(upperLastPoint.x, _bounds.min.y-landscapeDepth, upperLastPoint.z);
				Vector3 newPoint2 = new Vector3(upperFirstPoint.x, _bounds.min.y-landscapeDepth, upperFirstPoint.z);
				m_ContourPoints.Add(newPoint1);
				m_ContourPoints.Add(newPoint2);
				_bounds.Encapsulate(newPoint1);
				_bounds.Encapsulate(newPoint2);
			}
			bounds = _bounds;

			m_TotalLength = m_ContourLengths[m_ContourLengths.Count-1];
			m_PointCount = m_ContourPoints.Count;
			m_IsEmpty = false;

		}
		public override void Presize(int capacity) {
			if (m_ContourPoints == null)
				m_ContourPoints = new List<Vector3> (capacity);
			if (m_ContourOffsets == null)
				m_ContourOffsets = new List<Vector3> (capacity);
			if (m_ContourLengths == null)
				m_ContourLengths = new List<float> (capacity);

			if (m_ContourPoints.Capacity < capacity) m_ContourPoints.Capacity = capacity;
			if (m_ContourOffsets.Capacity < capacity) m_ContourOffsets.Capacity = capacity;
			if (m_ContourLengths.Capacity < capacity) m_ContourPoints.Capacity = capacity;
		}


		public Vector3 FindPointAtPercentage(float percentage) {
			float length = percentage * totalLength;
			return FindPointAtLength(length);
		}
		public Vector3 FindOffsetAtPercentage(float percentage) {
			float length = percentage * totalLength;
			return FindOffsetAtLength(length);
		}
		public Vector3 FindPointAtLength(float length) {
			if (m_IsEmpty || m_ContourPoints.Count < 2 || m_ContourLengths.Count < 2) {
				Debug.LogWarning("Cannot find Point. Contour is Empty.");
				return Vector3.zero;
			}
			int closestIndex = 0;
			while (closestIndex < m_ContourLengths.Count && m_ContourLengths[closestIndex] < length) closestIndex++;
			if (closestIndex > 0) closestIndex--;
			float prevLength = m_ContourLengths[closestIndex];
			float nextLength = m_ContourLengths[closestIndex+1];
			Vector3 prev = m_ContourPoints[closestIndex];
			Vector3 next = m_ContourPoints[closestIndex + 1];
			float t = Mathf.Clamp((length - prevLength) / (nextLength - prevLength), 0, 1);
			return Vector3.Lerp(prev, next, t);
		}
		public Vector3 FindOffsetAtLength(float length) {
			if (m_IsEmpty || m_ContourOffsets.Count < 2 || m_ContourLengths.Count < 2) {
				Debug.LogWarning("Cannot find Point. Contour is Empty.");
				return Vector3.zero;
			}
			int closestIndex = 0;
			while (closestIndex < m_ContourLengths.Count && m_ContourLengths[closestIndex] < length) closestIndex++;
			if (closestIndex > 0) closestIndex--;
			float prevLength = m_ContourLengths[closestIndex];
			float nextLength = m_ContourLengths[closestIndex+1];
			Vector3 prev = m_ContourOffsets[closestIndex];
			Vector3 next = m_ContourOffsets[closestIndex + 1];
			float t = Mathf.Clamp((length - prevLength) / (nextLength - prevLength), 0, 1);
			return Vector3.Lerp(prev, next, t);
		}
	}

	public abstract class LW_GraphicBuffer : LW_Buffer {

		public Matrix4x4 matrix  { get { return m_Matrix; } set { m_Matrix = value; } }
		protected Matrix4x4 m_Matrix = Matrix4x4.identity;

		public int pointCount {
			get {
				if (m_PointCount == 0) {
					for (int i=0; i<m_ContourBufferList.Count; i++) {
						LW_ContourBuffer contour = m_ContourBufferList[i];
						if (!contour.isValid || contour.points.Count < 2) continue;

						m_PointCount += contour.points.Count;
					}
				}
				return m_PointCount;
			}
		}
		protected int m_PointCount = 0;

		public int contourCount {
			get {
				if (m_ContourCount == 0) {
					m_ContourCount = m_ContourBufferList.Count;
				}
				return m_ContourCount;
			}
		}
		protected int m_ContourCount = 0;

		public float totalLength {
			get {
				if (m_TotalLength == 0) {
					for (int i=0; i<m_ContourBufferList.Count; i++) {
						LW_ContourBuffer contour = m_ContourBufferList[i];
						if (!contour.isValid || contour.points.Count < 2) continue;

						m_TotalLength += contour.totalLength;
					}
				}
				return m_TotalLength;
			}
		}
		protected float m_TotalLength = 0;

		protected List<LW_ContourBuffer> m_ContourBufferList = LW_ListPool<LW_ContourBuffer>.Get();

		public LW_GraphicBuffer(){}

		public override void Clear() {
			base.Clear();

			m_Matrix = Matrix4x4.identity;
			m_TotalLength = 0;
			m_PointCount = 0;
			m_ContourCount = 0;

			if (m_ContourBufferList != null) m_ContourBufferList.Clear();
		}
		public override void Dispose() {
			base.Dispose();

			m_Matrix = Matrix4x4.identity;
			m_TotalLength = 0;
			m_PointCount = 0;
			m_ContourCount = 0;

			if (m_ContourBufferList == null) return;
			for (int i=0; i<m_ContourBufferList.Count; i++) {
				if (m_ContourBufferList[i] != null) m_ContourBufferList[i].Dispose();
			}
			//LW_ListPool<LW_ContourBuffer>.Release(m_ContourBufferList);
			if (m_ContourBufferList != null) m_ContourBufferList.Clear();
			//m_ContourBufferList = null;
		}

		public override void Rebuild(LW_Graphic graphic, LW_Style style, LW_Canvas canvas, bool forceRebuild) {
			base.Rebuild(graphic, style, canvas, forceRebuild);

			if (forceRebuild || this.graphic != graphic || this.style != style || graphic.isDirty || style.isGeometryDirty || m_ContourBufferList.Count == 0) {

				// Rebuild Contours

				//List<LW_ContourBuffer> m_PrevContourBufferList = LW_ListPool<LW_ContourBuffer>.Get();
				//m_PrevContourBufferList.AddRange(m_ContourBufferList);
				//m_ContourBufferList.Clear();

				m_ContourBufferList.Clear();

				RebuildContours(graphic, style, canvas, forceRebuild);

				//for (int i=0; i< m_PrevContourBufferList.Count; i++) {
				//	if (!m_ContourBufferList.Contains(m_PrevContourBufferList[i])) LW_BufferPool<LW_ContourBuffer>.Release(m_PrevContourBufferList[i]);
				//}
				//m_PrevContourBufferList.Clear();
				//LW_ListPool<LW_ContourBuffer>.Release(m_PrevContourBufferList);

				if (m_ContourBufferList.Count == 0) return;

				// Presize Lists
				int capacity = 0;
				for (int i = 0; i < m_ContourBufferList.Count; i++) {
					LW_ContourBuffer contour = m_ContourBufferList[i];
					if (!contour.isValid || contour.points.Count < 2) continue;

					capacity += Mathf.Max(contour.points.Count, style.presizeVBO);
				}
				Presize(capacity);

				// Rebuild Bounds
				bool firstContour = true;
				for (int i = 0; i < m_ContourBufferList.Count; i++) {
					LW_ContourBuffer contour = m_ContourBufferList[i];
					if (contour.isValid && contour.points.Count < 2) continue;

					if (firstContour) {
						m_Bounds = contour.bounds;
						firstContour = false;
					}
					else m_Bounds.Encapsulate(contour.bounds);
				}
			}
		}

		protected virtual void RebuildContours(LW_Graphic graphic, LW_Style style, LW_Canvas canvas, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("RebuildContours graphic: " + graphic.name + " style: " + style.name + " forceRebuild: " + forceRebuild);
			#endif

			if (graphic is LW_Group) {
				LW_Group group = graphic as LW_Group;
				for (int i=0; i<group.Count; i++ ) {
					if (group[i] is LW_Group) RebuildContours(group[i], style, canvas, forceRebuild);
					else if (group[i] is LW_Shape) {
						LW_Shape shape = group[i] as LW_Shape;

						LW_ContourBuffer contour = LW_BufferPool<LW_ContourBuffer>.Get(shape, style, canvas);

						contour.Rebuild(shape, style, canvas, forceRebuild);

						if (!contour.isEmpty) {
							m_ContourBufferList.Add(contour);
						}
					}
				}
			}
			else if (graphic is LW_Shape) {
				LW_Shape shape = graphic as LW_Shape;

				LW_ContourBuffer contour = LW_BufferPool<LW_ContourBuffer>.Get(shape, style, canvas);

				contour.Rebuild(shape, style, canvas, forceRebuild);

				if (!contour.isEmpty) {
					m_ContourBufferList.Add(contour);
				}
			}
		}
	}

	public class LW_ColliderBuffer : LW_GraphicBuffer {

		public List<Vector2[]> paths { get { return m_Paths; } set { m_Paths = value; } }
		protected List<Vector2[]> m_Paths = LW_ListPool<Vector2[]>.Get();

		public ColliderType colliderType { get { return m_ColliderType; } set { m_ColliderType = value; } }
		protected ColliderType m_ColliderType;

		public LW_ColliderBuffer() : base() {}

		public override void Clear() {
			base.Clear();

			if (m_Paths != null) m_Paths.Clear();
		}
		public override void Dispose() {
			base.Dispose();

			for (int i=0; i<m_Paths.Count; i++) {
				m_Paths[i] = null;
			}
			//LW_ListPool<Vector2[]>.Release(m_Paths);
			if (m_Paths != null) m_Paths.Clear();
			//m_Paths = null;
		}

		public override void Rebuild(LW_Graphic graphic, LW_Style style, LW_Canvas canvas, bool forceRebuild) {
			base.Rebuild(graphic, style, canvas, forceRebuild);

			if (m_ContourBufferList.Count == 0) return;

			if (style is LW_Collider) {

				if (m_Paths.Capacity < style.presizeVBO) m_Paths.Capacity = style.presizeVBO;

				LW_Collider collider = style as LW_Collider;
				RebuildStaticCollider(collider, forceRebuild);
			}
		}
			
		private void AddPoints(Vector2[] points) {
			m_Paths.Add(points);
			m_IsEmpty = false;
		}
		public Vector2[] GetPoints(Matrix4x4 matrix) {
			Vector2[] array = new Vector2[0];
			if (m_Paths.Count > 0) {
				int count = 0;
				for (int i=0; i<m_Paths.Count; i++) if (m_Paths[i] != null) count += m_Paths[i].Length;
				array = new Vector2[count];
				int index = 0;
				for (int i=0; i<m_Paths.Count; i++) {
					if (m_Paths[i] != null) {
						for (int p=0; p<m_Paths[i].Length; p++) {
							array[index++] = matrix.MultiplyPoint3x4(m_Paths[i][p]);
						}
					}
				}
			}
			return array;
		}
		public Vector2[] GetPath(int index, Matrix4x4 matrix) {
			Vector2[] array = new Vector2[0];
			if (m_Paths.Count > index) {
				if (m_Paths[index] != null) {
					array = new Vector2[m_Paths[index].Length];
					for (int p=0; p<m_Paths[index].Length; p++) {
						array[p] = matrix.MultiplyPoint3x4(m_Paths[index][p]);
					}
				}
			}
			return array;
		}

		private void RebuildStaticCollider(LW_Collider style, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("VBO RebuildStaticCollider: " + style.isVisible + " && ( " + style.isDirty + " || " + forceRebuild + " || " + isEmpty + ")");
			#endif

			if (m_Paths.Capacity < m_ContourBufferList.Count) m_Paths.Capacity = m_ContourBufferList.Count;

			colliderType = style.colliderType;
			int pathIndex = 0;
			for (int i = 0; i < m_ContourBufferList.Count; i++) {
				LW_ContourBuffer contour = m_ContourBufferList[i];
				if (!contour.isValid || contour.points.Count < 2) continue;

				Vector2[] pointsArray = pathIndex < m_Paths.Count ? m_Paths[pathIndex] : new Vector2[contour.points.Count];
				if (pointsArray.Length != contour.points.Count) pointsArray = new Vector2[contour.points.Count];
				for (int p=0; p<pointsArray.Length; p++) pointsArray[p] = contour.points[p];
				AddPoints(pointsArray);
				pathIndex++;
			}

			if (m_Paths.Count > pathIndex) m_Paths.RemoveRange(pathIndex, m_Paths.Count-pathIndex);
		}
	}

	public class LW_MarkerBuffer : LW_GraphicBuffer {
		
		public List<LW_Graphic> graphicList { get { return m_GraphicList; } set { m_GraphicList = value; } }
		protected List<LW_Graphic> m_GraphicList = LW_ListPool<LW_Graphic>.Get();

		protected LW_Graphic m_MarkerGraphic;
		protected LW_ElementPool<LW_Graphic> m_GraphicPool = new LW_ElementPool<LW_Graphic>(null, null);

		public LW_MarkerBuffer() : base() {}

		public override void Clear() {
			base.Clear();

			if (m_GraphicList != null) m_GraphicList.Clear();

			//m_MarkerGraphic = null;
			//m_GraphicPool.Clear();
		}
		public override void Dispose() {
			base.Dispose();

			for (int i=0; i<m_GraphicList.Count; i++) {
				if (m_GraphicList[i] != null) LW_Utilities.SafeDestroy(m_GraphicList[i]);
			}
			//LW_ListPool<LW_Graphic>.Release(m_GraphicList);
			if (m_GraphicList != null) m_GraphicList.Clear();
			//m_GraphicList = null;

			m_GraphicPool.Clear();
			//m_GraphicPool = null;
		}

		public override void Rebuild(LW_Graphic graphic, LW_Style style, LW_Canvas canvas, bool forceRebuild) {
			base.Rebuild(graphic, style, canvas, forceRebuild);

			if (m_ContourBufferList.Count == 0) return;

			if (style is LW_Marker) {
				LW_Marker marker = style as LW_Marker;
				if (marker.graphic != null) RebuildStaticMarker(graphic, marker, matrix, forceRebuild);
			}
		}

		private void RebuildStaticMarker(LW_Graphic graphic, LW_Marker style, Matrix4x4 parentMatrix, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("VBO RebuildStaticMarker: " + style.isVisible + " && ( " + style.isDirty + " || " + forceRebuild + " || " + isEmpty + ")");
			#endif
			Vector3 position, offset;
			float scale;
			bool flip;

			if (m_MarkerGraphic == null || style.graphic != m_MarkerGraphic) m_GraphicPool.Clear();
			m_MarkerGraphic = style.graphic;

			for (int i = 0; i < m_ContourBufferList.Count; i++) {
				LW_ContourBuffer contour = m_ContourBufferList[i];
				if (!contour.isValid || contour.points.Count < 2) continue;

				int numOfMarkers = 0;

				if (style.atStart) {
					position = contour.FindPointAtPercentage(0);
					offset = contour.FindOffsetAtPercentage(0);
					scale = style.ScaleAtPercentage(0);
					flip = style.flipStart;

					PlaceMarker(style, position, offset, scale, flip, 0);
				}

				if (style.atEnd) {
					position = contour.FindPointAtPercentage(1);
					offset = contour.FindOffsetAtPercentage(1);
					scale = style.ScaleAtPercentage(1);
					flip = style.flipEnd;

					PlaceMarker(style, position, offset, scale, flip, 0);
				}

				if (style.atMiddle) {
					switch(style.placementMode) {
					case PlacementMode.SpaceEvenly:
						numOfMarkers = style.numberOfMarkers;
						for (int p = 0; p < numOfMarkers; p++) {
							float percentage = (float)(p+1) / (float)(numOfMarkers+1);
							position = contour.FindPointAtPercentage(percentage);
							offset = contour.FindOffsetAtPercentage(percentage);
							scale = style.ScaleAtPercentage(percentage);
							flip = false;

							PlaceMarker(style, position, offset, scale, flip, p);
						}
						break;
					case PlacementMode.AtFixedLengths:
						numOfMarkers = Mathf.FloorToInt(contour.totalLength / style.fixedSpacingLength);
						float remainder = contour.totalLength - numOfMarkers * style.fixedSpacingLength;
						float currLength = style.fixedSpacingLength;
						if (style.fixedJustification == Justification.center) currLength = remainder * 0.5f;
						else if (style.fixedJustification == Justification.right) currLength = remainder;

						for (int p = 0; p < numOfMarkers; p++) {
							float percentage = currLength / contour.totalLength;
							position = contour.FindPointAtPercentage(percentage);
							offset = contour.FindOffsetAtPercentage(percentage);
							scale = style.ScaleAtPercentage(percentage);
							flip = false;

							PlaceMarker(style, position, offset, scale, flip, p);

							currLength += style.fixedSpacingLength;
						}
						break;
					case PlacementMode.AtEveryPoint:
						numOfMarkers = contour.points.Count-2;
						for (int p = 0; p < numOfMarkers; p++) {
							float percentage = (float)(p+1) / (float)(numOfMarkers+1);
							position = contour.points[p+1];
							offset = contour.offsets[p];
							scale = style.ScaleAtPercentage(percentage);
							flip = false;

							PlaceMarker(style, position, offset, scale, flip, p);
						}
						break;
					}
				}
			}

			if (graphicList != null && graphicList.Count > 0) {
				for (int i=0; i<graphicList.Count; i++) {
					m_GraphicPool.Release(graphicList[i]);
				}
			}
		}
		private void PlaceMarker(LW_Marker style, Vector3 position, Vector3 offset, float scale, bool flip, int index = 0) {
			Vector3 angles = (style.faceForward ? Quaternion.LookRotation(Vector3.forward, -offset).eulerAngles : Vector3.zero) + style.eulerRotation;
			Vector3 scales = Vector3.Scale((flip ? new Vector3(-1,1,1) : Vector3.one), style.scale) * scale;
			Quaternion rotation = style.faceForward ? Quaternion.LookRotation(Vector3.forward, offset) : Quaternion.identity;

			Matrix4x4 mPos = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
			Matrix4x4 mScl = Matrix4x4.Scale((flip ? new Vector3(-1,1,1) : Vector3.one) * scale);
			Matrix4x4 mRot = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
			Matrix4x4 mTrn = style.transform;

			Matrix4x4 matrix = mPos * mScl * mRot * mTrn;

			LW_Graphic instance = m_GraphicPool.Get(m_MarkerGraphic);

			#if UNITY_EDITOR
			instance.SetHideFlags(HideFlags.HideAndDontSave);
			#endif

			instance.transform = matrix;
			instance.eulerRotation = angles;
			instance.scale = scales;

			m_GraphicList.Add(instance);
			m_IsEmpty = false;
		}
	}

	public class LW_VertexBuffer : LW_GraphicBuffer {

		private const float s_Root2 = 1.414213562373095f;
		private static readonly Vector4 s_DefaultTangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
		private static readonly Vector3 s_DefaultNormal = Vector3.back;
		private static readonly Vector3 s_Zero = Vector3.zero;
		private static readonly Color s_White = Color.white;

		public int subMeshIndex {
			get {
				return m_SubMeshIndex;
			}
			set {
				m_SubMeshIndex = value;
			}
		}
		protected int m_SubMeshIndex = -1;

		public int styleDataIndex {
			get {
				return m_StyleDataIndex;
			}
			set {
				if (m_StyleDataIndex != value) {
					m_StyleDataIndex = value;
				}
			}
		}
		protected int m_StyleDataIndex = -1;

		public int renderQueue {
			get {
				return m_RenderQueue;
			}
			set {
				if (m_RenderQueue != value) {
					m_RenderQueue = value;
				}
			}
		}
		protected int m_RenderQueue = -1;

		public int vertexCount {
			get {
				if (m_VertexCount == 0) m_VertexCount = m_Positions.Count;
				return m_VertexCount;
			}
		}
		private int m_VertexCount = 0;

		public int indexCount {
			get {
				if (m_IndexCount == 0) m_IndexCount = m_Indices.Count;
				return m_IndexCount;
			}
		}
		private int m_IndexCount = 0;

		public List<Vector3> positions { get { return m_Positions; } set { m_Positions = value; } }
		public List<Vector3> m_Positions = LW_ListPool<Vector3>.Get();

		public List<Vector3> normals { get { return m_Normals; } set { m_Normals = value; } }
		public List<Vector3> m_Normals = LW_ListPool<Vector3>.Get();

		public List<Vector3> tangents { get { return m_Tangents; } set { m_Tangents = value; } }
		public List<Vector3> m_Tangents = LW_ListPool<Vector3>.Get();

		public List<Vector2> uv1s { get { return m_Uv1s; } set { m_Uv1s = value; } }
		public List<Vector2> m_Uv1s = LW_ListPool<Vector2>.Get();

		public List<Vector2> uv2s { get { return m_Uv2s; } set { m_Uv2s = value; } }
		public List<Vector2> m_Uv2s = LW_ListPool<Vector2>.Get();

		public List<Color> colors { get { return m_Colors; } set { m_Colors = value; } }
		public List<Color> m_Colors = LW_ListPool<Color>.Get();

		public List<int> indices { get { return m_Indices; } set { m_Indices = value; } }
		public List<int> m_Indices = LW_ListPool<int>.Get();

		public LW_VertexBuffer() : base() {}

		public override void Clear() {
			base.Clear();

			m_SubMeshIndex = -1;
			m_StyleDataIndex = -1;
			m_RenderQueue = -1;
			m_VertexCount = 0;
			m_IndexCount = 0;
			if (m_Positions != null) m_Positions.Clear();
			if (m_Normals != null) m_Normals.Clear();
			if (m_Tangents != null) m_Tangents.Clear();
			if (m_Uv1s != null) m_Uv1s.Clear();
			if (m_Uv2s != null) m_Uv2s.Clear();
			if (m_Colors != null) m_Colors.Clear();
			if (m_Indices != null) m_Indices.Clear();
		}
		public override void Dispose() {
			base.Dispose();

			m_SubMeshIndex = -1;
			m_StyleDataIndex = -1;
			m_RenderQueue = -1;
			m_VertexCount = 0;
			m_IndexCount = 0;

			//LW_ListPool<Vector3>.Release(m_Positions);
			//LW_ListPool<Vector3>.Release(m_Normals);
			//LW_ListPool<Vector3>.Release(m_Tangents);
			//LW_ListPool<Vector2>.Release(m_Uv1s);
			//LW_ListPool<Vector2>.Release(m_Uv2s);
			//LW_ListPool<Color>.Release(m_Colors);
			//LW_ListPool<int>.Release(m_Indices);

			if (m_Positions != null) m_Positions.Clear();
			if (m_Normals != null) m_Normals.Clear();
			if (m_Tangents != null) m_Tangents.Clear();
			if (m_Uv1s != null) m_Uv1s.Clear();
			if (m_Uv2s != null) m_Uv2s.Clear();
			if (m_Colors != null) m_Colors.Clear();
			if (m_Indices != null) m_Indices.Clear();
				
			//m_Positions = null;
			//m_Normals = null;
			//m_Tangents = null;
			//m_Uv1s = null;
			//m_Uv2s = null;
			//m_Colors = null;
			//m_Indices = null;
		}

		public override void Rebuild(LW_Graphic graphic, LW_Style style, LW_Canvas canvas, bool forceRebuild) {
			base.Rebuild(graphic, style, canvas, forceRebuild);

			if (m_ContourBufferList.Count == 0) return;

			LW_PaintStyle paintStyle = style as LW_PaintStyle;
			Material customMaterial = paintStyle.material != null ? paintStyle.material : (canvas.material != null ? canvas.material : null);
			bool isLineWorksShader = customMaterial == null || customMaterial.shader.name.StartsWith("LineWorks");

			if (style is LW_Stroke) {
				LW_Stroke stroke = style as LW_Stroke;
				if (!isLineWorksShader || canvas.featureMode == FeatureMode.Simple) RebuildStaticStroke(stroke, canvas, forceRebuild);
				else if (canvas.strokeDrawMode == StrokeDrawMode.Draw2D) RebuildDynamicStroke2D(stroke, canvas, forceRebuild);
				else RebuildDynamicStroke3D(stroke, canvas, forceRebuild);
			}
			else if (style is LW_Fill) {
				LW_Fill fill = style as LW_Fill;
				if (!isLineWorksShader || canvas.featureMode == FeatureMode.Simple) RebuildStaticFill(fill, canvas, forceRebuild);
				else RebuildDynamicFill(fill, canvas, forceRebuild);
			}
		}
		public override void Presize(int capacity) {
			int vertexCapacity = capacity*4;
			int indexCapacity = capacity*6;
			if (m_Positions.Capacity < vertexCapacity) m_Positions.Capacity = vertexCapacity;
			if (m_Normals.Capacity < vertexCapacity) m_Normals.Capacity = vertexCapacity;
			if (m_Tangents.Capacity < vertexCapacity) m_Tangents.Capacity = vertexCapacity;
			if (m_Uv1s.Capacity < vertexCapacity) m_Uv1s.Capacity = vertexCapacity;
			if (m_Uv2s.Capacity < vertexCapacity) m_Uv2s.Capacity = vertexCapacity;
			if (m_Colors.Capacity < vertexCapacity) m_Colors.Capacity = vertexCapacity;
			if (m_Indices.Capacity < indexCapacity) m_Indices.Capacity = indexCapacity;
		}

		private void AddVert(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 uv1, Vector2 uv2, Color color) {
			//Debug.Log("AddVert position: " + position + " normal: " + normal + " tangent: " + tangent + " uv1: " + uv1 + " uv2: " + uv2 + " color: " + color);
			m_Positions.Add(position);
			m_Normals.Add(normal);
			m_Tangents.Add(tangent);
			m_Uv1s.Add(uv1);
			m_Uv2s.Add(uv2);
			m_Colors.Add(color);
			m_VertexCount++;
			m_IsEmpty = false;
		}
		private void AddTriangle(int idx0, int idx1, int idx2) {
			//Debug.Log("AddTriangle: " + positions.Count + " : " + idx0 + ", " + idx1 + ", " + idx2);
			m_Indices.Add(idx0);
			m_Indices.Add(idx1);
			m_Indices.Add(idx2);
			m_IndexCount += 3;
			m_IsEmpty = false;
		}

		private void RebuildStaticStroke(LW_Stroke style, LW_Canvas canvas, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("VBO RebuildStaticStroke: " + canvas.name + " : " + style.isVisible + " && ( " + style.isDirty + " || " + isEmpty + ")");
			#endif

			bool needsQuads = !(
				style.linecap == Linecap.Butt && 
				style.linejoin == Linejoin.Miter && 
				((style.material != null && style.material.mainTexture == null) || style.mainTexture == null) && 
				((canvas.material != null && canvas.material.mainTexture == null) || canvas.mainTexture == null)
			);

			float upperMultiplier = (style.justification == Justification.center ? 1 : (style.justification == Justification.right ? 2 : 0));
			float lowerMultiplier = (style.justification == Justification.center ? 1 : (style.justification == Justification.right ? 0 : 2));

			Vector3 position;
			Vector3 tangent;
			Vector3 normal = s_DefaultNormal;
			Vector2 uv1;
			Vector2 uv2;
			Color color = s_White;

			int prevIndex, currIndex, nextIndex;
			Vector3 prevPoint, currPoint, nextPoint;
			Vector3 currOffset;
			Vector3 prevPerpendicular, nextPerpendicular;
			Vector3 prevTangent, nextTangent;
			float prevTangentLength, currLength, nextTangentLength, currWidth;

			for (int c = 0; c < m_ContourBufferList.Count; c++) {
				LW_ContourBuffer contour = m_ContourBufferList[c];
				if (!contour.isValid || contour.points.Count < 2) continue;

				int numOfPoints = contour.points.Count;
				float totalLength = contour.totalLength;
				float totalWidth = style.MaxWidth();

				prevIndex = contour.isClosed ? contour.points.Count-2 : 1;
				prevPoint = contour.points[prevIndex];

				currIndex = 0;
				currPoint = contour.points[currIndex];
				currLength = 0;

				prevTangent = (currPoint - prevPoint).normalized;
				if (!contour.isClosed) prevTangent = -prevTangent;
				prevPerpendicular = CalcPerpendicular(prevTangent);
				prevTangentLength = Vector3.Distance(currPoint, prevPoint);

				for (int p = 0; p < numOfPoints; p++) {
					nextIndex = p < contour.points.Count-1 ? p+1 : contour.isClosed ? 1 : prevIndex;
					nextPoint = contour.points[nextIndex];
					nextTangent = (nextPoint - currPoint).normalized;
					if (p == numOfPoints-1 && !contour.isClosed) nextTangent = -nextTangent;
					nextPerpendicular = CalcPerpendicular(nextTangent);
					nextTangentLength = Vector3.Distance(nextPoint, currPoint);

					currOffset = contour.offsets[currIndex];
					currLength = contour.lengths[currIndex];
					currWidth = style.WidthAtPercentage(currLength/totalLength);

					if (style.angle != 0) {
						Vector3 axisOfRotation = -CalcPerpendicular(currOffset);
						Vector3 currOffsetNormal = currOffset.normalized;
						float currOffsetLength = currOffset.magnitude;
						currOffset = Quaternion.AngleAxis(-style.angle, axisOfRotation) * currOffsetNormal;
						prevPerpendicular = Quaternion.AngleAxis(-style.angle, prevTangent) * prevPerpendicular;
						nextPerpendicular = Quaternion.AngleAxis(-style.angle, nextTangent) * nextPerpendicular;
						if ((p>0 && p<numOfPoints-1) || contour.isClosed) {
							currOffset = new Vector3(currOffset.x * currOffsetLength, currOffset.y * currOffsetLength, currOffset.z);
						}
						if (style.angle == -180 || style.angle == 180) normal = -normal;
						else normal = -Vector3.Cross(axisOfRotation, currOffset);
					}

					Vector3 capOffset = s_Zero;

					// UV Setup
					float uDot = Vector2.Dot(currOffset, prevTangent) * 0.5f;
					float uvX = currLength / (style.uvMode == UvMode.Scaled ? totalWidth : totalLength);
					float uvOffsetX = style.uvMode == UvMode.Scaled ? uDot * (currWidth / totalWidth) : 0;
					float upperV = style.uvMode == UvMode.Scaled ? (currWidth * 0.5f) / totalWidth + 0.5f : 1;
					float lowerV = style.uvMode == UvMode.Scaled ? -(currWidth * 0.5f) / totalWidth + 0.5f : 0;
					float upperU = uvOffsetX * upperMultiplier;
					float lowerU = uvOffsetX * lowerMultiplier;

					float gradX = currLength / totalLength;
					//float upperGradV = -(currWidth / totalWidth * 0.5f) + 0.5f;
					//float lowerGradV = (currWidth / totalWidth * 0.5f) + 0.5f;
					float upperGradV = 1;
					float lowerGradV = 0;

					float fillOffsetX = style.uvMode == UvMode.Scaled ? Vector2.Distance(currPoint + (Vector3)prevPerpendicular * 0.5f, currPoint + (Vector3)nextPerpendicular * 0.5f) * 0.5f : 0;
					bool isLeftTurn = !IsLeftTurn(prevTangent, nextTangent);
					bool isBreak = (
						style.linejoin == Linejoin.Break 
						|| (!contour.isClosed && (p == 0 || p == numOfPoints-1))
						|| (Mathf.Abs(uDot) > prevTangentLength / currWidth || Mathf.Abs(uDot) > nextTangentLength / currWidth)
						);								
							
					Linejoin linejoin;
						
					if (isBreak) {
						linejoin = Linejoin.Break;
					}
					else if (style.linejoin == Linejoin.Miter && currOffset.sqrMagnitude > style.miterLimit*style.miterLimit) {
						linejoin = Linejoin.Bevel;
					}
					else linejoin = style.linejoin;


					switch (linejoin) {
					case Linejoin.Miter:
						if (p > 0 || !needsQuads) {
							tangent = prevTangent;

							//Upper Right 2
							position = currPoint + (currOffset * upperMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX + upperU, upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							//Lower Right 3
							position = currPoint + (-currOffset * lowerMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX - lowerU, lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							if (p > 0) {
								// Add Triangles
								int firstIndex = m_VertexCount - 4;
								AddTriangle (firstIndex + 2, firstIndex + 0, firstIndex + 1);
								AddTriangle (firstIndex + 1, firstIndex + 3, firstIndex + 2);

							}
						}
						if (p < numOfPoints - 1 && needsQuads) {
							tangent = nextTangent;

							//Upper Left 1
							position = currPoint + (currOffset * upperMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX - upperU, upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);


							//Lower Left 0
							position = currPoint + (-currOffset * lowerMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX + lowerU, lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

						}
						break;
					case Linejoin.Bevel:
						if (p > 0 || !needsQuads) {
							tangent = prevTangent;

							//Upper Right 2
							position = currPoint + ((isLeftTurn ? currOffset : prevPerpendicular) * upperMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX + (isLeftTurn ? upperU : 0), upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							//Lower Right 3
							position = currPoint + ((isLeftTurn ? -prevPerpendicular : -currOffset) * lowerMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX - (isLeftTurn ? 0 : lowerU), lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							if (p > 0) {
								// Add Triangles
								int firstIndex = m_VertexCount - 4;
								AddTriangle (firstIndex + 2, firstIndex + 0, firstIndex + 1);
								AddTriangle (firstIndex + 1, firstIndex + 3, firstIndex + 2);
							}

							if (p < numOfPoints - 1 || contour.isClosed) {

								// Point 3 same as 1 or 2 but with diferent uvs : -6
								position = currPoint + (isLeftTurn ? -nextPerpendicular * lowerMultiplier : prevPerpendicular * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX + (isLeftTurn ? fillOffsetX * lowerMultiplier : -fillOffsetX * upperMultiplier), (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? 	new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)), bounds);
								AddVert (position, normal, tangent, uv1, uv2, color);

								// Point 3
								position = currPoint + (isLeftTurn ? currOffset * upperMultiplier : -currOffset * lowerMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX, (isLeftTurn ? upperV : lowerV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? upperGradV : lowerGradV)) : (Vector2)position;
								color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, (isLeftTurn ? upperGradV : lowerGradV)), bounds);
								AddVert (position, normal, tangent, uv1, uv2, color);

								// Point 5 THis is the normalized Linecap.Round point : -4
								position = currPoint + (isLeftTurn ? -prevPerpendicular * lowerMultiplier : nextPerpendicular * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX + (isLeftTurn ? -fillOffsetX * lowerMultiplier : fillOffsetX * upperMultiplier), (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)), bounds);
								AddVert (position, normal, tangent, uv1, uv2, color);

								// Add Triangles
								int firstIndex = m_VertexCount - 3;
								AddTriangle (firstIndex + 0, firstIndex + 1, firstIndex + 2);
							}
						}
						if (p < numOfPoints - 1) {
							tangent = nextTangent;

							//Upper Left 1
							position = currPoint + ((isLeftTurn ? currOffset : nextPerpendicular) * upperMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX - (isLeftTurn ? upperU : 0), upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							//Lower Left 0
							position = currPoint + ((isLeftTurn ? -nextPerpendicular : -currOffset) * lowerMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX + (isLeftTurn ? 0 : lowerU), lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);
						}
						break;
					case Linejoin.Round:
						if (p > 0 || !needsQuads) {
							tangent = prevTangent;

							//Upper Right 2
							position = currPoint + ((isLeftTurn ? currOffset : prevPerpendicular) * upperMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX + (isLeftTurn ? upperU : 0), upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							//Lower Right 1
							position = currPoint + ((isLeftTurn ? -prevPerpendicular : -currOffset) * lowerMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX - (isLeftTurn ? 0 : lowerU), lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							if (p > 0) {
								// Add Triangles
								int firstIndex = m_VertexCount - 4;
								AddTriangle (firstIndex + 2, firstIndex + 0, firstIndex + 1);
								AddTriangle (firstIndex + 1, firstIndex + 3, firstIndex + 2);
							}

							if (p < numOfPoints - 1 || contour.isClosed) {

								// Point 3 same as 1 or 2 but with diferent uvs : -6
								position = currPoint + (isLeftTurn ? -prevPerpendicular * lowerMultiplier : nextPerpendicular * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX + (isLeftTurn ? -fillOffsetX * lowerMultiplier : fillOffsetX * upperMultiplier), (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)), bounds);
								AddVert (position, normal, tangent, uv1, uv2, color);

								// Point 3
								position = currPoint + (isLeftTurn ? -currOffset.normalized * lowerMultiplier : currOffset.normalized * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX, (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)), bounds);
								AddVert (position, normal, tangent, uv1, uv2, color);

								// Point 5 THis is the normalized Linecap.Round point : -4
								position = currPoint + (isLeftTurn ? -nextPerpendicular * lowerMultiplier : prevPerpendicular * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX + (isLeftTurn ? fillOffsetX * lowerMultiplier : -fillOffsetX * upperMultiplier), (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)), bounds);
								AddVert (position, normal, tangent, uv1, uv2, color);

								// Point 5 we just repeat this point so we have 4 points(Quad) for the joint.
								position = currPoint + (isLeftTurn ? currOffset * upperMultiplier : -currOffset * lowerMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX, (isLeftTurn ? upperV : lowerV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? upperGradV : lowerGradV)) : (Vector2)position;
								color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, (isLeftTurn ? upperGradV : lowerGradV)), bounds);
								AddVert (position, normal, tangent, uv1, uv2, color);

								// Add Triangles
								int firstIndex = m_VertexCount - 4;
								AddTriangle (firstIndex + 3, firstIndex + 0, firstIndex + 1);
								AddTriangle (firstIndex + 1, firstIndex + 2, firstIndex + 3);
							}
						}
						if (p < numOfPoints - 1 && needsQuads) {
							tangent = nextTangent;

							//Upper Left 1
							position = currPoint + ((isLeftTurn ? currOffset : nextPerpendicular) * upperMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX - (isLeftTurn ? upperU : 0), upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);

							//Lower Left 0
							position = currPoint + ((isLeftTurn ? -nextPerpendicular : -currOffset) * lowerMultiplier) * currWidth * 0.5f;
							uv1 = new Vector2 (uvX + (isLeftTurn ? 0 : lowerU), lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert (position, normal, tangent, uv1, uv2, color);
						}
						break;
					case Linejoin.Break:
						if (p > 0 || !needsQuads) {
							if (p == numOfPoints-1 && style.linecap == Linecap.Square && !contour.isClosed) {
								capOffset = prevTangent;
								uvX += 0.5f;
							}
							tangent = prevTangent;

							//Upper Right 2
							position = currPoint + (prevPerpendicular * upperMultiplier + capOffset) * currWidth * 0.5f;
							uv1 = new Vector2(uvX, upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2(gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert(position, normal, tangent, uv1, uv2, color);

							//Lower Right 3
							position = currPoint + (-prevPerpendicular * lowerMultiplier + capOffset) * currWidth * 0.5f;
							uv1 = new Vector2(uvX, lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2(gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert(position, normal, tangent, uv1, uv2, color);

							if (p > 0) {
								// Add Triangles
								int firstIndex = m_VertexCount - 4;
								AddTriangle (firstIndex + 2, firstIndex + 0, firstIndex + 1);
								AddTriangle (firstIndex + 1, firstIndex + 3, firstIndex + 2);
							}

							if (style.linecap == Linecap.Round) {
								/* not implemented yet
								Vector3 offset = prevPerpendicular;

								offset = Quaternion.Euler(0, 0, -45) * offset;
	
								// Point 3 same as 1 or 2 but with diferent uvs : -6
								position = currPoint + offset * lowerMultiplier : nextPerpendicular * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX + (isLeftTurn ? -fillOffsetX * lowerMultiplier : fillOffsetX * upperMultiplier), (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								AddVert (position, normal, tangent, uv1, uv2, currColor);

								// Point 3
								position = currPoint + (isLeftTurn ? -currOffset.normalized * lowerMultiplier : currOffset.normalized * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX, (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								AddVert (position, normal, tangent, uv1, uv2, currColor);

								// Point 5 THis is the normalized Linecap.Round point : -4
								position = currPoint + (isLeftTurn ? -nextPerpendicular * lowerMultiplier : prevPerpendicular * upperMultiplier) * currWidth * 0.5f;
								uv1 = new Vector2 (uvX + (isLeftTurn ? fillOffsetX * lowerMultiplier : -fillOffsetX * upperMultiplier), (isLeftTurn ? lowerV : upperV));
								uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2 (gradX, (isLeftTurn ? lowerGradV : upperGradV)) : (Vector2)position;
								AddVert (position, normal, tangent, uv1, uv2, currColor);

								// Add Triangles
								int firstIndex = m_VertexCount - 4;
								AddTriangle (firstIndex + 3, firstIndex + 0, firstIndex + 1);
								AddTriangle (firstIndex + 1, firstIndex + 2, firstIndex + 3);
								AddTriangle (firstIndex + 1, firstIndex + 2, firstIndex + 3);
								*/
							}
						}
						if (p < numOfPoints - 1) {
							if (style.linecap == Linecap.Square && !contour.isClosed && p == 0) {
								capOffset = -nextTangent;
								uvX -= 0.5f;
							}
							tangent = nextTangent;

							//Upper Left 1
							position = currPoint + (nextPerpendicular * upperMultiplier + capOffset) * currWidth * 0.5f;
							uv1 = new Vector2(uvX, upperV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2(gradX, upperGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, upperGradV), bounds);
							AddVert(position, normal, tangent, uv1, uv2, color);

							//Lower Left 0
							position = currPoint + (-nextPerpendicular * lowerMultiplier + capOffset) * currWidth * 0.5f;
							uv1 = new Vector2(uvX, lowerV);
							uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? new Vector2(gradX, lowerGradV) : (Vector2)position;
							color = style.ColorAtPosition((Vector2)position, new Vector2 (gradX, lowerGradV), bounds);
							AddVert(position, normal, tangent, uv1, uv2, color);
						}
						break;
					}

					currIndex = nextIndex;
					prevIndex = currIndex;

					prevPoint = currPoint;
					currPoint = nextPoint;

					prevTangent = nextTangent;
					prevTangentLength = nextTangentLength;
					prevPerpendicular = nextPerpendicular;
				}
			}
		}
		private void RebuildDynamicStroke2D(LW_Stroke style, LW_Canvas canvas, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("VBO RebuildDynamicStroke2D: " + style.isVisible + " && ( " + style.isDirty + " || " + isEmpty + ")");
			#endif

			bool needsQuads = !(
				style.linecap == Linecap.Butt
				&& style.linejoin == Linejoin.Miter
				&& ((style.material != null && style.material.mainTexture == null) || style.mainTexture == null)
				&& ((canvas.material != null && canvas.material.mainTexture == null) || canvas.mainTexture == null)
				&& (canvas.joinsAndCapsMode != JoinsAndCapsMode.Shader)
			);
				
			float boundsMin = Mathf.Min(bounds.min.x, bounds.min.y);
			float boundsMax = Mathf.Max(bounds.max.x, bounds.max.y);

			int prevIndex, currIndex, nextIndex;
			Vector3 prevPoint, currPoint, nextPoint;
			bool currIsBreak, nextIsBreak;
			Vector3 currOffset, nextOffset;
			Vector3 prevTangent, nextTangent;
			Vector3 prevPerpendicular, nextPerpendicular;
			float prevPackedJoins, nextPackedJoins;
			float currJoin, nextJoin;
			float currLength, nextLength;
			float currWidth, nextWidth;
			Vector2 currUv1 = Vector2.zero, currUv2 = Vector2.zero;
			Vector2 currBoundsPos;
			float currPackedLocalPos;
			Color currColor = Color.white;
			LW_ContourBuffer contour;


			for (int c = 0; c < m_ContourBufferList.Count; c++) {
				contour = m_ContourBufferList[c];
				if (!contour.isValid || contour.points.Count < 2) continue;

				int numOfPoints = contour.points.Count;
				float totalLength = contour.totalLength; //contour.lengths[contour.lengths.Count-1]; 
				float totalWidth = style.MaxWidth();

				prevIndex = contour.isClosed ? contour.points.Count-2 : 1;
				prevPoint = contour.points[prevIndex];

				currIndex = 0;
				currPoint = contour.points[currIndex];
				currOffset = contour.offsets[currIndex];
				currLength = 0;
				currWidth = style.WidthAtPercentage(currLength / totalLength) * 0.5f;
				currIsBreak = !contour.isClosed || style.miterLimit * style.miterLimit < currOffset.sqrMagnitude || style.linejoin == Linejoin.Break;
				currJoin = currIsBreak ? 2 : 1;

				prevTangent = (currPoint - prevPoint);
				if (!contour.isClosed) prevTangent = -prevTangent;
				prevPerpendicular = CalcPerpendicular(prevTangent.normalized);
				prevPackedJoins = new Vector2(0, currJoin).PackVector2(-2,2);

				for (int p = 0; p < numOfPoints; p++) {
					nextIndex = p < contour.points.Count-1 ? p+1 : contour.isClosed ? 1 : prevIndex;
					nextPoint = contour.points[nextIndex];
					nextOffset = contour.offsets[nextIndex];
					nextLength = contour.lengths[nextIndex];
					nextWidth = style.WidthAtPercentage(nextLength / totalLength) * 0.5f;
					nextIsBreak = (!contour.isClosed && nextIndex == numOfPoints - 1) || style.miterLimit * style.miterLimit < nextOffset.sqrMagnitude || style.linejoin == Linejoin.Break;
					nextJoin = nextIsBreak ? 2 : 1;

					nextTangent = (nextPoint - currPoint);
					if (p == numOfPoints-1 && !contour.isClosed) nextTangent = -nextTangent;
					nextPerpendicular = CalcPerpendicular(nextTangent.normalized);
					nextPackedJoins = new Vector2(currJoin, nextJoin).PackVector2(-2,2);

					currBoundsPos = new Vector2(currLength / totalLength, currWidth / totalWidth);
					currPackedLocalPos = ((Vector2)currPoint).PackVector2(boundsMin, boundsMax);
					currColor = canvas.gradientsMode == GradientsMode.Shader ? s_White : style.ColorAtPosition((Vector2)currPoint, currBoundsPos, bounds);

					if (style.angle != 0) {
						Vector3 axisOfRotation = -CalcPerpendicular(currOffset);
						Vector3 currOffsetNormal = currOffset.normalized;
						float currOffsetLength = currOffset.magnitude;
						currOffset = Quaternion.AngleAxis(-style.angle, axisOfRotation) * currOffsetNormal;
						prevPerpendicular = Quaternion.AngleAxis(-style.angle, prevTangent) * prevPerpendicular;
						nextPerpendicular = Quaternion.AngleAxis(-style.angle, nextTangent) * nextPerpendicular;
						if ((p>0 && p<numOfPoints-1) || contour.isClosed) {
							currOffset = new Vector3(currOffset.x * currOffsetLength, currOffset.y * currOffsetLength, currOffset.z);
						}
					}

					if (p > 0 || !needsQuads) {
						currUv2.y = prevPackedJoins;
						if (currJoin == 2) currOffset = prevPerpendicular;

						//Debug.Log(p + " : " + currLength + " : " + totalLength + " : " + currBoundsPos); //Upper Right 2
						currUv1 = new Vector2( new Vector2((currBoundsPos.x+1), ((currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, currOffset, prevTangent, currUv1, currUv2, currColor);

						//Lower Right 3
						currUv1 = new Vector2( new Vector2((currBoundsPos.x+1), -((-currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, currOffset, prevTangent, currUv1, currUv2, currColor);

						if (p > 0) {
							// Add Triangles
							int firstIndex = m_VertexCount - 4;
							AddTriangle (firstIndex + 2, firstIndex + 0, firstIndex + 1);
							AddTriangle (firstIndex + 1, firstIndex + 3, firstIndex + 2);
						}
					}
					if (p < numOfPoints - 1 && (needsQuads || (currIsBreak && p > 0))) {
						currUv2.y = nextPackedJoins;
						if (currJoin == 2) currOffset = nextPerpendicular;

						//Upper Left 0
						currUv1 = new Vector2( new Vector2(-(currBoundsPos.x+1), ((currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, currOffset, nextTangent, currUv1, currUv2, currColor);

						//Lower Left 1
						currUv1 = new Vector2( new Vector2(-(currBoundsPos.x+1), -((-currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, currOffset, nextTangent, currUv1, currUv2, currColor);
					}

					prevIndex = currIndex;
					currIndex = nextIndex;

					currPoint = nextPoint;
					currOffset = nextOffset;
					currLength = nextLength;
					currWidth = nextWidth;
					currIsBreak = nextIsBreak;
					currJoin = nextJoin;

					prevTangent = nextTangent;
					prevPerpendicular = nextPerpendicular;
					prevPackedJoins = nextPackedJoins;
				}
			}
		}
		private void RebuildDynamicStroke3D(LW_Stroke style, LW_Canvas canvas, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("VBO RebuildDynamicStroke3D: " + style.isVisible + " && ( " + style.isDirty + " || " + isEmpty + ")");
			#endif

			float boundsMin = Mathf.Min(bounds.min.x, bounds.min.y);
			float boundsMax = Mathf.Max(bounds.max.x, bounds.max.y);

			int prevIndex, currIndex, nextIndex;
			Vector3 prevPoint, currPoint, nextPoint;
			Vector3 prevTangent, nextTangent;
			float prevPackedJoins, nextPackedJoins;
			float currJoin, nextJoin;
			float currLength, nextLength;
			float currWidth, nextWidth;
			Vector2 currUv1 = Vector2.zero, currUv2 = Vector2.zero;
			Vector2 currBoundsPos;
			float currPackedLocalPos;
			Color currColor = Color.white;


			for (int c = 0; c < m_ContourBufferList.Count; c++) {
				LW_ContourBuffer contour = m_ContourBufferList[c];
				if (!contour.isValid || contour.points.Count < 2) return;

				int numOfPoints = contour.points.Count;
				float totalLength = contour.totalLength;
				float totalWidth = style.MaxWidth();

				prevIndex = contour.isClosed ? contour.points.Count-2 : 1;
				prevPoint = contour.points[prevIndex];

				currIndex = 0;
				currPoint = contour.points[currIndex];
				currLength = 0;
				currWidth = style.WidthAtPercentage(currLength / totalLength) * 0.5f;
				currJoin = !contour.isClosed || style.linejoin == Linejoin.Break ? 2 : 1;

				prevTangent = (currPoint - prevPoint);
				if (!contour.isClosed) prevTangent = -prevTangent;
				prevPackedJoins = new Vector2(0, currJoin).PackVector2(-2,2);

				for (int p = 0; p < numOfPoints; p++) {
					nextIndex = p < contour.points.Count-1 ? p+1 : contour.isClosed ? 1 : prevIndex;
					nextPoint = contour.points[nextIndex];
					nextLength = contour.lengths[nextIndex];
					nextWidth = style.WidthAtPercentage(nextLength / totalLength) * 0.5f;
					nextJoin = (!contour.isClosed && (nextIndex == 0 || nextIndex == numOfPoints - 1)) || style.linejoin == Linejoin.Break ? 2 : 1;

					nextTangent = (nextPoint - currPoint);
					if (p == numOfPoints-1 && !contour.isClosed) nextTangent = -nextTangent;
					nextPackedJoins = new Vector2(currJoin, nextJoin).PackVector2(-2,2);

					currBoundsPos = new Vector2(currLength / totalLength, currWidth / totalWidth);
					currPackedLocalPos = ((Vector2)currPoint).PackVector2(boundsMin, boundsMax);
					currColor = canvas.gradientsMode == GradientsMode.Shader ? s_White : style.ColorAtPosition((Vector2)currPoint, currBoundsPos, bounds);

					//Debug.Log(p + " : " + currPoint + " : " + prevTangent + " : " + nextTangent + " : " + currBoundsPos);

					if (p > 0) {
						currUv2.y = prevPackedJoins;

						//Upper Right 2
						currUv1 = new Vector2( new Vector2((currBoundsPos.x+1), ((currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, prevTangent, nextTangent, currUv1, currUv2, currColor);

						//Lower Right 3
						currUv1 = new Vector2( new Vector2((currBoundsPos.x+1), -((-currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, prevTangent, nextTangent, currUv1, currUv2, currColor);

						if (p > 0) {
							// Add Triangles
							int firstIndex = m_VertexCount - 4;
							AddTriangle (firstIndex + 2, firstIndex + 0, firstIndex + 1);
							AddTriangle (firstIndex + 1, firstIndex + 3, firstIndex + 2);
						}
					}
					if (p < numOfPoints - 1) {
						currUv2.y = nextPackedJoins;

						//Upper Left 0
						currUv1 = new Vector2( new Vector2(-(currBoundsPos.x+1), ((currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, prevTangent, nextTangent, currUv1, currUv2, currColor);

						//Lower Left 1
						currUv1 = new Vector2( new Vector2(-(currBoundsPos.x+1), -((-currBoundsPos.y+0.5f)+1)).PackVector2(-2,2), currPackedLocalPos);
						AddVert(currPoint, prevTangent, nextTangent, currUv1, currUv2, currColor);

					}

					currPoint = nextPoint;
					currLength = nextLength;
					currWidth = nextWidth;
					currJoin = nextJoin;

					prevTangent = nextTangent;
					prevPackedJoins = nextPackedJoins;
				}
			}
		}

		private void RebuildStaticFill(LW_Fill style, LW_Canvas canvas, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("VBO RebuildStaticFill: " + style.isVisible + " && ( " + style.isDirty + " || " + isEmpty + ")");
			#endif

			LibTessDotNet.Tess tess = new LibTessDotNet.Tess();
			Vector3 position;
			Vector3 normal = s_DefaultNormal;
			Vector3 tangent = s_DefaultTangent;
			Vector2 uv1;
			Vector2 uv2;
			Vector2 texturePos;
			Vector2 boundsPos;
			Vector2 localPos;
			Color color = s_White;

			float minSizeX = (style.uvMode == UvMode.Scaled) ? Mathf.Min(bounds.size.x, bounds.size.y) : bounds.size.x;
			float minSizeY = (style.uvMode == UvMode.Scaled) ? Mathf.Min(bounds.size.x, bounds.size.y) : bounds.size.y;

			for (int c = 0; c < m_ContourBufferList.Count; c++) {
				LW_ContourBuffer contour = m_ContourBufferList[c];
				if (!contour.isValid || contour.points.Count < 3) continue;

				tess.AddContour(contour.pointsForFill, LibTessDotNet.ContourOrientation.Clockwise);
				//tess.AddContour(contour.points, LibTessDotNet.ContourOrientation.Clockwise);
			}

			tess.Tessellate((LibTessDotNet.WindingRule)(int)style.fillRule, LibTessDotNet.ElementType.Polygons, 3);

			for (int i = 0; i < tess.VertexCount; i++) {
				LibTessDotNet.Vec3 vert = tess.Vertices[i].Position;
				position = new Vector3(vert.X, vert.Y, vert.Z);
				texturePos = new Vector2( ((position.x - bounds.min.x) / minSizeX) , ((position.y - bounds.min.y) / minSizeY));
				boundsPos = new Vector2((position.x - bounds.min.x) / bounds.size.x, (position.y - bounds.min.y) / bounds.size.y);
				localPos = new Vector2(position.x, position.y);
				uv1 = texturePos;
				uv2 = style.gradientUnits == GradientUnits.objectBoundingBox ? boundsPos : localPos;
				color = style.ColorAtPosition(localPos, boundsPos, bounds);

				AddVert(position, normal, tangent, uv1, uv2, color);
			}

			int numTriangles = tess.ElementCount;
			for (int t = 0; t < numTriangles; t++) {
				AddTriangle(tess.Elements[t * 3 + 2], tess.Elements[t * 3 + 1], tess.Elements[t * 3]);
			}
		}
		private void RebuildDynamicFill(LW_Fill style, LW_Canvas canvas, bool forceRebuild) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugBuffer) Debug.Log("VBO RebuildDynamicFill: " + style.isVisible + " && ( " + style.isDirty + " || " + isEmpty + ")");
			#endif

			LibTessDotNet.Tess tess = new LibTessDotNet.Tess();
			int vertexIndex = 0;

			Vector3 currPosition = Vector3.zero;
			Vector3 currOffset = Vector3.zero;
			float currOffsetLength = 0;
			Vector3 currTangent = Vector3.zero;
			Vector2 currBoundsPos = Vector2.zero;
			Vector2 currUv1 = Vector2.one;
			Vector2 currUv2 = Vector2.one;
			Color currColor = s_White;
			float currPackedLocalPos = 0;

			Vector3 prevTangent = Vector3.zero;
			Vector3 nextTangent = Vector3.zero;

			Vector3 currPoint, prevPoint, nextPoint;

			LibTessDotNet.ContourVertex[][] fillRegions = new LibTessDotNet.ContourVertex[0][];

			for (int c = 0; c < m_ContourBufferList.Count; c++) {
				LW_ContourBuffer contour = m_ContourBufferList[c];
				if (!contour.isValid || contour.points.Count < 3) continue;

				tess.AddContour(contour.pointsForFill, LibTessDotNet.ContourOrientation.Clockwise);
				//tess.AddContour(contour.points, ContourOrientation.Clockwise);
			}

			float boundsMin = Mathf.Min(bounds.min.x, bounds.min.y);
			float boundsMax = Mathf.Max(bounds.max.x, bounds.max.y);

			//Add Anti Alias Ring
			if (canvas != null && canvas.antiAliasingMode == AntiAliasingMode.On) {

				tess.Tessellate((LibTessDotNet.WindingRule)(int)style.fillRule, LibTessDotNet.ElementType.BoundaryContours, 3);
				//tess.Tessellate(style.fillRule, LibTessDotNet.ElementType.BoundaryContours, 3);

				if (tess.ElementCount > 0) {
					fillRegions = new LibTessDotNet.ContourVertex[tess.ElementCount][];
					for (int e=0; e<tess.ElementCount; e++) {
						int startIndex = tess.Elements[e*2];
						int length = tess.Elements[e*2+1];

						if (startIndex >= 0 && startIndex+length <= tess.VertexCount) {
							LibTessDotNet.ContourVertex[] fillPoints = new LibTessDotNet.ContourVertex[length];

							LibTessDotNet.Vec3 currVertPosition = tess.Vertices[startIndex].Position;
							currPoint = new Vector3(currVertPosition.X, currVertPosition.Y, currVertPosition.Z);
							//currPoint = tess.Vertices[startIndex].Position;

							LibTessDotNet.Vec3 prevVertPosition = tess.Vertices[startIndex+length-1].Position;
							prevPoint = new Vector3(prevVertPosition.X, prevVertPosition.Y, prevVertPosition.Z);
							//prevPoint = tess.Vertices[startIndex+length-1].Position;

							prevTangent = currPoint - prevPoint;
							prevTangent.Normalize();

							int numOfPoints = length+1;
							for (int p=0; p<numOfPoints; p++) {
								LibTessDotNet.Vec3 nextVertPosition = tess.Vertices[ (p<length-1 ? startIndex+p+1 : (p==length-1 ? startIndex : startIndex+1) ) ].Position;
								nextPoint = new Vector3(nextVertPosition.X, nextVertPosition.Y, nextVertPosition.Z);
								//nextPoint = tess.Vertices[ (p<length-1 ? startIndex+p+1 : (p==length-1 ? startIndex : startIndex+1) ) ].Position;

								nextTangent = nextPoint - currPoint;
								nextTangent.Normalize();

								currPosition = currPoint;
								currOffset = CalcPerpendicular(prevTangent + nextTangent).normalized;
								currOffsetLength = Mathf.Min(Mathf.Abs(1f/Vector2.Dot(currOffset, CalcPerpendicular(prevTangent))), 8);
							
								currBoundsPos = new Vector2((currPosition.x - bounds.min.x) / bounds.size.x, (currPosition.y - bounds.min.y) / bounds.size.y);
								currPackedLocalPos = ((Vector2)currPosition).PackVector2(boundsMin, boundsMax);
								currColor = canvas.gradientsMode == GradientsMode.Shader ? s_White : style.ColorAtPosition((Vector2)currPosition, currBoundsPos, bounds);

								if (p < length) {
									LibTessDotNet.ContourVertex vert = new LibTessDotNet.ContourVertex();
									vert.Position = new LibTessDotNet.Vec3(){X=currPoint.x, Y=currPoint.y, Z=currPoint.z};
									vert.Data = (currOffset * currOffsetLength);
									fillPoints[p] = vert;
									//fillPoints[p] = new LibTessDotNet.ContourVertex(currPoint, currOffset * currOffsetLength);
								}
								currOffset *= currOffsetLength;

								currUv2.y = new Vector2(1, -1).PackVector2(-2,2);

								//Lower Right 3
								currUv1 = new Vector2( new Vector2((currBoundsPos.x+1), -(currBoundsPos.y+1)).PackVector2(-2,2), currPackedLocalPos);
								AddVert(currPosition, currOffset, currTangent, currUv1, currUv2, currColor);

								//Upper Right 2
								currUv1 = new Vector2( new Vector2((currBoundsPos.x+1), (currBoundsPos.y+1)).PackVector2(-2,2), currPackedLocalPos);
								AddVert(currPosition, currOffset, currTangent, currUv1, currUv2, currColor);

								if (p > 0) {
									AddTriangle(vertexIndex + 3, vertexIndex + 1, vertexIndex + 0);
									AddTriangle(vertexIndex + 0, vertexIndex + 2, vertexIndex + 3);
									vertexIndex += 2;
								}								

								prevPoint = currPoint;
								currPoint = nextPoint;

								prevTangent = nextTangent;
							}
							vertexIndex += 2;
							fillRegions[e] = fillPoints;
							tess.AddContour(fillPoints);
						}
					}
				}
			}


			currTangent = s_DefaultTangent;
			currUv2.y = new Vector2(-1,1).PackVector2(-2,2);

			tess.Tessellate((LibTessDotNet.WindingRule)(int)style.fillRule, LibTessDotNet.ElementType.Polygons, 3);
			//tess.Tessellate(style.fillRule, LibTessDotNet.ElementType.Polygons, 3);

			for (int i = 0; i < tess.VertexCount; i++) {
				LibTessDotNet.ContourVertex currVert = tess.Vertices[i];

				currPosition = new Vector3(currVert.Position.X, currVert.Position.Y, currVert.Position.Z);
				if (currVert.Data != null) currOffset = -((Vector3)currVert.Data);
				//currPosition = currVert.Position;
				//currOffset = -currVert.Offset;
				currBoundsPos = new Vector2((currPosition.x - bounds.min.x) / bounds.size.x, (currPosition.y - bounds.min.y) / bounds.size.y);
				currPackedLocalPos = ((Vector2)currPosition).PackVector2(boundsMin, boundsMax);
				currUv1 = new Vector2( new Vector2(currBoundsPos.x+1, currBoundsPos.y+1).PackVector2(-2,2), currPackedLocalPos);
				currColor = canvas.gradientsMode == GradientsMode.Shader ? s_White : style.ColorAtPosition((Vector2)currPosition, currBoundsPos, bounds);

				AddVert(currPosition, currOffset, currTangent, currUv1, currUv2, currColor);
			}

			int numTriangles = tess.ElementCount;
			for (int t = 0; t < numTriangles; t++) {
				AddTriangle(vertexIndex + tess.Elements[t * 3 + 2], vertexIndex + tess.Elements[t * 3 + 1], vertexIndex + tess.Elements[t * 3]);
			}
		}

		private static bool IsLeftTurn(Vector2 p0, Vector2 p1, Vector2 p2) {
			Vector3 n1 = CalcOffset(p0, p1);
			Vector3 n2 = CalcOffset(p1, p2);
			Vector3 cross = Vector3.Cross(n1, n2);
			return cross.z > 0;
		}
		private static bool IsLeftTurn(Vector3 prevTangent, Vector3 nextTangent) {
			Vector3 cross = Vector3.Cross(prevTangent, nextTangent);
			return cross.z > 0;
		}
		private static Vector2 CalcOffset(Vector2 p0, Vector2 p1, Vector2 p2) {
			Vector2 n1, n2, norm;
			n1 = CalcOffset(p0, p1);
			n2 = CalcOffset(p1, p2);
			norm = (n1 + n2) / 2;
			norm.Normalize();
			//norm *= 1f/Vector2.Dot(norm,n1);
			return norm;
		}
		private static Vector2 CalcOffset(Vector2 p0, Vector2 p1) {
			Vector2 norm = Vector2.zero;
			norm.x = p0.y - p1.y;
			norm.y = -(p0.x - p1.x);
			norm.Normalize();
			return norm;
		}
		private static Linejoin CalcJoin(float MiterLength, LW_Stroke style) {
			if (style.linejoin == Linejoin.Miter && MiterLength > style.miterLimit*style.miterLimit) return Linejoin.Bevel;
			else return style.linejoin;
		}
	}
		
	public class LW_MaterialBuffer : IDisposable {
		public bool isDirty = true;
		public bool isAdvancedShader = false;
		public bool isLineWorksShader = false;

		public List<LW_VertexBuffer> vertexBuffers = LW_ListPool<LW_VertexBuffer>.Get();
		public Stack<int> emptyIndices = new Stack<int>();

		public void Dispose() {
			for (int i=0; i<vertexBuffers.Count; i++) {
				vertexBuffers[i].Dispose();
			}
			//LW_ListPool<LW_VertexBuffer>.Release(vertexBuffers);
			if (vertexBuffers != null) vertexBuffers.Clear();
			//vertexBuffers = null;

			if (emptyIndices != null) emptyIndices.Clear();
			//emptyIndices = null;
		}
	}
}
 