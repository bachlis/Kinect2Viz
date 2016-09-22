// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a two(2) dimensional bezier path.
	/// </summary>
	public class LW_Path2D : LW_Point2DShape {

		/// <summary>
		/// Create the specified points and isClosed.
		/// </summary>
		/// <param name="points">Points.</param>
		/// <param name="isClosed">If set to <c>true</c> is closed.</param>
		public static LW_Path2D Create(Vector2[] points, bool isClosed = false) {
			LW_Path2D instance = CreateInstance<LW_Path2D>();
			instance.Set(points, isClosed);
			return instance;
		}
		public static LW_Path2D Create(Vector3[] points, bool isClosed = false) {
			LW_Path2D instance = CreateInstance<LW_Path2D>();
			instance.Set(points, isClosed);
			return instance;
		}
		public static LW_Path2D Create(LW_Point2D[] points, bool isClosed = false) {
			LW_Path2D instance = CreateInstance<LW_Path2D>();
			instance.Set(points, isClosed);
			return instance;
		}

		/// <summary>
		/// Set all properties of the path in one call.
		/// </summary>
		/// <param name="points"></param>
		/// <param name="isClosed"></param>
		public void Set(Vector2[] points, bool isClosed = false) {
			LW_Point2D[] pathPoints = null;
			if (points != null && points.Length > 1) {
				pathPoints =  new LW_Point2D[points.Length];
				for (int i=0; i<points.Length; i++) pathPoints[i] = new LW_Point2D(points[i]);
			}
			Set(pathPoints, isClosed);
		}
		public void Set(Vector3[] points, bool isClosed = false) {
			LW_Point2D[] pathPoints = null;
			if (points != null && points.Length > 1) {
				pathPoints =  new LW_Point2D[points.Length];
				for (int i=0; i<points.Length; i++) pathPoints[i] = new LW_Point2D((Vector2)points[i]);
			}
			Set(pathPoints, isClosed);
		}
		public void Set(LW_Point2D[] points, bool isClosed = false) {
			m_IsClosed = isClosed;
			if (points == null) {

				points = new LW_Point2D[]{
					new LW_Point2D(new Vector2(-40,40), new Vector2(-40*c_ArcAprox,0), new Vector2(40*c_ArcAprox,0), PointType.Symetric),
					new LW_Point2D(new Vector2(0,-40), new Vector2(-40*c_ArcAprox,0), new Vector2(40*c_ArcAprox,0), PointType.Symetric),
					new LW_Point2D(new Vector2(40,40), new Vector2(-40*c_ArcAprox,0), new Vector2(40*c_ArcAprox,0), PointType.Symetric)
				};
			}
			m_Points = new List<LW_Point2D>(points);
			SetElementDirty();
		}

		/// <summary>
		/// Copy path.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Path2D) {
				element.CopyPropertiesFrom (this);
			}
			else {
				element = Copy(CreateInstance<LW_Path2D>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Path 2D";
			base.OnEnable();
		}
	}
}