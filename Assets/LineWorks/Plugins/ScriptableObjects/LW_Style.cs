// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace LineWorks {

	public abstract class LW_Style : LW_Appearance {

		#if UNITY_EDITOR
		public bool m_GeneralStyleExpanded = false;
		public bool m_SpecificStyleExpanded = false;
		#endif

		public bool isGeometryDirty {
			get {
				return m_GeometryDirty;
			}
			set {
				m_GeometryDirty = value;
			}
		}
		protected bool m_GeometryDirty = false;

		// LOD Properties

		/// <summary>
		/// Gets or sets the segmentation multiplier.
		/// </summary>
		/// <value>The segmentation multiplier.</value>
		public float segmentationMultiplier {
			get {
				return m_SegmentationMultiplier;
			}
			set {
				if (m_SegmentationMultiplier != value) {
					m_GeometryDirty = true;
					m_SegmentationMultiplier = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_SegmentationMultiplier = 1;

		/// <summary>
		/// Gets or sets the simplification multipiler.
		/// </summary>
		/// <value>The simplification multipiler.</value>
		public float simplificationMultipiler {
			get {
				return m_SimplificationMultiplier;
			}
			set {
				if (m_SimplificationMultiplier != value) {
					m_GeometryDirty = true;
					m_SimplificationMultiplier = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_SimplificationMultiplier = 1;

		// Offset Properties

		/// <summary>
		/// Gets or sets the lateral offset.
		/// </summary>
		/// <value>The lateral offset.</value>
		public float lateralOffset {
			get {
				return m_LateralOffset;
			}
			set {
				if (m_LateralOffset != value) {
					m_GeometryDirty = true;
					m_LateralOffset = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_LateralOffset = 0;

		/// <summary>
		/// Gets or sets the vertical offset.
		/// </summary>
		/// <value>The vertical offset.</value>
		public float verticalOffset {
			get {
				return m_VerticalOffset;
			}
			set {
				if (m_VerticalOffset != value) {
					m_GeometryDirty = true;
					m_VerticalOffset = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_VerticalOffset = 0;

		// Presize

		/// <summary>
		/// Gets or sets the initial capacity of the internal buffers.
		/// </summary>
		/// <remarks>
		/// If you plan to change the graphic elemnts at runtime, use this to presize the buffer and avoid some memory allocations.
		/// </remarks>
		/// <value>The value to use as the Capacity on internal buffer lists.</value>
		public int presizeVBO {
			get {
				return m_PresizeVBO;
			}
			set {
				if (m_PresizeVBO != value) {
					m_GeometryDirty = true;
					m_PresizeVBO = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected int m_PresizeVBO = 0;

		internal override void SetClean() {
			m_GeometryDirty = false;
			base.SetClean();
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Style style = element as LW_Style;
			if (style != null) {
				m_SegmentationMultiplier = style.m_SegmentationMultiplier;
				m_SimplificationMultiplier = style.m_SimplificationMultiplier;
				m_LateralOffset = style.m_LateralOffset;
				m_VerticalOffset = style.m_VerticalOffset;
			}
			base.CopyPropertiesFrom(element);
		}
		internal virtual void Reset() {
			m_SegmentationMultiplier = 1;
			m_SimplificationMultiplier = 1;
			m_LateralOffset = 0;
			m_VerticalOffset = 0;
		}
	}

	public enum PaintMode { Solid, LinearGradient, RadialGradient }
	public enum GradientUnits { userSpaceOnUse, objectBoundingBox }
	public enum SpreadMethod { pad, repeat, reflect };
	public enum UvMode { Scaled, Stretched } 

	public abstract class LW_PaintStyle : LW_Style {

		#if UNITY_EDITOR
		public  bool m_PaintExpanded = false;
		#endif

		public static readonly Color s_White = Color.white;

		public override bool isVisible {
			get {
				return base.isVisible && m_GradientColors != null && m_GradientColors.Count > 0 && ((m_PaintMode == PaintMode.Solid && m_GradientColors[0].color.a > 0) || (m_PaintMode != PaintMode.Solid && (m_GradientColors.Count > 1 || m_GradientColors[0].color.a > 0)));
			}
			set {
				base.isVisible = value;
			}
		}

		// Material Properties

		/// <summary>
		/// Gets or sets the local custom Material.
		/// </summary>
		/// <remarks>
		/// Only applies if overrideMaterial is set to true. If no material has been set, LineWorks will automatically 
		/// generate the necessary Materials to render the vector Graphics. The auto-generated Materials will use use 
		/// the shader specified by blendMode. If set this Material overrides the auto-generated Materials created by LineWorks. 
		/// </remarks>
		/// <value>The material.</value>
		public Material material {
			get {
				return m_Material;
			}
			set {
				if (m_Material != value) {
					m_Material = value;
				}
			}
		}
		[SerializeField] protected Material m_Material = null;

		/// <summary>
		/// Gets or sets the local texture.
		/// </summary>
		/// <remarks>
		/// Only applies if overrideMaterial is set to true. A texture is not necessary to render attached graphics.
		/// </remarks>
		/// <value>The main texture.</value>
		public Texture2D mainTexture {
			get {
				return m_MainTexture;
			}
			set {
				if (m_Material != null) {
					Debug.LogWarning("mainTexture can not be set because a custom material is set.");
				}
				else if (m_MainTexture != value) {
					m_MainTexture = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Texture2D m_MainTexture;

		// Paint Properties

		/// <summary>
		/// Gets or sets the method used to paint a style.
		/// </summary>
		/// <value>
		/// -	`Solid` - Solid color.
		/// -	`Linear Gradient` - A gradient that changes along a line drawn across a shape.
		/// -	`Radial Gradient` - A gradient that radiates out from a point.
		/// </value>
		public PaintMode paintMode {
			get {
				return m_PaintMode;
			}
			set {
				if (m_PaintMode != value) {
					m_PaintMode = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected PaintMode m_PaintMode = PaintMode.Solid;

		/// <summary>
		/// Gets or sets how texture UV's are calculated.
		/// </summary>
		/// <remarks>
		/// If set to true, UV's are correctly scaled to maintain a 1:1 aspect ratio. However, when true, seams between segments may be visible. If set to false, UV's are stretched to fit. This means there are no seams but unwanted skewing and stretching may appear.
		/// </remarks>
		/// <value>The uv mode.</value>
		public UvMode uvMode {
			get {
				return m_UvMode;
			}
			set {
				if (m_UvMode != value) {
					m_UvMode = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected UvMode m_UvMode = UvMode.Scaled;

		/// <summary>
		/// Gets or sets the mainTexture's UV tiling.
		/// </summary>
		/// <value>The uv tiling.</value>
		public Vector2 uvTiling {
			get {
				return m_UvTiling;
			}
			set {
				if (m_UvTiling != value) {
					m_UvTiling = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector2 m_UvTiling = Vector2.one;

		/// <summary>
		/// Gets or sets the mainTexture's UV offset.
		/// </summary>
		/// <value>The uv offset.</value>
		public Vector2 uvOffset {
			get {
				return m_UvOffset;
			}
			set {
				if (m_UvOffset != value) {
					m_UvOffset = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector2 m_UvOffset = Vector2.zero;

		// Gradient Properties

		/// <summary>
		/// Gets or sets the vertex color.
		/// </summary>
		/// <value>The color.</value>
		public Color color {
			get {
				if (m_GradientColors == null || m_GradientColors.Count == 0) m_GradientColors = new List<LW_ColorStop>() { new LW_ColorStop(Color.white, 0f), new LW_ColorStop(Color.black, 1f) };
				return m_GradientColors[0].color;
			}
			set {
				if (m_GradientColors == null || m_GradientColors.Count == 0) m_GradientColors = new List<LW_ColorStop>() { new LW_ColorStop(Color.white, 0f), new LW_ColorStop(Color.black, 1f) };
				if (m_GradientColors[0].color != value) {
					m_GradientColors[0] = new LW_ColorStop(value, 0);
					SetElementDirty();
				}
			}
		}
		/// <summary>
		/// Gets or sets a list of Color Stops to control the color of a line along the length of the line.
		/// </summary>
		/// <remarks>
		/// Each LW_ColorStop consists of a Color for color and a float for percentage (0 to 1). color controls the color of the line and percentage controls where along the length of the line to apply this width.  Inbetween two color stops, values are linearly interpolated. 
		/// </remarks>
		public List<LW_ColorStop> gradientColors {
			get {
				if (m_GradientColors == null || m_GradientColors.Count == 0) m_GradientColors = new List<LW_ColorStop>() { new LW_ColorStop(Color.white, 0f), new LW_ColorStop(Color.black, 1f) };
				//return m_GradientColors.Clone() as List<LW_ColorStop>;
				return m_GradientColors;
			}
			set {
				//if (m_GradientColors != value) {
					m_GradientColors = value;
					SetElementDirty();
				//}
			}
		}
		[SerializeField] protected List<LW_ColorStop> m_GradientColors = new List<LW_ColorStop>() { new LW_ColorStop(Color.white, 0f), new LW_ColorStop(Color.black, 1f) };

		/// <summary>
		/// Gets or sets the transformation matrix used to position the color gradient.
		/// </summary>
		public Matrix4x4 gradientTransform {
			get {
				return Matrix4x4.TRS(gradientPosition, gradientRotation, gradientScale);
			}
			set {
				gradientPosition = value.ExtractTranslationFromMatrix();
				gradientRotation = value.ExtractRotationFromMatrix();
				gradientScale = value.ExtractScaleFromMatrix();
			}
		}

		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>The position.</value>
		public Vector3 gradientPosition {
			get {
				return m_GradientPosition;
			}
			set {
				if (m_GradientPosition != value) {
					m_GradientPosition = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_GradientPosition = Vector3.zero;

		/// <summary>
		/// Gets or sets the rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public Quaternion gradientRotation {
			get {
				return Quaternion.Euler(m_GradientRotation);
			}
			set {
				gradientEulerRotation = value.eulerAngles;
			}
		}

		/// <summary>
		/// Gets or sets the euler rotation.
		/// </summary>
		/// <value>The euler rotation.</value>
		public Vector3 gradientEulerRotation {
			get {
				return m_GradientRotation;
			}
			set {
				if (m_GradientRotation != value) {
					m_GradientRotation = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_GradientRotation = Vector3.zero;

		/// <summary>
		/// Gets or sets the scale.
		/// </summary>
		/// <value>The scale.</value>
		public Vector3 gradientScale {
			get {
				return m_GradientScale;
			}
			set {
				if (m_GradientScale != value) {
					m_GradientScale = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_GradientScale = Vector3.one;

		/// <summary>
		/// This is currently not implemented
		/// </summary>
		public SpreadMethod gradientSpreadMethod {
			get {
				return m_GradientSpreadMethod;
			}
			set {
				if (m_GradientSpreadMethod != value) {
					m_GradientSpreadMethod = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected SpreadMethod m_GradientSpreadMethod = SpreadMethod.pad;
		
		/// <summary>
		/// Gets or sets how a gradient's position is defined.
		/// </summary>
		/// <value>
		/// -	`userSpaceOnUse` - Position is described in the same space as the associated shape.
		/// -	`objectBoundingBox` - Position is described by the associated shape's bounding box. (0,0) in the lower left and (1,1) in the upper right.
		/// </value>
		public GradientUnits gradientUnits {
			get {
				return m_GradientUnits;
			}
			set {
				if (m_GradientUnits != value) {
					m_GradientUnits = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected GradientUnits m_GradientUnits = GradientUnits.objectBoundingBox;
		
		/// <summary>
		/// Gets or sets the start point for a linear gradient or the center point for a radial gradient.
		/// </summary>
		public Vector2 gradientStart {
			get {
				return m_GradientStart;
			}
			set {
				if (m_GradientStart != value) {
					m_GradientStart = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector2 m_GradientStart = new Vector2(0, 0);
		
		/// <summary>
		/// Gets or sets the end point for a linear gradient or a point on the outer ring of a radial gradient.
		/// </summary>
		public Vector2 gradientEnd {
			get {
				return m_GradientEnd;
			}
			set {
				if (m_GradientEnd != value) {
					m_GradientEnd = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector2 m_GradientEnd = new Vector2(1, 0);

		/// <summary>
		/// Gets or sets the global opacity.
		/// </summary>
		/// <value>The opacity.</value>
		public float opacity {
			get {
				return m_Opacity;
			}
			set {
				if (m_Opacity != value) {
					m_Opacity = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_Opacity = 1;

		/// <summary>
		/// Adds a LW_ColorStop to the list of color stops.
		/// </summary>
		/// <param name="stop"></param>
		public void AddStop(LW_ColorStop stop) {
			//Debug.Log("AddingStop: "  + stop.color + " : " + stop.percentage);
			m_GradientColors.Add(stop);
			SetElementDirty();
		}
		
		/// <summary>
		/// Removes the provided LW_ColorStop to the list of color stops.
		/// </summary>
		/// <param name="stop"></param>
		public void RemoveStop(LW_ColorStop stop) {
			List<LW_ColorStop> colors = gradientColors;
			if (colors.Contains(stop)) colors.Remove(stop);
			gradientColors = colors;
			SetElementDirty();
		}
			
		internal override void Reset() {
			base.Reset();
			m_Material = null;
			m_MainTexture = null;
			m_PaintMode = PaintMode.Solid;
			m_UvMode = UvMode.Scaled;
			m_UvTiling = Vector2.one;
			m_UvOffset = Vector2.zero;
			m_GradientColors = new List<LW_ColorStop>() { new LW_ColorStop(Color.white, 0f), new LW_ColorStop(Color.black, 1f) };
			m_GradientPosition = Vector3.zero;
			m_GradientRotation = Vector3.zero;
			m_GradientScale = Vector3.one;
			m_GradientSpreadMethod = SpreadMethod.pad;
			m_GradientUnits = GradientUnits.objectBoundingBox;
			m_GradientStart = new Vector2(0, 0);
			m_GradientEnd = new Vector2(1, 0);
			m_Opacity = 1;
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_PaintStyle style = element as LW_PaintStyle;
			if (style != null) {
				m_Material = style.m_Material;
				m_MainTexture = style.m_MainTexture;
				m_PaintMode = style.m_PaintMode;
				m_UvMode = style.m_UvMode;
				m_UvTiling = style.m_UvTiling;
				m_UvOffset = style.m_UvOffset;
				if (style.m_GradientColors != null) m_GradientColors = new List<LW_ColorStop>(style.m_GradientColors);
				else m_GradientColors = null;
				m_GradientPosition = style.m_GradientPosition;
				m_GradientRotation = style.m_GradientRotation;
				m_GradientScale = style.m_GradientScale;
				m_GradientSpreadMethod = style.m_GradientSpreadMethod;
				m_GradientUnits = style.m_GradientUnits;
				m_GradientStart = style.m_GradientStart;
				m_GradientEnd = style.m_GradientEnd;
				m_Opacity = style.m_Opacity;
			}
			base.CopyPropertiesFrom(element);
		}

		internal Matrix4x4 LocalToBoundsMatrix(Bounds bounds) {
			Matrix4x4 matrix = Matrix4x4.identity;
			matrix.m00 = bounds.extents.x > 0 ? 1f / (bounds.max.x - bounds.min.x) : 1f;
			matrix.m10 = 0;
			matrix.m01 = 0;
			matrix.m11 = bounds.extents.y > 0 ? 1f / (bounds.max.y - bounds.min.y) : 1f;
			matrix.m03 = bounds.extents.x > 0 ? -bounds.min.x / (bounds.max.x - bounds.min.x) : 0f;
			matrix.m13 = bounds.extents.y > 0 ? -bounds.min.y / (bounds.max.y - bounds.min.y) : 0f;
			return matrix;
		}
		internal Matrix4x4 BoundsToLocalMatrix(Bounds bounds) {
			Matrix4x4 matrix = Matrix4x4.identity;
			matrix.m00 = bounds.extents.x > 0 ? bounds.max.x - bounds.min.x : 1f;
			matrix.m10 = 0;
			matrix.m01 = 0;
			matrix.m11 = bounds.extents.y > 0 ? bounds.max.y - bounds.min.y : 1f;
			matrix.m03 = bounds.extents.x > 0 ? bounds.min.x : 0f;
			matrix.m13 = bounds.extents.y > 0 ? bounds.min.y : 0f;
			return matrix;
		}

		internal virtual Color ColorAtPercentage(float percentage) {
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
				else color = s_White;
				break;
			}
			color.a *= m_Opacity;
			return color;
		}

		internal Color ColorAtPosition(Vector2 worldPosition, Vector2 boundsPosition, Bounds bounds) {
			Color color = s_White;

			if (m_PaintMode == PaintMode.Solid) {
				color = ColorAtPercentage(0);
			}
			else {
				Matrix4x4 matrix = m_GradientUnits == GradientUnits.objectBoundingBox ?
					BoundsToLocalMatrix(bounds) * gradientTransform * LocalToBoundsMatrix(bounds) :
					gradientTransform;
				Vector2 start = matrix.MultiplyPoint(gradientStart);
				Vector2 end = matrix.MultiplyPoint(gradientEnd);
				Vector2 dir = end - start;
				float angle = Mathf.Atan2(dir.x, dir.y);
				float scale = dir.magnitude;
				Vector2 coords = m_GradientUnits == GradientUnits.objectBoundingBox ? 
					(boundsPosition - start) / scale :
					(worldPosition - start) / scale;
				float t = Mathf.Clamp(m_PaintMode == PaintMode.LinearGradient ? 
					coords.x * Mathf.Sin(angle) + coords.y * Mathf.Cos(angle) : 
					Mathf.Sqrt(coords.x * coords.x + coords.y * coords.y), 0, 1);
				color = ColorAtPercentage(t);
			}

			return color;
		}

		#if UNITY_EDITOR
		protected override void OnValidate() {
			if (m_GradientColors == null || m_GradientColors.Count == 0) m_GradientColors = new List<LW_ColorStop>() { new LW_ColorStop(Color.white, 0f), new LW_ColorStop(Color.black, 1f) };
			if (m_GradientColors != null && m_GradientColors.Count > 0) {
				float min = 0;
				float max = 1;
				for (int i = 0; i < m_GradientColors.Count; i++) {
					max = i < m_GradientColors.Count - 1 ? m_GradientColors[i + 1].percentage : 1;
					LW_ColorStop stop = m_GradientColors[i];
					stop.percentage = Mathf.Clamp(stop.percentage, min, max);
					m_GradientColors[i] = stop;
					min = stop.percentage;
				}
				/*
				m_GradientColors.Sort(delegate(LW_ColorStop x, LW_ColorStop y){ 
					if (x.percentage - y.percentage > 0) return 1;
					else if (x.percentage - y.percentage < 0) return -1;
					else return 0;
				});
				*/
				LW_ColorStop firstStop = m_GradientColors[0];
				firstStop.percentage = 0;
				m_GradientColors[0] = firstStop;
				if (m_GradientColors.Count > 1) {
					LW_ColorStop lastStop = m_GradientColors[m_GradientColors.Count - 1];
					lastStop.percentage = 1;
					m_GradientColors[m_GradientColors.Count - 1] = lastStop;
				}
			}
			base.OnValidate();
		}
		#endif
	}

	[System.Serializable]
	public struct LW_ColorStop {
		public Color color {
			get {
				return m_Value;
			}
			set {
				if (m_Value != value) {
					m_Value = value;
				}
			}
		}
		[FormerlySerializedAs("m_Color")]
		[SerializeField] private Color m_Value;
		public float percentage {
			get {
				return m_Percentage;
			}
			set {
				if (m_Percentage != value) {
					m_Percentage = Mathf.Clamp(value, 0, 1);
				}
			}
		}
		[SerializeField] private float m_Percentage;

		public LW_ColorStop(Color color, float percentage) {
			m_Value = color;
			m_Percentage = percentage;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) {
			return other is LW_ColorStop && EqualStop((LW_ColorStop)other);
		}
		public static bool operator ==(LW_ColorStop lhs, object rhs) {
			return lhs.Equals(rhs);
		}
		public static bool operator !=(LW_ColorStop lhs, object rhs) {
			return !lhs.Equals(rhs);
		}
		private bool EqualStop(LW_ColorStop other) {
			return (
				percentage == other.percentage &&
				color == other.color
				);
		}
	}

	[System.Serializable]
	public struct LW_WidthStop {
		public float width {
			get {
				return m_Value;
			}
			set {
				if (m_Value != value) {
					m_Value = value;
				}
			}
		}
		[FormerlySerializedAs("m_Width")]
		[SerializeField] private float m_Value;
		public float percentage {
			get {
				return m_Percentage;
			}
			set {
				if (m_Percentage != value) {
					m_Percentage = Mathf.Clamp(value, 0, 1);
				}
			}
		}
		[SerializeField] private float m_Percentage;

		public LW_WidthStop(float width, float percentage) {
			m_Value = width;
			m_Percentage = percentage;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override bool Equals(object other) {
			return other is LW_WidthStop && EqualStop((LW_WidthStop)other);
		}
		public static bool operator ==(LW_WidthStop lhs, object rhs) {
			return lhs.Equals(rhs);
		}
		public static bool operator !=(LW_WidthStop lhs, object rhs) {
			return !lhs.Equals(rhs);
		}
		private bool EqualStop(LW_WidthStop other) {
			return (
				percentage == other.percentage &&
				width == other.width
				);
		}
	}
}