Shader "LineWorks/Default" {

	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}	
		_Color ("Tint", Color) = (1,1,1,1)
		_Width ("Width", Float) = 1.0

		// AlphaTest
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

		// Additive Soft
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		// UI
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15

		[HideInInspector] _CapTex ("Linecaps Texture", 2D) = "white" {}
		[HideInInspector] _JoinTex ("Linejoins Texture", 2D) = "white" {}	

		[HideInInspector] _StyleData ("Style Data", 2D) = "white" {}
		[HideInInspector] _StyleDataSize ("Size", Vector) = (1,1,0,0)

		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0

		[HideInInspector] _BlendMode ("__blendmode", Float) = 0.0
		[HideInInspector] _FeatureMode ("__featuresmode", Float) = 0.0
		[HideInInspector] _StrokeDrawMode ("__strokedrawmode", Float) = 0.0
		[HideInInspector] _StrokeScaleMode ("__strokescalemode", Float) = 0.0
		[HideInInspector] _JoinsAndCapsMode ("__joinscapsmode", Float) = 0.0
		[HideInInspector] _GradientsMode ("__gradientsmode", Float) = 0.0
		[HideInInspector] _AntiAliasingMode ("__antialiasmode", Float) = 0.0

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader {
		Tags { 
			"IgnoreProjector"="True" 
			"RenderType"="Opaque" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		LOD 100

		Stencil {
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Back
		Lighting Off
		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		ZTest [unity_GUIZTestMode]
		ColorMask [_ColorMask]

		Pass {
			CGPROGRAM
			#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization 

			#pragma multi_compile _ _BLEND_ALPHATEST _BLEND_UI _BLEND_ADDITIVESOFT
			#pragma multi_compile _ _TEXTURES_ON
			#pragma multi_compile _ _ADVANCED_ON
			#pragma multi_compile _ _STROKE_3D
			#pragma multi_compile _ _STROKE_UNSCALED _STROKE_SCREENSPACE
			#pragma multi_compile _ _JOINSCAPS_ON
			#pragma multi_compile _ _GRADIENTS_ON 
			#pragma multi_compile _ _ANTIALIAS_ON
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			#include "LineWorks.cginc"

			v2f vert (appdata i) {
				v2f o = SetupVert(i);

				#if _BLEND_UI
				o.worldPosition = i.vertex;
				#ifdef UNITY_HALF_TEXEL_OFFSET
				o.vertex.xy += (_ScreenParams.zw-1.0) * float2(-1,1) * o.vertex.w;
				#endif

				#elif _BLEND_ALPHATEST
				UNITY_TRANSFER_FOG(o,o.vertex);

				#elif _BLEND_ADDITIVESOFT
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				UNITY_TRANSFER_FOG(o,o.vertex);

				#else // _BLEND_UI
				UNITY_TRANSFER_FOG(o,o.vertex);

				#endif // _BLEND_UI

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 color = SetupFrag(i);

			    #if _BLEND_UI
			    color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				#elif _BLEND_ALPHATEST
				clip(color.a - _Cutoff);
				UNITY_APPLY_FOG(i.fogCoord, color);

				#elif _BLEND_ADDITIVESOFT
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				color.a *= fade;
				#endif
				color.rgb *= color.a;
				UNITY_APPLY_FOG_COLOR(i.fogCoord, color, fixed4(0,0,0,0)); // fog towards black due to our blend mode

				#else // if _BLEND_ALPHABLEND
				UNITY_APPLY_FOG(i.fogCoord, color);

				#endif // end _BLEND_UI

				return color;
			}
			ENDCG
		}
	}

	FallBack "VertexLit"
	CustomEditor "LW_ShaderEditor"
}
