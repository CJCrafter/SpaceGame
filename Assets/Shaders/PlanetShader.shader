// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PlanetShader"
{
	// Properties are like public variables, set in the inspector
	Properties{
		_elevationMinMax ("MinMax", Vector) = (0, 0, 0, 0)
		_texture ("Texture", 2D) = "white" {}
	}
	SubShader{
		Pass {
			
			CGPROGRAM
			
			#pragma enable_d3d11_debug_symbols
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

			#define TAU 6.28318530718


			struct Interpolator {
				float4 pos : SV_POSITION;
				float3 diff : COLOR0;
				float3 ambient : COLOR1;
				float2 uv : TEXCOORD1;
				float3 normal : NORMAL;
				SHADOW_COORDS(1)
			};


			// These variables are set during runtime by scripts
			sampler2D _texture;
			float4 _elevationMinMax;

			float InverseLerp(float a, float b, float t) {
				return (t - a) / (b - a);
			}

			float GetMin() {
				return _elevationMinMax.x;
			}

			float GetMax() {
				return _elevationMinMax.y;
			}

			float4 GetTexture(float2 uv) {
				float4 temp = tex2Dlod(_texture, float4(uv, 0.0f, 0.0f));
				return temp;
			}


			Interpolator vert(appdata_base v) {
                Interpolator o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
				o.normal = v.normal;
                // compute shadows data
                TRANSFER_SHADOW(o)
				return o;
			}

			float4 frag(Interpolator vertex) : COLOR {
				// https://developer.download.nvidia.com/cg/index_stdlib.html
                // clamp01 = saturate = clamp between 0 and 1

				float waterPercentage = InverseLerp(GetMin(), 0.0f, vertex.uv.y);
				float waterGradient = lerp(0.0f, 0.5f, waterPercentage);
				float landPercentage = InverseLerp(0.0f, GetMax(), vertex.uv.y);
				float landGradient = lerp(0.5f, 1.0f, landPercentage);

				float i = floor(waterPercentage);
				float total = landGradient * i + waterGradient * (1.0f - i);

				float4 color = tex2D(_texture, float2(total, vertex.uv.x));
				float shadow = SHADOW_ATTENUATION(vertex);
				float3 light = vertex.diff * shadow + vertex.ambient;
				
				color.rgb *= light;
				return color;
			}
			ENDCG
		}
		
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
	FallBack "Diffuse"
}
