// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace LineWorks {

	public enum PlacementMode { SpaceEvenly, AtFixedLengths, AtEveryPoint }

	/// <summary>
	/// The marker element defines the graphics or GameObjects that can be used for drawing arrowheads or polymarkers on an associated shape.
	/// </summary>
	public class LW_Marker : LW_Style {

		public override bool isVisible {
			get {
				return base.isVisible && graphic != null && (m_AtStart || m_AtMiddle || m_AtEnd);
			}
			set {
				base.isVisible = value;
			}
		}

		// Marker Reference

		/// <summary>
		/// The object that will be used as the marker.
		/// </summary>
		/// <remarks>
		/// Not all Objects will work as a Marker. Only LW_Graphic or a Prefab GameObject are acceptable options.
		/// </remarks>
		public LW_Graphic graphic {
			get {
				return m_Graphic;
			}
			set {
				if (m_Graphic != value) {
					UnregisterChildren();
					m_Graphic = value;
					RegisterChildren();
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected LW_Graphic m_Graphic = null;

		// Transformation

		/// <summary>
		/// Gets or sets the matrix.
		/// </summary>
		/// <value>The matrix.</value>
		public Matrix4x4 transform {
			get {
				return Matrix4x4.TRS(position, rotation, scale);
			}
			set {
				position = value.ExtractTranslationFromMatrix();
				rotation = value.ExtractRotationFromMatrix();
				scale = value.ExtractScaleFromMatrix();
			}
		}

		/// <summary>
		/// Gets or sets the position.
		/// </summary>
		/// <value>The position.</value>
		public Vector3 position {
			get {
				return m_Position;
			}
			set {
				if (m_Position != value) {
					m_Position = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_Position = Vector3.zero;

		/// <summary>
		/// Gets or sets the Quaterion rotation.
		/// </summary>
		/// <value>The rotation.</value>
		public Quaternion rotation {
			get {
				return Quaternion.Euler(eulerRotation);
			}
			set {
				eulerRotation = value.eulerAngles;
			}
		}

		/// <summary>
		/// Gets or sets the euler rotation.
		/// </summary>
		/// <value>The euler rotation.</value>
		public Vector3 eulerRotation {
			get {
				return m_EulerRotation;
			}
			set {
				if (m_EulerRotation != value) {
					m_EulerRotation = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_EulerRotation = Vector3.zero;

		/// <summary>
		/// Gets or sets the scale.
		/// </summary>
		/// <value>The scale.</value>
		public Vector3 scale {
			get {
				return m_Scale;
			}
			set {
				if (m_Scale != value) {
					m_Scale = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Vector3 m_Scale = Vector3.one;

		/// <summary>
		/// Whether the markers should be rotated to be tangent to the associated shapes path.
		/// </summary>
		public bool faceForward {
			get {
				return m_FaceForward;
			}
			set {
				if (m_FaceForward != value) {
					m_FaceForward = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_FaceForward = false;


		/// <summary>
		/// Arbitrary rotation of markers.
		/// </summary>
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

		// Variable Scaling

		public bool scaleWithStroke {
			get {
				return m_ScaleWithStroke;
			}
			set {
				if (m_ScaleWithStroke != value) {
					m_ScaleWithStroke = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_ScaleWithStroke = false;

		public LW_Stroke stroke {
			get {
				if (m_ScaleWithStroke && m_Stroke == null && m_OnElementDirtyCallback != null) {
					LW_Styles styles = m_OnElementDirtyCallback.Target as LW_Styles;
					if (styles != null) {
						for (int i=0; i<styles.Count; i++) {
							if (styles[i] is LW_Stroke) m_Stroke = styles[i] as LW_Stroke;
						}
					}
				}
				return m_Stroke;
			}
			set {
				if (m_Stroke != value) {
					m_Stroke = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected LW_Stroke m_Stroke = null;

		/// <summary>
		/// A list of Width Stops to control the width of a line along the length of the line.
		/// </summary>
		/// <remarks>
		/// Each LW_WidthStop consists of a float for width and a float for percentage (0 to 1). width controls the thickness of the line and percentage controls where along the length of the line to apply this width.  Inbetween two width stops, values are linearly interpolated. 
		/// </remarks>
		public List<LW_WidthStop> variableScales {
			get {
				return m_VariableScales;
			}
			set {
				//if (m_VariableScales != value) {
					m_VariableScales = value;
					SetElementDirty();
				//}
			}
		}
		[SerializeField] protected List<LW_WidthStop> m_VariableScales = new List<LW_WidthStop>() { new LW_WidthStop(1f, 0f) };

		// Start

		/// <summary>
		/// Should the marker be places at the beginning of the associated shapes.
		/// </summary>
		public bool atStart {
			get {
				return m_AtStart;
			}
			set {
				if (m_AtStart != value) {
					m_AtStart = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_AtStart = false;

		/// <summary>
		/// Flips the marker at the Start.
		/// </summary>
		/// <remarks>
		/// Useful for arrowheads.
		/// </remarks>
		public bool flipStart {
			get {
				return m_FlipStart;
			}
			set {
				if (m_FlipStart != value) {
					m_FlipStart = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_FlipStart = false;

		// End

		/// <summary>
		/// Should the marker be places at the end of the associated shapes.
		/// </summary>
		public bool atEnd {
			get {
				return m_AtEnd;
			}
			set {
				if (m_AtEnd != value) {
					m_AtEnd = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_AtEnd = false;

		/// <summary>
		/// Flips the marker at the end.
		/// </summary>
		/// <remarks>
		/// Useful for arrowheads.
		/// </remarks>
		public bool flipEnd {
			get {
				return m_FlipEnd;
			}
			set {
				if (m_FlipEnd != value) {
					m_FlipEnd = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_FlipEnd = false;

		// Middle

		/// <summary>
		/// Should the marker be places in the middle of the associated shapes.
		/// </summary>
		/// <remarks>
		/// See spaceEvenly to set how the markers are positioned.</remarks>
		public bool atMiddle {
			get {
				return m_AtMiddle;
			}
			set {
				if (m_AtMiddle != value) {
					m_AtMiddle = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected bool m_AtMiddle = false;

		/// <summary>
		/// Gets or sets the placement mode for markers along the length of a shape.
		/// </summary>
		/// <value>The placement mode.</value>
		public PlacementMode placementMode {
			get {
				return m_PlacementMode;
			}
			set {
				if (m_PlacementMode != value) {
					m_PlacementMode = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected PlacementMode m_PlacementMode = PlacementMode.SpaceEvenly;

		/// <summary>
		/// Gets or sets the length of the fixed spacing.
		/// </summary>
		/// <remarks>
		/// This only has an effect if the placementMode is set to "AtFixedLengths"
		/// </remarks>
		/// <value>The length of the fixed spacing.</value>
		public float fixedSpacingLength {
			get {
				return m_FixedSpacingLength;
			}
			set {
				if (m_PlacementMode != PlacementMode.AtFixedLengths) {
					Debug.Log("fixedSpacingLength has no effect when placementMode is NOT set to \"AtFixedLengths\"");
				}
				if (m_FixedSpacingLength != value) {
					m_FixedSpacingLength = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected float m_FixedSpacingLength = 5f;

		public Justification fixedJustification {
			get {
				return m_FixedJustification;
			}
			set {
				if (m_PlacementMode == PlacementMode.AtEveryPoint) {
					Debug.Log("fixedJustification has no effect when placementMode is set to \"AtEveryPoint\"");
				}
				if (m_FixedJustification != value) {
					m_FixedJustification = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected Justification m_FixedJustification = Justification.center;

		/// <summary>
		/// Gets or sets the number of markers.
		/// </summary>
		/// <remarks>
		/// This only has an effect if the placementMode is set to "AtFixedLengths" or "SpaceEvenly"
		/// </remarks>
		/// <value>The number of markers.</value>
		public int numberOfMarkers {
			get {
				return m_NumberOfMarkers;
			}
			set {
				if (m_NumberOfMarkers != value) {
					m_NumberOfMarkers = value;
					SetElementDirty();
				}
			}
		}
		[SerializeField] protected int m_NumberOfMarkers = 3;


		/// <summary>
		/// Set all properties of the marker in one call.
		/// </summary>
		/// <param name="marker"></param>
		/// <param name="atStart"></param>
		/// <param name="atMiddle"></param>
		/// <param name="atEnd"></param>
		/// <param name="faceForward"></param>
		/// <param name="angle"></param>
		/// <param name="spaceEvenly"></param>
		/// <param name="numberOfMarker"></param>
		public void Set(LW_Graphic graphic = null, bool atStart = false, bool atMiddle = false, bool atEnd = false, PlacementMode placement = PlacementMode.SpaceEvenly, int numberOfMarkers = 3) {
			this.graphic = graphic;
			this.atStart = atStart;
			this.atMiddle = atMiddle;
			this.atEnd = atEnd;
			this.placementMode = placement;
			SetElementDirty();
		}
		public void Set(LW_Graphic graphic = null, bool atStart = false, bool atMiddle = false, bool atEnd = false, float fixedSpacingLength = 5f, Justification fixedJustification = Justification.center) {
			this.graphic = graphic;
			this.atStart = atStart;
			this.atMiddle = atMiddle;
			this.atEnd = atEnd;
			this.placementMode = PlacementMode.AtFixedLengths;
			this.fixedSpacingLength = fixedSpacingLength;
			this.fixedJustification = fixedJustification;
			SetElementDirty();
		}
		public void Set(LW_Graphic graphic = null, bool atStart = false, bool atMiddle = false, bool atEnd = false, int numberOfMarkers = 3) {
			this.graphic = graphic;
			this.atStart = atStart;
			this.atMiddle = atMiddle;
			this.atEnd = atEnd;
			this.placementMode = PlacementMode.SpaceEvenly;
			this.numberOfMarkers = numberOfMarkers;
			SetElementDirty();
		}

		public static LW_Marker Create(LW_Graphic graphic = null, bool atStart = false, bool atMiddle = false, bool atEnd = false, PlacementMode placement = PlacementMode.SpaceEvenly, int numberOfMarkers = 3) {
			LW_Marker instance = CreateInstance<LW_Marker>();
			instance.Set(graphic, atStart, atMiddle, atEnd, placement, numberOfMarkers);
			return instance;
		}

		public override void SetElementDirty() {
			if (m_ScaleWithStroke && m_Stroke == null && m_OnElementDirtyCallback != null) {
				LW_Styles styles = m_OnElementDirtyCallback.Target as LW_Styles;
				if (styles != null) {
					for (int i=0; i<styles.Count; i++) {
						if (styles[i] is LW_Stroke) m_Stroke = styles[i] as LW_Stroke;
					}
				}
			}
			base.SetElementDirty();
		}

		/// <summary>
		/// Copy marker.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public override LW_Element Copy(LW_Element element = null) {
			if (element != null && element is LW_Marker) {
				element.CopyPropertiesFrom(this);
			}
			else {
				element = Copy(CreateInstance<LW_Marker>());
				//element.UnregisterChildren();
				//element.RegisterChildren();
			}
			return element;
		}
			
		internal override void Reset() {
			base.Reset();
			m_Graphic = null;
			m_Position = Vector3.zero;
			m_EulerRotation = Vector3.zero;
			m_Scale = Vector3.one;
			m_AtStart = false;
			m_AtMiddle = false;
			m_AtEnd = false;
			m_FlipEnd = false;
			m_FaceForward = false;
			m_PlacementMode = PlacementMode.SpaceEvenly;
			m_FixedSpacingLength = 5f;
			m_FixedJustification = Justification.center;
			m_NumberOfMarkers = 3;
		}
		internal override void SetClean() {
			base.SetClean();
			if (m_Graphic != null) m_Graphic.SetClean();
		}
		internal override void CopyPropertiesFrom(LW_Element element) {
			LW_Marker marker = element as LW_Marker;
			if (marker != null) {
				m_Graphic = marker.m_Graphic;
				m_Position = marker.m_Position;
				m_EulerRotation = marker.m_EulerRotation;
				m_Scale = marker.m_Scale;
				m_ScaleWithStroke = marker.m_ScaleWithStroke;
				m_Stroke = marker.m_Stroke;
				m_VariableScales = marker.m_VariableScales;
				m_AtStart = marker.m_AtStart;
				m_AtMiddle = marker.m_AtMiddle;
				m_AtEnd = marker.m_AtEnd;
				m_FlipEnd = marker.m_FlipEnd;
				m_FaceForward = marker.m_FaceForward;
				m_PlacementMode = marker.m_PlacementMode;
				m_FixedSpacingLength = marker.m_FixedSpacingLength;
				m_FixedJustification = marker.m_FixedJustification;
				m_NumberOfMarkers = marker.m_NumberOfMarkers;
			}
			base.CopyPropertiesFrom(element);
		}

		internal override void RegisterChildren() {
			base.RegisterChildren();
			if (m_Graphic != null) m_Graphic.RegisterCallbacks(OnChildSetElementDirty);
		}
		internal override void UnregisterChildren() {
			base.UnregisterChildren();
			if (m_Graphic != null) m_Graphic.UnregisterCallbacks(OnChildSetElementDirty);
		}

		internal float MaxScale() {
			float scale = 1;
			if (m_ScaleWithStroke) {
				if (m_Stroke != null) scale = m_Stroke.MaxWidth();
			}
			else if (m_VariableScales != null && m_VariableScales.Count > 0) {
				if (m_VariableScales.Count == 1) {
					scale = m_VariableScales[0].width;
				}
				else {
					for (int i = 0; i < m_VariableScales.Count; i++) {
						if (m_VariableScales[i].width > scale) scale = m_VariableScales[i].width;
					}
				}
			}
			else scale = 1;
			return scale;
		}
		internal float ScaleAtPercentage(float percentage) {
			float scale = 1;
			if (m_ScaleWithStroke) {
				if (m_Stroke != null) scale = m_Stroke.WidthAtPercentage(percentage);
			}
			else if (m_VariableScales != null && m_VariableScales.Count > 0) {
				if (m_VariableScales.Count == 1) {
					scale = m_VariableScales[0].width;
				}
				else {
					int closestIndex = 0;
					for (int i = 0; i < m_VariableScales.Count; i++) {
						if (m_VariableScales[i].percentage < percentage) closestIndex = i;
						else break;
					}
					LW_WidthStop start = m_VariableScales[closestIndex];
					LW_WidthStop end = closestIndex < m_VariableScales.Count - 1 ? m_VariableScales[closestIndex + 1] : m_VariableScales[closestIndex];
					if (start.percentage != end.percentage) {
						float t = (percentage - start.percentage) / (end.percentage - start.percentage);
						scale = Mathf.Lerp(start.width, end.width, t);
					}
					else {
						scale = start.width;
					}
				}
			}
			return scale;
		}

		protected override void OnEnable() {
			if (string.IsNullOrEmpty(name)) name = "Marker";
			base.OnEnable();
		}
	}
}
