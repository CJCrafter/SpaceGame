Shader "Custom/StarShader" {
    Properties {
        //[HDR]_Emission("Emission", Color) = (0, 0, 0, 1)
        _Gradient("_Gradient", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        ZWrite Off
		Lighting Off
        Cull Front

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma enable_d3d11_debug_symbols

            #include "UnityCG.cginc"

            struct Vertex {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD;
            };

            struct Interpolator {
                float4 vertex : SV_POSITION;
                float brightness : TEXCOORD0;
                float3 color : COLOR; 
            };

            //float3 _Emission;   
            sampler2D _Gradient;
    
            Interpolator vert(Vertex v) {

                // Since we are serial killers, we store the brightness of the star
                // in the vertex uv
                float brightness = v.uv[0];
                float time = v.uv[1];

                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.brightness = brightness;
                o.color = tex2Dlod(_Gradient, float4(time, 0.5, 0, 0));
                return o;
            }

            float4 frag(Interpolator vertex) : SV_Target {
                float4 color = float4(vertex.color, 1.0);
                color *= vertex.brightness;
                color.a = 1.0;
                
                return color;
            }
            ENDCG
        }
    }
}
