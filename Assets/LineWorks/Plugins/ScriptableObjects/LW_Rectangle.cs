// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	/// <summary>
	/// Defines a rectangle.
	/// </summary>
	public class LW_Rectangle : LW_Point2DShape {

		/// <summary>
		/// Center point of rectangle.
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
		/// Width of rectangle.
		/// </summary>
		public float width {
			get {
				return m_Width;
			}
			set {
				if (m_Width != value) {
					m_Width = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_Width = 80;

		/// <summary>
		/// Height of rectangle.
		/// </summary>
		public float height {
			get {
				return m_Height;
			}
			set {
				if (m_Height != value) {
					m_Height = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_Height = 80;

		/// <summary>
		/// Radius of the rounded corners in the x-axis.
		/// </summary>
		/// <remarks>
		/// If set to 0, rectangle will not have rounded corners.
		/// </remarks>
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
		[SerializeField] protected float m_RadiusX = 0;

		/// <summary>
		/// Radius of the rounded corners in the y-axis.
		/// </summary>
		/// <remarks>
		/// If set to 0, rectangle will not have rounded corners.
		/// </remarks>
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
		[SerializeField] protected float m_RadiusY = 0;

		/// <summary>
		/// Create the specified center, width, height, cornerRadiusX and cornerRadiusY.
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="cornerRadiusX">Corner radius x.</param>
		/// <param name="cornerRadiusY">Corner radius y.</param>
		public static LW_Rectangle Create(Vector2 center = default(Vector2), float width = 80f, float height = 80f, float cornerRadiusX = 0, float cornerRadiusY = 0) {
			LW_Rectangle instance = CreateInstance<LW_Rectangle>();
			instance.Set(center, width, height, cornerRadiusX, cornerRadiusY);
			return instance;
		}

		/// <summary>
		/// Set all properties of the rectangle in one call.
		/// </summary>
		/// <param name="center"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="cornerRadiusX"></param>
		/// <param name="cornerRadiusY"></param>
		public void Set(Vector2 center = default(Vector2), float width = 80f, float height = 80f, float cornerRadiusX = 0, float cornerRadiusY = 0) {
			m_IsClosed = true;
			m_Center = center;
			m_Width = width;
			m_Height = height;
			m_RadiusX = cornerRadiusX;
			m_RadiusY = cornerRadiusY;
			SetElementDirty();
		}

		/// <summary>
		/// Copy rectangle.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Rectangle) {
				element.CopyPropertiesFrom (this);
			}
			else {
				element = Copy(CreateInstance<LW_Rectangle>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}

		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Rectangle rectangle = element as LW_Rectangle;
			if (rectangle != null) {
				m_Center = rectangle.m_Center;
				m_Width = rectangle.m_Width;
				m_Height = rectangle.m_Height;
				m_RadiusX = rectangle.m_RadiusX;
				m_RadiusY = rectangle.m_RadiusY;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Rectangle";
			base.OnEnable();
		}
		internal override void RebuildShape(bool force = false) {
			if (m_ElementDirty || force) {
				m_IsClosed = true;
				if (m_Points == null) new List<LW_Point2D>(8);
				else m_Points.Clear();

				Vector2 corner = new Vector2(m_Center.x-m_Width/2f, m_Center.y-m_Height/2f);
				if (m_RadiusX != 0 && m_RadiusY != 0) {
					m_Points.Add(new LW_Point2D(new Vector2(corner.x, corner.y+m_RadiusY), new Vector2(0,-m_RadiusY*c_ArcAprox), Vector2.zero));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x, corner.y+m_Height-m_RadiusY), Vector2.zero, new Vector2(0,m_RadiusY*c_ArcAprox)));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_RadiusX, corner.y+m_Height), new Vector2(-m_RadiusX*c_ArcAprox,0), Vector2.zero));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_Width-m_RadiusX, corner.y+m_Height), Vector2.zero, new Vector2(m_RadiusX*c_ArcAprox,0)));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_Width, corner.y+m_Height-m_RadiusY), new Vector2(0,m_RadiusY*c_ArcAprox), Vector2.zero));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_Width, corner.y+m_RadiusY), Vector2.zero, new Vector2(0,-m_RadiusY*c_ArcAprox)));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_Width-m_RadiusX, corner.y), new Vector2(m_RadiusX*c_ArcAprox,0), Vector2.zero));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_RadiusX, corner.y), Vector2.zero, new Vector2(-m_RadiusX*c_ArcAprox,0)));
				}
				else {
					m_Points.Add(new LW_Point2D(new Vector2(corner.x, corner.y)));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x, corner.y+m_Height)));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_Width, corner.y+m_Height)));
					m_Points.Add(new LW_Point2D(new Vector2(corner.x+m_Width, corner.y)));
				}
			}
			base.RebuildShape(force);
		}
	}
}