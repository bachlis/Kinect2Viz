// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Base abstract class for all possible shapes.
	/// </summary>
	public abstract class LW_Shape : LW_Graphic {

		public const float c_ArcAprox = 0.552284749831f;
		public static bool s_DebugShapes = false;

		/// <summary>
		/// Gets or sets a value indicating whether the points of this <see cref="LineWorks.LW_Shape"/> should be iterated in reverse order.
		/// </summary>
		/// <remarks>
		/// When applied in conjunction with a stroke style, this reverses the gradent and texture direction. In conjunction with a fill style and the set fillRule, reversing direction may affect whether an area is filled.
		/// </remarks>
		/// <value><c>true</c> if reverse direction; otherwise, <c>false</c>.</value>
		public virtual bool reverseDirection {
			get {
				return m_ReverseDirection;
			}
			set {
				if (m_ReverseDirection != value) {
					m_ReverseDirection = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_ReverseDirection = false;

		/// <summary>
		/// Gets or sets a value indicating whether the points of this <see cref="LineWorks.LW_Shape"/> form a closed loop.
		/// </summary>
		/// <value><c>true</c> if is closed; otherwise, <c>false</c>.</value>
		public virtual bool isClosed {
			get {
				return m_IsClosed;
			}
			set {
				if (m_IsClosed != value) {
					m_IsClosed = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_IsClosed = false;

		#if UNITY_EDITOR
		/// <summary>
		/// Gets or sets the color of the graphic in the editor Scene View.
		/// </summary>
		/// <value>The color of the graphic in the Scene View.</value>
		public virtual Color editorColor {
			get {
				return m_EditorColor;
			}
			set {
				if (m_EditorColor != value) {
					m_EditorColor = value;
				}
			}
		}
		[SerializeField] protected Color m_EditorColor = Color.magenta;

		public bool m_GeneralShapeExpanded = false;
		public bool m_SpecificShapeExpanded = false;
		#endif

		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Shape";
			base.OnEnable();
		}

		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Shape) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Shape>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Shape shape = element as LW_Shape;
			if (shape != null) {
				m_ReverseDirection = shape.m_ReverseDirection;
				m_IsClosed = shape.m_IsClosed;
				#if UNITY_EDITOR
				m_EditorColor = shape.m_EditorColor;
				#endif
			}
			base.CopyPropertiesFrom(element);
		}

		public virtual LW_Polyline2D ConvertToPolyline2D() {
			return CreateInstance<LW_Polyline2D>();
		}
		public virtual LW_Polyline3D ConvertToPolyline3D() {
			return CreateInstance<LW_Polyline3D>();
		}
		public virtual LW_Path2D ConvertToPath2D() {
			return CreateInstance<LW_Path2D>();
		}
		public virtual LW_Path3D ConvertToPath3D() {
			return CreateInstance<LW_Path3D>();

		}

		private List<Vector3> m_ContourPoints;
		private List<float> m_ContourLengths;

		public Vector3 FindPointAtPercentage(float percentage, int segmentation = 8) {
			RebuildContourPoints(ref m_ContourPoints, segmentation);
			RebuildContourLengths(ref m_ContourLengths, m_ContourPoints);

			if (m_ContourLengths.Count == 0) return Vector3.zero;

			float length = percentage * m_ContourLengths[m_ContourLengths.Count - 1];
			return FindPointAtLength(length, m_ContourPoints, m_ContourLengths);
		}
		public Vector3 FindPointAtLength(float length, int segmentation = 8) {
			RebuildContourPoints(ref m_ContourPoints, segmentation);
			RebuildContourLengths(ref m_ContourLengths, m_ContourPoints);

			if (m_ContourLengths.Count == 0) return Vector3.zero;

			return FindPointAtLength(length, m_ContourPoints, m_ContourLengths);
		}
		private Vector3 FindPointAtLength(float length, List<Vector3> contourPoints, List<float> contourLengths) {
			int closestIndex = 0;
			while (closestIndex < contourLengths.Count && contourLengths[closestIndex] < length) closestIndex++;
			if (closestIndex > 0) closestIndex--;
			float t = Mathf.Clamp((length - contourLengths[closestIndex]) / (contourLengths[closestIndex + 1] - contourLengths[closestIndex]), 0, 1);
			Vector3 prevPoint = contourPoints[closestIndex];
			Vector3 nextPoint = closestIndex < contourPoints.Count - 1 ? contourPoints[closestIndex + 1] : m_IsClosed ? closestIndex == contourPoints.Count - 1 ? contourPoints[0] : contourPoints[1] : contourPoints[closestIndex];
			return Vector3.Lerp(prevPoint, nextPoint, t);
		}

		internal virtual void RebuildContourPoints(ref List<Vector3> contourPoints, int segmentation = 8) {
			/*
			Vector3 lastPoint, currPoint, nextPoint;

			int index = 1;
			lastPoint = contourPoints[0];
			for (int i=1; i<contourPoints.Count-1; i++) {
				currPoint = contourPoints[i];
				nextPoint = contourPoints[i+1];
				if (LW_Utilities.CalcArea(lastPoint, currPoint, nextPoint) > 0) {
					contourPoints[index++] = currPoint;
					lastPoint = currPoint;
				}
			}

			contourPoints[index++] = contourPoints[contourPoints.Count-1];

			contourPoints.RemoveRange(index, contourPoints.Count-index);
			*/
		}

		internal virtual void RebuildContourOffsets(ref List<Vector3> contourOffsets, List<Vector3> contourPoints) {

			int capacity = contourPoints.Count;
			if (contourOffsets == null) contourOffsets = new List<Vector3>(capacity);
			else if (contourOffsets.Capacity < capacity) contourOffsets.Capacity = capacity;
			contourOffsets.Clear();

			Vector3 prevTangent, nextTangent, prevPoint, currPoint, nextPoint;
			currPoint = contourPoints[0];
			prevPoint = m_IsClosed ? contourPoints[contourPoints.Count-2] : contourPoints[1];
			prevTangent = (currPoint - prevPoint).normalized;

			for (int p=0; p<contourPoints.Count; p++) {
				nextPoint = p<contourPoints.Count-1 ? contourPoints[p+1] : m_IsClosed ? contourPoints[1] : prevPoint;
				nextTangent = (nextPoint - currPoint).normalized;

				if (p==0 && !m_IsClosed) {
					contourOffsets.Add(CalcPerpendicular((Vector2)nextTangent).normalized);
				}
				else if (p == contourPoints.Count-1 && !m_IsClosed) {
					contourOffsets.Add(CalcPerpendicular((Vector2)prevTangent).normalized);
				}
				else {
					Vector3 offset = CalcPerpendicular((Vector2)prevTangent + (Vector2)nextTangent).normalized;
					Vector3 perpendicular = CalcPerpendicular((Vector2)prevTangent).normalized;
					contourOffsets.Add(offset * 1f/Vector2.Dot(offset, perpendicular));
				}

				prevTangent = nextTangent;
				prevPoint = currPoint;
				currPoint = nextPoint;
			}
		}
		internal virtual void RebuildContourLengths(ref List<float> contourLengths, List<Vector3> contourPoints) {
			int capacity = contourPoints.Count;
			if (contourLengths == null) contourLengths = new List<float>(capacity);
			else if (contourLengths.Capacity < capacity) contourLengths.Capacity = capacity;
			contourLengths.Clear();

			float currLength = 0;
			Vector3 prevPoint, currPoint;
			prevPoint = contourPoints[0];

			for (int p=0; p<contourPoints.Count; p++) {
				currPoint = contourPoints[p];
				if (prevPoint != currPoint) currLength += Vector3.Distance(prevPoint, currPoint);
				contourLengths.Add(currLength);

				prevPoint = currPoint;
			}
		}

		public virtual float SignedArea(List<Vector3> points) {
			float area = 0.0f;

			for (int i = 0; i < points.Count; i++) {
				Vector3 p0 = points[i];
				Vector3 p1 = points[(i + 1) % points.Count];

				area += p0.x * p1.y;
				area -= p0.y * p1.x;
			}
			return area * 0.5f;
		}

		protected Vector2 CalcPerpendicular(Vector2 v0) {
			return new Vector2(v0.y, -v0.x);
		}
		protected Vector3 CalcPerpendicular(Vector3 v0) {
			return new Vector3(v0.y, -v0.x, v0.z);
		}
	}

	/// <summary>
	/// Base Generic class used for all shape types with the addition of a generic collection of points.
	/// </summary>
	/// <typeparam name="T">The type of points that the shape should use</typeparam>
	public abstract class LW_PointsShape<T> : LW_Shape {

		/// <summary>
		/// Gets or sets the List collection of points.
		/// </summary>
		/// <value>The points.</value>
		public List<T> points {
			get {
				if (m_Points == null) m_Points = new List<T>();
				return m_Points;
			}
			set {
				//if (m_Points != value) {
					m_Points = value;
					SetElementDirty();
				//}
			}
		}
		[SerializeField] protected List<T> m_Points = new List<T>();

		/// <summary>
		/// Reverses the list of points
		/// </summary>
		public void Reverse() {
			if (m_Points != null && m_Points.Count > 1) {
				List<T> reversedPoints = new List<T>(m_Points.Count);
				for (int i = 0; i < m_Points.Count; i++) {
					reversedPoints.Add(m_Points[m_Points.Count - 1 - i]);
				}
				m_Points = reversedPoints;
				SetElementDirty();
			}
		}

		/// <summary>
		/// Adds a point to the collection.
		/// </summary>
		/// <param name="element"></param>
		public void Add(T element) {
			if (!points.Contains(element)) {
				points.Add(element);
				SetElementDirty();
			}
		}

		/// <summary>
		/// Adds a range of points to the collection.
		/// </summary>
		/// <param name="elements"></param>
		public void AddRange(T[] elements) {
			for (int i = 0; i < elements.Length; i++) {
				Add(elements[i]);
			}
		}

		/// <summary>
		/// Adds a range of points to the collection.
		/// </summary>
		/// <param name="elements"></param>
		public void AddRange(List<T> elements) {
			for (int i = 0; i < elements.Count; i++) {
				Add(elements[i]);
			}
		}

		/// <summary>
		/// Removes point from collection.
		/// </summary>
		/// <param name="element"></param>
		public void Remove(T element) {
			if (points.Contains(element)) {
				points.Remove(element);
				SetElementDirty();
			}
		}

		/// <summary>
		/// Moves point from the provided source index to destination index.
		/// </summary>
		/// <param name="srcIndex">source index</param>
		/// <param name="dstIndex">destination index</param>
		public void Move(int srcIndex, int dstIndex) {
			if (dstIndex > -1 && dstIndex < points.Count) {
				T item = points[srcIndex];
				points.RemoveAt(srcIndex);
				points.Insert(dstIndex, item);
				SetElementDirty();
			}
		}

		/// <summary>
		/// Insert a point at the provided index.
		/// </summary>
		/// <param name="index"></param>
		public void InsertAt(int index) {
			if (index > -1 && index < points.Count) {
				T item = points[index];
				points.Insert(index, item);
				SetElementDirty();
			}
		}

		/// <summary>
		/// Remove point at the provided index.
		/// </summary>
		/// <param name="index"></param>
		public void RemoveAt(int index) {
			if (index > -1 && index < points.Count) {
				Remove(points[index]);
			}
		}

		/// <summary>
		/// Clears the list of point.
		/// </summary>
		public void Clear() {
			points.Clear();
			SetElementDirty();
		}

		protected override void OnEnable() {
			if (m_Points == null) m_Points = new List<T>();
			base.OnEnable();
		}

		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_PointsShape<T> shape = element as LW_PointsShape<T>;
			if (shape != null) {
				m_Points = new List<T>(shape.m_Points);
			}
			base.CopyPropertiesFrom(element);
		}

	}

	/// <summary>
	/// Derived abstract class for all shapes that are made up of Vector2 points.
	/// </summary>
	/// <remarks>
	/// Line, Polygon, and Polyline2D are all LW_Vector2Shape.
	/// </remarks>
	public abstract class LW_Vector2Shape : LW_PointsShape<Vector2> {

		public override LW_Polyline2D ConvertToPolyline2D() {
			LW_Polyline2D polyline = CreateInstance<LW_Polyline2D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector3[] polylinePoints = new Vector3[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i];
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Polyline3D ConvertToPolyline3D() {
			LW_Polyline3D polyline = CreateInstance<LW_Polyline3D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector3[] polylinePoints = new Vector3[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i];
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Path2D ConvertToPath2D() {
			LW_Path2D path = CreateInstance<LW_Path2D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point2D[] pathPoints = new LW_Point2D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = new LW_Point2D(m_Points[i]);
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}
		public override LW_Path3D ConvertToPath3D() {
			LW_Path3D path = CreateInstance<LW_Path3D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point3D[] pathPoints = new LW_Point3D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = new LW_Point3D(m_Points[i]);
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}
			
		internal override void RebuildContourPoints(ref List<Vector3> contourPoints, int segmentation = 8) {
			if (contourPoints == null || m_Points == null || m_Points.Count < 2) return;

			int capacity = m_IsClosed ? m_Points.Count+1 : m_Points.Count;

			if (contourPoints == null) contourPoints = new List<Vector3>(capacity);
			else if (contourPoints.Capacity < capacity) contourPoints.Capacity = capacity;
			contourPoints.Clear();

			for (int i=0; i<m_Points.Count; i++) {
				contourPoints.Add(m_Points[i]);
			}
			if (m_IsClosed) contourPoints.Add(m_Points[0]);

			base.RebuildContourPoints(ref contourPoints, segmentation);
		}
	}

	/// <summary>
	/// Derived abstract class for all shapes that are made up of Vector3 points.
	/// </summary>
	/// <remarks>
	/// Polyline3D is a LW_Vector3Shape.
	/// </remarks>
	public abstract class LW_Vector3Shape : LW_PointsShape<Vector3> {

		public override LW_Polyline2D ConvertToPolyline2D() {
			LW_Polyline2D polyline = CreateInstance<LW_Polyline2D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector2[] polylinePoints = new Vector2[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i];
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Polyline3D ConvertToPolyline3D() {
			LW_Polyline3D polyline = CreateInstance<LW_Polyline3D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector3[] polylinePoints = new Vector3[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i];
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Path2D ConvertToPath2D() {
			LW_Path2D path = CreateInstance<LW_Path2D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point2D[] pathPoints = new LW_Point2D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = new LW_Point2D(m_Points[i]);
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}
		public override LW_Path3D ConvertToPath3D() {
			LW_Path3D path = CreateInstance<LW_Path3D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point3D[] pathPoints = new LW_Point3D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = new LW_Point3D(m_Points[i]);
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}

		internal override void RebuildContourPoints(ref List<Vector3> contourPoints, int segmentation = 8) {
			if (contourPoints == null || m_Points == null || m_Points.Count < 2) return;

			int capacity = m_IsClosed ? m_Points.Count+1 : m_Points.Count;

			if (contourPoints == null) contourPoints = new List<Vector3>(capacity);
			else if (contourPoints.Capacity < capacity) contourPoints.Capacity = capacity;
			contourPoints.Clear();

			for (int i=0; i<m_Points.Count; i++) {
				contourPoints.Add(m_Points[i]);
			}
			if (m_IsClosed) contourPoints.Add(m_Points[0]);

			base.RebuildContourPoints(ref contourPoints, segmentation);
		}
	}

	/// <summary>
	/// Derived abstract class for all shapes that are made up of LW_Point2D points.
	/// </summary>
	/// <remarks>
	/// Circle, Ellipse, and Path2D are all LW_Point2DShape.
	/// </remarks>
	public abstract class LW_Point2DShape : LW_PointsShape<LW_Point2D> {

		public override LW_Polyline2D ConvertToPolyline2D() {
			LW_Polyline2D polyline = CreateInstance<LW_Polyline2D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector2[] polylinePoints = new Vector2[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i].position;
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Polyline3D ConvertToPolyline3D() {
			LW_Polyline3D polyline = CreateInstance<LW_Polyline3D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector3[] polylinePoints = new Vector3[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i].position;
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Path2D ConvertToPath2D() {
			LW_Path2D path = CreateInstance<LW_Path2D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point2D[] pathPoints = new LW_Point2D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = m_Points[i];
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}
		public override LW_Path3D ConvertToPath3D() {
			LW_Path3D path = CreateInstance<LW_Path3D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point3D[] pathPoints = new LW_Point3D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = m_Points[i];
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}

		internal override void RebuildContourPoints(ref List<Vector3> contourPoints, int segmentation = 8) {
			if (contourPoints == null || m_Points == null || m_Points.Count < 2) return;
			LW_Point2D pt1, pt2;

			// Calc Capacity
			int capacity = 1;
			pt1 = m_Points[0];
			for (int i = 1; i < m_Points.Count; i++) {
				pt2 = m_Points[i];
				if (pt1.hasHandleOut || pt2.hasHandleIn) capacity += segmentation;
				else capacity++;
				pt1 = pt2;
			}
			if (isClosed) {
				pt2 = m_Points[0];
				if (pt1.hasHandleOut || pt2.hasHandleIn) capacity += segmentation;
				else capacity++;
			}

			if (contourPoints == null) contourPoints = new List<Vector3>(capacity);
			else if (contourPoints.Capacity < capacity) contourPoints.Capacity = capacity;
			contourPoints.Clear();

			// Calc Points
			if (capacity > 0) {
				pt1 = m_Points[0];
				contourPoints.Add(pt1.position);
				for (int i=1; i<m_Points.Count; i++) {
					pt2 = m_Points[i];
					if ((pt1.position - pt2.position).sqrMagnitude > 0.01f) CalcCurve(ref contourPoints, pt1, pt2, segmentation);
					pt1 = pt2;
				}

				if (isClosed) {
					pt2 = m_Points[0];
					if ((pt1.position - pt2.position).sqrMagnitude > 0.01f) CalcCurve(ref contourPoints, pt1, pt2, segmentation);
				}
			}

			base.RebuildContourPoints(ref contourPoints, segmentation);
		}
			
		protected void CalcCurve(ref List<Vector3> pointsArray, LW_Point2D pt1, LW_Point2D pt2, int segmentation) {
			if (!pt1.hasHandleOut && !pt2.hasHandleIn) {
				pointsArray.Add(pt2.position);
			}
			else {
				Vector2 p1 = pt1.position;
				Vector2 c1 = pt1.hasHandleOut ? pt1.position+pt1.handleOut : pt1.position;
				Vector2 c2 = pt2.hasHandleIn ? pt2.position+pt2.handleIn : pt2.position;
				Vector2 p2 = pt2.position;

				float t = 0;
				Vector3 point;

				for(int i=0; i<segmentation; i++){
					t = (i+1)*1.0f/segmentation;
					point = ((p1*(1-t) + c1*t)*(1-t) + (c1*(1-t) + c2*t)*t)*(1-t) + ((c1*(1-t) + c2*t)*(1-t) + (c2*(1-t) + p2*t)*t)*t;
					pointsArray.Add(point);
				}
			}
		}
	}

	/// <summary>
	/// Derived abstract class for all shapes that are made up of LW_Point3D points.
	/// </summary>
	/// <remarks>
	/// Path3D are all LW_Point3DShape.
	/// </remarks>
	public abstract class LW_Point3DShape : LW_PointsShape<LW_Point3D> {

		public override LW_Polyline2D ConvertToPolyline2D() {
			LW_Polyline2D polyline = CreateInstance<LW_Polyline2D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector2[] polylinePoints = new Vector2[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i].position;
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Polyline3D ConvertToPolyline3D() {
			LW_Polyline3D polyline = CreateInstance<LW_Polyline3D>();
			if (m_Points != null && m_Points.Count > 1) {
				Vector3[] polylinePoints = new Vector3[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					polylinePoints[i] = m_Points[i].position;
				}
				polyline.Set(polylinePoints, m_IsClosed);
			}
			return polyline;
		}
		public override LW_Path2D ConvertToPath2D() {
			LW_Path2D path = CreateInstance<LW_Path2D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point2D[] pathPoints = new LW_Point2D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = m_Points[i];
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}
		public override LW_Path3D ConvertToPath3D() {
			LW_Path3D path = CreateInstance<LW_Path3D>();
			if (m_Points != null && m_Points.Count > 1) {
				LW_Point3D[] pathPoints = new LW_Point3D[m_Points.Count];
				for (int i = 0; i < m_Points.Count; i++) {
					pathPoints[i] = m_Points[i];
				}
				path.Set(pathPoints, m_IsClosed);
			}
			return path;
		}

		protected Vector3 lastPoint;
		protected Vector3 currPoint;
		protected Vector3 nextPoint;

		internal override void RebuildContourPoints(ref List<Vector3> contourPoints, int segmentation = 8) {
			if (contourPoints == null || m_Points == null || m_Points.Count < 2) return;
			LW_Point3D pt1, pt2;

			// Calc Capacity
			int capacity = 1;
			pt1 = m_Points[0];
			for (int i = 1; i < m_Points.Count; i++) {
				pt2 = m_Points[i];
				if (pt1.hasHandleOut || pt2.hasHandleIn) capacity += segmentation;
				else capacity++;
				pt1 = pt2;
			}
			if (isClosed) {
				pt2 = m_Points[0];
				if (pt1.hasHandleOut || pt2.hasHandleIn) capacity += segmentation;
				else capacity++;
			}

			if (contourPoints == null) contourPoints = new List<Vector3>(capacity);
			else if (contourPoints.Capacity < capacity) contourPoints.Capacity = capacity;
			contourPoints.Clear();

			// Calc Points
			if (capacity > 0) {
				pt1 = m_Points[0];
				contourPoints.Add(pt1.position);
				for (int i=1; i<m_Points.Count; i++) {
					pt2 = m_Points[i];
					if ((pt1.position - pt2.position).sqrMagnitude > 0.01f) CalcCurve(ref contourPoints, pt1, pt2, segmentation);
					pt1 = pt2;
				}

				if (isClosed) {
					pt2 = m_Points[0];
					if ((pt1.position - pt2.position).sqrMagnitude > 0.01f) CalcCurve(ref contourPoints, pt1, pt2, segmentation);
				}
			}

			base.RebuildContourPoints(ref contourPoints, segmentation);
		}
			
		protected void CalcCurve(ref List<Vector3> pointsArray, LW_Point3D pt1, LW_Point3D pt2, int segmentation) {
			if (!pt1.hasHandleOut && !pt2.hasHandleIn) {
				pointsArray.Add(pt2.position);
			}
			else {
				Vector3 p1 = pt1.position;
				Vector3 c1 = pt1.hasHandleOut ? pt1.position+pt1.handleOut : pt1.position;
				Vector3 c2 = pt2.hasHandleIn ? pt2.position+pt2.handleIn : pt2.position;
				Vector3 p2 = pt2.position;

				float t = 0;
				Vector3 point;

				for(int i=0; i<segmentation; i++){
					t = (i+1)*1.0f/segmentation;
					point = ((p1*(1-t) + c1*t)*(1-t) + (c1*(1-t) + c2*t)*t)*(1-t) + ((c1*(1-t) + c2*t)*(1-t) + (c2*(1-t) + p2*t)*t)*t;
					pointsArray.Add(point);
				}
			}
		}

	}

	public enum PointType { Free, Smooth, Symetric };

	/// <summary>
	/// Control Point using Vector2 points.
	/// </summary>
	[System.Serializable]
	public struct LW_Point2D {

		/// <summary>
		/// The point position
		/// </summary>
		public Vector2 position {
			get {
				return m_Position;
			}
			set {
				if (m_Position != value) {
					m_Position = value;
				}
			}
		}
		[SerializeField] private Vector2 m_Position;

		/// <summary>
		/// The point type controls the relationship between the handle-in and the handle-out.
		/// </summary>
		/// <remarks>
		/// Free - There is no relationship between handles. This allows for corners.
		/// Smooth - Handles are parrallel but can be of different magnitude.
		/// Symetric = Handles are equal and opposite. 
		/// </remarks>
		public PointType pointType {
			get {
				return m_PointType;
			}
			set {
				if (m_PointType != value) {
					m_PointType = value;
					EnforcePointType();
				}
			}
		}
		[SerializeField] private PointType m_PointType;

		/// <summary>
		/// The handle point controlling how the bezier curve looks before this point.
		/// </summary>
		public Vector2 handleIn {
			get {
				return m_HandleIn;
			}
			set {
				m_HasHandleIn = true;
				if (m_HandleIn != value) {
					m_HandleIn = value;
					EnforcePointType(true);
				}
			}
		}
		[SerializeField] private Vector2 m_HandleIn;

		/// <summary>
		/// Toggles if the point should use the handle in.
		/// </summary>
		public bool hasHandleIn {
			get {
				return m_HasHandleIn;
			}
			set {
				if (m_HasHandleIn != value) {
					m_HasHandleIn = value;
					EnforcePointType(false);
				}
			}
		}
		[SerializeField] private bool m_HasHandleIn;

		/// <summary>
		/// The handle point controlling how the bezier curve looks after this point.
		/// </summary>
		public Vector2 handleOut {
			get {
				return m_HandleOut;
			}
			set {
				m_HasHandleOut = true;
				if (m_HandleOut != value) {
					m_HandleOut = value;
					EnforcePointType(false);
				}
			}
		}
		[SerializeField] private Vector2 m_HandleOut;

		/// <summary>
		/// Toggles if the point should use the handle out.
		/// </summary>
		public bool hasHandleOut {
			get {
				return m_HasHandleOut;
			}
			set {
				if (m_HasHandleOut != value) {
					m_HasHandleOut = value;
					EnforcePointType(true);
				}
			}
		}
		[SerializeField] private bool m_HasHandleOut;

		public LW_Point2D(Vector2 position, Vector2 handleIn = default(Vector2), Vector2 handleOut = default(Vector2), PointType pointType = PointType.Free) {
			m_Position = position;
			if (handleIn != Vector2.zero) {
				m_HandleIn = handleIn;
				m_HasHandleIn = true;
			}
			else {
				m_HandleIn = -Vector2.one;
				m_HasHandleIn = false;
			}
			if (handleOut != Vector2.zero) {
				m_HandleOut = handleOut;
				m_HasHandleOut = true;
			}
			else {
				m_HandleOut = Vector2.one;
				m_HasHandleOut = false;
			}
			m_PointType = pointType;
		}

		public override string ToString() {
			return "Position: " + m_Position + " HandleIn: (" + m_HasHandleIn + ", " + m_HandleIn + ") HandleOut: (" + m_HasHandleOut + ", " + m_HandleOut + ")";
		}
		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) {
			return other is LW_Point2D && EqualPoint((LW_Point2D)other);
		}

		public static LW_Point2D zero {
			get {
				return new LW_Point2D(Vector2.zero);
			}
		}
		public static bool operator ==(LW_Point2D lhs, object rhs) {
			return lhs.Equals(rhs);
		}
		public static bool operator !=(LW_Point2D lhs, object rhs) {
			return !lhs.Equals(rhs);
		}

		public static implicit operator Vector2(LW_Point2D point) {
			return point.position;
		}
		public static implicit operator LW_Point2D(Vector2 point) {
			return new LW_Point2D(point);
		}
		public static implicit operator LW_Point3D(LW_Point2D point) {
			LW_Point3D newPoint = new LW_Point3D(point.position, point.handleIn, point.handleOut, point.pointType);
			newPoint.hasHandleIn = point.hasHandleIn;
			newPoint.hasHandleOut = point.hasHandleOut;
			return newPoint;
		}

		private void EnforcePointType(bool keepHandleIn = true) {
			if (keepHandleIn) {
				if (pointType == PointType.Smooth) {
					m_HandleOut = -m_HandleIn.normalized * handleOut.magnitude;
				}
				else if (pointType == PointType.Symetric) {
					m_HandleOut = -m_HandleIn;
				}
			}
			else {
				if (pointType == PointType.Smooth) {
					m_HandleIn = -m_HandleOut.normalized * handleIn.magnitude;
				}
				else if (pointType == PointType.Symetric) {
					m_HandleIn = -m_HandleOut;
				}
			}

		}
		private bool EqualPoint(LW_Point2D other) {
			return (
				position == other.position &&
				pointType == other.pointType &&
				handleIn == other.handleIn &&
				hasHandleIn == other.hasHandleIn &&
				handleOut == other.handleOut &&
				hasHandleOut == other.hasHandleOut
			);
		}
	}

	[System.Serializable]
	public struct LW_Point3D {

		/// <summary>
		/// The point position
		/// </summary>
		public Vector3 position {
			get {
				return m_Position;
			}
			set {
				if (m_Position != value) {
					m_Position = value;
				}
			}
		}
		[SerializeField] private Vector3 m_Position;

		/// <summary>
		/// The point type controls the relationship between the handle-in and the handle-out.
		/// </summary>
		/// <remarks>
		/// Free - There is no relationship between handles. This allows for corners.
		/// Smooth - Handles are parrallel but can be of different magnitude.
		/// Symetric = Handles are equal and opposite. 
		/// </remarks>
		public PointType pointType {
			get {
				return m_PointType;
			}
			set {
				if (m_PointType != value) {
					m_PointType = value;
					EnforcePointType();
				}
			}
		}
		[SerializeField] private PointType m_PointType;

		/// <summary>
		/// The handle point controlling how the bezier curve looks before this point.
		/// </summary>
		public Vector3 handleIn {
			get {
				return m_HandleIn;
			}
			set {
				m_HasHandleIn = true;
				if (m_HandleIn != value) {
					m_HandleIn = value;
					EnforcePointType(true);
				}
			}
		}
		[SerializeField] private Vector3 m_HandleIn;

		/// <summary>
		/// Toggles if the point should use the handle in.
		/// </summary>
		public bool hasHandleIn {
			get {
				return m_HasHandleIn;
			}
			set {
				if (m_HasHandleIn != value) {
					m_HasHandleIn = value;
					EnforcePointType(false);
				}
			}
		}
		[SerializeField] private bool m_HasHandleIn;

		/// <summary>
		/// The handle point controlling how the bezier curve looks after this point.
		/// </summary>
		public Vector3 handleOut {
			get {
				return m_HandleOut;
			}
			set {
				m_HasHandleOut = true;
				if (m_HandleOut != value) {
					m_HandleOut = value;
					EnforcePointType(false);
				}
			}
		}
		[SerializeField] private Vector3 m_HandleOut;

		/// <summary>
		/// Toggles if the point should use the handle out.
		/// </summary>
		public bool hasHandleOut {
			get {
				return m_HasHandleOut;
			}
			set {
				if (m_HasHandleOut != value) {
					m_HasHandleOut = value;
					EnforcePointType(true);
				}
			}
		}
		[SerializeField] private bool m_HasHandleOut;

		public LW_Point3D(Vector3 position, Vector3 handleIn = default(Vector3), Vector3 handleOut = default(Vector3), PointType pointType = PointType.Free) {
			m_Position = position;
			if (handleIn != Vector3.zero) {
				m_HandleIn = handleIn;
				m_HasHandleIn = true;
			}
			else {
				m_HandleIn = -Vector3.one;
				m_HasHandleIn = false;
			}
			if (handleOut != Vector3.zero) {
				m_HandleOut = handleOut;
				m_HasHandleOut = true;
			}
			else {
				m_HandleOut = Vector3.one;
				m_HasHandleOut = false;
			}
			m_PointType = pointType;
		}

		public override string ToString() {
			return "Position: " + m_Position + " HandleIn: (" + m_HasHandleIn + ", " + m_HandleIn + ") HandleOut: (" + m_HasHandleOut + ", " + m_HandleOut + ")";
		}
		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) {
			return other is LW_Point3D && EqualPoint((LW_Point3D)other);
		}

		public static LW_Point3D zero {
			get {
				return new LW_Point3D(Vector3.zero);
			}
		}
		public static bool operator ==(LW_Point3D lhs, object rhs) {
			return lhs.Equals(rhs);
		}
		public static bool operator !=(LW_Point3D lhs, object rhs) {
			return !lhs.Equals(rhs);
		}

		public static implicit operator Vector3(LW_Point3D point) {
			return point.position;
		}
		public static implicit operator LW_Point3D(Vector3 point) {
			return new LW_Point3D(point);
		} 
		public static implicit operator LW_Point2D(LW_Point3D point) {
			LW_Point2D newPoint = new LW_Point2D(point.position, point.handleIn, point.handleOut, point.pointType);
			newPoint.hasHandleIn = point.hasHandleIn;
			newPoint.hasHandleOut = point.hasHandleOut;
			return newPoint;
		}

		private void EnforcePointType(bool keepHandleIn = true) {
			if (keepHandleIn) {
				if (pointType == PointType.Smooth) {
					m_HandleOut = -m_HandleIn.normalized * handleOut.magnitude;
				}
				else if (pointType == PointType.Symetric) {
					m_HandleOut = -m_HandleIn;
				}
			}
			else {
				if (pointType == PointType.Smooth) {
					m_HandleIn = -m_HandleOut.normalized * handleIn.magnitude;
				}
				else if (pointType == PointType.Symetric) {
					m_HandleIn = -m_HandleOut;
				}
			}

		}
		private bool EqualPoint(LW_Point3D other) {
			return (
				position == other.position &&
				pointType == other.pointType &&
				handleIn == other.handleIn &&
				hasHandleIn == other.hasHandleIn &&
				handleOut == other.handleOut &&
				hasHandleOut == other.hasHandleOut
			);
		}

	}

}
