// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a star.
	/// </summary>
	public class LW_Star : LW_Vector2Shape {

		/// <summary>
		/// Center point of star.
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
		/// The first radius of star.
		/// </summary>
		public float radiusX {
			get {
				return m_RadiusX;
			}
			set {
				if (m_RadiusX != value) {
					m_RadiusX = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_RadiusX = 40;

		/// <summary>
		/// The second radius of star.
		/// </summary>
		public float radiusY {
			get {
				return m_RadiusY;
			}
			set {
				if (m_RadiusY != value) {
					m_RadiusY = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_RadiusY = 20;

		/// <summary>
		/// The number of points on the star.
		/// </summary>
		public int sides {
			get {
				return m_Sides;
			}
			set {
				if (m_Sides != value) {
					m_Sides = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected int m_Sides = 5;

		/// <summary>
		/// Create the specified center, sides, radiusX and radiusY.
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="sides">Sides.</param>
		/// <param name="radiusX">Radius x.</param>
		/// <param name="radiusY">Radius y.</param>
		public static LW_Star Create(Vector2 center = default(Vector2), int sides = 3, float radiusX = 40f, float radiusY = 20f) {
			LW_Star instance = CreateInstance<LW_Star>();
			instance.Set(center, sides, radiusX, radiusY);
			return instance;
		}

		/// <summary>
		/// Set all properties of the star in one call.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="sides"></param>
		/// <param name="radiusX"></param>
		/// <param name="radiusY"></param>
		public void Set(Vector2 center = default(Vector2), int sides = 3, float radiusX = 40f, float radiusY = 20f) {
			m_IsClosed = true;
			m_Center = center;
			m_Sides = sides;
			m_RadiusX = radiusX;
			m_RadiusY = radiusY;
			SetElementDirty();
		}

		/// <summary>
		/// Copy star.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Star) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Star>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Star star = element as LW_Star;
			if (star != null) {
				m_Center = star.m_Center;
				m_RadiusX = star.m_RadiusX;
				m_RadiusY = star.m_RadiusY;
				m_Sides = star.m_Sides;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Star";
			base.OnEnable();
		}
		internal override void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
				m_IsClosed = true;
				if (m_Sides > 2) {
					int parts = m_Sides * 2;
					float partAngle = 360.0f / parts * Mathf.Deg2Rad;
					float currAngle = 90 * Mathf.Deg2Rad;
					if (m_Points == null) new List<Vector2>(parts);
					else m_Points.Clear();

					for (int i = 0; i < parts; i++) {
						if (i % 2 == 0) m_Points.Add(m_Center + new Vector2(Mathf.Cos(currAngle) * m_RadiusY, Mathf.Sin(currAngle) * m_RadiusY));
						else m_Points.Add(m_Center + new Vector2(Mathf.Cos(currAngle) * m_RadiusX, Mathf.Sin(currAngle) * m_RadiusX));
						currAngle += partAngle;
					}
				}
				else m_Points = new List<Vector2>();
			}
			base.RebuildShape(force);
		}
	}
}