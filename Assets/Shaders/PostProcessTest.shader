Shader "Hidden/PostProcessTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"


            sampler2D _MainTex;

            float4 frag (v2f_img i) : COLOR {
                float4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                col.rgb = 1.0f - col.rgb;
                return col;
            }
            ENDCG
        }
    }
}
