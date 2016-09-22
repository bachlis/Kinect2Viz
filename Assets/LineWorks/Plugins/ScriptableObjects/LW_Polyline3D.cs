// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a three(3) dimensional polyline.
	/// </summary>
	public class LW_Polyline3D : LW_Vector3Shape {

		/// <summary>
		/// Create the specified points and isClosed.
		/// </summary>
		/// <param name="points">Points.</param>
		/// <param name="isClosed">If set to <c>true</c> is closed.</param>
		public static LW_Polyline3D Create(Vector2[] points, bool isClosed = false) {
			LW_Polyline3D instance = CreateInstance<LW_Polyline3D>();
			instance.Set(points, isClosed);
			return instance;
		}
		public static LW_Polyline3D Create(Vector3[] points, bool isClosed = false) {
			LW_Polyline3D instance = CreateInstance<LW_Polyline3D>();
			instance.Set(points, isClosed);
			return instance;
		}

		/// <summary>
		/// Set all properties of the polyline in one call.
		/// </summary>
		/// <param name="points"></param>
		/// <param name="isClosed"></param>
		public void Set(Vector2[] points, bool isClosed = false) {
			m_IsClosed = isClosed;
			if (points != null) {
				m_Points = new List<Vector3>(points.Length);
				for (int i = 0; i < points.Length; i++) m_Points.Add((Vector3)points[i]);
			}
			else {
				m_Points = new List<Vector3>(3);
				m_Points.Add(new Vector3(-40, 40, -40));
				m_Points.Add(new Vector3(0, -40, 40));
				m_Points.Add(new Vector3(40, 40, -40));
			}
			SetElementDirty();
		}
		public void Set(Vector3[] points, bool isClosed = false) {
			m_IsClosed = isClosed;
			if (points != null) {
				m_Points = new List<Vector3>(points.Length);
				for (int i = 0; i < points.Length; i++) m_Points.Add(points[i]);
			}
			if (points == null) {
				m_Points = new List<Vector3>(3);
				m_Points.Add(new Vector3(-40, 40, -40));
				m_Points.Add(new Vector3(0, -40, 40));
				m_Points.Add(new Vector3(40, 40, -40));
			}
			SetElementDirty();
		}

		/// <summary>
		/// Copy polyline.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Polyline3D) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Polyline3D>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Polyline 3D";
			base.OnEnable();
		}
	}
}
