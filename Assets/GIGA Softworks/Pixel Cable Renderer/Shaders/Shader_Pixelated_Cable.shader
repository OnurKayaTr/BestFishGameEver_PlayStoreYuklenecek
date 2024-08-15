Shader "GIGA Softworks/CableRenderer/Cable"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _CableTexture("Cable Texture", 2D) = "white" {}
		_InverseCanvasSize("Inverse Canvas Size",float) = 0
		[PerRendererData] _CableWindAmount("Wind", float) = 0
		[PerRendererData] _CableWindSpeed("Wind Speed", float) = 0
		[PerRendererData] _RippleToggle("Wind Ripple", int) = 0
		[PerRendererData] _PixelSize("Pixel Size", int) = 0
		//[PerRendererData] _Color("Color",Color) = (0,0,0,1)
		//[PerRendererData] _Color2("Color2",Color) = (0,0,0,1)
		[PerRendererData] _ColorMode("ColorMode",Int) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="False"
        }

        Cull Back
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
		//BlendOp Max 
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
			#pragma multi_compile_local TEXTURE_OFF TEXTURE_ON
            #pragma fragment frag
            #include "UnityCG.cginc"
			#include "CableRendererShared.cginc"
		
			// Properties
			fixed _CableWindAmount;
			fixed _CableWindSpeed;
			fixed _Smoothness;
			fixed _RippleToggle;
			float _BellParam1 = 0.8f;			// Bell Shape Modifier
			float _BellParam3 = 20.0f;			// Ripple Shape Modifier

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed scaleX = IN.color.x * SCALE_COMPRESSION;		// using the color channel to pass mesh scale to the shader, to avoid batching issues when batching multiple cables
				fixed scaleY = IN.color.y * SCALE_COMPRESSION;
				fixed _Softness = IN.color.b;				// using the color channel to pass softness the shader, to avoid breaking batching when changing softness				
				
				fixed pixelScaleFactor = 1 / (_InverseCanvasSize * _PixelSize);

				int canvasWidth = scaleX * pixelScaleFactor;
				int allowedYDelta = scaleY * pixelScaleFactor * 0.25f;			// The allowed vertical canvas space for the cable to move.

				int pixelX = (IN.texcoord.x * scaleX) * pixelScaleFactor ;
				int pixelY = (IN.texcoord.y * scaleY) * pixelScaleFactor ;
				float distanceFromCenter = abs(pixelX - (canvasWidth / 2.0f)) / (canvasWidth / 2.0f);

				// Calculating wind modifiers
				float time = _Time.y * _TimeEnabled;
				float windModifier_sin = sin(time * _CableWindSpeed * _CableWindSpeed) * _CableWindAmount * 0.4f;
				float windModifier_bell = lerp(_BellParam1,sin(time * _CableWindSpeed * 2 * _CableWindAmount ), _RippleToggle );

				// Calculating pixel height values
				fixed heightValueSin = sin(pixelX / (canvasWidth * 0.5) * RAD_90) * allowedYDelta * (-_Softness + windModifier_sin * 0.5f);
				fixed heightValueBell = lerp(allowedYDelta * (-_Softness + abs(windModifier_sin)) * exp( -(pow(pixelX - canvasWidth * windModifier_bell , 2) / (canvasWidth * _BellParam3))) , 0 , distanceFromCenter);
				
				fixed minShapeMix = _RippleToggle * 0.25f;
				fixed bellDistanceFromCenter = abs(_BellParam1 - 0.5f) / 0.5f;
				fixed shapeMix = lerp(minShapeMix, 0.5f, bellDistanceFromCenter);
				int heightValue = lerp(heightValueSin, heightValueBell, shapeMix);

				// Applying color
				fixed4 c1 = fixed4(0,0,0,0);
				fixed4 c2 = fixed4(0,0,0,0);
				int pixelHeight = floor(((0.5 * scaleY) * pixelScaleFactor) + heightValue);
				fixed pixelAlpha = (pixelY == pixelHeight);

				c1.a = _Color1.a * pixelAlpha;
				c1.rgb = _Color1.rgb * c1.a;
				c2.a = _Color2.a * pixelAlpha;
				c2.rgb = _Color2.rgb * c2.a;
				


				float2 modUV = float2((IN.texcoord.x * scaleX) * pixelScaleFactor - floor((IN.texcoord.x * scaleX) * pixelScaleFactor),(IN.texcoord.y * scaleY) * pixelScaleFactor - floor((IN.texcoord.y * scaleY) * pixelScaleFactor));

#if TEXTURE_OFF	
				// Pixel only rendering
				fixed4 col = fixed4(0,0,0,0);
				col = lerp(c1,c2,_ColorMode == 3 && pixelX % 2 != 0);												// Alternate pixels
				col = lerp(col,c2,(pixelX / (scaleX /(_InverseCanvasSize * _PixelSize))) * (_ColorMode == 1));		// Linear gradient
				col = lerp(col,c2, distanceFromCenter * (_ColorMode == 2));											// Center gradient
				col = lerp(col,_IndexedColors[round(IN.color.a * 16)] * c1.a, _ColorMode == 4);							// Indexed color

#else
				// Texture rendering (NOT SUPPORTED IN LITE)
				fixed4 col = fixed4(0,0,0,0);
				col = lerp(c1,c2,_ColorMode == 3 && pixelX % 2 != 0);												// Alternate pixels
				col = lerp(col,c2,(pixelX / (scaleX /(_InverseCanvasSize * _PixelSize))) * (_ColorMode == 1));		// Linear gradient
				col = lerp(col,c2, distanceFromCenter * (_ColorMode == 2));											// Center gradient
				col = lerp(col,_IndexedColors[round(IN.color.a * 16)] * c1.a, _ColorMode == 4);						// Indexed color

#endif
				// Applying Smoothness
				col *= 1 - _Smoothness *( abs(0.5f - modUV.y) / 0.5f);
				return col;
			}

			ENDCG
        }
    }
}