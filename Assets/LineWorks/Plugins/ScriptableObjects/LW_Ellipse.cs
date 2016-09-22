// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines an Ellipse
	/// </summary>
	public class LW_Ellipse : LW_Point2DShape {

		/// <summary>
		/// Center point of Ellipse
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
		/// Radius in the x-axis
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
		/// Radius in the y-axis
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
		/// Create the specified center, radiusX and radiusY.
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="radiusX">Radius x.</param>
		/// <param name="radiusY">Radius y.</param>
		public static LW_Ellipse Create(Vector2 center = default(Vector2), float radiusX = 20f, float radiusY = 20f) {
			LW_Ellipse instance = CreateInstance<LW_Ellipse>();
			instance.Set(center, radiusX, radiusY);
			return instance;
		}

		/// <summary>
		/// Set all of the ellipse properties in one call.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="radiusX"></param>
		/// <param name="radiusY"></param>
		public void Set(Vector2 center = default(Vector2), float radiusX = 20f, float radiusY = 20f) {
			m_IsClosed = true;
			m_Center = center;
			m_RadiusX = radiusX;
			m_RadiusY = radiusY;
			SetElementDirty();
		}

		/// <summary>
		/// Creates a copy of this ellipse.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Ellipse) {
				element.CopyPropertiesFrom (this);
			}
			else {
				element = Copy(CreateInstance<LW_Ellipse>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Ellipse ellipse = element as LW_Ellipse;
			if (ellipse != null) {
				m_Center = ellipse.m_Center;
				m_RadiusX = ellipse.m_RadiusX;
				m_RadiusY = ellipse.m_RadiusY;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Ellipse";
			base.OnEnable();
		}
		internal override void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
				m_IsClosed = true;
				if (m_Points == null) new List<LW_Point2D>(4);
				else m_Points.Clear();

				m_Points.Add(new LW_Point2D(m_Center+new Vector2(0,m_RadiusY), new Vector2(m_RadiusX*c_ArcAprox,0), new Vector2(-m_RadiusX*c_ArcAprox,0), PointType.Symetric));
				m_Points.Add(new LW_Point2D(m_Center+new Vector2(-m_RadiusX,0), new Vector2(0,m_RadiusY*c_ArcAprox), new Vector2(0,-m_RadiusY*c_ArcAprox), PointType.Symetric));
				m_Points.Add(new LW_Point2D(m_Center+new Vector2(0,-m_RadiusY), new Vector2(-m_RadiusX*c_ArcAprox,0), new Vector2(m_RadiusX*c_ArcAprox,0), PointType.Symetric));
				m_Points.Add(new LW_Point2D(m_Center+new Vector2(m_RadiusX,0), new Vector2(0,-m_RadiusY*c_ArcAprox), new Vector2(0,m_RadiusY*c_ArcAprox), PointType.Symetric));
			}
			base.RebuildShape(force);
		}
	}
}
