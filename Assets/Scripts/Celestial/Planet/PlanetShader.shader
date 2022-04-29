Shader "Celestial/PlanetShader" {
    Properties {
        _ElevationMinMax("_ElevationMinMax", Vector) = (0, 0, 0, 0)
        _ShoreHeight("_ShoreHeight", Float) = 0
        _ShoreColor("_ShoreColor", Color) = (0, 0, 0, 0)
        _Gradient ("_Gradient", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM



            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct Vertex {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0; // [angle, elevation]
            };

            struct Interpolator {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD1;

                float3 diff : COLOR0;
				float3 ambient : COLOR1;
                SHADOW_COORDS(1)
            };
            
            float4 _ElevationMinMax;
            float _ShoreHeight;
            float3 _ShoreColor;
            sampler2D _Gradient;

            Interpolator vert(Vertex vertex) {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(vertex.vertex);
                o.normal = vertex.normal;
                o.uv = vertex.uv;

                float3 worldNormal = UnityObjectToWorldNormal(vertex.normal);
                o.diff = _LightColor0.rgb * max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.ambient = ShadeSH9(float4(worldNormal, 1));
                TRANSFER_SHADOW(o);
                return o;
            }

            float3 frag(Interpolator vertex) : SV_Target {
                float3 color;
                if (vertex.uv.y <= _ShoreHeight)
                    color = _ShoreColor;
                else
                    color = tex2Dlod(_Gradient, float4(vertex.uv.x, 0.5, 0, 0));
                
                //const float shadow = SHADOW_ATTENUATION(vertex);
                //const float3 light = vertex.diff * shadow + vertex.ambient;
                const float3 light = float3(1, 1, 1);
                return color * light;
            }


            
            ENDCG
        }
        
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    FallBack "Diffuse"
}