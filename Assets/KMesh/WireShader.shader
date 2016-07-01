Shader "VertexWire"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ResX("ResX",Int) = 10
		_ResY("ResY",Int) = 10
		_GridWidth("GridWidth",Range(0,1)) = .1
		_GridAlpha("GridAlpha",Range(0,1)) = .1
		_BGAlpha("BGAlpha",Range(0,1)) = .1

		
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color: COLOR;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform int _ResX;
			uniform int _ResY;
			uniform float _GridWidth;
			uniform float _GridAlpha;
			uniform float _BGAlpha;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
			

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				
				float4 col;

				float x = frac(i.uv[0] *_ResX);
				float y = frac(i.uv[1] *_ResY);

				if (x > 1-_GridWidth/2 || y > 1-_GridWidth/2 || x < _GridWidth/2 || y < _GridWidth/2)
				{
					col = i.color*_GridAlpha;
				} else
				{
					col = i.color*_BGAlpha;
				}

				return col;
			}
			ENDCG
		}
	}
}
