// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LineWorks {

	#if UNITY_EDITOR
	[InitializeOnLoad]
	#endif
	public static class LW_MaterialPool {

		#if UNITY_EDITOR || DEVELOPMENT
		private static readonly bool s_DebugMaterialPool = false;
		#endif

		public static Dictionary<Material, LW_MaterialBuffer> materialDict { get { return s_MaterialDict; } }
		private static readonly Dictionary<Material, LW_MaterialBuffer> s_MaterialDict = new Dictionary<Material, LW_MaterialBuffer>();

		private static Shader defaultShader {
			get {
				if (s_DefaultShader == null) s_DefaultShader = Shader.Find("LineWorks/Default");
				return s_DefaultShader;
			}
		}
		private static Shader s_DefaultShader;

		private static bool s_MaterialPoolDirty = false;
		private static int s_MaterialNameCount = 0;
		private static Material s_LastMaterial = null;
		private static Stack<Texture2D> s_DirtyTextures = new Stack<Texture2D>();
		private static List<Material> s_ToRemoveFromDict = new List<Material>();
		private static int s_GradientDataLength = 8;
		private static int s_GradientPrecision = 24;

		#if UNITY_EDITOR
		static LW_MaterialPool() {
			if (s_DebugMaterialPool) Debug.Log("LW_MaterialPool Static Constructor");
			Clean();
			EditorApplication.hierarchyWindowChanged += OnHierarchyChange;
		}
		static void OnHierarchyChange() {
			if (s_DebugMaterialPool) Debug.Log("LW_MaterialPool OnHierarchyChanged");
			Clean();
		}
		#endif

		public static void SetDirty() {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugMaterialPool) Debug.Log("LW_MaterialPool SetDirty");
			#endif
			s_MaterialPoolDirty = true;
		}
		public static void Clear() {
			if (s_MaterialDict.Count == 0) return;
			foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
				kvp.Value.Dispose();
				Material.Destroy(kvp.Key);
			}
			s_MaterialDict.Clear();
			s_DefaultShader = null;
			s_MaterialPoolDirty = false;
			s_MaterialNameCount = 0;
			s_LastMaterial = null;
			s_DirtyTextures.Clear();
			s_ToRemoveFromDict.Clear();
			s_GradientDataLength = 8;
			s_GradientPrecision = 24;
		}
		public static void Clean() {
			s_LastMaterial = null;

			if (s_DirtyTextures.Count > 0) {
				while (s_DirtyTextures.Count > 0) s_DirtyTextures.Pop().Apply();
			}

			if (s_MaterialPoolDirty) {
				#if UNITY_EDITOR || DEVELOPMENT
				if (s_DebugMaterialPool) Debug.Log("Clean MaterialPool");
				#endif

				s_ToRemoveFromDict.Clear();

				foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
					Material poolMat = kvp.Key;
					LW_MaterialBuffer materialBuffer = kvp.Value;
					List<LW_VertexBuffer> vertexBuffers = materialBuffer.vertexBuffers;
					Stack<int> emptyIndices = materialBuffer.emptyIndices;

					if (poolMat == null || vertexBuffers.Count == 0 || emptyIndices.Count == vertexBuffers.Count) {
						s_ToRemoveFromDict.Add(poolMat);
					}
					else {
						bool removeMaterialFromDict = true;

						for (int i=0; i<vertexBuffers.Count; i++) {
							if (emptyIndices.Contains(i)) 
								continue;
							
							LW_VertexBuffer buffer = vertexBuffers[i];
							if (buffer.isValid && MaterialMatchesBuffer(poolMat, buffer) && poolMat.renderQueue == buffer.renderQueue)
								removeMaterialFromDict = false;
							else {
								#if UNITY_EDITOR || DEVELOPMENT
								if (s_DebugMaterialPool) Debug.Log("Cleaning MaterialPool: vBuffer: " + i);
								#endif

								emptyIndices.Push(i);
							}
						}

						if (removeMaterialFromDict) s_ToRemoveFromDict.Add(poolMat);
					}
				}

				for (int i=0; i<s_ToRemoveFromDict.Count; i++) {
					if (s_MaterialDict.ContainsKey(s_ToRemoveFromDict[i])) s_MaterialDict.Remove(s_ToRemoveFromDict[i]);
				}

				s_MaterialPoolDirty = false;
			}
		}

		public static Material Get(LW_VertexBuffer buffer) {
			if (!buffer.isValid) 
				return null;

			// Setup
			Material material = null;

			// Check for Custom Materials
			LW_PaintStyle style = buffer.style as LW_PaintStyle;
			if (style.material != null) {
				material = style.material;
				buffer.renderQueue = material.renderQueue;
			}
			else if (buffer.canvas.material != null) {
				material = buffer.canvas.material;
				buffer.renderQueue = material.renderQueue;
			}

			// Check Last Material and Update RenderQue If needed.
			if (material == null) {
				buffer.renderQueue = GetRenderQueue(buffer);

				if (s_LastMaterial != null && buffer.renderQueue < s_LastMaterial.renderQueue)
					buffer.renderQueue = s_LastMaterial.renderQueue+1;

				if (s_LastMaterial != null && MaterialMatchesBuffer(s_LastMaterial, buffer))
					material = s_LastMaterial;
			}

			// Check Material Dictionary for Match
			if (material == null) {
				foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
					if (kvp.Key == null) 
						continue;

					Material poolMat = kvp.Key;

					if (MaterialMatchesBuffer(poolMat, buffer) && poolMat.renderQueue == buffer.renderQueue) {
						material = kvp.Key;
						break;
					}
				}
			}
				
			// Check Material Dictionary for Unused Material
			if (material == null) {
				foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
					if (kvp.Key == null) 
						continue;

					Material poolMat = kvp.Key;
					List<LW_VertexBuffer> bufferList = kvp.Value.vertexBuffers;

					if (bufferList == null || bufferList.Count == 0 || (bufferList.Count == 1 && bufferList[0] == buffer)) {
						material = poolMat;
						MatchMaterialToBuffer(material, buffer);
						break;
					}
				}
			}

			// Create New Material
			if (material == null) {
				Shader shader = defaultShader;
				if (shader == null) {
					Debug.Log("LineWorks Shader: \"LineWorks/Default\" is Missing. Make sure required LineWorks Shader is included in the scene or in a Resources Folder. Using UI/Default shader but Vector Graphics may not display correctly.");
					shader = Shader.Find("UI/Default");
				}
				material = new Material(shader);
				material.hideFlags = HideFlags.DontSave;
				material.name = string.Format("LineWorks Material {0}", s_MaterialNameCount++);
				MatchMaterialToBuffer(material, buffer);
			}

			//MatchMaterialToBuffer(material, buffer);

			// Add Material to Dictionary if needed.
			LW_MaterialBuffer materialBuffer = null;
			if (s_MaterialDict.ContainsKey(material)) {
				materialBuffer = s_MaterialDict[material];
				List<LW_VertexBuffer> vertexBuffers = materialBuffer.vertexBuffers;
				Stack<int> emptyIndices = materialBuffer.emptyIndices;

				if (!vertexBuffers.Contains(buffer)) {
					if (emptyIndices.Count > 0) vertexBuffers[emptyIndices.Pop()] = buffer;
					else vertexBuffers.Add(buffer);
					buffer.styleDataIndex = -1;
				}
			}
			else {
				materialBuffer = new LW_MaterialBuffer();
				materialBuffer.vertexBuffers.Add(buffer);
				s_MaterialDict.Add(material, materialBuffer);
				buffer.styleDataIndex = -1;
			}

			materialBuffer.isDirty = !buffer.isValid || buffer.style.isDirty || buffer.styleDataIndex == -1;
			materialBuffer.isLineWorksShader = material.shader.name.StartsWith("LineWorks");
			materialBuffer.isAdvancedShader = materialBuffer.isLineWorksShader && material.IsKeywordEnabled("_ADVANCED_ON");

			// Finish
			s_LastMaterial = material;

			return material;
		}
		public static void SetMaterialKeywords(Material material) {
			#if UNITY_EDITOR || DEVELOPMENT
			if (s_DebugMaterialPool) Debug.Log("SetMaterialKeywords for material: " + material.name);
			#endif

			if (s_MaterialDict.ContainsKey(material))
				s_MaterialDict[material].isDirty = true;

			switch ((BlendMode)material.GetFloat("_BlendMode")) {
			case BlendMode.Opaque:
				material.SetOverrideTag("RenderType", "Opaque");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_BLEND_ALPHATEST");
				material.DisableKeyword("_BLEND_ALPHABLEND");
				material.DisableKeyword("_BLEND_UI");
				material.DisableKeyword("_BLEND_ADDITIVESOFT");
				material.renderQueue = 2000;
				break;
			case BlendMode.AlphaTest:
				material.SetOverrideTag("RenderType", "TransparentCutout");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 0);
				material.EnableKeyword("_BLEND_ALPHATEST");
				material.DisableKeyword("_BLEND_ALPHABLEND");
				material.DisableKeyword("_BLEND_UI");
				material.DisableKeyword("_BLEND_ADDITIVESOFT");
				material.renderQueue = 2450;
				break;
			case BlendMode.AlphaBlend:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_BLEND_ALPHATEST");
				material.EnableKeyword("_BLEND_ALPHABLEND");
				material.DisableKeyword("_BLEND_UI");
				material.DisableKeyword("_BLEND_ADDITIVESOFT");
				material.renderQueue = 3000;
				break;
			case BlendMode.UI:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_BLEND_ALPHATEST");
				material.DisableKeyword("_BLEND_ALPHABLEND");
				material.EnableKeyword("_BLEND_UI");
				material.DisableKeyword("_BLEND_ADDITIVESOFT");
				material.renderQueue = 3000;
				break;
			case BlendMode.AdditiveSoft:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_BLEND_ALPHATEST");
				material.DisableKeyword("_BLEND_ALPHABLEND");
				material.DisableKeyword("_BLEND_UI");
				material.EnableKeyword("_BLEND_ADDITIVESOFT");
				material.renderQueue = 3000;
				break;
			}

			if (material.mainTexture == null && material.GetTexture("_CapTex") == null && material.GetTexture("_JoinTex") == null) material.DisableKeyword("_TEXTURES_ON");
			else material.EnableKeyword("_TEXTURES_ON");

			if ((FeatureMode)material.GetFloat("_FeatureMode") == FeatureMode.Simple || !SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat)) {
				
				material.DisableKeyword("_ADVANCED_ON");
				material.DisableKeyword("_STROKE_3D_MS");
				material.DisableKeyword("_SCALE_STROKES_ON");
				material.DisableKeyword("_JOINSCAPS_ON");
				material.DisableKeyword("_GRADIENTS_ON");
				material.DisableKeyword("_ANTIALIAS_ON");
			}
			else {
				material.EnableKeyword("_ADVANCED_ON");

				switch((StrokeDrawMode)material.GetFloat("_StrokeDrawMode")) {
				case StrokeDrawMode.Draw2D:
					material.DisableKeyword("_STROKE_3D");
					break;
				case StrokeDrawMode.Draw3D:
					material.EnableKeyword("_STROKE_3D");
					break;
				}

				switch((StrokeScaleMode)material.GetFloat("_StrokeScaleMode")) {
				case StrokeScaleMode.Scaled:
					material.DisableKeyword("_STROKE_UNSCALED");
					material.DisableKeyword("_STROKE_SCREENSPACE");
					break;
				case StrokeScaleMode.Unscaled:
					material.EnableKeyword("_STROKE_UNSCALED");
					material.DisableKeyword("_STROKE_SCREENSPACE");
					break;
				case StrokeScaleMode.ScreenSpace:
					material.DisableKeyword("_STROKE_UNSCALED");
					material.EnableKeyword("_STROKE_SCREENSPACE");
					break;
				}

				switch((JoinsAndCapsMode)material.GetFloat("_JoinsAndCapsMode")) {
				case JoinsAndCapsMode.Vertex:
					material.DisableKeyword("_JOINSCAPS_ON");
					break;
				case JoinsAndCapsMode.Shader:
					material.EnableKeyword("_JOINSCAPS_ON");
					break;
				}

				switch((GradientsMode)material.GetFloat("_GradientsMode")) {
				case GradientsMode.Vertex:
					material.DisableKeyword("_GRADIENTS_ON");
					break;
				case GradientsMode.Shader:
					material.EnableKeyword("_GRADIENTS_ON");
					break;
				}

				switch((AntiAliasingMode)material.GetFloat("_AntiAliasingMode")) {
				case AntiAliasingMode.Off:
					material.DisableKeyword("_ANTIALIAS_ON");
					break;
				case AntiAliasingMode.On:
					material.EnableKeyword("_ANTIALIAS_ON");
					break;
				}
			}

			#if UNITY_EDITOR
			//UnityEditor.SceneView.RepaintAll();
			#endif
		}
		public static void UpdateMaterials(bool forceRebuild = false) {
			foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
				RebuildStyleData(kvp.Key, kvp.Value, forceRebuild);
			}
		}

		private static void MatchMaterialToBuffer(Material material, LW_VertexBuffer buffer) {

			// Setup
			LW_PaintStyle style = buffer.style as LW_PaintStyle;
			LW_Canvas canvas = buffer.canvas;

			Material customMaterial = style.material != null ? style.material : (canvas.material != null ? canvas.material : null);
			bool isLineWorksShader = customMaterial == null || customMaterial.shader.name.StartsWith("LineWorks");
			bool isCustomMaterial = customMaterial != null;

			// Update LineWorks Material to Match Buffer
			if (isLineWorksShader && !isCustomMaterial) {
				Shader shader = defaultShader;
				if (material.shader != shader) material.shader = shader;

				// this was from a previous version where the width and color property were directly applied to the material.
				//if (style is LW_Stroke) {
				//	LW_Stroke stroke = style as LW_Stroke;
				//	if (material.GetFloat("_Width") != stroke.widthMultiplier) material.SetFloat("_Width", stroke.widthMultiplier);
				//}
				//if (material.GetColor("_Color") != style.color) material.SetColor("_Color", style.color);

				Texture2D texture = style.mainTexture != null ? style.mainTexture : canvas.mainTexture != null ? canvas.mainTexture : null;
				Vector2 uvOffset = style.mainTexture != null ? style.uvOffset : Vector2.zero;
				Vector2 uvTiling = style.mainTexture != null ? style.uvTiling : Vector2.one;

				if (material.GetTexture("_MainTex") != texture) 
					material.SetTexture("_MainTex", texture);
				if (material.GetTextureOffset("_MainTex") != uvOffset) 
					material.SetTextureOffset("_MainTex", uvOffset);
				if (material.GetTextureScale("_MainTex") != uvTiling) 
					material.SetTextureScale("_MainTex", uvTiling);

				if (style is LW_Stroke) {
					LW_Stroke stroke = style as LW_Stroke;
					if (stroke.linecap == Linecap.Texture && stroke.capTexture != null && material.GetTexture("_CapTex") != stroke.capTexture)
						material.SetTexture("_CapTex", stroke.capTexture);
					if (stroke.linejoin == Linejoin.Texture && stroke.joinTexture != null && material.GetTexture("_JoinTex") != stroke.joinTexture)
						material.SetTexture("_JoinTex", stroke.joinTexture);
				}

				SetMaterialPopups(material, buffer);
				SetMaterialKeywords(material);
			}

			material.renderQueue = buffer.renderQueue;

			//if (isAdvancedShader) RebuildStyleData(material, s_MaterialDict[material]);
		}

		private static void RebuildStyleData(Material material, LW_MaterialBuffer materialBuffer, bool forceRebuild = false) {
			//Debug.Log("RebuildStyleData material: " + material.name);

				if (!(material != null && materialBuffer != null && materialBuffer.vertexBuffers != null && materialBuffer.isAdvancedShader && (forceRebuild  || materialBuffer.isDirty)))
				return;

			List<LW_VertexBuffer> vertexBuffers = materialBuffer.vertexBuffers;
			Stack<int> emptyIndices = materialBuffer.emptyIndices;

			// Setup
			int width = s_GradientDataLength + s_GradientPrecision;
			int height = vertexBuffers.Count;
			bool hasChanged = false;
			bool rebuildAllRows = forceRebuild;
			Texture2D styleData = material.GetTexture("_StyleData") as Texture2D;

			// Resizing StyleData
			if (styleData != null) {
				if (styleData.height < height) {
					int oldHeight = styleData.height;
					int oldWidth = styleData.width;
					Color[] pixels = styleData.GetPixels();
					styleData.Resize(width, height, TextureFormat.RGBAFloat, false);
					styleData.SetPixels(0,0, oldWidth, oldHeight, pixels, 0);
					material.SetVector("_StyleDataSize", new Vector4(width, height, s_GradientDataLength, s_GradientPrecision));
					hasChanged = true;
				}
				else if (styleData.height > height) 
					styleData = null;
			}

			// Creating StyleData
			if (styleData == null) {
				styleData = new Texture2D (width, height, TextureFormat.RGBAFloat, false);
				styleData.hideFlags = HideFlags.DontSave;
				styleData.filterMode = FilterMode.Bilinear;
				styleData.wrapMode = TextureWrapMode.Clamp;
				styleData.anisoLevel = 0;
				material.SetTexture("_StyleData", styleData);
				material.SetVector("_StyleDataSize", new Vector4(width, height, s_GradientDataLength, s_GradientPrecision));
				hasChanged = true;
				rebuildAllRows = true;
			}

			// Updating StyleData
			for (int i=0; i<vertexBuffers.Count; i++) {
				int index = i;

				LW_VertexBuffer buffer = vertexBuffers[i];


				if (buffer.isEmpty || emptyIndices.Contains(i) || (buffer.styleDataIndex == index && !buffer.style.isDirty && !rebuildAllRows))
					continue;

				//Debug.Log("buffer.styleDataIndex = " + index);

				hasChanged = true; 
				buffer.styleDataIndex = index;

				//LW_Graphic graphic = buffer.graphic;
				//LW_Canvas canvas = buffer.canvas;
				LW_PaintStyle style = buffer.style as LW_PaintStyle;

				float boundsMin = Mathf.Min(buffer.bounds.min.x, buffer.bounds.min.y);
				float boundsMax = Mathf.Max(buffer.bounds.max.x, buffer.bounds.max.y);

				float strokeJustification = -1;
				float strokeWidth = 0;
				float strokeLength = 0;
				float screenSpaceMiterLimit = 0;
				float caps = -1;
				float joins = -1;

				// Vertex Data
				if (style is LW_Stroke) {
					LW_Stroke stroke = style as LW_Stroke;
					strokeJustification = ((int)stroke.justification-1);
					strokeWidth = stroke.MaxWidth();
					strokeLength = buffer.totalLength;
					screenSpaceMiterLimit = stroke.screenSpace ? stroke.miterLimit : -stroke.miterLimit;
					caps = (int)stroke.linecap;
					joins = (int)stroke.linejoin;
				}

				if (style is LW_Fill) {
					strokeWidth = buffer.bounds.size.y;
					strokeLength = buffer.bounds.size.x;
					caps = (int)Linecap.Round;
					joins = (int)Linejoin.Round;
				}

				// Fragment Data
				Matrix4x4 gradTransform = style.gradientUnits == GradientUnits.objectBoundingBox ?
					style.BoundsToLocalMatrix(buffer.bounds) * style.gradientTransform * style.LocalToBoundsMatrix(buffer.bounds) :
					style.gradientTransform;
				Vector2 start = gradTransform.MultiplyPoint(style.gradientStart);
				Vector2 end = gradTransform.MultiplyPoint(style.gradientEnd);
				Vector2 gradientDirection = end - start;
				float gradAngle = Mathf.Atan2(gradientDirection.x, gradientDirection.y);
				float gradScale = gradientDirection.magnitude;

				styleData.SetPixel(0, index, new Color(screenSpaceMiterLimit, strokeJustification, strokeWidth, strokeLength));
				styleData.SetPixel(1, index, new Color((int)style.uvMode, (int)style.gradientUnits, boundsMin, boundsMax));
				styleData.SetPixel(2, index, new Color(start.x, start.y, (int)style.gradientSpreadMethod, (int)style.paintMode));
				styleData.SetPixel(3, index, new Color(gradAngle, gradScale, caps, joins));

				for (int p=0; p<s_GradientPrecision+1; p++) {
					float percentage = (float)p/(float)s_GradientPrecision;
					Color color = style.ColorAtPercentage(percentage);
					styleData.SetPixel(s_GradientDataLength+p, i, color);
				}
			}

			if (hasChanged) {
				if (!s_DirtyTextures.Contains(styleData)) s_DirtyTextures.Push(styleData);
			}

			materialBuffer.isDirty = false;
		}
		private static void SetMaterialPopups(Material material, LW_VertexBuffer buffer) {
			LW_Canvas canvas = buffer.canvas;

			material.SetFloat("_BlendMode", (int)canvas.blendMode);
			material.SetFloat("_FeatureMode", (int)canvas.featureMode);
			material.SetFloat("_StrokeDrawMode", (int)canvas.strokeDrawMode);
			material.SetFloat("_StrokeScaleMode", (int)canvas.strokeScaleMode);
			material.SetFloat("_JoinsAndCapsMode", (int)canvas.joinsAndCapsMode);
			material.SetFloat("_GradientsMode", (int)canvas.gradientsMode);
			material.SetFloat("_AntiAliasingMode", (int)canvas.antiAliasingMode);
		}
		private static int  GetRenderQueue(LW_VertexBuffer buffer) {
			switch(buffer.canvas.blendMode) {
			case BlendMode.Opaque:
				return 2000;
			case BlendMode.AlphaTest:
				return 2450;
			case BlendMode.AlphaBlend:
				return 3000;
			case BlendMode.UI:
				return 3000;
			}
			return 2000;
		}
		private static bool MaterialMatchesBuffer(Material material, LW_VertexBuffer buffer) {
			LW_PaintStyle style = buffer.style as LW_PaintStyle;
			LW_Canvas canvas = buffer.canvas;

			// Get Custom Material
			Material customMaterial = style.material != null ? style.material : (canvas.material != null ? canvas.material : null);
			bool isLineWorksShader = customMaterial == null || customMaterial.shader.name.StartsWith("LineWorks");

			// Get Buffer Textures
			Texture bufferTexture = style.mainTexture != null ? style.mainTexture : canvas.mainTexture != null ? canvas.mainTexture : null;
			Vector2 bufferUvOffset = style.mainTexture != null ? style.uvOffset : Vector2.zero;
			Vector2 bufferUvTiling = style.mainTexture != null ? style.uvTiling : Vector2.one;

			// Get Material Textures
			Texture materialTexture = isLineWorksShader ? material.GetTexture("_MainTex") : null;
			Vector2 materialUvOffset = isLineWorksShader ? material.GetTextureOffset("_MainTex") : Vector2.zero;
			Vector2 materialUvTiling = isLineWorksShader ? material.GetTextureScale("_MainTex") : Vector2.one;

			Texture bufferCapTexture = null;
			Texture bufferJoinTexture = null;
			if (style is LW_Stroke) {
				LW_Stroke stroke = style as LW_Stroke;
				bufferCapTexture = stroke.linecap == Linecap.Texture ? stroke.capTexture : null;
				bufferJoinTexture = stroke.linejoin == Linejoin.Texture ? stroke.joinTexture : null;
			}
			Texture materialCapTexture = isLineWorksShader && material.HasProperty("_CapTex") ? material.GetTexture("_CapTex") : null;
			Texture materialJoinTexture = isLineWorksShader && material.HasProperty("_JoinTex") ? material.GetTexture("_JoinTex") : null;
			// Check for Match

			bool isMatch =
				customMaterial != null && customMaterial == material
				|| (
					customMaterial == null 
					&& bufferCapTexture == materialCapTexture
					&& bufferJoinTexture == materialJoinTexture
					&& bufferTexture == materialTexture 
					&& bufferUvOffset == materialUvOffset
					&& bufferUvTiling == materialUvTiling 
					&& (
						(canvas.blendMode == BlendMode.Opaque && !material.IsKeywordEnabled("_BLEND_ALPHATEST") && !material.IsKeywordEnabled("_BLEND_ALPHABLEND") && !material.IsKeywordEnabled("_BLEND_UI") && !material.IsKeywordEnabled("_BLEND_ADDITIVESOFT"))
						|| (canvas.blendMode == BlendMode.AlphaTest && material.IsKeywordEnabled("_BLEND_ALPHATEST"))
						|| (canvas.blendMode == BlendMode.AlphaBlend && material.IsKeywordEnabled("_BLEND_ALPHABLEND"))
						|| (canvas.blendMode == BlendMode.UI && material.IsKeywordEnabled("_BLEND_UI"))
						|| (canvas.blendMode == BlendMode.AdditiveSoft && material.IsKeywordEnabled("_BLEND_ADDITIVESOFT"))
					) 
					&& (
						(
							(canvas.featureMode == FeatureMode.Advanced && material.IsKeywordEnabled("_ADVANCED_ON"))
							|| (canvas.featureMode == FeatureMode.Simple && !material.IsKeywordEnabled("_ADVANCED_ON"))
						) && (
							(canvas.strokeDrawMode == StrokeDrawMode.Draw3D && material.IsKeywordEnabled("_STROKE_3D"))
							|| ((canvas.featureMode == FeatureMode.Simple || canvas.strokeDrawMode == StrokeDrawMode.Draw2D) && !material.IsKeywordEnabled("_STROKE_3D"))
						) && (
							((canvas.featureMode == FeatureMode.Simple || canvas.strokeScaleMode == StrokeScaleMode.Scaled) && !material.IsKeywordEnabled("_STROKE_UNSCALED") && !material.IsKeywordEnabled("_STROKE_SCREENSPACE"))
							|| (canvas.strokeScaleMode == StrokeScaleMode.Unscaled && material.IsKeywordEnabled("_STROKE_UNSCALED"))
							|| (canvas.strokeScaleMode == StrokeScaleMode.ScreenSpace && material.IsKeywordEnabled("_STROKE_SCREENSPACE"))
						) && (
							(canvas.joinsAndCapsMode == JoinsAndCapsMode.Shader && material.IsKeywordEnabled("_JOINSCAPS_ON"))
							|| ((canvas.featureMode == FeatureMode.Simple || canvas.joinsAndCapsMode != JoinsAndCapsMode.Shader) && !material.IsKeywordEnabled("_JOINSCAPS_ON"))
						) && (
							(canvas.gradientsMode == GradientsMode.Shader && material.IsKeywordEnabled("_GRADIENTS_ON"))
							|| ((canvas.featureMode == FeatureMode.Simple || canvas.gradientsMode != GradientsMode.Shader) &&  !material.IsKeywordEnabled("_GRADIENTS_ON"))
						) && (
							(canvas.antiAliasingMode == AntiAliasingMode.On && material.IsKeywordEnabled("_ANTIALIAS_ON"))
							|| ((canvas.featureMode == FeatureMode.Simple || canvas.antiAliasingMode != AntiAliasingMode.On) && !material.IsKeywordEnabled("_ANTIALIAS_ON"))
						)
					)
				);
			/*
			Debug.Log(material.name + " == " + buffer.style.name +
				"(" 
				+ (customMaterial != null)
				+ " && " + (customMaterial == material)
				+ ")" 
				+ " || ("
				+ (customMaterial == null)
				+ " && " + (bufferTexture == materialTexture) 
				+ " && " + (bufferUvOffset == materialUvOffset)
				+ " && " + (bufferUvTiling == materialUvTiling)
				+ " && ("
				+ (canvas.blendMode == BlendMode.Opaque && !material.IsKeywordEnabled("_BLEND_ALPHATEST") && !material.IsKeywordEnabled("_BLEND_ALPHABLEND") && !material.IsKeywordEnabled("_BLEND_UI") && !material.IsKeywordEnabled("_BLEND_ADDITIVESOFT"))
				+ " || " + (canvas.blendMode == BlendMode.AlphaTest && material.IsKeywordEnabled("_BLEND_ALPHATEST"))
				+ " || " + (canvas.blendMode == BlendMode.AlphaBlend && material.IsKeywordEnabled("_BLEND_ALPHABLEND"))
				+ " || " + (canvas.blendMode == BlendMode.UI && material.IsKeywordEnabled("_BLEND_UI"))
				+ " || " + (canvas.blendMode == BlendMode.AdditiveSoft && material.IsKeywordEnabled("_BLEND_ADDITIVESOFT"))
				+ ") && ("
				//+ !isAdvancedShader
				//+ " || (("
				+ (canvas.featureMode == FeatureMode.Advanced && material.IsKeywordEnabled("_ADVANCED_ON"))
				+ " || " + (canvas.featureMode == FeatureMode.Simple && !material.IsKeywordEnabled("_ADVANCED_ON"))
				+ ") && ("
				+ (canvas.strokeDrawMode == StrokeDrawMode.Draw3D && material.IsKeywordEnabled("_STROKE_3D"))
				+ " || " + ((canvas.featureMode == FeatureMode.Simple || canvas.strokeDrawMode == StrokeDrawMode.Draw2D) && !material.IsKeywordEnabled("_STROKE_3D"))
				+ ") && ("
				+ ((canvas.featureMode == FeatureMode.Simple || canvas.strokeScaleMode == StrokeScaleMode.Scaled) && !material.IsKeywordEnabled("_STROKE_UNSCALED") && !material.IsKeywordEnabled("_STROKE_SCREENSPACE"))
				+ " || " + (canvas.strokeScaleMode == StrokeScaleMode.Unscaled && material.IsKeywordEnabled("_STROKE_UNSCALED"))
				+ " || " + (canvas.strokeScaleMode == StrokeScaleMode.ScreenSpace && material.IsKeywordEnabled("_STROKE_SCREENSPACE"))
				+ ") && ("
				+ (canvas.joinsAndCapsMode == JoinsAndCapsMode.Shader && material.IsKeywordEnabled("_JOINSCAPS_ON"))
				+ " || " + ((canvas.featureMode == FeatureMode.Simple || canvas.joinsAndCapsMode != JoinsAndCapsMode.Shader) && !material.IsKeywordEnabled("_JOINSCAPS_ON"))
				+ ") && ("
				+ (canvas.gradientsMode == GradientsMode.Shader && material.IsKeywordEnabled("_GRADIENTS_ON"))
				+ " || " + ((canvas.featureMode == FeatureMode.Simple || canvas.gradientsMode != GradientsMode.Shader) &&  !material.IsKeywordEnabled("_GRADIENTS_ON"))
				+ ") && ("
				+ (canvas.antiAliasingMode == AntiAliasingMode.On && material.IsKeywordEnabled("_ANTIALIAS_ON"))
				+ " || " + ((canvas.featureMode == FeatureMode.Simple || canvas.antiAliasingMode != AntiAliasingMode.On) && !material.IsKeywordEnabled("_ANTIALIAS_ON"))
				+ ")))"
			);
			*/
			return isMatch;
		}

		private static void RemoveBufferFromDict(LW_VertexBuffer buffer) {
			foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
				LW_MaterialBuffer materialBuffer = kvp.Value;
				if (materialBuffer.vertexBuffers.Contains(buffer)) {
					int index = materialBuffer.vertexBuffers.IndexOf(buffer);
					if (!materialBuffer.emptyIndices.Contains(index)) materialBuffer.emptyIndices.Push(index);
				}
			}
		}
		private static void RemoveStyleFromDict(LW_Style style) {
			foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
				LW_MaterialBuffer materialBuffer = kvp.Value;
				for (int i=0; i<materialBuffer.vertexBuffers.Count; i++) {
					LW_VertexBuffer buffer = materialBuffer.vertexBuffers[i];
					if (buffer.style == style) {
						if (!materialBuffer.emptyIndices.Contains(i)) materialBuffer.emptyIndices.Push(i);
					}
				}
			}
		}
		private static void RemoveGraphicFromDict(LW_Graphic graphic) {
			foreach(KeyValuePair<Material, LW_MaterialBuffer> kvp in s_MaterialDict) {
				LW_MaterialBuffer materialBuffer = kvp.Value;
				for (int i=0; i<materialBuffer.vertexBuffers.Count; i++) {
					LW_VertexBuffer buffer = materialBuffer.vertexBuffers[i];
					if (buffer.graphic == graphic) {
						if (!materialBuffer.emptyIndices.Contains(i)) materialBuffer.emptyIndices.Push(i);
					}
				}
			}
		}
	}

	#if UNITY_EDITOR
	//[InitializeOnLoad]
	#endif
	public static class LW_BufferPool<T> where T : LW_Buffer, new() {

		public static Dictionary<int, T> bufferDict { get { return s_BufferDict; } }
		private static readonly Dictionary<int, T> s_BufferDict = new Dictionary<int, T>();

		private static bool s_PoolDirty = false;
		private static List<int> s_ToRemoveFromDict = LW_ListPool<int>.Get();

		#if UNITY_EDITOR
		static LW_BufferPool() {
			Clean();
			EditorApplication.hierarchyWindowChanged += OnHierarchyChange;
		}
		static void OnHierarchyChange() {
			Clean();
		}
		#endif

		public static void SetDirty() {
			s_PoolDirty = true;
		}
		public static void Clean() {

			if (s_PoolDirty) {
				s_ToRemoveFromDict.Clear();

				foreach(KeyValuePair<int, T> kvp in s_BufferDict) {
					int key = kvp.Key;
					T buffer = kvp.Value;
					if (!buffer.isValid || buffer.id != key) s_ToRemoveFromDict.Add(key);
				}
				for (int i=0; i<s_ToRemoveFromDict.Count; i++) {
					if (s_BufferDict.ContainsKey(s_ToRemoveFromDict[i])) s_BufferDict.Remove(s_ToRemoveFromDict[i]);
				}
				s_PoolDirty = false;
			}
		}
		public static void Clear() {
			if (s_BufferDict.Count == 0) return;
			foreach(KeyValuePair<int, T> kvp in s_BufferDict) {
				kvp.Value.Dispose();
			}
			s_BufferDict.Clear();
		}

		public static T Get(LW_Graphic graphic, LW_Style style) {
			int key = GetCacheKey(graphic.id, style.id);
			T buffer = Get(key);

			buffer.graphic = graphic;
			buffer.style = style;

			return buffer;
		}
		public static T Get(LW_Graphic graphic, LW_Style style, LW_Canvas canvas) {
			int key = GetCacheKey(graphic.id, style.id, canvas.id);
			T buffer = Get(key);

			buffer.graphic = graphic;
			buffer.style = style;
			buffer.canvas = canvas;

			return buffer;
		}
		public static T Get(int key) {
			T buffer = null;
			if (s_BufferDict.ContainsKey(key)) {
				buffer = s_BufferDict[key];
			}
			else {
				buffer = new T();

				s_BufferDict.Add(key, buffer);
			}

			buffer.id = key;

			s_PoolDirty = true;

			return buffer;
		}

		public static void Release(T buffer) {
			if (s_BufferDict.ContainsKey(buffer.id)) s_BufferDict.Remove(buffer.id);
			buffer.Dispose();
		}

		private static int GetCacheKey(int obj1, int obj2) {
			int key = 1;
			int prime = 31;
			key = prime * key + obj1;
			key = prime * key + obj2;
			return key;
		}
		private static int GetCacheKey(int obj1, int obj2, int obj3) {
			int key = 1;
			int prime = 31;
			key = prime * key + obj1;
			key = prime * key + obj2;
			key = prime * key + obj3;
			return key;
		}
	}

	public static class LW_ListPool<T> {
		private static readonly LW_ObjectPool<List<T>> s_ListPool = new LW_ObjectPool<List<T>>(null, l => l.Clear());
		public static List<T> Get() {
			return s_ListPool.Get();
		}
		public static void Release(List<T> toRelease) {
			s_ListPool.Release(toRelease);
		}
		public static void Clear() {
			s_ListPool.Clear();
		}
	}

	public class LW_ObjectPool<T> : LW_Pool<T> where T : new () {
		public LW_ObjectPool() : base(null, null) {}
		public LW_ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease) : base(actionOnGet, actionOnRelease) {}
		public T Get() {
			T element;
			if (m_Stack.Count == 0) {
				//Debug.Log("Creating New: " + typeof(T).ToString());
				element = new T();
				countAll++;
			}
			else {
				//Debug.Log("Popping Released: " + typeof(T).ToString());
				element = m_Stack.Pop();
			}
			if (m_ActionOnGet != null)
				m_ActionOnGet(element);
			return element;
		}
	}

	public class LW_ElementPool<T> : LW_Pool<T> where T : LW_Element {
		private T m_BaseElement = null;
		public LW_ElementPool() : base(null, null) {}
		public LW_ElementPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease) : base(actionOnGet, actionOnRelease) {}
		public T Get(T element) {
			if (m_BaseElement != null && element != m_BaseElement) Clear();
			m_BaseElement = element;
			T newElement = null;

			if (m_Stack.Count == 0) {
				//Debug.Log("Creating New: " + typeof(T).ToString());
				//newelement = ScriptableObject.CreateInstance<T>();
				newElement = element.Copy() as T;
				countAll++;
			}
			else {
				//Debug.Log("Popping Released: " + typeof(T).ToString());
				newElement = m_Stack.Pop();
				newElement.CopyPropertiesFrom(element);
			}
			if (m_ActionOnGet != null)
				m_ActionOnGet(newElement);
			return newElement;
		}
		public override void Clear() {
			while (m_Stack.Count > 0) LW_Utilities.SafeDestroy(m_Stack.Pop());
			base.Clear();
		}
	}

	public abstract class LW_Pool<T> {
		protected readonly Stack<T> m_Stack = new Stack<T>();
		protected readonly UnityAction<T> m_ActionOnGet;
		protected readonly UnityAction<T> m_ActionOnRelease;

		public int countAll { get; protected set; }
		public int countActive { get { return countAll - countInactive; } }
		public int countInactive { get { return m_Stack.Count; } }

		public LW_Pool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease) {
			m_ActionOnGet = actionOnGet;
			m_ActionOnRelease = actionOnRelease;
		}
		public virtual void Clear() {
			m_Stack.Clear();
			countAll = 0;
		}
		public virtual void Release(T element) {
			if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element)) return;
				//Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
			if (m_ActionOnRelease != null)
				m_ActionOnRelease(element);
			m_Stack.Push(element);
		}
	}
}
