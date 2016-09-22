// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	public enum FillRule { EvenOdd, NonZero, Positive, Negative, AbsGeqTwo }

	/// <summary>
	/// Fill sets the color inside an associated shape.
	/// </summary>
	public class LW_Fill : LW_PaintStyle {

		/// <summary>
		/// The rule for how a shape is filled by evaluating a value for each region to determine the "insideness" of a point .
		/// </summary>
		/// <remarks>
		/// A regions value is determined by drawing a ray from that point to infinity in any direction and then examining the places where a segment of the shape crosses the ray.
		/// </remarks>
		/// <value>
		/// -	`EvenOdd` - If a region's number is odd, the point is inside; if even, the point is outside.
		/// -	`NonZero` - If a region's number is non-zero, the point is inside.
		/// -	`Positive` - If a region's number is positive, the point is inside.
		/// -	`Negative` - If a region's number is negative, the point is inside.
		/// -	`AbsGeqTwo` - If the absolute value of a region's number is greater than or equal to 2.
		/// </value>
		public FillRule fillRule {
			get {
				return m_FillRule;
			}
			set {
				if (m_FillRule != value) {
					m_FillRule = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected FillRule m_FillRule = FillRule.EvenOdd;

		/// <summary>
		/// A value to control how a non-closed shape can have a fill applied bellow the line to the provided depth.
		/// </summary>
		/// <remarks>
		/// If set to 0, this has no effect. If the value is not zero, additional points are added to the fill to create the landscape effect.
		/// </remarks>
		public float landscapeDepth {
			get {
				return m_LandscapeDepth;
			}
			set {
				if (m_LandscapeDepth != value) {
					m_GeometryDirty = true;
					m_LandscapeDepth = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_LandscapeDepth = 0f;

		/// <summary>
		/// Sets a fill to the provided color.
		/// </summary>
		/// <param name="color"></param>
		public void Set(Color color = default(Color)) {
			if (color == Color.clear) color = Color.white;
			this.color = color;
			SetElementDirty();
		}

		public static LW_Fill Create(Color color = default(Color)) {
			LW_Fill instance = CreateInstance<LW_Fill>();
			instance.Set(color);
			return instance;
		}

		/// <summary>
		/// Copy fill.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Fill) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Fill>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}
			
		internal override void Reset() {
			base.Reset();
			m_FillRule = FillRule.EvenOdd;
			m_LandscapeDepth = 0f;
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Fill fill = element as LW_Fill;
			if (fill != null) {
				m_FillRule = fill.m_FillRule;
				m_LandscapeDepth = fill.m_LandscapeDepth;
			}
			base.CopyPropertiesFrom(element);
		}
		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Fill";
			base.OnEnable();
		}
	}
}