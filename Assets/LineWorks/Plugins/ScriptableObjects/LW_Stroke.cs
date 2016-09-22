// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {


	public enum Linecap { Butt, Square, Round, Texture }
	public enum Linejoin { Miter, Bevel, Round, Texture, Break }
	public enum Justification { left, center, right }
	
	/// <summary>
	/// Stroke sets the color of the line drawn around an associated shape.
	/// </summary>
	public class LW_Stroke : LW_PaintStyle {

		public override bool isVisible {
			get {
				return base.isVisible && MaxWidth() > 0;
			}
			set {
				base.isVisible = value;
			}
		}

		/// <summary>
		/// A global value for width of the stroke.
		/// </summary>
		/// <remarks>
		/// This sets the global width for the stroke. It's value is multiplied with any variableWidths that the stoke defines.
		/// </remarks>
		public float globalWidth {
			get {
				return m_GlobalWidth;
			}
			set {
				if (m_GlobalWidth != value) {
					m_GlobalWidth = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_GlobalWidth = 1;

		/// <summary>
		/// A list of Width Stops to control the width of a line along the length of the line.
		/// </summary>
		/// <remarks>
		/// Each LW_WidthStop consists of a float for width and a float for percentage (0 to 1). width controls the thickness of the line and percentage controls where along the length of the line to apply this width.  Inbetween two width stops, values are linearly interpolated. 
		/// </remarks>
		public List<LW_WidthStop> widths {
			get {
				//return m_VariableWidths.Clone() as float[];
				return m_VariableWidths;
			}
			set {
				//if (m_VariableWidths != value) {
					m_VariableWidths = value;
					SetElementDirty();
				//}
			}
		}
		[SerializeField] protected List<LW_WidthStop> m_VariableWidths = new List<LW_WidthStop>() { new LW_WidthStop(1f, 0f) };

		/// <summary>
		/// Toggles whether the list of variables widths should use the LW_WidthStop's percentage value to calculate the width or just space them all evenly along the length of a shape.
		/// </summary>
		/// <remarks>
		/// When the value is <c>true</c>, variables widths will use the LW_WidthStop's percentage value to calculate the width. 
		/// When <c>false</c>, variables widths will be spaced evenly along the length of a shapes contour.
		/// </remarks>
		public bool spaceWidthsEvenly {
			get {
				return m_SpaceWidthsEvenly;
			}
			set {
				if (m_SpaceWidthsEvenly != value) {
					m_SpaceWidthsEvenly = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_SpaceWidthsEvenly = false;

		/// <summary>
		/// Toggles whether the list of variables colors should use the LW_ColorStop's percentage value to calculate the color or just space them all evenly along the length of a shape.
		/// </summary>
		/// <remarks>
		/// When the value is <c>true</c>, variables colors will use the LW_ColorStop's percentage value to calculate the color. 
		/// When <c>false</c>, variables colors will be spaced evenly along the length of a shapes contour.
		/// </remarks>
		public bool spaceColorsEvenly {
			get {
				return m_SpaceColorsEvenly;
			}
			set {
				if (m_SpaceColorsEvenly != value) {
					m_SpaceColorsEvenly = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_SpaceColorsEvenly = false;

		/// <summary>
		/// The local toggle for screen-space stroke widths applied to the associated shapes. Only applies if overrideScreenSpace is set to true.
		/// </summary>
		/// <remarks>
		/// It controls how the width of strokes are rendered by the LineWorks shader. Only relevant when shaderVertexMode is not set to Static and material is empty or uses a LineWorks shader. When set to false, stroke width is dynamically calculated by the LineWorks shader in model space units. When set to true, stroke width is dynamically calculated by the LineWorks shader in screen space units.
		/// </remarks>
		public bool screenSpace {
			get {
				return m_ScreenSpace;
			}
			set {
				if (m_ScreenSpace != value) {
					m_ScreenSpace = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_ScreenSpace = false;

		/// <summary>
		/// Specifies the shape to be used at the end of open shapes.
		/// </summary>
		/// <value>
		/// -	`Butt` - Adds no shape to the ends of the line.
		/// -	`Round` - Adds a half-round to the ends of the line.
		/// -	`Square` - Adds a square to the ends of the line.
		/// </value>
		public Linecap linecap {
			get {
				return m_Linecap;
			}
			set {
				if (m_Linecap != value) {
					m_Linecap = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Linecap m_Linecap = Linecap.Butt;

		/// <summary>
		/// Specifies the how to join two adjacent line segments together.
		/// </summary>
		/// <value>
		/// -	`Miter` - Welds the two segments together at the bisecting angle.
		/// -	`Round` - Adds a rounded join where the two segments meet.
		/// -	`Bevel` - Adds a simple bevel where the two segments join.
		/// -	`Break` - No join is applied and the two segments are not joined together.
		/// </value>
		public Linejoin linejoin {
			get {
				return m_Linejoin;
			}
			set {
				if (m_Linejoin != value) {
					m_Linejoin = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Linejoin m_Linejoin = Linejoin.Miter;

		/// <summary>
		/// Imposed limit on the ratio of the miter length to the stroke width. When the limit is exceeded, the join is converted from a miter to a bevel.
		/// </summary>
		public float miterLimit {
			get {
				return m_MiterLimit;
			}
			set {
				if (m_MiterLimit != value) {
					m_MiterLimit = Mathf.Max(value, 1f);
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_MiterLimit = 4f;

		/// <summary>
		/// Controls the justification of drawn stroke to the base line.
		/// </summary>
		/// <value>
		/// -	`Left` - Aligns stroke to the left side of the line.
		/// -	`Center` - Aligns stroke to the center of the line.
		/// -	`Right` - Aligns stroke to the right side of the line.
		/// </value>
		public Justification justification {
			get {
				return m_Justification;
			}
			set {
				if (m_Justification != value) {
					m_Justification = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Justification m_Justification = Justification.center;

		/// <summary>
		/// Controls an angle of rotation to apply to a stroke.
		/// </summary>
		/// <remarks>
		/// This only effects strokes with shaderVertexMode set to Static. Only useful for 3D applications. 
		/// </remarks>
		public float angle {
			get {
				return m_Angle;
			}
			set {
				if (m_Angle != value) {
					m_Angle = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_Angle = 0f;

		/// <summary>
		/// Gets or sets the linecap texture.
		/// </summary>
		/// <remarks>
		/// Only applies if linecap is set to Linecap.Texture. A texture is not necessary to render attached graphics.
		/// </remarks>
		/// <value>The linecap texture.</value>
		public Texture2D capTexture {
			get {
				return m_CapTexture;
			}
			set {
				if (m_Material != null) {
					Debug.LogWarning("capTexture can not be set because a custom material is set.");
				}
				else if (m_CapTexture != value) {
					m_CapTexture = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Texture2D m_CapTexture;

		/// <summary>
		/// Gets or sets the linejoin texture.
		/// </summary>
		/// <remarks>
		/// Only applies if linejoin is set to Linejoin.Texture. A texture is not necessary to render attached graphics.
		/// </remarks>
		/// <value>The linejoin texture.</value>
		public Texture2D joinTexture {
			get {
				return m_JoinTexture;
			}
			set {
				if (m_Material != null) {
					Debug.LogWarning("joinTexture can not be set because a custom material is set.");
				}
				else if (m_JoinTexture != value) {
					m_JoinTexture = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Texture2D m_JoinTexture;

		/// <summary>
		/// Set the color and width properties of the stroke in one call.
		/// </summary>
		/// <param name="color"></param>
		/// <param name="width"></param>
		public void Set(Color color = default(Color), float width = 1f) {
			if (color == Color.clear) color = Color.white;
			this.color = color;
			this.globalWidth = width;
			SetElementDirty();
		}

		public static LW_Stroke Create(Color color = default(Color), float width = 1f) {
			LW_Stroke instance = CreateInstance<LW_Stroke>();
			instance.Set(color, width);
			return instance;
		}

		/// <summary>
		/// Copy stroke.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Stroke) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Stroke>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}
			
		internal override void Reset() {
			base.Reset();
			m_GlobalWidth = 1;
			m_VariableWidths = new List<LW_WidthStop>() { new LW_WidthStop(1f, 0f) };
			m_SpaceWidthsEvenly = false;
			m_SpaceColorsEvenly = false;
			m_ScreenSpace = false;
			m_Linecap = Linecap.Butt;
			m_Linejoin = Linejoin.Miter;
			m_MiterLimit = 4f;
			m_Justification = Justification.center;
			m_Angle = 0f;
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Stroke stroke = element as LW_Stroke;
			if (stroke != null) {
				m_GlobalWidth = stroke.m_GlobalWidth;
				if (stroke.m_VariableWidths != null) m_VariableWidths = new List<LW_WidthStop>(stroke.m_VariableWidths);
				else m_VariableWidths = null;
				m_SpaceWidthsEvenly = stroke.m_SpaceWidthsEvenly;
				m_SpaceColorsEvenly = stroke.m_SpaceColorsEvenly;
				m_ScreenSpace = stroke.m_ScreenSpace;
				m_Linecap = stroke.m_Linecap;
				m_Linejoin = stroke.m_Linejoin;
				m_MiterLimit = stroke.m_MiterLimit;
				m_Justification = stroke.m_Justification;
				m_Angle = stroke.m_Angle;
			}
			base.CopyPropertiesFrom(element);
		}
		internal float MaxWidth() {
			float width = 0;
			if (m_VariableWidths != null && m_VariableWidths.Count > 0) {
				if (m_VariableWidths.Count == 1) {
					width = m_VariableWidths[0].width;
				}
				else {
					for (int i = 0; i < m_VariableWidths.Count; i++) {
						if (m_VariableWidths[i].width > width) width = m_VariableWidths[i].width;
					}
				}
			}
			else width = 1;
			return width * globalWidth;
		}
			
		internal float WidthAtPercentage(float percentage) {
			float width = 0;
			if (m_VariableWidths != null && m_VariableWidths.Count > 0) {
				if (m_VariableWidths.Count == 1) {
					width = m_VariableWidths[0].width;
				}
				else {
					if (m_SpaceWidthsEvenly) {
						int beforeIndex = Mathf.FloorToInt(percentage * (m_VariableWidths.Count-1));
						int afterIndex = beforeIndex < m_VariableWidths.Count - 1 ? beforeIndex + 1 : beforeIndex;
						LW_WidthStop beforeStop = m_VariableWidths[beforeIndex];
						LW_WidthStop afterStop = m_VariableWidths[afterIndex];
						float beforePercentage = (float)beforeIndex / (float)(m_VariableWidths.Count-1);
						float afterPercentage = (float)afterIndex / (float)(m_VariableWidths.Count-1);
						if (beforePercentage != afterPercentage) {
							float t = (percentage - beforePercentage) / (afterPercentage - beforePercentage);
							width = Mathf.Lerp(beforeStop.width, afterStop.width, t);
						}
						else {
							width = beforeStop.width;
						}

					}
					else {
						int closestIndex = 0;
						for (int i = 0; i < m_VariableWidths.Count; i++) {
							if (m_VariableWidths[i].percentage < percentage) closestIndex = i;
							else break;
						}
						LW_WidthStop start = m_VariableWidths[closestIndex];
						LW_WidthStop end = closestIndex < m_VariableWidths.Count - 1 ? m_VariableWidths[closestIndex + 1] : m_VariableWidths[closestIndex];

						if (start.percentage != end.percentage) {
							float t = (percentage - start.percentage) / (end.percentage - start.percentage);
							width = Mathf.Lerp(start.width, end.width, t);
						}
						else {
							width = start.width;
						}
					}
				}
			}
			else width = 1;
			return width * globalWidth;
		}
		internal override Color ColorAtPercentage(float percentage) {
			Color color = s_White;
			switch (m_PaintMode) {
				case PaintMode.Solid:
					if (m_GradientColors != null && m_GradientColors.Count > 0) {
						color = m_GradientColors[0].color;
					}
					else color = s_White;
					break;
				case PaintMode.LinearGradient:
				case PaintMode.RadialGradient:
					if (m_GradientColors != null && m_GradientColors.Count > 0) {
						if (m_GradientColors.Count == 1) {
							color = m_GradientColors[0].color;
						}
						else {
							if (m_SpaceColorsEvenly) {
								int beforeIndex = Mathf.FloorToInt(percentage * (m_GradientColors.Count-1));
								int afterIndex = beforeIndex < m_GradientColors.Count - 1 ? beforeIndex + 1 : beforeIndex;
								LW_ColorStop beforeStop = m_GradientColors[beforeIndex];
								LW_ColorStop afterStop = m_GradientColors[afterIndex];
								float beforePercentage = (float)beforeIndex / (float)(m_GradientColors.Count-1);
								float afterPercentage = (float)afterIndex / (float)(m_GradientColors.Count-1);
								if (beforePercentage != afterPercentage) {
									float t = (percentage - beforePercentage) / (afterPercentage - beforePercentage);
									color = Color.Lerp(beforeStop.color, afterStop.color, t);
								}
								else {
									color = beforeStop.color;
								}
							}
							else {
								int closestIndex = 0;
								for (int i = 0; i < m_GradientColors.Count; i++) {
									if (m_GradientColors[i].percentage < percentage) closestIndex = i;
									else break;
								}
								LW_ColorStop start = m_GradientColors[closestIndex];
								LW_ColorStop end = closestIndex < m_GradientColors.Count - 1 ? m_GradientColors[closestIndex + 1] : m_GradientColors[closestIndex];
								if (start.percentage != end.percentage) {
									float t = (percentage - start.percentage) / (end.percentage - start.percentage);
									color = Color.Lerp(start.color, end.color, t);
								}
								else {
									color = start.color;
								}
							}
						}
					}
					else color = s_White;
					break;
			}
			color.a *= m_Opacity;
			return color;
		}

		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Stroke";
			base.OnEnable();
		}

		#if UNITY_EDITOR
		protected override void OnValidate() {
			m_MiterLimit = Mathf.Max(m_MiterLimit, 1f);
			//if (m_VariableWidths == null) m_VariableWidths = new List<LW_WidthStop>(){new LW_WidthStop(1f, 0f)};
			if (m_VariableWidths != null && m_VariableWidths.Count > 0) {
				float min = 0;
				float max = 1;
				for (int i = 0; i < m_VariableWidths.Count; i++) {
					max = i < m_VariableWidths.Count - 1 ? m_VariableWidths[i + 1].percentage : 1;
					LW_WidthStop stop = m_VariableWidths[i];
					stop.percentage = Mathf.Clamp(stop.percentage, min, max);
					m_VariableWidths[i] = stop;
					min = stop.percentage;
				}
				/*
				m_VariableWidths.Sort(delegate(LW_WidthStop x, LW_WidthStop y){ 
					if (x.percentage - y.percentage > 0) return 1;
					else if (x.percentage - y.percentage < 0) return -1;
					else return 0;
				});
				*/
				LW_WidthStop firstStop = m_VariableWidths[0];
				firstStop.percentage = 0;
				m_VariableWidths[0] = firstStop;
				if (m_VariableWidths.Count > 1) {
					LW_WidthStop lastStop = m_VariableWidths[m_VariableWidths.Count - 1];
					lastStop.percentage = 1;
					m_VariableWidths[m_VariableWidths.Count - 1] = lastStop;
				}
			}
			base.OnValidate();
		}
		#endif
	}
}
