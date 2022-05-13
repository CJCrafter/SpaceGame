// https://developer.nvidia.com/gpugems/gpugems2/part-ii-shading-lighting-and-shadows/chapter-16-accurate-atmospheric-scattering
// https://www.desmos.com/calculator/riheoqfknc


Shader "Celestial/AtmosphereShader" {
    Properties {
    }
    SubShader {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/Math.cginc"

            struct Vertex {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolator {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 dir : TEXCOORD1;
            };

            // This is a post processing effect
            sampler2D _MainTex;

            // Internal stuff
            float3 _sunDirection;
            float _sunIntensity;

            // Editor stuff
            float3 _planet;
            float _atmosphereRadius;
            float _elevation;
            int _outPoints;
            int _inPoints;
            float4 _wavelengths;
            float _averageDensity; // 0-1
            float _scatteringCoefficient;
            

            Interpolator vert(Vertex vertex) {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(vertex.vertex);
                o.uv = vertex.uv;
                float3 view = mul(unity_CameraInvProjection, float4(vertex.uv.xy * 2 - 1, 0, -1));
                o.dir = mul(unity_CameraToWorld, float4(view, 0));
                return o;
            }

            float CalculateShape(float cosAngle) {
                // This function is incomplete, and designed only to approximate
                // Rayleigh scattering (g -> 0). Check the docs ^^^ to find full
                const float factor = 0.589048622548; // 3pi / 16;
                return factor * (1.0 + cosAngle * cosAngle);
            }

            float CalculateDensity(float3 origin) {
                float height = distance(origin, _planet) - _elevation;
                height /= _atmosphereRadius;
                return exp(-height / _averageDensity) * (1 - height);
            }

            // Todo optimize this by using exp once (sum up all beta * distance)
            float CalculateIntensity(float intensity, float beta, float distance, float density) {
                return intensity * exp(-beta * distance * density);
            }

            bool OpticalDepth(float3 origin, float3 dir, out float accumulate)
            {
                accumulate = 0;
                float start, stop;
                raySphere(_planet, _atmosphereRadius, origin, dir, start, stop);
                float time = 0;
                float step = (stop - start) / (float) _inPoints;

                for (int i = 0; i < _inPoints; i++) {
                    float3 center = origin + dir * (time + step * 0.5);
                    float density = CalculateDensity(center);

                    // Inside the planet
                    if (density > 1)
                        return false;

                    accumulate += density * step;
                    time += step;
                }
                return true;
            }

            float CalculateLight(float3 origin, float3 dir, float start, float stop) {
                float accumulate = 0;
                float time = start;
                float step = (stop - start) / (float) _outPoints;
                
                for (int i = 0; i < _outPoints; i++) {
                    float3 center = origin + dir * (time + step * 0.5);
                    
                    float lightIn;
                    bool overground = OpticalDepth(center, _sunDirection, lightIn);
                    float lightOut = CalculateDensity(center) * step;

                    if (overground)
                    {
                        float transmittance = exp(-_scatteringCoefficient * (lightIn + lightOut) * _wavelengths);
                        accumulate += transmittance * step;
                    }
                    
                    time += step;
                }

                float cosAngle = dot(-dir, _sunDirection);
                float phase = CalculateShape(cosAngle);
                return _sunIntensity * _scatteringCoefficient * phase * _wavelengths * accumulate;
            }

            fixed4 frag(Interpolator vertex) : SV_Target {
                fixed4 col = tex2D(_MainTex, vertex.uv);
                float3 origin = _WorldSpaceCameraPos;
                float3 dir = normalize(vertex.dir);

                float start, stop;
                if (!raySphere(_planet, _atmosphereRadius, origin, dir, start, stop)) {
                    return col;
                }

                float planetStart, planetStop;
                raySphere(_planet, _elevation, origin, dir, planetStart, planetStop);
                
                float light = CalculateLight(origin, dir, start, min(planetStart, stop));
                return col * light;
            }
            ENDCG
        }
    }
}