// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LineWorks {

	public enum ScaleMode { NativeSize, ScaleToFit, ScaleToFill, StretchToFill, NineSlice}
	public enum BlendMode { Opaque, AlphaTest, AlphaBlend, UI, AdditiveSoft }
	public enum FeatureMode { Simple, Advanced }
	public enum StrokeDrawMode { Draw2D, Draw3D }
	public enum StrokeScaleMode { Scaled, Unscaled, ScreenSpace }
	public enum JoinsAndCapsMode { Vertex, Shader }
	public enum GradientsMode { Vertex, Shader }
	public enum AntiAliasingMode { Off, On }

	/// <summary>
	/// The LW_Canvas Component is the root for all vector graphics.
	/// </summary>
	/// <remarks>
	/// The component defines the area for %LineWorks graphics, the base style settings, 
	/// and all of the Styles and Shapes to be rendered. LW_Canvas is used both for creating 
	/// UI elements and for creating traditional Meshes. %LineWorks will determine if it is a 
	/// UI element by checking to see if the gameObject is the Child of a UI Canvas.
	/// </remarks>
	public class LW_Canvas : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter {

		// Use these to get console information about what The LW_Canvas is doing.
		#if UNITY_EDITOR || DEVELOPMENT
		private static readonly bool s_DebugCanvas = false;
		private static readonly bool s_DebugComponents = false;
		#endif

		// Used for identification and for checking for duplicates.
		internal int id {
			get {
				if (m_Id == -1) m_Id = GetHashCode();
				return m_Id;
			}
			set {
				if (m_Id != value) {
					m_Id = value;
				}
			}
		}
		[SerializeField] protected int m_Id = -1;

		#if UNITY_EDITOR
		// Editor properties
		public LW_Graphic m_SelectedGraphic = null;
		public LW_Appearance m_SelectedAppearance = null;
		public bool m_CanvasExpanded = false;
		public bool m_RendererExpanded = false;
		public bool m_AppearanceExpanded = false;
		public bool m_LODExpanded = false;
		#endif

		/// <summary>
		/// Gets the current vertex count.
		/// </summary>
		/// <value>int value represents the vertex count.</value>
		public int vertexCount {
			get {
				return m_VertexCount;
			}
		}
		private int m_VertexCount;

		/// <summary>
		/// Gets the current index count.
		/// </summary>
		/// <value>int value represents the index count.</value>
		public int indexCount {
			get {
				return m_IndexCount;
			}
		}
		private int m_IndexCount;

		/// <summary>
		/// Gets the current sub mesh count.
		/// </summary>
		/// <value>int value represents the sub mesh count.</value>
		public int subMeshCount {
			get {
				return m_SubMeshCount;
			}
		}
		private int m_SubMeshCount;

		/// <summary>
		/// Gets the current pixels per unit. This is used to scale the LineWork relative to the Canvas and take 
		/// into account any CanvasScaler Components.
		/// </summary>
		/// <value>float value represents the pixels per unit.</value>
		public float pixelsPerUnit {
			get	{
				float spritePixelsPerUnit = 100;
				float referencePixelsPerUnit = 100;
				if (canvas) referencePixelsPerUnit = canvas.referencePixelsPerUnit;

				return spritePixelsPerUnit / referencePixelsPerUnit;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="LineWorks.LW_Canvas"/> is prefab.
		/// </summary>
		/// <value><c>true</c> if is prefab; otherwise, <c>false</c>.</value>
		public bool isPrefab {
			get {
				#if UNITY_EDITOR
				return this != null && PrefabUtility.GetPrefabType(this) == PrefabType.Prefab;
				#else
				return false;
				#endif
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="LineWorks.LW_Canvas"/> is saved on a Project Asset.
		/// </summary>
		/// <value><c>true</c> if is project asset; otherwise, <c>false</c>.</value>
		public bool isAsset {
			get {
				#if UNITY_EDITOR
				return AssetDatabase.Contains(this);
				#else
				return false;
				#endif
			}
		}

		/// <summary>
		/// Gets a value indicating whether the Graphic will act as a UI element.
		/// </summary>
		/// <remarks>
		/// When the value is <c>true</c>, vertices are sent to the CanvasRenderer and will be batched by the UI Canvas. When <c>false</c>, vertices are sent to a MeshFilter and MeshRenderer.
		/// </remarks>
		public bool isLineWorksShader {
			get {
				return m_IsLineWorksShader;
			}
		}
		[SerializeField] protected bool m_IsLineWorksShader = true;

		/// <summary>
		/// Gets a value indicating whether the Graphic will act as a UI element.
		/// </summary>
		/// <remarks>
		/// When the value is <c>true</c>, vertices are sent to the CanvasRenderer and will be batched by the UI Canvas. When <c>false</c>, vertices are sent to a MeshFilter and MeshRenderer.
		/// </remarks>
		public bool isUIElement {
			get {
				return m_UICanvas != null;
			}
		}
		[NonSerialized] private Canvas m_UICanvas = null;

		/// <summary>
		/// Gets a value indicating whether this <see cref="LineWorks.LW_Canvas"/> has parent LW_Canvas Elements or not.
		/// </summary>
		/// <remarks>
		/// Currently, this has no purpose. In future versions, some internal mesh batching may be possible by allowing a root LW_Canvas to manager it's children.
		/// </remarks>
		/// <value><c>true</c> if is root element; otherwise, <c>false</c>.</value>
		public bool isRootElement {
			get {
				return m_Root != this;
			}
		}
		[NonSerialized] private LW_Canvas m_Root = null;

		/// <summary>
		/// Gets a value indicating whether this <see cref="LineWorks.LW_Canvas"/> is dirty.
		/// </summary>
		/// <value><c>true</c> if is dirty; otherwise, <c>false</c>.</value>
		public bool isDirty {
			get {
				return m_ScalerDirty || m_BuffersDirty || m_VertsDirty || m_MaterialDirty;
			}
		}
		[NonSerialized] private bool m_TransformDirty = true;

		// Renderer Properties

		/// <summary>
		/// A convenience property that gets or sets the sorting order on the MeshRenderer.
		/// </summary>
		/// <remarks>
		/// Only effective if isUIElement is <c>false</c>.
		/// </remarks>
		/// <value>The int value represents the sorting order.</value>
		public int sortingOrder {
			get {
				return m_SortingOrder;
			}
			set {
				if (m_SortingOrder != value) {
					m_SortingOrder = value;
					if (!isUIElement && m_MeshRenderer != null) m_MeshRenderer.sortingOrder = m_SortingOrder;
				}
			}
		}
		[SerializeField] private int m_SortingOrder = 0;

		/// <summary>
		/// A convenience property that gets or sets the sorting layer on the MeshRenderer.
		/// </summary>
		/// <remarks>
		/// Only effective if isUIElement is <c>false</c>.
		/// </remarks>
		/// <value>The int value represents the sorting layer identifier.</value>
		public int sortingLayerId {
			get {
				return m_SortingLayerID;
			}
			set {
				if (m_SortingLayerID != value) {
					m_SortingLayerID = value;
					if (!isUIElement && m_MeshRenderer != null) m_MeshRenderer.sortingLayerID = m_SortingLayerID;
				}
			}
		}
		[SerializeField] private int m_SortingLayerID = 0;

		// Canvas Properties

		/// <summary>
		/// Gets or sets the Rect area for the display of %LineWorks elements.
		/// </summary>
		/// <remarks>
		/// viewBox is used in conjunction with the scaleMode to define how %LineWorks elements should be located and scaled. When the LW_Canvas is visible in the inspector, the viewBox is shown as a green rectangle in the Scene View.
		/// </remarks>
		/// <value>The Rect value represents the view box.</value>
		public Rect viewBox {
			get {
				return m_ViewBox;
			}
			set {
				if (m_ViewBox != value) {
					SetScalerDirty();
					m_ViewBox = value;
				}
			}
		}
		[SerializeField] protected Rect m_ViewBox = new Rect(0, 0, 100, 100);

		/// <summary>
		/// Gets or sets a scale mode to control how %LineWorks elements will be scaled using the relationship of the viewBox to the RectTransform.
		/// </summary>
		/// <value>
		/// -	<c>NativeSize</c> - No scaling will occur.
		/// -	<c>ScaleToFit</c> - viewBox is scaled to Fit within the RectTransform while maintaining the aspect ratio.
		/// -	<c>ScaleToFill</c> - viewBox is scaled to Fill the RectTransform while maintaining the aspect ratio.
		/// -	<c>StretchToFill</c> - viewBox is scaled to Fill the RectTransform without maintained the aspect ratio.
		/// -	<c>NineSlice</c> - viewBox is scaled to Fit the RectTransform while mantains the corners at a 1:1 scale.
		/// </value>
		public ScaleMode scaleMode {
			get {
				return m_ScaleMode;
			}
			set {
				if (m_ScaleMode != value) {
					SetScalerDirty();
					m_ScaleMode = value;
				}
			}
		}
		[SerializeField] protected ScaleMode m_ScaleMode = ScaleMode.NativeSize;

		/// <summary>
		/// Gets or sets the border for Nine-Sliced scaleMode.
		/// </summary>
		/// <value>The Rect value represents the nine-slice offset borders.</value>
		public Rect border {
			get {
				return m_Border;
			}
			set {
				if (m_Border != value) {
					SetScalerDirty();
					m_Border = value;
				}
			}
		}
		[SerializeField] protected Rect m_Border = new Rect(0, 0, 0, 0);

		/// <summary>
		/// Gets or sets the corners of a free-form deformation.
		/// </summary>
		/// <remarks>
		/// Currently not implemented. Free-form deformation is a potential future feature.
		/// </remarks>
		/// <value>The LW_Point2D array represents the four(4) ffd(free-form deformation) corners.</value>
		public LW_Point2D[] ffdCorners {
			get {
				return m_FFDCorners;
			}
			set {
				if (m_FFDCorners != value) {
					if (value != null && value.Length == 4) {
						SetScalerDirty();
						m_FFDCorners = value;
					}
					else Debug.Log("ffdCorners must be an array of (4) LW_Point2D points");
				}
			}
		}
		[SerializeField] private LW_Point2D[] m_FFDCorners = new LW_Point2D[]{
			new LW_Point2D(Vector2.zero, Vector2.down, Vector2.right),
			new LW_Point2D(Vector2.zero, Vector2.left, Vector2.down),
			new LW_Point2D(Vector2.zero, Vector2.up, Vector2.left),
			new LW_Point2D(Vector2.zero, Vector2.right, Vector2.up)
		};
			
		/// <summary>
		/// Gets the current scaler Matrix.
		/// </summary>
		/// <remarks>
		/// This matrix is used for final positioning of elements relative to the RectTransform size and pivot.
		/// This is calculated using the current RectTransform, viewBox, and scaleMode. (Read Only).
		/// </remarks>
		/// <value>The Matrix4x4 value represents the scale Transformation.</value>
		public Matrix4x4 scaler {
			get {
				return m_Scaler;
			}
		}
		[SerializeField] private Matrix4x4 m_Scaler;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="LineWorks.LW_Canvas"/> should use accurate raycast testing for a hit on a UI Element.
		/// </summary>
		/// <remarks>
		/// When set to true, LineWorks will perform the extra calculations to see if a click was directly on a LineWorks Element. 
		/// Otherwise, clicks will be captured if the click is within the RectTransform.
		/// NOTE: The calculations are relatively cheap for Fills and Simple Strokes, however, Advanced Strokes require additional calculations.
		/// </remarks>
		/// <value><c>true</c> if use accurate raycasting; otherwise, <c>false</c>.</value>
		public bool useAccurateRaycasting {
			get {
				return m_UseAccurateRaycasting;
			}
			set {
				m_UseAccurateRaycasting = value;
			}
		}
		[SerializeField] protected bool m_UseAccurateRaycasting = false;

		// LOD Properties

		/// <summary>
		/// Gets or sets the base value for curve segmentation.
		/// </summary>
		/// <remarks>
		/// This is applied to all attached graphics unless an individual graphic overrides it.
		/// Segmentation controls the number of line segments to be created for each bezier curve.
		/// </remarks>
		/// <value>The int value represents the number of curve segments.</value>
		public int segmentation {
			get {
				return m_Segmentation;
			}
			set {
				if (m_Segmentation != value) {
					m_ForceRebuild = true;
					SetAllDirty();
					m_Segmentation = value;
				}
			}
		}
		[SerializeField] protected int m_Segmentation = 8;

		/// <summary>
		/// Gets or sets a value indicating whether the points of this <see cref="LineWorks.LW_Canvas"/> are optimized.
		/// </summary>
		/// <value><c>true</c> if an additional optimization step should be run; otherwise, <c>false</c>.</value>
		public bool optimize {
			get {
				return m_Optimize;
			}
			set {
				if (m_Optimize != value) {
					m_ForceRebuild = true;
					SetAllDirty();
					m_Optimize = value;
					if (!m_Optimize) m_Simplification = 0;
				}
			}
		}
		[SerializeField] protected bool m_Optimize = false;

		/// <summary>
		/// Gets or sets the base value for simplification.
		/// </summary>
		/// <remarks>
		/// This is applied to all attached graphics unless an individual graphic overrides it.
		/// When simplification is set to any value greater than 0, and/or optimize is set to true, an algorithm is 
		/// run to remove points from the geometry that fails to meet the threshold. 
		/// > The simplification Algorithm that %LineWorks uses is a method known as the Visvalingam and Whyatt 
		/// > (VW) algorithm. Basically it takes a collection of points, calculates the area of the triangle 
		/// > each point makes with it's neighbor points and then recursively removes points who's triangular area 
		/// > is below the set threshold.  Running this algorithm comes with a performance cost and is probably not 
		/// > best suited for used in a fully dynamic senario.  That said, if you're vector graphics do not change 
		/// > (built once and never rebuilt), applying a simplification value is a great way to reduce the total 
		/// > number of vertices without a significant loss in quality.
		/// </remarks>
		/// <value>The float value represents the minimum area threshold for the simplification algorithm.</value>
		public float simplification {
			get {
				return m_Simplification;
			}
			set {
				if (m_Simplification != value) {
					m_ForceRebuild = true;
					SetAllDirty();
					m_Simplification = value;
					if (m_Simplification > 0) m_Optimize = true;
				}
			}
		}
		[SerializeField] protected float m_Simplification = 0;

		// Material Properties

		/// <summary>
		/// Gets or sets the base vertex color.
		/// </summary>
		/// <remarks>
		/// This is applied to all attached graphics unless an individual graphic overrides it.
		/// Best used for tinting all graphic elements.
		/// </remarks>
		/// <value>The Color value represents the vertex color that is applied/multiplied to all vertices.</value>
		public new Color color {
			get {
				return base.color;
			}
			set {
				base.color = value;
			}
		}

		/// <summary>
		/// Gets or sets the base custom Material.
		/// </summary>
		/// <remarks>
		/// This is applied to all attached graphics unless an individual graphic overrides it.
		/// This is usually left blank and %LineWorks will automatically create an appropriate Material. The auto-generated Materials 
		/// will use use the shader specified by mainTexture blendMode, featureMode, strokeDrawMode, 
		/// joinsAndCapsMode, gradientsMode, and antiAliasingMode. 
		/// If set this Material overrides the auto-generated Materials created by %LineWorks. 
		/// </remarks>
		/// <value>the Material value represents the custom material.</value>
		public override Material material {
			get {
				return m_Material;
				//return m_Material != null ? m_Material : defaultMaterial;
			}
			set {
				if (m_Material != value) {
					m_Material = value;
					SetMaterialDirty(); 
					if (m_Material != null) UpdatePropertiesToMatchMaterial(m_Material);
				}
			}
		}
		//[SerializeField] protected Material m_Material = null;

		/// <summary>
		/// Gets or sets the Material texture.
		/// </summary>
		/// <remarks>
		/// This is applied to all attached graphics unless a custom Material overrides it.
		/// A texture is not necessary to render attached graphics.
		/// </remarks>
		/// <value>The Texture2D value represents the main texture.</value>
		public new Texture2D mainTexture {
			get {
				//if (m_Material != null) m_MainTexture = m_Material.mainTexture as Texture2D;
				return m_MainTexture;
			}
			set {					
				if (m_MainTexture != value) {
					m_MainTexture = value;
					SetMaterialDirty();
					if (m_Material != null) m_Material.mainTexture = m_MainTexture;
				}
			}
		}
		[SerializeField] protected Texture2D m_MainTexture = null;

		/// <summary>
		/// Gets or sets the mainTexture's UV tiling.
		/// </summary>
		/// <value>The uv tiling.</value>
		public Vector2 uvTiling {
			get {
				//if (m_Material != null) m_UvTiling = m_Material.mainTextureScale;
				return m_UvTiling;
			}
			set {
				if (m_UvTiling != value) {
					m_UvTiling = value;
					SetMaterialDirty();
					if (m_Material != null) m_Material.mainTextureScale = m_UvTiling;

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
				//if (m_Material != null) m_UvOffset = m_Material.mainTextureOffset;
				return m_UvOffset;
			}
			set {
				if (m_UvOffset != value) {
					m_UvOffset = value;
					SetMaterialDirty();
					if (m_Material != null) m_Material.mainTextureOffset = m_UvOffset;
				}
			}
		}
		[SerializeField] protected Vector2 m_UvOffset = Vector2.zero;

		/// <summary>
		/// Gets or sets the base shader blend mode.
		/// </summary>
		/// <remarks>
		/// This is applied to all attached graphics unless an individual graphic overrides it.
		/// The shader mode controls the blend mode for the %LineWorks shader. Use the simplest mode for best performance.
		/// </remarks>
		/// <value>
		/// -	<c>Opaque</c> - Uses shader with “RenderType = Opaque” and no Blending.
		/// -	<c>AlphaTest</c> - Uses shader with “RenderType = TransparentCutout” and no Blending.
		/// -	<c>AlphaBlend</c> - Uses shader with “RenderType = Transparent” and “Blend SrcAlpha OneMinusSrcAlpha”.
		/// -	<c>UI</c> - Uses shader with “RenderType = Transparent”, “Blend SrcAlpha OneMinusSrcAlpha”, and and all the 
		/// 	other stuff to render UI elements correctly like Stencils and ZTests.
		/// -	<c>AdditiveSoft</c> - Uses shader with “RenderType = Transparent” and “Blend One OneMinusSrcColor”.
		/// </value>
		public BlendMode blendMode {
			get {
				//if (m_Material != null && m_IsLineWorksShader) m_BlendMode = (BlendMode)m_Material.GetFloat("_BlendMode");
				return m_BlendMode;
			}
			set {
				if (m_BlendMode != value) {
					m_BlendMode = value;
					m_ForceRebuild = true;
					SetAllDirty();
					if (m_Material != null && m_IsLineWorksShader) {
						m_Material.SetFloat("_BlendMode", (int)m_BlendMode);
						LW_MaterialPool.SetMaterialKeywords(m_Material);
					}
				}
			}
		}
		[SerializeField] protected BlendMode m_BlendMode = BlendMode.AlphaBlend;

		/// <summary>
		/// Gets or sets the shader feature mode.
		/// </summary>
		/// <remarks>
		/// This is applied to all attached graphics unless a custom Material overrides it.
		/// the feature mode controls whether advance %LineWorks shader features should be used.
		/// </remarks>
		/// <value>
		/// -	<c>Simple</c> - Disables all %LineWorks Shader features.
		/// -	<c>Advanced</c> - Enables all %LineWorks Shader features. 
		/// </value>
		public FeatureMode featureMode {
			get {
				if (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat)) return FeatureMode.Simple;
				else {
					//if (m_Material != null && m_IsLineWorksShader) m_FeatureMode = (FeatureMode)m_Material.GetFloat("_FeatureMode");
					return m_FeatureMode;
				}
			}
			set {
				if (m_FeatureMode != value) {
					m_FeatureMode = value;
					m_ForceRebuild = true;
					SetAllDirty();
					if (m_Material != null && m_IsLineWorksShader) {
						m_Material.SetFloat("_FeatureMode", (int)m_FeatureMode);
						LW_MaterialPool.SetMaterialKeywords(m_Material);
					}
				}
			}
		}
		[SerializeField] protected FeatureMode m_FeatureMode = FeatureMode.Simple;

		/// <summary>
		/// Gets or sets  a value indicating how the auto-generated shader should calculate the front-facing direction from all strokes. 
		/// </summary>
		/// <remarks>
		/// This applied to all attached graphics unless a custom Material overrides it or if the 
		/// <c>featureMode</c> is set to <c>Simple</c>.
		/// </remarks>
		/// <value>
		/// -	Draw2D - Renders Lines flat on the XY plane.
		/// -	Draw3D - Renders Lines to always face the camera. 
		/// </value>
		public StrokeDrawMode strokeDrawMode {
			get {
				//if (m_Material != null && m_IsLineWorksShader) m_StrokeDrawMode = (StrokeDrawMode)m_Material.GetFloat("_StrokeDrawMode");
				return m_StrokeDrawMode;
			}
			set {
				if (m_StrokeDrawMode != value) {
					m_StrokeDrawMode = value;
					m_ForceRebuild = true;
					SetAllDirty();
					if (m_Material != null && m_IsLineWorksShader) {
						m_Material.SetFloat("_StrokeDrawMode", (int)m_StrokeDrawMode);
						LW_MaterialPool.SetMaterialKeywords(m_Material);
					}
				}
			}
		}
		[SerializeField] protected StrokeDrawMode m_StrokeDrawMode = StrokeDrawMode.Draw2D;

		/// <summary>
		/// Gets or sets a value indicating how the auto-generated shader should calculate shader stroke widths.
		/// </summary>
		/// <remarks>
		/// This applied to all attached graphics unless a custom Material overrides it or if the 
		/// <c>featureMode</c> is set to <c>Simple</c>.
		/// </remarks>
		/// <value>
		/// -	Scaled - Renders stroke widths in model-space units and is scaled by the RectTransform and scaleMode.
		/// -	Unscaled - Renders stroke widths in model-space units and is NOT scaled by the RectTransform and scaleMode. 
		/// -	ScreenSpace - Renders stroke widths in screen-space pixels. 
		/// </value>
		public StrokeScaleMode strokeScaleMode {
			get {
				//if (m_Material != null && m_IsLineWorksShader) m_StrokeScaleMode = (StrokeScaleMode)m_Material.GetFloat("_StrokeScaleMode");
				return m_StrokeScaleMode;
			}
			set {
				if (m_StrokeScaleMode != value) {
					m_StrokeScaleMode = value;
					m_ForceRebuild = true;
					SetAllDirty();
					if (m_Material != null && m_IsLineWorksShader) {
						m_Material.SetFloat("_StrokeScaleMode", (int)m_StrokeScaleMode);
						LW_MaterialPool.SetMaterialKeywords(m_Material);
					}
				}
			}
		}
		[SerializeField] protected StrokeScaleMode m_StrokeScaleMode = StrokeScaleMode.Scaled;

		/// <summary>
		/// Gets or sets a value indicating whether the auto-generated shader should calculate Joins and Caps.
		/// </summary>
		/// <remarks>
		/// This applied to all attached graphics unless a custom Material overrides it or if the 
		/// <c>featureMode</c> is set to <c>Simple</c>.
		/// </remarks>
		/// <value>
		/// -	Off - Do not calculate stroke joins and caps. This removes somes shader calculations.
		/// -	UseVertices - Use vertices to build joins and caps. (Not currently Implemented. has the same effect as "Off"). 
		/// -	Shader - Use the shader's fragment program to calculate and render stroke joins and caps. 
		/// </value>
		public JoinsAndCapsMode joinsAndCapsMode {
			get {
				//if (m_Material != null && m_IsLineWorksShader) m_JoinsAndCapsMode = (JoinsAndCapsMode)m_Material.GetFloat("_JoinsAndCapsMode");
				return m_JoinsAndCapsMode;
			}
			set {
				if (m_JoinsAndCapsMode != value) {
					m_JoinsAndCapsMode = value;
					m_ForceRebuild = true;
					SetAllDirty();
					if (m_Material != null && m_IsLineWorksShader) {
						m_Material.SetFloat("_JoinsAndCapsMode", (int)m_JoinsAndCapsMode);
						LW_MaterialPool.SetMaterialKeywords(m_Material);
					}
				}
			}
		}
		[SerializeField] protected JoinsAndCapsMode m_JoinsAndCapsMode = JoinsAndCapsMode.Vertex;

		/// <summary>
		/// Gets or sets a value indicating whether the auto-generated shader should calculate Gradients.
		/// </summary>
		/// <remarks>
		/// This applied to all attached graphics unless a custom Material overrides it or if the 
		/// <c>featureMode</c> is set to <c>Simple</c>.
		/// </remarks>
		/// <value>
		/// -	Off - Do not calculate gradients. This removes somes shader calculations. 
		/// 	(Not currently implemented. Has the same effect as "UseVertices").
		/// -	UseVertices - Apply gradient colors to the vertex color. If there is a high vertex density 
		/// 	or the gradients are simple, This can produce acceptable results without the need for expensive shader calculations. 
		/// -	Shader - Use the shader's fragment program to calculate and render gradients.
		/// </value>
		public GradientsMode gradientsMode {
			get {
				//if (m_Material != null && m_IsLineWorksShader) m_GradientsMode = (GradientsMode)m_Material.GetFloat("_GradientsMode");
				return m_GradientsMode;
			}
			set {
				if (m_GradientsMode != value) {
					m_GradientsMode = value;
					m_ForceRebuild = true;
					SetAllDirty();
					if (m_Material != null && m_IsLineWorksShader) {
						m_Material.SetFloat("_GradientsMode", (int)m_GradientsMode);
						LW_MaterialPool.SetMaterialKeywords(m_Material);
					}
				}
			}
		}
		[SerializeField] protected GradientsMode m_GradientsMode = GradientsMode.Vertex;

		/// <summary>
		/// Gets or sets a value indicating whether the auto-generated shader should calculate Anti-Aliasing.
		/// </summary>
		/// <remarks>
		/// This applied to all attached graphics unless a custom Material overrides it or if the 
		/// <c>featureMode</c> is set to <c>Simple</c>.
		/// </remarks>
		/// <value>
		/// -	Off - Do not calculate shader anti-aliasing. This removes somes shader calculations. 
		/// -	On - Use the shader's fragment program to calculate and render anti-aliasing. This can increase the 
		/// 	vertex count and add some shader calculations.
		/// </value>
		public AntiAliasingMode antiAliasingMode {
			get {
				//if (m_Material != null && m_IsLineWorksShader) m_AntiAliasingMode = (AntiAliasingMode)m_Material.GetFloat("_AntiAliasingMode");
				return m_AntiAliasingMode;
			}
			set {
				if (m_AntiAliasingMode != value) {
					m_AntiAliasingMode = value;
					m_ForceRebuild = true;
					SetAllDirty();
					if (m_Material != null && m_IsLineWorksShader) {
						m_Material.SetFloat("_AntiAliasingMode", (int)m_AntiAliasingMode);
						LW_MaterialPool.SetMaterialKeywords(m_Material);
					}
				}
			}
		}
		[SerializeField] protected AntiAliasingMode m_AntiAliasingMode = AntiAliasingMode.Off;

		// Graphic Element

		public LW_Group graphics { get { return graphic; } }
		/// <summary>
		/// Gets or sets the root graphic element.
		/// </summary>
		/// <remarks>
		/// If no graphic element is provided an empty LW_Group is created.
		/// </remarks>
		/// <value>The graphic.</value>
		public LW_Group graphic {
			get {
				if (m_Graphic == null) m_Graphic = ScriptableObject.CreateInstance<LW_Group>();
				return m_Graphic;
			}
			set {
				if (m_Graphic != value) {
					m_ForceRebuild = true;
					SetAllDirty();
					UnregisterChildren();
					m_Graphic = value;
					RegisterChildren();
				}
			}
		}
		[SerializeField] private LW_Group m_Graphic;

		/// <summary>
		/// Gets the materials for this LW_Canvas.
		/// </summary>
		/// <remarks>
		/// If any individual style elements override the LW_Canvas settings, multiple materials and sub-meshes 
		/// will be created. This is the collection of Materials for all sub-meshes, in order.
		/// </remarks>
		/// <value>The materials.</value>
		public List<Material> materials {
			get {
				return m_MaterialsList;
			}
		}
		private List<Material> m_MaterialsList = LW_ListPool<Material>.Get();

		// Internal private fields
		private EdgeCollider2D[] m_EdgeColliderArray;
		private PolygonCollider2D[] m_PolygonColliderArray;
		private Stack<EdgeCollider2D> m_EdgeColliderPool;
		private Stack<PolygonCollider2D> m_PolygonColliderPool;
		private bool m_ScalerDirty = false;
		private bool m_BuffersDirty = false;
		private bool m_VertsDirty = false;
		private bool m_MaterialDirty = false;
		private bool m_ForceRebuild = false;
		private UnityAction m_OnDirtyScalerCallback;
		private UnityAction m_OnDirtyLW_CanvasCallback;
		private Matrix4x4 m_ScalerHorizontalLeft;
		private Matrix4x4 m_ScalerHorizontalCenter;
		private Matrix4x4 m_ScalerHorizontalRight;
		private Matrix4x4 m_ScalerVerticalTop;
		private Matrix4x4 m_ScalerVerticalCenter;
		private Matrix4x4 m_ScalerVerticalBottom;
		private List<int> m_SubMeshIndexCount = LW_ListPool<int>.Get();
		private List<List<int>> m_SubMeshIndicesList = LW_ListPool<List<int>>.Get();
		private List<LW_VertexBuffer> m_VertexBufferList = LW_ListPool<LW_VertexBuffer>.Get();
		private List<LW_MarkerBuffer> m_MarkerBufferList = LW_ListPool<LW_MarkerBuffer>.Get();
		private List<LW_ColliderBuffer> m_ColliderBufferList = LW_ListPool<LW_ColliderBuffer>.Get();
		private List<Vector3> m_PositionsList = LW_ListPool<Vector3>.Get();
		private List<Vector3> m_NormalsList = LW_ListPool<Vector3>.Get();
		private List<Vector4> m_TangentsList = LW_ListPool<Vector4>.Get();
		private List<Vector2> m_Uv1sList = LW_ListPool<Vector2>.Get();
		private List<Vector2> m_Uv2sList = LW_ListPool<Vector2>.Get();
		private List<Color32> m_ColorsList = LW_ListPool<Color32>.Get();
		private Material[] m_RendererMaterials = new Material[0];
		private Mesh m_Mesh;
		private MeshRenderer m_MeshRenderer;
		private MeshFilter m_MeshFilter;
		private CanvasRenderer m_CanvasRenderer;
		private Collider2D m_Collider;
		private LW_GraphicBuffer m_Buffer;
		private static bool s_BuffersDirty = false;


		/// <summary>
		/// Creates a GameObject with a LW_Canvas component attached.
		/// </summary>
		/// <param name="context">GameObject to attach the new GameObject to.</param>
		/// <param name="name">Name used for the GameObject.</param>
		/// <param name="isUIElement">sets the LW_Canvas to be a UI element and creates the UI Canvas if needed.</param>
		/// <returns>Returns the Created LW_Canvas.</returns>
		public static LW_Canvas Create(GameObject context = null, string name = "LineWorks", bool isUIElement = false) {
			if (context == null) context = CreateElementRoot(name, context, isUIElement);
			LW_Canvas vectorCanvas = context.GetComponent<LW_Canvas>();
			if (vectorCanvas == null) vectorCanvas = context.AddComponent<LW_Canvas>();
			else {
				GameObject newContext = new GameObject(name);
				newContext.transform.parent = context.transform;
				vectorCanvas = newContext.AddComponent<LW_Canvas>();
			}

			vectorCanvas.name = name;
			return vectorCanvas;
		}

		private static GameObject CreateElementRoot(string name, GameObject context, bool isUIElement) {
			GameObject child = new GameObject(name);
			GameObject parent = context;

			if (isUIElement) {
				if (parent == null || parent.GetComponentInParent<Canvas>() == null) parent = GetOrCreateCanvasGameObject();
			}
			if (parent != null) {
				#if UNITY_EDITOR
				child.name = GameObjectUtility.GetUniqueNameForSibling(parent.transform, name);
				Undo.RegisterCreatedObjectUndo(child, "Create " + child.name);
				Undo.SetTransformParent(child.transform, parent.transform, "Parent " + child.name);
				GameObjectUtility.SetParentAndAlign(child, parent);
				Selection.activeGameObject = child;
				#else
				child.transform.parent = context.transform;
				#endif
			}

			return child;
		}
		private static GameObject GetOrCreateCanvasGameObject() {
			#if UNITY_EDITOR
			GameObject selectedGo = Selection.activeGameObject;
			#else
			GameObject selectedGo = null;
			#endif

			// Try to find a gameobject that is the selected GO or one if its parents.
			Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
			if (canvas != null && canvas.gameObject.activeInHierarchy)
				return canvas.gameObject;

			// No canvas in selection or its parents? Then use just any canvas..
			canvas = UnityEngine.Object.FindObjectOfType(typeof(Canvas)) as Canvas;
			if (canvas != null && canvas.gameObject.activeInHierarchy)
				return canvas.gameObject;

			// No canvas in the scene at all? Then create a new one.
			return CreateNewUI();
		}
		private static GameObject CreateNewUI() {
			// Root for the UI
			var root = new GameObject("Canvas");
			root.layer = LayerMask.NameToLayer("UI");
			Canvas uiCanvas = root.AddComponent<Canvas>();
			uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			root.AddComponent<CanvasScaler>();
			root.AddComponent<GraphicRaycaster>();
			#if UNITY_EDITOR
			Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);
			#endif

			// if there is no event system add one...
			#if UNITY_EDITOR
			CreateEventSystem(false);
			#endif
			return root;
		}
		#if UNITY_EDITOR
		private static void CreateEventSystem(bool select) {CreateEventSystem(select, null); }
		private static void CreateEventSystem(bool select, GameObject parent) {
			var esys = UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
			if (esys == null) {
				var eventSystem = new GameObject("EventSystem");
				GameObjectUtility.SetParentAndAlign(eventSystem, parent);
				esys = eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
				eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
				//eventSystem.AddComponent<UnityEngine.EventSystems.TouchInputModule>();
				Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
			}

			if (select && esys != null) {
				Selection.activeGameObject = esys.gameObject;
			}
		}
		#endif

		protected override void OnEnable() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("OnEnable Canvas: " + name);
			#endif

			#if UNITY_5_4_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += CleanupAfterSceneLoaded;
			#endif

			if (m_CanvasRenderer == null) m_CanvasRenderer = gameObject.GetComponent<CanvasRenderer>();
			if (m_MeshRenderer == null) m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (m_MeshFilter == null) m_MeshFilter = gameObject.GetComponent<MeshFilter>();

			/*
			#if UNITY_EDITOR
			int instanceID = GetHashCode();
			if (m_Id == -1) m_Id = instanceID;
			else if (m_Id != instanceID) {
				if (m_Graphic != null && !AssetDatabase.Contains(m_Graphic)) {
					if (s_DebugCanvas) Debug.Log("IDs don't match: " + m_Id + " : " + instanceID + " Making a Copy");
					m_Graphic = m_Graphic.Copy() as LW_Group;
					Resources.UnloadUnusedAssets();
				}
				m_Id = instanceID;
			}
			#endif
			*/

			if (m_Graphic == null) {
				m_Graphic = ScriptableObject.CreateInstance<LW_Group>();
				m_Graphic.hideFlags = this.hideFlags;
			}

			//m_ForceRebuild = true;
			SetClean();
			base.OnEnable();
		}
		protected override void OnDisable() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("OnDisable Canvas: " + name);
			#endif

			#if UNITY_5_4_OR_NEWER
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= CleanupAfterSceneLoaded;
			#endif

			UnregisterChildren();
			base.OnDisable();
		}
		protected override void OnDestroy() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("OnDestroy Canvas: " + name);
			#endif

			UnregisterChildren();
			#if UNITY_EDITOR
			if (m_Graphic != null) m_Graphic = LW_Utilities.SafeDestroy<LW_Group>(m_Graphic, false);
			#endif

			if (m_VertexBufferList != null) for (int i=0; i<m_VertexBufferList.Count; i++) m_VertexBufferList[i].Dispose();
			if (m_MarkerBufferList != null) for (int i=0; i<m_MarkerBufferList.Count; i++) m_MarkerBufferList[i].Dispose();
			if (m_ColliderBufferList != null) for (int i=0; i<m_ColliderBufferList.Count; i++) m_ColliderBufferList[i].Dispose();

			if (m_MaterialsList != null) m_MaterialsList.Clear();
			if (m_SubMeshIndexCount != null) m_SubMeshIndexCount.Clear();
			if (m_SubMeshIndicesList != null) m_SubMeshIndicesList.Clear();
			if (m_PositionsList != null) m_PositionsList.Clear();
			if (m_NormalsList != null) m_NormalsList.Clear();
			if (m_TangentsList != null) m_TangentsList.Clear();
			if (m_Uv1sList != null) m_Uv1sList.Clear();
			if (m_Uv2sList != null) m_Uv2sList.Clear();
			if (m_ColorsList != null) m_ColorsList.Clear();
			if (m_VertexBufferList != null) m_VertexBufferList.Clear();
			if (m_MarkerBufferList != null) m_MarkerBufferList.Clear();
			if (m_ColliderBufferList != null) m_ColliderBufferList.Clear();

			LW_MaterialPool.SetDirty();
			LW_BufferPool<LW_VertexBuffer>.SetDirty();
			LW_BufferPool<LW_MarkerBuffer>.SetDirty();
			LW_BufferPool<LW_ColliderBuffer>.SetDirty();

			base.OnDestroy();
		}

		#if !UNITY_5_4
		protected void OnLevelWasLoaded(int level) {
			CleanupAfterSceneLoaded();
		}
		#endif

		#if UNITY_5_4_OR_NEWER
		protected void CleanupAfterSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) {
			if (!scene.IsValid() || mode == UnityEngine.SceneManagement.LoadSceneMode.Additive) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("LW_Canvas CleanupAfterSceneLoaded: " + scene.name + " : " + mode.ToString());
			#endif

			CleanupAfterSceneLoaded();
		}
		#endif
		protected void CleanupAfterSceneLoaded() {
			LW_MaterialPool.Clear();
			LW_BufferPool<LW_VertexBuffer>.Clear();
			LW_BufferPool<LW_MarkerBuffer>.Clear();
			LW_BufferPool<LW_ColliderBuffer>.Clear();

			if (s_BuffersDirty) {
				Resources.UnloadUnusedAssets();
				System.GC.Collect();
				s_BuffersDirty = false;
			}
		}
	
		#if UNITY_EDITOR
		protected override void OnValidate() {
			if (s_DebugCanvas) Debug.Log("OnValidate Canvas: " + name);
			base.OnValidate();
		}
		#endif
		protected override void OnRectTransformDimensionsChange() {
			if (!gameObject.activeInHierarchy) return;
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("OnRectTransformDimensionsChange: " + name);
			#endif
			SetScalerDirty();
			base.OnRectTransformDimensionsChange();
		}
		protected override void OnBeforeTransformParentChanged() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("OnBeforeTransformParentChanged: " + name);
			#endif
			base.OnBeforeTransformParentChanged();
		}
		protected override void OnTransformParentChanged() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("OnTransformParentChanged: " + name);
			#endif
			base.OnTransformParentChanged();
		}
		protected override void OnCanvasHierarchyChanged() {
			m_TransformDirty = true;
			SetBuffersDirty();
			base.OnCanvasHierarchyChanged();
		}

		/// <summary>
		/// Sets the RectTransform size and pivot to match the view box.
		/// </summary>
		public override void SetNativeSize() {
			SetRectSizeToViewBox();
			SetRectPivotToViewBox();
		}
		/// <summary>
		/// Sets the RectTransform size to match the view box.
		/// </summary>
		public void SetRectSizeToViewBox() {
			SetScalerDirty();
			Rect m_ViewBox = viewBox;
			float width = m_ViewBox.width;
			float height = m_ViewBox.height;
			rectTransform.anchorMax = rectTransform.anchorMin;
			rectTransform.sizeDelta = new Vector2(width, height);
		}
		/// <summary>
		/// Sets the RectTranform pivot to match the view box.
		/// </summary>
		public void SetRectPivotToViewBox() {
			SetScalerDirty();
			Rect m_ViewBox = viewBox;
			Vector2 previousPivot = rectTransform.pivot;
			float pivotX = 0.5f - m_ViewBox.x / m_ViewBox.width;
			float pivotY = 0.5f - m_ViewBox.y / m_ViewBox.height;
			rectTransform.anchorMax = rectTransform.anchorMin;
			rectTransform.pivot = new Vector2(pivotX, pivotY);
			rectTransform.anchoredPosition += new Vector2((rectTransform.pivot.x - previousPivot.x) * rectTransform.sizeDelta.x, (rectTransform.pivot.y - previousPivot.y) * rectTransform.sizeDelta.y);

		}
		/// <summary>
		/// Sets the ViewBox to match the RectTransform.
		/// </summary>
		public void SetViewBoxToRect() {
			SetScalerDirty();
			Rect m_ViewBox = viewBox;
			//float pivotX = rectTransform.pivot.x;
			//float pivotY = rectTransform.pivot.y;
			float width = rectTransform.sizeDelta.x;
			float height = rectTransform.sizeDelta.y;
			m_ViewBox.width = width;
			m_ViewBox.height = height;
			//m_ViewBox.x = (1 - pivotX) * width - 0.5f * width;
			//m_ViewBox.y = (1 - pivotY) * height - 0.5f * height;
			viewBox = m_ViewBox;
		}
		/// <summary>
		/// Sets the view box to bounds of the attached LineWorks elements and centers the element about the Pivot.
		/// </summary>
		public void SetViewBoxToBounds() {
			SetScalerDirty();
			Rect m_ViewBox = viewBox;
			if (m_VertexBufferList == null || m_VertexBufferList.Count == 0) {
				UpdateBuffers();
			}
			if (m_VertexBufferList != null && m_VertexBufferList.Count > 0) {
				Bounds bounds = new Bounds();
				bool isFirstBufer = true;
				for (int i=0; i<m_VertexBufferList.Count; i++) {
					LW_VertexBuffer vBuffer = m_VertexBufferList[i];
					if (!vBuffer.isValid || vBuffer.positions.Count < 3) continue;

					if (isFirstBufer) {
						bounds = vBuffer.bounds;
						isFirstBufer = false;
					}
					else bounds.Encapsulate(vBuffer.bounds);
				}
				m_ViewBox.size = bounds.size;
				m_ViewBox.position = bounds.center;
			}
			viewBox = m_ViewBox;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
		}

		/// <summary>
		/// Sets the Transformation scaler Dirty. The scaler will be updated on the next Canvas Rebuild.
		/// </summary>
		public virtual void SetScalerDirty() {
			if (!IsActive() || m_ScalerDirty || isPrefab) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("SetScalerDirty: " + name);
			#endif

			m_ScalerDirty = true;
			SetVerticesDirty();
			if (m_OnDirtyScalerCallback != null) m_OnDirtyScalerCallback();
		}
		/// <summary>
		/// Sets the linework Graphic Dirty. The linework Graphic will be updated on the next Canvas Rebuild.
		/// </summary>
		public virtual void SetBuffersDirty() {
			if (!IsActive() || m_BuffersDirty || isPrefab) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("SetBuffersDirty: " + name + " NotActive: " + !IsActive() + " alreadyDirty: " + m_BuffersDirty + " isPrefab: " + isPrefab);
			#endif

			UnregisterChildren();
			RegisterChildren();
			m_BuffersDirty = true;
			SetMaterialDirty();
			SetVerticesDirty();
			if (m_OnDirtyLW_CanvasCallback != null) m_OnDirtyLW_CanvasCallback();
		}
		/// <summary>
		/// Sets the Mesh vertices Dirty. The vertices will be updated on the next Canvas Rebuild.
		/// </summary>
		public override void SetVerticesDirty() {
			if (!IsActive() || m_VertsDirty || isPrefab) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("SetVerticesDirty: " + name);
			#endif

			m_VertsDirty = true;
			if (!CanvasUpdateRegistry.IsRebuildingGraphics()) CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
			if (m_OnDirtyVertsCallback != null) m_OnDirtyVertsCallback();
		}
		/// <summary>
		/// Sets the renderer's material Dirty. The renderer's material will be updated on the next Canvas Rebuild.
		/// </summary>
		public override void SetMaterialDirty() {
			if (!IsActive() || m_MaterialDirty || isPrefab) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("SetMaterialDirty: " + name);
			#endif

			LW_MaterialPool.SetDirty();
			m_MaterialDirty = true;

			if (!CanvasUpdateRegistry.IsRebuildingGraphics()) CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild(this);
			if (m_OnDirtyMaterialCallback != null) m_OnDirtyMaterialCallback();
		}
		/// <summary>
		/// Sets the UI Canvas layout Dirty. The UI Canvas layout will be updated on the next Canvas Rebuild.
		/// </summary>
		public override void SetLayoutDirty() {
			if (!IsActive() || isPrefab || !isUIElement) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("SetLayoutDirty: " + name);
			#endif

			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
		/// <summary>
		/// Sets the scaler, linework, vertices, material, and layout Dirty. The everything will be updated on the next Canvas Rebuild.
		/// </summary>
		public override void SetAllDirty() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("SetAllDirty: " + name);
			#endif

			m_TransformDirty = true;
			SetScalerDirty();
			SetBuffersDirty();
			base.SetAllDirty();
		}
		/// <summary>
		/// Forces the total rebuild of the LW_Canvas.  This will recalculate all materials and meshes whether or not anything has changed.
		/// </summary>
		public void ForceTotalRebuild() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("Force Total Rebuild");
			#endif

			m_ForceRebuild = true;
			SetAllDirty();
			if (m_Material != null) UpdatePropertiesToMatchMaterial(m_Material);
		}

		protected void UpdateScaler() {
			if (!IsActive()) return;

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("UpdateScaler: " + name);
			#endif

			if (rectTransform != null && rectTransform.rect.width != 0 && rectTransform.rect.height != 0) {
				Rect rect = rectTransform.rect;
				Vector2 pivot = rectTransform.pivot;
				float scaleX = rect.width / m_ViewBox.width;
				float scaleY = rect.height / m_ViewBox.height;
				Vector3 scale = Vector3.one;
				Vector3 position = Vector3.zero;
				switch (scaleMode) {
				case ScaleMode.ScaleToFit:
					scale = new Vector3(Mathf.Min(scaleX, scaleY), Mathf.Min(scaleX, scaleY), 1);
					break;
				case ScaleMode.ScaleToFill:
					scale = new Vector3(Mathf.Max(scaleX, scaleY), Mathf.Max(scaleX, scaleY), 1);
					break;
				case ScaleMode.StretchToFill:
					scale = new Vector3(scaleX, scaleY, 1);
					break;
				case ScaleMode.NineSlice:
					// Left
					scale = Vector3.one;
					position = new Vector3(-(rect.width - m_ViewBox.width) * 0.5f, 0, 0);
					m_ScalerHorizontalLeft.SetTRS(position, Quaternion.identity, scale);
					// Right
					scale = Vector3.one;
					position = new Vector3((rect.width - m_ViewBox.width) * 0.5f, 0, 0);
					m_ScalerHorizontalRight.SetTRS(position, Quaternion.identity, scale);
					//Center
					scale = new Vector3((rect.width - m_Border.x - m_Border.y) / (m_ViewBox.width - m_Border.x - m_Border.y), 1, 1);
					position = Vector3.zero;
					m_ScalerHorizontalCenter.SetTRS(position, Quaternion.identity, scale);

					// Left
					scale = Vector3.one;
					position = new Vector3(0, (rect.height - m_ViewBox.height) * 0.5f, 0);
					m_ScalerVerticalTop.SetTRS(position, Quaternion.identity, scale);
					// Right
					scale = Vector3.one;
					position = new Vector3(0, -(rect.height - m_ViewBox.height) * 0.5f, 0);
					m_ScalerVerticalBottom.SetTRS(position, Quaternion.identity, scale);
					//Center
					scale = new Vector3(1, (rect.height - m_Border.width - m_Border.height) / (m_ViewBox.height - m_Border.width - m_Border.height), 1);
					position = Vector3.zero;
					m_ScalerVerticalCenter.SetTRS(position, Quaternion.identity, scale);

					//scale = new Vector3(scaleX, scaleY, 1);
					scale = Vector3.one;
					break;
				/*
				case ScaleMode.FFD:
					if (m_FFDCorners == null || m_FFDCorners.Length != 4) m_FFDCorners = new LW_Point2D[]{
						new LW_Point2D(Vector2.zero, Vector2.down, Vector2.right),
						new LW_Point2D(Vector2.zero, Vector2.left, Vector2.down),
						new LW_Point2D(Vector2.zero, Vector2.up, Vector2.left),
						new LW_Point2D(Vector2.zero, Vector2.right, Vector2.up)
					};
					break;
				*/
				}

				position = new Vector2((-(pivot.x * 2 - 1) * (m_ViewBox.width * 0.5f * scale.x)) - m_ViewBox.x * scale.x, (-(pivot.y * 2 - 1) * (m_ViewBox.height * 0.5f * scale.y)) - m_ViewBox.y * scale.y);
				m_Scaler.SetTRS(position, Quaternion.identity, scale);
			}
			else m_Scaler = Matrix4x4.identity;

			if (m_Border.x < 0) m_Border.x = 0;
			if (m_Border.y < 0) m_Border.y = 0;
			if (m_Border.width < 0) m_Border.width = 0;
			if (m_Border.height < 0) m_Border.height = 0;

		}
		protected void UpdateBuffers() {
			
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("UpdateBuffers: " + name + " forceRebuild: " + (m_ForceRebuild));
			#endif

			m_VertexCount = 0;
			m_IndexCount = 0;

			m_VertexBufferList.Clear();
			m_MarkerBufferList.Clear();
			m_ColliderBufferList.Clear();

			if (m_Graphic != null) RebuildGraphic(m_Graphic, Matrix4x4.identity, m_ForceRebuild, IsActive());
		}
		protected override void UpdateMaterial() {
			
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("UpdateMaterial: " + name + " bufferCount: " + m_VertexBufferList.Count);
			#endif

			// Update Renderer
			if (UpdateRenderers()) return;

			// Initialize Lists
			m_SubMeshCount = 0;
			m_SubMeshIndexCount.Clear();
			m_MaterialsList.Clear();
			List<Material> m_MaterialsForRendererList = LW_ListPool<Material>.Get();

			// Update Submesh Index Count
			for (int c = 0; c < m_VertexBufferList.Count; c++) {
				LW_VertexBuffer vBuffer = m_VertexBufferList[c];
				if (!vBuffer.isValid || vBuffer.positions.Count < 3) continue;

				Material material = LW_MaterialPool.Get(vBuffer);
				vBuffer.material = material;

				if (material != null) {
					if (m_MaterialsList.Count > 0 && m_MaterialsList[m_MaterialsList.Count - 1] == material) {
						int subMeshIndex = m_MaterialsList.Count - 1;
						vBuffer.subMeshIndex = subMeshIndex;
						m_SubMeshIndexCount[subMeshIndex] += vBuffer.indexCount;
					}
					else {
						m_MaterialsList.Add(material);
						m_SubMeshIndexCount.Add(vBuffer.indexCount);
						m_SubMeshCount++;
						vBuffer.subMeshIndex = m_MaterialsList.Count - 1;
					}
				}
			}

			LW_MaterialPool.UpdateMaterials(m_ForceRebuild);


			// Apply Material Modifiers
			var components = LW_ListPool<Component>.Get();
			GetComponents(typeof(IMaterialModifier), components);
			for (int i = 0; i < m_MaterialsList.Count; i++) {
				Material currentMat = m_MaterialsList[i];
				for (var c = 0; c < components.Count; c++) currentMat = (components[c] as IMaterialModifier).GetModifiedMaterial(currentMat);
				m_MaterialsForRendererList.Add(currentMat);
			}
			LW_ListPool<Component>.Release(components);

			// Update Renderer Materials
			if (isUIElement) {
				bool hasChanged = false;

				if (m_MaterialsForRendererList.Count != m_CanvasRenderer.materialCount) hasChanged = true;
				else {
					for (int i = 0; i < m_CanvasRenderer.materialCount; i++) {
						if (m_CanvasRenderer.GetMaterial(i) != m_MaterialsForRendererList[i]) {
							hasChanged = true;
							break;
						}
					}
				}
				if (hasChanged || m_ForceRebuild) {
					m_CanvasRenderer.materialCount = m_MaterialsForRendererList.Count;
					for (int i = 0; i < m_MaterialsForRendererList.Count; i++) {
						m_CanvasRenderer.SetMaterial(m_MaterialsForRendererList[i], i);
					}
				}
			}
			else {
				bool hasChanged = false;

				if (m_RendererMaterials == null || m_RendererMaterials.Length == 0) m_RendererMaterials = m_MeshRenderer.sharedMaterials;
				if (m_MaterialsForRendererList != null) {
					if (m_MaterialsForRendererList.Count != m_RendererMaterials.Length) hasChanged = true;
					else if (m_MaterialsForRendererList != null) {
						for (int i = 0; i < m_RendererMaterials.Length; i++) {
							if (m_RendererMaterials[i] != m_MaterialsForRendererList[i]) {
								hasChanged = true;
								break;
							}
						}
					}
				}
				if (hasChanged || m_ForceRebuild) {
					if (m_RendererMaterials == null || m_RendererMaterials.Length != m_MaterialsForRendererList.Count) 
						m_RendererMaterials = new Material[m_MaterialsForRendererList.Count];
					for (int i=0; i<m_MaterialsForRendererList.Count; i++)
						m_RendererMaterials[i] = m_MaterialsForRendererList[i];

					m_MeshRenderer.sharedMaterials = m_RendererMaterials;
				}

				if (m_MeshRenderer.sortingLayerID != m_SortingLayerID) m_MeshRenderer.sortingLayerID = m_SortingLayerID;
				if (m_MeshRenderer.sortingOrder != m_SortingOrder) m_MeshRenderer.sortingOrder = m_SortingOrder;
			}

			// Release Temp list
			LW_ListPool<Material>.Release(m_MaterialsForRendererList);
		}
		protected override void UpdateGeometry() {

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("UpdateGeometry: " + name + " bufferCount: " + m_VertexBufferList.Count);
			#endif

			// Update Renderer
			if (UpdateRenderers()) return;

			int vertexCapacity = 0;
			for (int c=0; c<m_VertexBufferList.Count; c++) {
				LW_VertexBuffer vBuffer = m_VertexBufferList[c];
				if (!vBuffer.isValid || vBuffer.positions.Count < 3) continue;

				vertexCapacity += vBuffer.positions.Capacity;
			}
			vertexCapacity = Mathf.Max(m_VertexCount, vertexCapacity, 65534);
			if (m_PositionsList.Capacity < vertexCapacity) {
				m_PositionsList.Capacity = vertexCapacity;
				m_NormalsList.Capacity = vertexCapacity;
				m_TangentsList.Capacity = vertexCapacity;
				m_Uv1sList.Capacity = vertexCapacity;
				m_Uv2sList.Capacity = vertexCapacity;
				m_ColorsList.Capacity = vertexCapacity;
			}
			m_PositionsList.Clear();
			m_NormalsList.Clear();
			m_TangentsList.Clear();
			m_Uv1sList.Clear();
			m_Uv2sList.Clear();
			m_ColorsList.Clear();

			if (m_SubMeshIndicesList.Count >= m_SubMeshIndexCount.Count) {
				for (int i = 0; i < m_SubMeshIndicesList.Count; i++) {
					m_SubMeshIndicesList[i].Clear();
					if (i < m_SubMeshIndexCount.Count && m_SubMeshIndicesList[i].Capacity < m_SubMeshIndexCount[i]) m_SubMeshIndicesList[i].Capacity = m_SubMeshIndexCount[i];
				}
			}
			else {
				for (int i = 0; i < m_SubMeshIndicesList.Count; i++) {
					m_SubMeshIndicesList[i].Clear();
					if (i < m_SubMeshIndexCount.Count && m_SubMeshIndicesList[i].Capacity < m_SubMeshIndexCount[i]) m_SubMeshIndicesList[i].Capacity = m_SubMeshIndexCount[i];
				}
				m_SubMeshIndicesList.Capacity = m_SubMeshIndexCount.Count;
				for (int i = m_SubMeshIndexCount.Count-m_SubMeshIndicesList.Count; i < m_SubMeshIndexCount.Count; i++) {
					List<int> subMeshIndices = LW_ListPool<int>.Get();
					if (subMeshIndices.Capacity < m_SubMeshIndexCount[i]) subMeshIndices.Capacity = m_SubMeshIndexCount[i];
					m_SubMeshIndicesList.Add(subMeshIndices);
				}
			}

			int firstVertexIndex = 0;
			int firstIndiceIndex = 0;

			for (int c=0; c<m_VertexBufferList.Count; c++) {
				LW_VertexBuffer vBuffer = m_VertexBufferList[c];
				if (!vBuffer.isValid || vBuffer.positions.Count < 3) continue;

				Matrix4x4 matrix = m_Scaler * vBuffer.matrix;
				Vector3 scale = matrix.ExtractScaleFromMatrix();
				if (scale == Vector3.zero || vBuffer.positions.Count < 3) continue;
				if (m_PositionsList.Count + vBuffer.positions.Count > 65534) {
					Debug.Log("Mesh Vertex Limit of 64k was exceeded. Please reduce segmentation or increase simplification.");
					break;
				}

				bool isFlipped = Mathf.Sign(matrix.m00 * matrix.m11 * matrix.m22) < 0;
				float uvScale = featureMode == FeatureMode.Advanced ? ((Vector2)(matrix).ExtractScaleFromMatrix()).PackVector2(-10,10) : 1;

				#if UNITY_EDITOR || DEVELOPMENT
				if (s_DebugCanvas) Debug.Log("Adding Buffer To Mesh: " + c + " : " + vBuffer.styleDataIndex + " : " + vBuffer.style.name + " : " + vBuffer.graphic.name + " : " + vBuffer.canvas.name);
				#endif

				for (int v = 0; v < vBuffer.positions.Count; v++) {
					

					if (m_ScaleMode == ScaleMode.NineSlice) {
						Vector3 position = vBuffer.matrix.MultiplyPoint3x4(vBuffer.positions[v]);
						Matrix4x4 slicedMatrix = Matrix4x4.identity;

						if (position.x < m_ViewBox.x - (m_ViewBox.width / 2f) + m_Border.x) slicedMatrix *= m_ScalerHorizontalLeft;
						else if (position.x > m_ViewBox.x + (m_ViewBox.width / 2f) - m_Border.y) slicedMatrix *= m_ScalerHorizontalRight;
						else slicedMatrix *= m_ScalerHorizontalCenter;

						if (position.y > m_ViewBox.y + (m_ViewBox.height / 2f) - m_Border.width) slicedMatrix *= m_ScalerVerticalTop;
						else if (position.y < m_ViewBox.y - (m_ViewBox.height / 2f) + m_Border.height) slicedMatrix *= m_ScalerVerticalBottom;
						else slicedMatrix *= m_ScalerVerticalCenter;

						matrix = slicedMatrix * m_Scaler * vBuffer.matrix;
						isFlipped = Mathf.Sign(matrix.m00 * matrix.m11 * matrix.m22) < 0;
					}

					if (v < vBuffer.positions.Count) m_PositionsList.Add(matrix.MultiplyPoint3x4(vBuffer.positions[v]));
					if (v < vBuffer.normals.Count) m_NormalsList.Add(matrix.MultiplyVector(vBuffer.normals[v]));
					if (v < vBuffer.tangents.Count) m_TangentsList.Add((Vector4)matrix.MultiplyVector(vBuffer.tangents[v]) + new Vector4(0,0,0,vBuffer.styleDataIndex + 1) );
					if (v < vBuffer.uv1s.Count) m_Uv1sList.Add(vBuffer.uv1s[v]);
					if (v < vBuffer.uv2s.Count) m_Uv2sList.Add(featureMode == FeatureMode.Advanced ? new Vector2(uvScale, vBuffer.uv2s[v].y) : vBuffer.uv2s[v]);
					if (v < vBuffer.colors.Count) m_ColorsList.Add(vBuffer.colors[v] * color);
				}


				int subMeshIndex = vBuffer.subMeshIndex;
				if (subMeshIndex != -1 && subMeshIndex < m_SubMeshIndicesList.Count) {
					List<int> subMeshIndices = m_SubMeshIndicesList[subMeshIndex];
					int numOfTriangles = vBuffer.indices.Count/3;
					for (int i=0; i<numOfTriangles; i++) {
						if (isFlipped) {
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+2]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+1]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3]);
						}
						else {
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+1]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+2]);
						}
					}
				}
				else {
					List<int> subMeshIndices = LW_ListPool<int>.Get();

					int numOfTriangles = vBuffer.indices.Count/3;
					for (int i=0; i<numOfTriangles; i++) {
						if (isFlipped) {
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+2]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+1]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3]);
						}
						else {
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+1]);
							subMeshIndices.Add(firstVertexIndex + vBuffer.indices[i*3+2]);
						}
					}
					m_SubMeshIndicesList.Add(subMeshIndices);
				}

				firstIndiceIndex += vBuffer.indices.Count;
				firstVertexIndex += vBuffer.positions.Count;
			}

			if (!isUIElement && m_Mesh == null) {
				m_Mesh = new Mesh();
				m_Mesh.name = name + " Mesh";

				// Change this if you want to save the Mesh.
				#if UNITY_EDITOR
				m_Mesh.hideFlags = s_DebugComponents ? HideFlags.DontSave : HideFlags.HideAndDontSave;
				#endif
			}
			Mesh mesh = isUIElement ? workerMesh : m_Mesh;

			if (m_PositionsList.Count == 0) mesh.Clear();
			else {
				if (mesh.vertexCount > m_PositionsList.Count) {
					mesh.subMeshCount = m_SubMeshIndicesList.Count;
					for (int i = 0; i < m_SubMeshIndicesList.Count; i++) mesh.SetTriangles(m_SubMeshIndicesList[i], i);

					mesh.SetVertices(m_PositionsList);
					mesh.SetNormals(m_NormalsList);
					mesh.SetTangents(m_TangentsList);
					mesh.SetUVs(0, m_Uv1sList);
					mesh.SetUVs(1, m_Uv2sList);
					mesh.SetColors(m_ColorsList);
				}
				else {
					mesh.SetVertices(m_PositionsList);
					mesh.SetNormals(m_NormalsList);
					mesh.SetTangents(m_TangentsList);
					mesh.SetUVs(0, m_Uv1sList);
					mesh.SetUVs(1, m_Uv2sList);
					mesh.SetColors(m_ColorsList);

					mesh.subMeshCount = m_SubMeshIndicesList.Count;
					for (int i = 0; i < m_SubMeshIndicesList.Count; i++) mesh.SetTriangles(m_SubMeshIndicesList[i], i);
				}
			}

			mesh.RecalculateBounds();

			List<Component> components = LW_ListPool<Component>.Get();
			GetComponents(typeof(IMeshModifier), components);
			#pragma warning disable 618
			for (var i = 0; i < components.Count; i++)
				((IMeshModifier)components[i]).ModifyMesh(mesh);
			#pragma warning restore 618
			LW_ListPool<Component>.Release(components);

			if (isUIElement) m_CanvasRenderer.SetMesh(mesh);
			else {
				m_MeshFilter.sharedMesh = mesh;
				/* since we're not saving the mesh we don't need this.
				#if UNITY_EDITOR
				if (isPrefab && !AssetDatabase.Contains(mesh)) {
					mesh.hideFlags = HideFlags.None;
					if (isPrefab) {
						GameObject prefab = PrefabUtility.GetPrefabParent(this) as GameObject;
						if (prefab != null && AssetDatabase.Contains(prefab)) AssetDatabase.AddObjectToAsset(mesh, prefab);
					}
				}
				#endif
				*/
			}
		}
		protected void UpdateColliders() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("UpdateColliders: " + name + " bufferCount: " + m_ColliderBufferList.Count);
			#endif

			if (!IsActive() || m_ColliderBufferList == null || m_ColliderBufferList.Count == 0 || isUIElement) return;

			if (m_EdgeColliderPool == null) m_EdgeColliderPool = new Stack<EdgeCollider2D>();
			else m_EdgeColliderPool.Clear();

			m_EdgeColliderArray = GetComponents<EdgeCollider2D>() as EdgeCollider2D[];
			for (int i=0; i<m_EdgeColliderArray.Length; i++) m_EdgeColliderPool.Push(m_EdgeColliderArray[i]);

			if (m_PolygonColliderPool == null) m_PolygonColliderPool = new Stack<PolygonCollider2D>();
			else m_PolygonColliderPool.Clear();

			m_PolygonColliderArray = GetComponents<PolygonCollider2D>() as PolygonCollider2D[];
			for (int i=0; i<m_PolygonColliderArray.Length; i++) m_PolygonColliderPool.Push(m_PolygonColliderArray[i]);

			for (int c=0; c<m_ColliderBufferList.Count; c++) {
				LW_ColliderBuffer cBuffer = m_ColliderBufferList[c];
				Matrix4x4 matrix = m_Scaler * cBuffer.matrix;
				Vector3 scale = matrix.ExtractScaleFromMatrix();
				if (scale == Vector3.zero) continue;

				switch(cBuffer.colliderType) {
				case ColliderType.Edge:
					EdgeCollider2D edgeCollider = null;
					if (m_EdgeColliderPool.Count > 0) edgeCollider = m_EdgeColliderPool.Pop();
					if (edgeCollider == null) edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
					if (edgeCollider != null) edgeCollider.points = cBuffer.GetPoints(matrix);
					break;
				case ColliderType.Polygon:
					PolygonCollider2D polygonCollider = null;
					if (m_PolygonColliderPool.Count > 0) polygonCollider = m_PolygonColliderPool.Pop();
					if (polygonCollider == null) polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
					if (polygonCollider != null) {
						polygonCollider.pathCount = cBuffer.paths.Count;
						for (int i=0; i<cBuffer.paths.Count; i++) polygonCollider.SetPath(i, cBuffer.GetPath(i, matrix));
					}
					break;
				}
			}

			while (m_EdgeColliderPool.Count > 0) LW_Utilities.SafeDestroy<EdgeCollider2D>(m_EdgeColliderPool.Pop());
			while (m_PolygonColliderPool.Count > 0) LW_Utilities.SafeDestroy<PolygonCollider2D>(m_PolygonColliderPool.Pop());
		}
		protected void SetClean() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("SetClean: " + name);
			#endif
			m_ScalerDirty = false;
			m_BuffersDirty = false;
			m_VertsDirty = false;
			m_MaterialDirty = false;
			m_ForceRebuild = false;
			m_TransformDirty = false;
			if (m_Graphic != null) m_Graphic.SetClean();


			LW_MaterialPool.Clean();
			LW_BufferPool<LW_VertexBuffer>.Clean();
			LW_BufferPool<LW_MarkerBuffer>.Clean();
			LW_BufferPool<LW_ColliderBuffer>.Clean();
		}

		private void RebuildGraphic(LW_Graphic graphic, Matrix4x4 parentMatrix, bool forceRebuild, bool isParentVisible) {

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("RebuildGraphic: " + this.name + " graphic: " + graphic.name + " forceRebuild: " + forceRebuild);
			#endif

			if (graphic == null) return;

			Matrix4x4 matrix = parentMatrix * graphic.transform;
			//Debug.Log("Rebuilding Graphic:"  + graphic.name + " : " + matrix);

			// Rebuild Shape
			if (graphic.styles != null) {
				int styleCount = graphic.styles.Count;
				for (int i=0; i<styleCount; i++) {
					RebuildShapeStyle(graphic, graphic.styles[i], matrix, forceRebuild, isParentVisible);
				}
			}

			// Iterate Group Children
			if (graphic is LW_Group) {
				LW_Group group = graphic as LW_Group;
				int groupCount = group.Count;
				for (int i=0; i<groupCount; i++) {
					RebuildGraphic(group[i], matrix, forceRebuild, isParentVisible && group.isVisible);
				}
			}

		}
		private void RebuildShapeStyle(LW_Graphic graphic, LW_Style style, Matrix4x4 matrix, bool forceRebuild, bool isParentVisible) {

			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("RebuildShapeStyle: " + this.name + " graphic: " + graphic.name + " style: " + style.name + " forceRebuild: " + forceRebuild);
			#endif

			if (style == null) return;

			//If we wanted to do passdown inheritance this is where we'd do it.
			//style.InheritPropertiesFrom(this);

			m_Buffer = null;
			if (isParentVisible && graphic.isVisible && graphic.styles.isVisible && style.isVisible) {
				if (style is LW_PaintStyle) m_Buffer = LW_BufferPool<LW_VertexBuffer>.Get(graphic, style, this);
				else if (style is LW_Marker) m_Buffer = LW_BufferPool<LW_MarkerBuffer>.Get(graphic, style, this);
				else if (style is LW_Collider) m_Buffer = LW_BufferPool<LW_ColliderBuffer>.Get(graphic, style, this);
			}

			if (m_Buffer != null) {
				s_BuffersDirty = true;
				if (graphic.isDirty || graphic.styles.isDirty || style.isDirty || forceRebuild || m_Buffer.isEmpty) {
					m_Buffer.Rebuild(graphic, style, this, forceRebuild);
				}

				if (!m_Buffer.isEmpty) {
					
					if (m_Buffer is LW_VertexBuffer) {
						LW_VertexBuffer vBuffer = m_Buffer as LW_VertexBuffer;
						vBuffer.matrix = matrix;
						m_VertexCount += vBuffer.vertexCount;
						m_IndexCount += vBuffer.indexCount;
						
						m_VertexBufferList.Add(vBuffer);
					}
					else if (m_Buffer is LW_MarkerBuffer) {
						LW_MarkerBuffer mBuffer = m_Buffer as LW_MarkerBuffer;
						mBuffer.matrix = matrix;
						for (int i=0; i<mBuffer.graphicList.Count; i++) {
							RebuildGraphic(mBuffer.graphicList[i], matrix, forceRebuild, isParentVisible);
						}
						
						m_MarkerBufferList.Add(mBuffer);
					}
					else if (m_Buffer is LW_ColliderBuffer) {
						LW_ColliderBuffer cBuffer = m_Buffer as LW_ColliderBuffer;
						cBuffer.matrix = matrix;
						
						m_ColliderBufferList.Add(cBuffer);
					}
				}
			}

		}

		private void UpdatePropertiesToMatchMaterial(Material material){
			if (material == null) return;

			m_MainTexture = material.mainTexture as Texture2D;
			m_UvTiling = material.mainTextureScale;
			m_UvOffset = material.mainTextureOffset;
			m_IsLineWorksShader = material.shader.name.StartsWith("LineWorks");
			if (m_IsLineWorksShader){
				m_BlendMode = (BlendMode)material.GetFloat("_BlendMode");
				m_FeatureMode = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat) ? (FeatureMode)material.GetFloat("_FeatureMode") : FeatureMode.Simple;
				m_FeatureMode = (FeatureMode)material.GetFloat("_FeatureMode");
				m_StrokeDrawMode = (StrokeDrawMode)material.GetFloat("_StrokeDrawMode");
				m_StrokeScaleMode = (StrokeScaleMode)material.GetFloat("_StrokeScaleMode");
				m_AntiAliasingMode = (AntiAliasingMode)material.GetFloat("_AntiAliasingMode");
			}
		}
		private void UpdateMaterialToMatchProperties(Material material){
			if (material == null) return;

			material.mainTexture = m_MainTexture;
			material.mainTextureScale = m_UvTiling;
			material.mainTextureOffset = m_UvOffset;
			bool isLineWorksShader = material.shader.name.StartsWith("LineWorks");
			if (isLineWorksShader){
				material.SetFloat("_BlendMode", (int)m_BlendMode);
				material.SetFloat("_FeatureMode", (int)m_FeatureMode);
				material.SetFloat("_StrokeDrawMode", (int)m_StrokeDrawMode);
				material.SetFloat("_StrokeScaleMode", (int)m_StrokeScaleMode);
				material.SetFloat("_AntiAliasingMode", (int)m_AntiAliasingMode);
			}
		}
		private bool UpdateRenderers() {
			if (isUIElement) {
				if (m_CanvasRenderer == null) m_CanvasRenderer = gameObject.GetComponent<CanvasRenderer>();
				if (m_CanvasRenderer == null) m_CanvasRenderer = gameObject.AddComponent<CanvasRenderer>();

				#if UNITY_EDITOR
				if (m_CanvasRenderer != null) m_CanvasRenderer.hideFlags = s_DebugComponents ? HideFlags.None : HideFlags.HideInInspector;
				#endif

				if (m_MeshFilter != null) m_MeshFilter = LW_Utilities.SafeDestroy<MeshFilter>(m_MeshFilter);
				if (m_MeshRenderer != null) m_MeshRenderer = LW_Utilities.SafeDestroy<MeshRenderer>(m_MeshRenderer);
				if (m_RendererMaterials == null || m_RendererMaterials.Length > 0) m_RendererMaterials = new Material[0];
			}
			else {
				if (m_MeshFilter == null) m_MeshFilter = gameObject.GetComponent<MeshFilter>();
				if (m_MeshFilter == null) m_MeshFilter = gameObject.AddComponent<MeshFilter>();
				if (m_MeshRenderer == null) m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
				if (m_MeshRenderer == null) m_MeshRenderer = gameObject.AddComponent<MeshRenderer>();

				#if UNITY_EDITOR
				if (m_MeshFilter != null) m_MeshFilter.hideFlags = s_DebugComponents ? HideFlags.None : HideFlags.HideInInspector;
				if (m_MeshRenderer != null) m_MeshRenderer.hideFlags = s_DebugComponents ? HideFlags.None : HideFlags.HideInInspector;
				EditorUtility.SetSelectedWireframeHidden(m_MeshRenderer, !s_DebugComponents);
				#endif

				if (m_CanvasRenderer != null) {
					#if UNITY_EDITOR
					m_CanvasRenderer.hideFlags = s_DebugComponents ? HideFlags.None : HideFlags.HideInInspector;
					#endif
					m_CanvasRenderer.Clear();
					m_CanvasRenderer.materialCount = 0;
				}
			}

			if (!IsActive() || m_VertexBufferList == null || m_VertexBufferList.Count == 0 || m_VertexCount > 65000) {
				if (m_VertexCount > 65000) Debug.Log("LineWork Canvas exceeds 65000 vertex limit");

				if (isUIElement) {
					if (m_CanvasRenderer != null) m_CanvasRenderer.Clear();
				}
				else {
					if (m_RendererMaterials == null || m_RendererMaterials.Length > 0) m_RendererMaterials = new Material[0];
					if (m_MeshRenderer != null && m_MeshRenderer.sharedMaterials != null) m_MeshRenderer.sharedMaterials = m_RendererMaterials;
					if (m_MeshFilter != null && m_MeshFilter.sharedMesh != null) m_MeshFilter.sharedMesh.Clear();
				}
				return true;
			}
			else 
				return false;
		}

		internal void RegisterCallbacks(UnityAction scalerDirtyAction = null, UnityAction elementDirtyAction = null, UnityAction verticesDirtyAction = null, UnityAction materialsDirtyAction = null) {
			if (scalerDirtyAction != null) m_OnDirtyScalerCallback += scalerDirtyAction;
			if (elementDirtyAction != null) m_OnDirtyLW_CanvasCallback += elementDirtyAction;
			if (verticesDirtyAction != null) m_OnDirtyVertsCallback += verticesDirtyAction;
			if (materialsDirtyAction != null) m_OnDirtyMaterialCallback += materialsDirtyAction;
			RegisterChildren();
		}
		internal void UnregisterCallbacks(UnityAction scalerDirtyAction = null, UnityAction elementDirtyAction = null, UnityAction verticesDirtyAction = null, UnityAction materialsDirtyAction = null) {
			if (scalerDirtyAction != null) m_OnDirtyScalerCallback -= scalerDirtyAction;
			if (elementDirtyAction != null) m_OnDirtyLW_CanvasCallback -= elementDirtyAction;
			if (verticesDirtyAction != null) m_OnDirtyVertsCallback -= verticesDirtyAction;
			if (materialsDirtyAction != null) m_OnDirtyMaterialCallback -= materialsDirtyAction;
			UnregisterChildren();
		}

		internal void RegisterChildren() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("RegisterChildren LW_Canvas: " + name);
			#endif

			if (m_Graphic != null) m_Graphic.RegisterCallbacks(SetBuffersDirty);
		}
		internal void UnregisterChildren() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("UnregisterChildren LW_Canvas: " + name);
			#endif

			if (m_Graphic != null) m_Graphic.UnregisterCallbacks(SetBuffersDirty);
		}

		#region Unity Graphic stuff
		public override void Rebuild(CanvasUpdate update) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugCanvas) Debug.Log("Rebuild: " + update.ToString() + " transformDirty: " + m_TransformDirty + " scalerDirty: " + m_ScalerDirty + " materialDirty: " + m_MaterialDirty + " lineworkDirty: " + m_BuffersDirty + " vertsDirty: " + m_VertsDirty + " forceRebuild: " + m_ForceRebuild);
			#endif

			if (m_TransformDirty) {
				m_Root = transform.parent != null ? transform.parent.GetComponentInParent<LW_Canvas>() : this;
				m_UICanvas = transform.parent != null ? transform.parent.GetComponentInParent<Canvas>() : null;
			}
			if (isUIElement && m_CanvasRenderer != null && m_CanvasRenderer.cull) return;

			switch (update) {
				case CanvasUpdate.PostLayout:
					break;
				case CanvasUpdate.PreRender:
					if (m_ScalerDirty) UpdateScaler();
					if (m_BuffersDirty) UpdateBuffers();
					if (m_MaterialDirty) UpdateMaterial();
					if (m_VertsDirty) UpdateGeometry();
					if (m_VertsDirty) UpdateColliders();
					break;
				case CanvasUpdate.LatePreRender:
					SetClean();
					break;
			}

			#if UNITY_EDITOR
			SceneView.RepaintAll();
			#endif
		}
		#endregion

		#region ILayoutElement
		public virtual void CalculateLayoutInputHorizontal() { }
		public virtual void CalculateLayoutInputVertical() { }
		public virtual float minWidth { get { return 0; } }
		public virtual float preferredWidth	{
			get	{
				return m_ViewBox.size.x / pixelsPerUnit;
			}
		}
		public virtual float flexibleWidth { get { return -1; } }
		public virtual float minHeight { get { return 0; } }
		public virtual float preferredHeight {
			get {
				return m_ViewBox.size.y / pixelsPerUnit;
			}
		}
		public virtual float flexibleHeight { get { return -1; } }
		public virtual int layoutPriority { get { return 0; } }
		#endregion

		#region ICanvasRaycastFilter
		public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
			if (m_VertexBufferList != null && m_VertexBufferList.Count > 0) {
				if (!m_UseAccurateRaycasting) return true;
				else {
					Vector2 localPos;
					RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out localPos);

					for (int c=0; c<m_VertexBufferList.Count; c++) {
						LW_VertexBuffer vBuffer = m_VertexBufferList[c];
						if (!vBuffer.isValid || vBuffer.positions.Count < 3) continue;

						bool isStroke = (vBuffer.style is LW_Stroke);
						bool isDynamicStroke = isStroke && vBuffer.material.GetFloat("_FeatureMode") == 1.0f;
						LW_Stroke stroke = isStroke ? vBuffer.style as LW_Stroke : null;
						Matrix4x4 matrix = m_Scaler * vBuffer.matrix;
						Vector3 scale = matrix.ExtractScaleFromMatrix();
						if (scale == Vector3.zero || vBuffer.positions.Count < 3) continue;
						Vector2 scalePos = matrix.inverse.MultiplyPoint3x4(localPos);
						Bounds bounds = vBuffer.bounds;
						if (isStroke) {
							float width = stroke.MaxWidth();
							Vector2 size = bounds.size;
							size.x += width;
							size.y += width;
							bounds.size = size;
						}
						if (bounds.Contains(scalePos)) {
							for (int t=0; t<vBuffer.indices.Count-2; t+=3) {
								Vector2 v0 = vBuffer.positions[vBuffer.indices[t+0]];
								Vector2 v1 = vBuffer.positions[vBuffer.indices[t+1]];
								Vector2 v2 = vBuffer.positions[vBuffer.indices[t+2]];
								if (isDynamicStroke) {
									Vector2 b0 = vBuffer.uv1s[vBuffer.indices[t+0]].x.UnpackVector2(-2,2);
									Vector2 b1 = vBuffer.uv1s[vBuffer.indices[t+1]].x.UnpackVector2(-2,2);
									Vector2 b2 = vBuffer.uv1s[vBuffer.indices[t+2]].x.UnpackVector2(-2,2);
									float w0 = stroke.WidthAtPercentage(Mathf.Abs(b0.x)-1) * 0.5f;
									float w1 = stroke.WidthAtPercentage(Mathf.Abs(b1.x)-1) * 0.5f;
									float w2 = stroke.WidthAtPercentage(Mathf.Abs(b2.x)-1) * 0.5f;
									Vector2 o0 = vBuffer.normals[vBuffer.indices[t+0]] * Mathf.Sign(b0.y);
									Vector2 o1 = vBuffer.normals[vBuffer.indices[t+1]] * Mathf.Sign(b1.y);
									Vector2 o2 = vBuffer.normals[vBuffer.indices[t+2]] * Mathf.Sign(b2.y);
									v0 += o0 * w0;
									v1 += o1 * w1;
									v2 += o2 * w2;
								}
								Vector2 min = new Vector2 (Mathf.Min(v0.x, v1.x, v2.x), Mathf.Min(v0.y, v1.y, v2.y));
								Vector2 max = new Vector2 (Mathf.Max(v0.x, v1.x, v2.x), Mathf.Max(v0.y, v1.y, v2.y));
								if (scalePos.x > min.x && scalePos.x <= max.x && scalePos.y > min.y && scalePos.y < max.y) {
									if (PointInTriangle(scalePos, v0, v1, v2)) return true;
								}
							}
						}
					}

					return false;
				}
			}
			return false;
		}
		private bool PointInTriangle (Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3) {
			bool b1, b2, b3;

			b1 = sign(pt, v1, v2) < 0.0f;
			b2 = sign(pt, v2, v3) < 0.0f;
			b3 = sign(pt, v3, v1) < 0.0f;

			return ((b1 == b2) && (b2 == b3));
		}
		private float sign (Vector3 p1, Vector3 p2, Vector3 p3) {
			return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
		}
		#endregion
	}
}