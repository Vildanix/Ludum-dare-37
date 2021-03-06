﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "LD37/GridHighlight"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CursorPos("CursorPos", Vector) = (1,1,1,1)
		_Radius("HighlightRadius", Range(0.05,2)) = 1
		_Overlay("OverlayIntensity", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color: Color; // Vertex color
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color : Color;
				float distance : Float;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _CursorPos;
			float _Radius;
			float _Overlay;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.color = v.color;

			    // calculate cursor distance from current vertex with both coords in world space
				o.distance = distance(_CursorPos.xyz, mul(unity_ObjectToWorld, v.vertex).xyz) * _Radius;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture

				fixed4 col = tex2D(_MainTex, i.uv + i.color.gr / 2 ) * (1 - min(i.distance, 1 - i.color.g)) * 2 * _Overlay;
				col.a *= saturate(1 - min(i.distance, 1 - i.color.g));
				return col;
			}
			ENDCG
		}
	}
}
