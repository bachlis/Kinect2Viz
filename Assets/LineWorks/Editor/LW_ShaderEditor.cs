// Version 1.1.1
// ©2016 Point Line Plane LLC. All rights reserved. Redistribution of source code without permission not allowed.

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LineWorks;

//namespace LineWorks {
	
public class LW_ShaderEditor : ShaderGUI {

	private static class Styles	{

		public static GUIContent albedoTextureText = new GUIContent("Main Texture", "Albedo (RGB) and Transparency (A)");
		public static GUIContent capTextureText = new GUIContent("Line Caps Texture", "Albedo (RGB) and Transparency (A)");
		public static GUIContent joinTextureText = new GUIContent("Line Joins Texture", "Albedo (RGB) and Transparency (A)");
		public static GUIContent albedoColorText = new GUIContent("Color Multiplier", "Albedo (RGB) and Transparency (A)");
		public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
		public static GUIContent widthMultiplierText = new GUIContent("Width Multiplier", "");
		public static GUIContent styleDataText = new GUIContent("Style Data", "RGBAFloat Format");
		public static GUIContent styleDataSizeText = new GUIContent("Style Data Size", "Used by Shader for Texture Lookup");

		public static GUIContent stencilCompText = new GUIContent("Stencil Comparison", "");
		public static GUIContent stencilIdText = new GUIContent("Stencil ID", "");
		public static GUIContent stencilOpText = new GUIContent("Stencil Operation", "");
		public static GUIContent stencilWriteMaskText = new GUIContent("Stencil Write Mask", "");
		public static GUIContent stencilReadMaskText = new GUIContent("Stencil Read Mask", "");
		public static GUIContent colorMaskText = new GUIContent("Color Mask", "");
		public static GUIContent useUIAlphaClipText = new GUIContent("Use Alpha Clip", "");

		public static string renderSettingsText = "Rendering Settings";
		public static string advancedSettingsText = "Advanced Settings";
		public static string primaryMapsText = "Primary Maps";
		public static string secondaryMapsText = "Style Data (Not set by user)";
		public static string alphaTestText = "Alpha Test";
		public static string uiText = "UI";

		public static string blendingModeText = "Blend Mode";
		public static string featuresModeText = "Features Mode";
		public static string strokeModeText = "Stroke Mode";
		public static string strokeScaleModeText = "Shader Scale Strokes Mode";
		public static string joinsCapsModeText = "Shader Joins/Caps Mode";
		public static string gradientsModeText = "Shader Gradients";
		public static string antiAliasModeText = "AntiAliasing Mode";

		public static GUIContent featuresHelpBoxText = new GUIContent ("Feature Mode must be set to Advanced for more options.");

		public static readonly string[] blendingNames = System.Enum.GetNames (typeof (BlendMode));
		public static readonly string[] featuresNames = System.Enum.GetNames (typeof (FeatureMode));
		public static readonly string[] strokesNames = System.Enum.GetNames (typeof (StrokeDrawMode));
		public static readonly string[] strokeScaleName = System.Enum.GetNames (typeof (StrokeScaleMode));
		public static readonly string[] joinsCapsNames = System.Enum.GetNames (typeof (JoinsAndCapsMode));
		public static readonly string[] gradientsNames = System.Enum.GetNames (typeof (GradientsMode));
		public static readonly string[] antiAliasNames = System.Enum.GetNames (typeof (AntiAliasingMode));
	}

	MaterialProperty blendingMode = null;
	MaterialProperty featuresMode = null;
	MaterialProperty strokeMode = null;
	MaterialProperty scaleStrokesMode = null;
	MaterialProperty joinsCapsMode = null;
	MaterialProperty gradientsMode = null;
	MaterialProperty antiAliasMode = null;
	MaterialProperty albedoMap = null;
	MaterialProperty capMap = null;
	MaterialProperty joinMap = null;
	MaterialProperty albedoColor = null;
	MaterialProperty alphaCutoff = null;
	MaterialProperty widthMultiplier = null;
	MaterialProperty styleDataSize = null;
	MaterialProperty styleDataMap = null;
	MaterialProperty stencilComp = null;
	MaterialProperty stencilId = null;
	MaterialProperty stencilOp = null;
	MaterialProperty stencilWriteMask = null;
	MaterialProperty stencilReadMask = null;
	MaterialProperty colorMask = null;
	MaterialProperty useUIAlphaClip = null;
	MaterialEditor m_MaterialEditor;
	bool m_FirstTimeApply = true;

	public void FindProperties (MaterialProperty[] props) {
		blendingMode = FindProperty ("_BlendMode", props);
		featuresMode = FindProperty ("_FeatureMode", props, false);
		strokeMode = FindProperty ("_StrokeDrawMode", props, false);
		scaleStrokesMode = FindProperty ("_StrokeScaleMode", props, false);
		joinsCapsMode = FindProperty ("_JoinsAndCapsMode", props, false);
		gradientsMode = FindProperty ("_GradientsMode", props, false);
		antiAliasMode = FindProperty ("_AntiAliasingMode", props);
		albedoColor = FindProperty ("_Color", props);
		widthMultiplier = FindProperty ("_Width", props);

		albedoMap = FindProperty ("_MainTex", props);
		capMap = FindProperty ("_CapTex", props);
		joinMap = FindProperty ("_JoinTex", props);
		alphaCutoff = FindProperty ("_Cutoff", props);
		styleDataSize = FindProperty ("_StyleDataSize", props);
		styleDataMap = FindProperty ("_StyleData", props);

		stencilComp = FindProperty ("_StencilComp", props);
		stencilId = FindProperty ("_Stencil", props);
		stencilOp = FindProperty ("_StencilOp", props);
		stencilWriteMask = FindProperty ("_StencilWriteMask", props);
		stencilReadMask = FindProperty ("_StencilReadMask", props);
		colorMask = FindProperty ("_ColorMask", props);
		useUIAlphaClip = FindProperty ("_UseUIAlphaClip", props);

	}

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props) {
		FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		m_MaterialEditor = materialEditor;
		Material material = materialEditor.target as Material;

		ShaderPropertiesGUI (material);

		// Make sure that needed keywords are set up if we're switching some existing
		// material to a standard shader.
		if (m_FirstTimeApply) {
			//LW_MaterialManager.SetKeywords(material);
			//LW_MaterialPool.SetMaterialKeywords(material);
			m_FirstTimeApply = false;
		}
	}

	public void ShaderPropertiesGUI (Material material) {
		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		// Detect any changes to the material
		EditorGUI.BeginChangeCheck();

		// Rendering Settings
		EditorGUILayout.Space();
		GUILayout.Label (Styles.renderSettingsText, EditorStyles.boldLabel);
		ModePopup<BlendMode>(blendingMode, Styles.blendingModeText, Styles.blendingNames);
		ModePopup<FeatureMode>(featuresMode, Styles.featuresModeText, Styles.featuresNames);

		// Advanced Settings
		EditorGUILayout.Space();
		GUILayout.Label (Styles.advancedSettingsText, EditorStyles.boldLabel);
		if (((FeatureMode)material.GetFloat("_FeatureMode") == FeatureMode.Advanced)) {
			ModePopup<StrokeDrawMode>(strokeMode, Styles.strokeModeText, Styles.strokesNames);
			ModePopup<StrokeScaleMode>(scaleStrokesMode, Styles.strokeScaleModeText, Styles.strokeScaleName);
			ModePopup<JoinsAndCapsMode>(joinsCapsMode, Styles.joinsCapsModeText, Styles.joinsCapsNames);
			ModePopup<GradientsMode>(gradientsMode, Styles.gradientsModeText, Styles.gradientsNames);
			ModePopup<AntiAliasingMode>(antiAliasMode, Styles.antiAliasModeText, Styles.antiAliasNames);
			EditorGUILayout.Space();
			m_MaterialEditor.ShaderProperty(albedoColor, Styles.albedoColorText.text);
			m_MaterialEditor.ShaderProperty(widthMultiplier, Styles.widthMultiplierText.text);
		}
		else EditorGUILayout.HelpBox(Styles.featuresHelpBoxText.text, MessageType.Info);

		// Primary properties
		EditorGUILayout.Space();
		GUILayout.Label (Styles.primaryMapsText, EditorStyles.boldLabel);
		//m_MaterialEditor.TexturePropertySingleLine(Styles.albedoTextureText, albedoMap, albedoColor);
		m_MaterialEditor.TexturePropertySingleLine(Styles.albedoTextureText, albedoMap);
		m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);

		m_MaterialEditor.TexturePropertySingleLine(Styles.capTextureText, capMap);
		m_MaterialEditor.TextureScaleOffsetProperty(capMap);

		m_MaterialEditor.TexturePropertySingleLine(Styles.joinTextureText, joinMap);
		m_MaterialEditor.TextureScaleOffsetProperty(joinMap);


		if (((BlendMode)material.GetFloat("_BlendMode") == BlendMode.AlphaTest)) {
			EditorGUILayout.Space();
			GUILayout.Label (Styles.alphaTestText, EditorStyles.boldLabel);
			m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text);
		}
		else if (((BlendMode)material.GetFloat("_BlendMode") == BlendMode.UI)) {
			EditorGUILayout.Space();
			GUILayout.Label (Styles.uiText, EditorStyles.boldLabel);
			m_MaterialEditor.ShaderProperty(stencilComp, Styles.stencilCompText.text);
			m_MaterialEditor.ShaderProperty(stencilId, Styles.stencilIdText.text);
			m_MaterialEditor.ShaderProperty(stencilOp, Styles.stencilOpText.text);
			m_MaterialEditor.ShaderProperty(stencilWriteMask, Styles.stencilWriteMaskText.text);
			m_MaterialEditor.ShaderProperty(stencilReadMask, Styles.stencilReadMaskText.text);
			m_MaterialEditor.ShaderProperty(colorMask, Styles.colorMaskText.text);
			m_MaterialEditor.ShaderProperty(useUIAlphaClip, Styles.useUIAlphaClipText.text);
		}

		EditorGUILayout.Space();

		// Secondary properties
		GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
		GUI.enabled = false;
		m_MaterialEditor.TexturePropertySingleLine(Styles.styleDataText, styleDataMap);
		m_MaterialEditor.VectorProperty(styleDataSize, Styles.styleDataSizeText.text);
		GUI.enabled = true;

		bool changed = EditorGUI.EndChangeCheck();

		if (changed) foreach (var obj in blendingMode.targets) LW_MaterialPool.SetMaterialKeywords((Material)obj);
		else {
			EditorGUILayout.Space();

			GUILayout.Label ("Debug", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("RenderQueue: " + material.renderQueue);
			EditorGUILayout.LabelField("Keywords: ");
			foreach (string keyWord in material.shaderKeywords) {
				EditorGUILayout.LabelField(keyWord);
			}
		}
	}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(material, oldShader, newShader);

		if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/")) return;

		BlendMode blendingMode = BlendMode.Opaque;
		if (oldShader.name.Contains("/Transparent/Cutout/")) blendingMode = BlendMode.AlphaTest;
		else if (oldShader.name.Contains("/Transparent/")) blendingMode = BlendMode.AlphaBlend;

		material.SetFloat("_BlendMode", (float)blendingMode);

		LW_MaterialPool.SetMaterialKeywords(material);
	}

	void ModePopup<T>(MaterialProperty mode, string text, string[] options) where T : struct, System.IConvertible {
		EditorGUI.showMixedValue = mode.hasMixedValue;
		var value = mode.floatValue;

		EditorGUI.BeginChangeCheck();
		value = EditorGUILayout.Popup(text, (int)value, options);
		if (EditorGUI.EndChangeCheck()) {
			m_MaterialEditor.RegisterPropertyChangeUndo(text);
			mode.floatValue = (float)value;
		}

		EditorGUI.showMixedValue = false;
	}
}
//}