#include "UnityCG.cginc"

// Unpacks one float into two halfs. Used to fit more data in the vertex data.
inline half2 UnpackRange(float input, float rangeMin, float rangeMax) {
	int PRECISION = 4096;
	half2 output;
    output.y = input % PRECISION;
    output.x = floor(input / PRECISION);
    output /= (PRECISION - 1);
	float spread = rangeMax - rangeMin;
	float offset = 0 - rangeMin;
	output.x = (output.x * spread) - offset;
	output.y = (output.y * spread) - offset;
	return output;
}

// Full appdata with full precision
struct appdata {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	fixed4 color : COLOR;
};

// Frag input with additional data.
struct v2f {
	float4 vertex : SV_POSITION;
	half4 texcoord : TEXCOORD0;
	half4 fragData1 : TEXCOORD1;
	half4 fragData2 : TEXCOORD2;
	half4 fragData3 : TEXCOORD3;
	half4 fragData4 : TEXCOORD4;

	#if _BLEND_UI
	half4 worldPosition : TEXCOORD5;

	#elif _BLEND_ALPHATEST
	UNITY_FOG_COORDS(5)

	#elif _BLEND_ADDITIVESOFT
	UNITY_FOG_COORDS(5)
	#ifdef SOFTPARTICLES_ON
	float4 projPos : TEXCOORD6;
	#endif

	#else // if _BLEND_ALPHABLEND
	UNITY_FOG_COORDS(5)

	#endif // end _BLEND_UI

	fixed4 color : COLOR;
}; 

#if _TEXTURES_ON
sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _CapTex;
float4 _CapTex_ST;
sampler2D _JoinTex;
float4 _JoinTex_ST;
#endif

float4 _StyleDataSize;
sampler2D _StyleData;

#if _ADVANCED_ON
float _Width;
fixed4 _Color;

#endif

#if _BLEND_UI
fixed4 _TextureSampleAdd;
float4 _ClipRect;

#elif _BLEND_ALPHATEST
fixed _Cutoff;

#elif _BLEND_ADDITIVESOFT
sampler2D_float _CameraDepthTexture;
float _InvFade;

#endif 

// Adjust Vertex Position, UVs, and setup data for Frag Program
inline v2f SetupVert(inout appdata i) {
	v2f o;
	
	float4 msVertPosition = i.vertex.xyzw;
	
	half4 fragData1;
	half4 fragData2;
	half4 fragData3;
	half4 fragData4;
	
	// Setup Variables from Vertex Data
	#if _ADVANCED_ON
	
	// General Style Data
	half currStyleDataIndex = abs(i.tangent.w)-1;
	half2 pixelOffset = 0.5/_StyleDataSize.xy; 
	half styleDataRow = currStyleDataIndex/_StyleDataSize.y + pixelOffset.y;

	// Style Data
	half4 pixelData1 = tex2Dlod(_StyleData, half4(0.0/_StyleDataSize.x + pixelOffset.x, styleDataRow,0,0));
	half4 pixelData2 = tex2Dlod(_StyleData, half4(1.0/_StyleDataSize.x + pixelOffset.x, styleDataRow,0,0));
	half4 pixelData3 = tex2Dlod(_StyleData, half4(2.0/_StyleDataSize.x + pixelOffset.x, styleDataRow,0,0));
	half4 pixelData4 = tex2Dlod(_StyleData, half4(3.0/_StyleDataSize.x + pixelOffset.x, styleDataRow,0,0));
	
	// Stroke Data
	half miterLimit = abs(pixelData1.x);
	half totalWidth = pixelData1.z * _Width;
	half totalLength = pixelData1.w;
	
	// Bounds
	half boundsMin = pixelData2.z;
	half boundsMax = pixelData2.w;
	
	// Screenspace scale Data
	half msUnitsPerPixel = abs(UNITY_MATRIX_P[3][3] == 1 ? 2 / UNITY_MATRIX_P[1][1] : (2 * -mul( UNITY_MATRIX_MV, i.vertex ).z) / UNITY_MATRIX_P[1][1])/_ScreenParams.y;
	half msWidthMultiplier = msUnitsPerPixel;
	
	// Coords
	half2 uvCoords = UnpackRange(i.texcoord.x, -2, 2);
	half2 localPos = UnpackRange(i.texcoord.y, boundsMin, boundsMax);
	half3 scale = half3( UnpackRange(i.texcoord1.x, -10, 10), 1 );
	half2 tcoord1 = UnpackRange(i.texcoord1.y, -2, 2);
	half2 boundsPos = abs(uvCoords)-1;
	half2 currCoords = half2(sign(uvCoords.x), sign(uvCoords.y));
	half2 joins = (abs(tcoord1)-1.5)*2.0;
	
	// Bools
	half isFill = step(miterLimit, 0.5);
	half isStroke = 1 - isFill;
	half isUvsScaled = step(pixelData2.x, 0.5);
	half isUvsStretched = 1 - isUvsScaled;
	half isGradWorldSpace = step(pixelData2.y, 0.5);
	half isGradObjectSpace = 1 - isGradWorldSpace;
	half isEnd = currCoords.x * 0.5 + 0.5;
	half isStart = 1 - isEnd;
	half isInnerFill = step(tcoord1.x, -0.5);
	half isNotInnerFill = 1 - isInnerFill; //clamp(isStroke + isOuterFill, 0, 1);
	half isOuterFill = step(tcoord1.y, -0.5);
	half isNotOuterFill = 1 - isOuterFill; //clamp(isStroke + isInnerFill, 0, 1);
	half isBreak = isStroke * (isStart * step(0.5, joins.x) + isEnd * step(0.5, joins.y));
	half isNotBreak = 1 - isBreak;

	// Stroke Data
	#if _STROKE_SCREENSPACE
	totalWidth *= msUnitsPerPixel;
	#endif

	half currLength = boundsPos.x * totalLength;
	half currWidth = isStroke * abs(boundsPos.y - 0.5) * totalWidth;

	// 3D Strokes
	#if _STROKE_3D
	
	// Tangents
	half3 msPrevTangent = i.normal.xyz;
	half3 msNextTangent = i.tangent.xyz;

	// Calculate Screenspace Data
	half4 prevVertPosition = half4(msVertPosition.xyz - msPrevTangent, msVertPosition.w);
	half4 nextVertPosition = half4(msVertPosition.xyz + msNextTangent, msVertPosition.w);
	half4 prevHPosition = ComputeScreenPos(mul (UNITY_MATRIX_MVP, prevVertPosition));
	prevHPosition = half4((prevHPosition.xy/prevHPosition.w), 0, 1);
	half2 ssPrevPosition = prevHPosition * _ScreenParams.xy;
	half4 currHPosition = ComputeScreenPos(mul (UNITY_MATRIX_MVP, msVertPosition));
	currHPosition = half4((currHPosition.xy/currHPosition.w), 0, 1);
	half2 ssCurrPosition = currHPosition * _ScreenParams.xy;
	half4 nextHPosition = ComputeScreenPos(mul (UNITY_MATRIX_MVP, nextVertPosition));
	nextHPosition = half4((nextHPosition.xy/nextHPosition.w), 0, 1);
	half2 ssNextPosition = nextHPosition * _ScreenParams.xy;
	half2 ssPrevTangent = normalize (ssCurrPosition - ssPrevPosition);
	half2 ssNextTangent = normalize (ssNextPosition - ssCurrPosition);
	half ssPrevLength = distance (ssCurrPosition, ssPrevPosition);
	half ssNextLength = distance (ssNextPosition, ssCurrPosition);
	half2 ssPrevOffset = half2(-ssPrevTangent.y, ssPrevTangent.x);
	half2 ssNextOffset = half2(-ssNextTangent.y, ssNextTangent.x);
	half2 ssSegmentTangent = isEnd * ssPrevTangent + isStart * ssNextTangent;
	half2 ssSegmentOffset = half2(-ssSegmentTangent.y, ssSegmentTangent.x);
	half2 ssVertOffset = normalize (ssPrevOffset + ssNextOffset);
	half ssOffsetLength = abs(1/dot (ssVertOffset, ssPrevOffset));
	ssVertOffset = -currCoords.y * ssVertOffset * ssOffsetLength;

	half uDot = dot(ssVertOffset.xy, ssSegmentTangent.xy) * 0.5;
	
	// Check for Break
	isBreak = clamp(isBreak + step(miterLimit, ssOffsetLength) + step(ssPrevLength/(currWidth/msUnitsPerPixel), abs(uDot)) + step(ssNextLength/(currWidth/msUnitsPerPixel), abs(uDot)), 0, 1);
	isNotBreak = 1 - isBreak;
	
	// Calculate Offset
	#if _JOINSCAPS_ON
	ssVertOffset = (isBreak * (-currCoords.y * ssSegmentOffset + currCoords.x * ssSegmentTangent)) + (isNotBreak * ssVertOffset);
	#else
	ssVertOffset = (isBreak * -currCoords.y * ssSegmentOffset) + (isNotBreak * ssVertOffset);
	#endif
	
	// Calculate Modelspace data from Screenspace data.
	half3 msVertOffset = mul(half3(ssVertOffset,0), (fixed3x3)UNITY_MATRIX_IT_MV);
	half3 msVertTangent = mul(half3(ssSegmentTangent,0), (fixed3x3)UNITY_MATRIX_IT_MV);
	half3 usVertOffset = msVertOffset;
	half3 usVertSegment = msVertTangent;
	half3 usVertTangent = normalize(usVertSegment);
	half usSegmentLength = (isEnd * ssPrevLength + isStart * ssNextLength) * msUnitsPerPixel;
	
	// Acount for scaling
	#if !_STROKE_UNSCALED && !_STROKE_SCREENSPACE
	msVertOffset *= scale;
	msVertTangent *= scale;
	#endif
	
	// AntiAliasing
	#if !_STROKE_UNSCALED && !_STROKE_SCREENSPACE && _ANTIALIAS_ON
	half msAntiAliasingWidth = msUnitsPerPixel / length(normalize(msVertOffset) * scale);
	#elif _ANTIALIAS_ON
	half msAntiAliasingWidth = msUnitsPerPixel;
	#else // if !_ANTIALIAS_ON
	half msAntiAliasingWidth = 0;
	#endif // end _ANTIALIAS_ON

	half currOffsetWidth = currWidth + msAntiAliasingWidth;
	
	// Calculate UV Offset
	#if _JOINSCAPS_ON
	uDot = dot(ssVertOffset.xy, ssSegmentTangent.xy) * currOffsetWidth / totalWidth;
	#else
	uDot = 0;
	#endif
	
	// 2D Strokes
	#else // if !_STROKE_3D
	half justification = isStroke * abs(pixelData1.y + currCoords.y) + isFill;

	#if !_STROKE_UNSCALED && !_STROKE_SCREENSPACE
	half3 msVertOffset = i.normal.xyz;
	half3 msVertTangent = i.tangent.xyz;
	half3 usVertOffset = msVertOffset / scale;
	half3 usVertSegment = msVertTangent / scale;
	#else
	half3 msVertOffset = i.normal.xyz / scale;
	half3 msVertTangent = i.tangent.xyz / scale;
	half3 usVertOffset = msVertOffset;
	half3 usVertSegment = msVertTangent;
	#endif
	
	half3 usVertTangent = normalize(usVertSegment);
	half usSegmentLength = length(usVertSegment);

	// Antialiasing
	#if !_STROKE_UNSCALED && !_STROKE_SCREENSPACE && _ANTIALIAS_ON
	half msAntiAliasingWidth = msUnitsPerPixel / length(normalize(msVertOffset) * scale);
	#elif _ANTIALIAS_ON
	half msAntiAliasingWidth = msUnitsPerPixel;
	#else // if !_ANTIALIAS_ON
	half msAntiAliasingWidth = 0;
	#endif // end _ANTIALIAS_ON

	half currOffsetWidth = currWidth + msAntiAliasingWidth;

	half uDot = !isInnerFill * currCoords.y * justification * dot(usVertOffset, usVertTangent) * currOffsetWidth / totalWidth;

	if (isBreak) {
		half3 usVertPerpen = normalize(cross(cross(usVertTangent, usVertOffset), usVertTangent));
		usVertOffset = currCoords.y * justification * usVertPerpen;

		#if _JOINSCAPS_ON
		usVertOffset += currCoords.x * usVertTangent;
		uDot = !isInnerFill * currCoords.x * currOffsetWidth / totalWidth;
		#else
		uDot = 0;
		#endif // _JOINSCAPS_ON

		#if !_STROKE_UNSCALED && !_STROKE_SCREENSPACE
		msVertOffset = usVertOffset * scale;

		#else
		msVertOffset = usVertOffset;

		#endif
	}
	else {
		usVertOffset *= currCoords.y * justification;
		msVertOffset *= currCoords.y * justification;
	}

	/*
	half3 usVertPerpen = normalize(cross(cross(usVertTangent, usVertOffset), usVertTangent));
	usVertOffset = (isBreak * currCoords.y * justification * usVertPerpen) + (isNotBreak * usVertOffset);

	#if _JOINSCAPS_ON
	usVertOffset += isBreak * currCoords.x * usVertTangent;
	half uDot = (isBreak * isNotInnerFill * currCoords.x * currOffsetWidth / totalWidth) + (isNotBreak * isNotInnerFill * currCoords.y * justification * dot(usVertOffset, usVertTangent) * currOffsetWidth / totalWidth);
	#else
	half uDot = (isBreak * 0) + (isNotBreak * isNotInnerFill * currCoords.y * justification * dot(usVertOffset, usVertTangent) * currOffsetWidth / totalWidth);
	#endif // _JOINSCAPS_ON

	usVertOffset = (isBreak * usVertOffset) + (isNotBreak * usVertOffset * currCoords.y * justification);
	
	#if !_STROKE_UNSCALED && !_STROKE_SCREENSPACE
	msVertOffset = (isBreak * usVertOffset * scale) + (isNotBreak * msVertOffset * currCoords.y * justification);
	#else
	msVertOffset = (isBreak * usVertOffset) + (isNotBreak * msVertOffset * currCoords.y * justification);
	#endif
	*/

	#endif // end _STROKE_3D
	
	half vDot = isNotInnerFill * currCoords.y *  msAntiAliasingWidth / totalWidth;
	boundsPos += isFill * msVertOffset * currOffsetWidth / half2(totalLength, totalWidth);
	localPos += isFill * msVertOffset * msAntiAliasingWidth / scale;

	i.texcoord.xy = (isUvsScaled * half2(boundsPos.x * totalLength / totalWidth + uDot, boundsPos.y + vDot)) + (isUvsStretched * half2(boundsPos.x, (isStroke * (currCoords.y*0.5+0.5)) + (isFill * boundsPos.y)));
	i.texcoord.zw = (isGradWorldSpace * (localPos + usVertOffset * currOffsetWidth)) + (isGradObjectSpace * half2(boundsPos.x, (isStroke * (currCoords.y*0.5+0.5)) + (isFill * boundsPos.y)));
	i.vertex.xyz = msVertPosition + msVertOffset * currOffsetWidth;
	i.color *= _Color;
		
	// Segment Data
	msAntiAliasingWidth = isNotInnerFill * msAntiAliasingWidth;
	currCoords.y += isOuterFill;

	half u = isNotInnerFill * (step(0, currCoords.x) * usSegmentLength + uDot * totalWidth);
	half l = isNotInnerFill * usSegmentLength;
	half v = isNotInnerFill * currCoords.y * currOffsetWidth;
	half w = currWidth - (isNotOuterFill * msAntiAliasingWidth);

	// Joins and Caps
	fragData1 = half4(u, v, l, w);
	fragData2 = half4(joins, styleDataRow, msAntiAliasingWidth);
    fragData3 = pixelData3;
    fragData4 = pixelData4;

    #else // if !_ADVANCED_ON

	i.texcoord.zw = i.texcoord1.xy;
	fragData1 = half4(0, 0, 0, 0); 
	fragData2 = half4(0, 0, 0, 0);
	fragData3 = half4(0, 0, 0, 0);
    fragData4 = half4(0, 0, 0, 0);

	#endif // end _ADVANCED_ON
	
	o.vertex = mul(UNITY_MATRIX_MVP, i.vertex);
	o.color = i.color;
	o.fragData1 = fragData1;
    o.fragData2 = fragData2;
    o.fragData3 = fragData3;
	o.fragData4 = fragData4;

	#if _TEXTURES_ON
	o.texcoord.xy = TRANSFORM_TEX(i.texcoord.xy, _MainTex);
	#else // if !_TEXTURES_ON
	o.texcoord.xy = i.texcoord.xy;
	#endif // _TEXTURES_ON

	o.texcoord.zw = i.texcoord.zw;
				
	return o;
}

inline fixed4 SetupFrag(in v2f i) {
	fixed4 color = i.color;

	#if _GRADIENTS_ON || _JOINSCAPS_ON || _ANTIALIAS_ON

		#if _GRADIENTS_ON || _JOINSCAPS_ON
		half styleDataOffset = 0.5/_StyleDataSize.x;
		half styleDataRow = i.fragData2.z;

		#endif // _GRADIENTS_ON || _JOINSCAPS_ON

		half v = abs(i.fragData1.y);
		half w = i.fragData1.w;
		half d = v;

		#if _GRADIENTS_ON
		half2 gradientStart = i.fragData3.xy;
		half paintMode = i.fragData3.w;
		half gradientAngle = i.fragData4.x;
		half gradientScale = abs(i.fragData4.y);
		half sine = 0.0;
		half cosine = 0.0;
		sincos(gradientAngle, sine, cosine);
		half2 gradCoords = (i.texcoord.zw - gradientStart.xy) / (gradientScale);
		half t = 0.0;
		if (paintMode > 0.5) t = gradCoords.x * sine + gradCoords.y * cosine;
		if (paintMode > 1.5) t = sqrt(gradCoords.x * gradCoords.x + gradCoords.y * gradCoords.y);
		t = (_StyleDataSize.z + clamp(t, 0.0, 1.0) * (_StyleDataSize.w - 1.0)) / _StyleDataSize.x + styleDataOffset;
		color *= tex2D(_StyleData, half2(t, styleDataRow));
		#endif // _GRADIENTS_ON

		#if _JOINSCAPS_ON
		half u = i.fragData1.x;
		half l = i.fragData1.z;
		half capInt = i.fragData4.z;
		half joinInt = i.fragData4.w;
		half2 joins = i.fragData2.xy;

		//bool isCap = step(0, -u) * step(0, joins.x) + step(l, u) * step(0, joins.y);
		bool isCap = (u < 0 && joins.x > 0.5) || (u > l && joins.y > 0.5);

		//bool isJoin = step(0, -u) * step(0, -joins.x) + step(l, u) * step(0, -joins.y);
		bool isJoin = (u < 0 && joins.x < 0.5) || (u > l && joins.y < 0.5);

		half2 texCoord = i.texcoord.xy;
		if (u < l * 0.5) texCoord.x = u / (w * 2) + 0.5;
		else texCoord.x = (u - l) / (w * 2) + 0.5;

		u = max(0, -u) + max(0, u - l);
		if (isCap) {
			if (capInt > -0.5) d = max(u + w, v); // Butt
			if (capInt > 0.5) d = max(u, v); // Square
			if (capInt > 1.5) d = sqrt(u * u + v * v); // Round
			if (capInt > 2.5) d = 1;
		} else {
			if (joinInt > 1.5) d = sqrt(u * u + v * v);	// Round
			if (joinInt > 2.5) d = 1;
		}

		#endif // _JOINSCAPS_ON

	    d = d - w;

	    #if _ANTIALIAS_ON
	   	half msAntiAliasingWidth = i.fragData2.w;
	    if( d > 0.0 ) {
	        d /= msAntiAliasingWidth; 
	        color.a *= exp(-d*d);
	    }

	    #else // if !_ANTIALIAS_ON
	    clip(-d);

	    #endif // end _ANTIALIAS_ON

	#endif // _GRADIENTS_ON || _JOINSCAPS_ON || _ANTIALIAS_ON

	//color *= 0.5;
	#if _TEXTURES_ON && _BLEND_UI && _JOINSCAPS_ON
	if (isCap && capInt > 2.5) color *= tex2D(_CapTex, texCoord) + _TextureSampleAdd;
	else if (isJoin && joinInt > 2.5) color *= tex2D(_JoinTex, texCoord) + _TextureSampleAdd;
	else color *= tex2D(_MainTex, i.texcoord) + _TextureSampleAdd;

	#elif _TEXTURES_ON && _JOINSCAPS_ON
	if (isCap && capInt > 2.5) color *= tex2D(_CapTex, texCoord);
	else if (isJoin && joinInt > 2.5) color *= tex2D(_JoinTex, texCoord);
	else color *= tex2D(_MainTex, i.texcoord);

	#elif _TEXTURES_ON && _BLEND_UI
	color *= tex2D(_MainTex, i.texcoord) + _TextureSampleAdd;

	#elif _TEXTURES_ON
	color *= tex2D(_MainTex, i.texcoord);

	#endif // _TEXTURES_ON && _BLEND_UI

	return color;
}
