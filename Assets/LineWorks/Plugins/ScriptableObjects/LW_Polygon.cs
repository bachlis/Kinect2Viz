// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a regular polygon.
	/// </summary>
	public class LW_Polygon : LW_Vector2Shape {

		/// <summary>
		/// Center point of polygon.
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
		/// Radius of polygon.
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
		/// The number of polygon sides.
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
		/// Create the specified center, sides and radius.
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="sides">Sides.</param>
		/// <param name="radius">Radius.</param>
		public static LW_Polygon Create(Vector2 center = default(Vector2), int sides = 5, float radius = 40f) {
			LW_Polygon instance = CreateInstance<LW_Polygon>();
			instance.Set(center, sides, radius);
			return instance;
		}

		/// <summary>
		/// Set all properties of the polygon in one call.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="sides"></param>
		/// <param name="radius"></param>
		public void Set(Vector2 center = default(Vector2), int sides = 5, float radius = 40f) {
			m_IsClosed = true;
			m_Center = center;
			m_Radius = radius;
			m_Sides = sides;
			SetElementDirty();
		}

		/// <summary>
		/// Copy polygon.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Polygon) {
				element.CopyPropertiesFrom (this);
			}
			else {
				element = Copy(CreateInstance<LW_Polygon>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Polygon polygon = element as LW_Polygon;
			if (polygon != null) {
				m_Center = polygon.m_Center;
				m_Radius = polygon.m_Radius;
				m_Sides = polygon.m_Sides;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Polygon";
			base.OnEnable();
		}
		internal override void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
				m_IsClosed = true;
				if (m_Sides > 2) {
					float partAngle = 360.0f/m_Sides*Mathf.Deg2Rad;
					float currAngle = 90*Mathf.Deg2Rad;
					if (m_Points == null) new List<Vector2>(m_Sides);
					else m_Points.Clear();
					for (int i=0; i<m_Sides; i++) {
						m_Points.Add(m_Center + new Vector2(Mathf.Cos(currAngle)*m_Radius, Mathf.Sin(currAngle)*m_Radius));
						currAngle += partAngle;
					}
				}
				else m_Points = new List<Vector2>();
			}
			base.RebuildShape(force);
		}
	}
}