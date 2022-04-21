Shader "Space Graphics Toolkit/Depth"
{
	Properties
	{
	}
	SubShader
	{
		Tags
		{
			"Queue"           = "Transparent+1"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Lighting Off
			ZWrite On

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Interpolator
			{
				float4 vertex : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			struct f2g
			{
				float4 color : SV_TARGET;
			};

			void Vert(a2v i, out Interpolator o)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_OUTPUT(Interpolator, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(i.vertex);
			}

			void Frag(Interpolator i, out f2g o)
			{
				o.color = float4(0.0f, 0.0f, 0.0f, 0.0f);
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader