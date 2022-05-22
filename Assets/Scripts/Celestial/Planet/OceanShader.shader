Shader "Custom/OceanShader" {
	Properties {
		_color ("Color", Color) = (1,1,1,1)
		_occlusion ("Occlusion", Range(0,1)) = 1.0
		_metallic ("Metallic", Range(0,1)) = 1.0
		_glossiness ("Smoothness", Range(0,1)) = 0.5
		_waveHeight ("Wave Height", Range(0,1)) = 1.0
		_frequency ("Wave Frequency", Float) = 1.0
		_waveSpeed ("Wave Speed", Float) = 1.0
		_alpha ("Transparency", Range(0,1)) = 1.0
	}
	SubShader {
		//Tags { "RenderType"="Transparent" }
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
		Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alpha
		#pragma target 3.0
		#include "Assets/Shaders/Noise.cginc"
		

		struct Input {
			float3 worldPos;
		};

		half _glossiness;
		float _occlusion;
		float _metallic;
		fixed4 _color;
		fixed _alpha;
		float _waveHeight;
		float _waveSpeed;
		float _frequency;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)
 		
		void surf(Input vertex, inout SurfaceOutputStandard o) {

			float3 local = mul(unity_WorldToObject, float4(vertex.worldPos, 1)).xyz;
			float3 normal = normalize(local);
			float3 random = normalize(snoise_grad(float3(_frequency * normal + _Time.y * _waveSpeed)).xyz) * _waveHeight;

			o.Normal = normalize(normal + random);
			//o.Albedo = o.Normal;

			o.Albedo = _color.rgb;
			o.Occlusion = _occlusion;
			o.Smoothness = _glossiness;
			o.Metallic = _metallic;
			o.Alpha = _alpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}