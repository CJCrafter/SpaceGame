Shader "Custom/StarShader" {
    Properties {
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Cull Front

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct Interpolator {
                float4 vertex : SV_POSITION;
                float4 color : Color;
            };
    
            Interpolator vert (appdata_full v) {

                // Since we are serial killers, we store the brightness of the star
                // in the vertex uv
                float intensity = v.texcoord.x;

                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * intensity;
                return o;
            }

            fixed4 frag (Interpolator vertex) : SV_Target {
                return vertex.color;
            }
            ENDCG
        }
    }
}
