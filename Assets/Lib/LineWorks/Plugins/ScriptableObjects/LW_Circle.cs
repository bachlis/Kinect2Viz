// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a Circle.
	/// </summary>
	public class LW_Circle : LW_Point2DShape {

		/// <summary>
		/// Center point of circle.
		/// </summary>
		public Vector2 center {
			get {
				return m_Center;
			}
			set {
				if (m_Center != value) {
					m_Center = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector2 m_Center = Vector2.zero;

		/// <summary>
		/// radius of circle.
		/// </summary>
		public float radius {
			get {
				return m_Radius;
			}
			set {
				if (m_Radius != value) {
					m_Radius = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_Radius = 40;

		/// <summary>
		/// Create the specified center and radius.
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="radius">Radius.</param>
		public static LW_Circle Create(Vector2 center = default(Vector2), float radius = 40f) {
			LW_Circle instance = CreateInstance<LW_Circle>();
			instance.Set(center, radius);
			return instance;
		}

		/// <summary>
		/// Set all of the circle properties in one call.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radius"></param>
		public void Set(Vector2 center = default(Vector2), float radius = 40f) {
			m_IsClosed = true;
			m_Center = center;
			m_Radius = radius;
			SetElementDirty();
		}

		/// <summary>
		/// Creates a copy of this circle.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Circle) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Circle>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Circle circle = element as LW_Circle;
			if (circle != null) {
				m_Center = circle.m_Center;
				m_Radius = circle.m_Radius;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Circle";
			base.OnEnable();
		}

		internal override void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
				//Debug.Log("RebuildCircle: " + name + " forceRebuild: " + force + " m_ElementDirty: "  + m_ElementDirty + " : " + m_Radius);
				m_IsClosed = true;
				if (m_Points == null) new List<LW_Point2D>(4);
				else m_Points.Clear();

				m_Points.Add(new LW_Point2D(m_Center + new Vector2(0, m_Radius), new Vector2(m_Radius * c_ArcAprox, 0), new Vector2(-m_Radius * c_ArcAprox, 0), PointType.Symetric));
				m_Points.Add(new LW_Point2D(m_Center + new Vector2(-m_Radius, 0), new Vector2(0, m_Radius * c_ArcAprox), new Vector2(0, -m_Radius * c_ArcAprox), PointType.Symetric));
				m_Points.Add(new LW_Point2D(m_Center + new Vector2(0, -m_Radius), new Vector2(-m_Radius * c_ArcAprox, 0), new Vector2(m_Radius * c_ArcAprox, 0), PointType.Symetric));
				m_Points.Add(new LW_Point2D(m_Center + new Vector2(m_Radius, 0), new Vector2(0, -m_Radius * c_ArcAprox), new Vector2(0, m_Radius * c_ArcAprox), PointType.Symetric));
			}
			base.RebuildShape(force);
		}
	}
}
