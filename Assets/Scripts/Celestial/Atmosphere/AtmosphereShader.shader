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
            #pragma enable_d3d11_debug_symbols
            
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
            float3 _wavelengths;
            float _scatteringStrength; 
            

            Interpolator vert(Vertex vertex) {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(vertex.vertex);
                o.uv = vertex.uv;
                float3 view = mul(unity_CameraInvProjection, float4(vertex.uv.xy * 2 - 1, 0, -1));
                o.dir = mul(unity_CameraToWorld, float4(view, 0));
                return o;
            }

            float CalculateShape(float cosAngle) {
                return 1;
                
                // This function is incomplete, and designed only to approximate
                // Rayleigh scattering (g -> 0). Check the docs ^^^ to find full
                const float factor = 0.589048622548; // 3pi / 16;
                return factor * (1.0 + cosAngle * cosAngle);
            }

            //float CalculateDensity(float3 origin) {
            //    float height = distance(origin, _planet) - _elevation;
            //    return exp(-height * _scatteringStrength);
            //}
            
            float CalculateDensity(float3 origin) {
                float height = distance(origin, _planet) - _elevation;
                height /= _atmosphereRadius - _elevation;
                return exp(-height * _scatteringStrength) * (1 - height);
            }

            float OpticalDepth(float3 origin, float3 dir, float stop) {
                float time = 0;
                float dt = stop / (_inPoints - 1);
                float midpointDelta = dt * 0.5;

                float accumulate = 0;
                for (int i = 0; i < _inPoints; i++) {
                    float3 midpoint = origin + dir * (time + midpointDelta);
                    float localDensity = CalculateDensity(midpoint);

                    // If we never hit the planet, we should stop the ray right
                    // there and return however much scattering has already happened.
                    if (localDensity > 1)
                        return 0;
                    
                    accumulate += localDensity * dt / _atmosphereRadius;
                    time += dt;
                }
                return accumulate;
            }

            float OpticalDepth(float3 origin, float3 dir) {
                float start, stop;
                raySphere(_planet, _atmosphereRadius, origin, dir, start, stop);
                return OpticalDepth(origin, dir, stop);
            }

            float3 CalculateLight(float3 origin, float3 direction, float start, float stop) {
                float opticalDepthBackwards = 0;
                float time = start;
                float dt = (stop - start) / (_outPoints - 1);
                float midpointDelta = dt * 0.5;

                float3 accumulate = 0;
                for (int i = 0; i < _outPoints; i++) {
                    float3 midpoint = origin + direction * (time + midpointDelta);

                    float density = CalculateDensity(midpoint);
                    float localOpticalDepth = density * dt / _atmosphereRadius ;
                    float sunOpticalDepth = OpticalDepth(midpoint, _sunDirection);
                    opticalDepthBackwards += localOpticalDepth;

                    if (sunOpticalDepth == 0)
                        continue;
                    
                    float3 transmittance = exp(1000000 * -_wavelengths * (opticalDepthBackwards + sunOpticalDepth));

                    accumulate += transmittance * localOpticalDepth;
                    time += dt;
                }

                return accumulate * _wavelengths * 100000;
            }

            float4 frag(Interpolator vertex) : SV_Target {
                float3 col = tex2D(_MainTex, vertex.uv);
                float3 origin = _WorldSpaceCameraPos;
                float3 dir = normalize(vertex.dir);

                float start, stop;
                bool hitAtmosphere = raySphere(_planet, _atmosphereRadius, origin, dir, start, stop);
                if (!hitAtmosphere)
                    return float4(col, 1);

                float planetStart, planetStop;
                bool collidesPlanet = raySphere(_planet, _elevation, origin, dir, planetStart, planetStop);
                
                float3 light = CalculateLight(origin, dir, start, collidesPlanet ? planetStart : stop);
                return float4(col + light * _sunIntensity, 1);
            }
            ENDCG
        }
    }
}