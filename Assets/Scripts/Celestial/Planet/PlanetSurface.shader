Shader "Celestial/PlanetSurface" {
    Properties {
        _ElevationMinMax("_ElevationMinMax", Vector) = (0, 0, 0, 0)
        _ShoreHeight("_ShoreHeight", Float) = 0
        _ShoreColor("_ShoreColor", Color) = (0, 0, 0, 0)
        
        _Gradient ("_Gradient", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct appdata {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        
        struct Input {
            float4 data;
            
        };

        float4 _ElevationMinMax;
        float _ShoreHeight;
        float3 _ShoreColor;
        sampler2D _Gradient;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void vert (inout appdata vertex, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            o.data = vertex.texcoord;
        }
       
        void surf (Input IN, inout SurfaceOutputStandard o) {
            float4 c;
            if (IN.data.y <= _ShoreHeight)
                c = float4(_ShoreColor * _Color, 0);
            else
                c = tex2Dlod (_Gradient, float4(IN.data.x, 0.5, 0, 0));

            //c = float4(1, 1, 1, 0);
            o.Albedo = c.rgb;   
            //o.Alpha = 0;
            o.Metallic = _Metallic;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
