Shader "Custom/PlanetShader" {
	Properties {
		_occlusion ("Occlusion", Range(0,1)) = 1.0
		_metallic ("Metallic", Range(0,1)) = 1.0
		_glossiness ("Smoothness", Range(0,1)) = 0.5
        _elevationMinMax("Elevation Bounds", Vector) = (0, 0, 0, 0)
        _shoreHeight("Beach Height", Float) = 0
        _shoreColor("Beach Color", Color) = (0, 0, 0, 0)
        _gradient ("Planet Colors", 2D) = "white" {}
		_blobWeight ("Blob Texturing Weight", Range(0,1)) = 0.1
		_blobFrequency ("Blob Texturing Frequency", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0
		#pragma enable_d3d11_debug_symbols
		#include "Assets/Shaders/Noise.cginc"

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		half _occlusion;
		half _glossiness;
		half _metallic;
		float _planetRadius;
		float _blobWeight;
		float _blobFrequency;
		float4 _elevationMinMax;
		float _shoreHeight;
		float3 _shoreColor;
		sampler2D _gradient;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		float arccos(float x) {
			return -1.26 * x * x * x + 1.26;
		} 
		
		void surf(Input vertex, inout SurfaceOutputStandard o) {

			float3 local = mul(unity_WorldToObject, float4(vertex.worldPos, 1)).xyz;
			float distance = length(local);
			float elevation = distance / _planetRadius - 1;
			
            float3 color;
            if (elevation <= _shoreHeight) {
	            color = _shoreColor;
            }
			
			else {
				float3 normal = mul(unity_WorldToObject, vertex.worldNormal).xyz;
				float angle = arccos(dot(normal, local) / (distance * length(normal))) / (3.1415926 / 2.0);
	            color = tex2Dlod(_gradient, float4(angle, 0.5, 0, 0));
            }

			float random = snoise(float3(_blobFrequency * local)) * _blobWeight;
			
			o.Albedo = color * (1 + _blobWeight / 2 - random);
			o.Occlusion = _occlusion;
			o.Smoothness = _glossiness;
			o.Metallic = _metallic;
		}
		ENDCG
	}
	FallBack "Diffuse"
}