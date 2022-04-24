Shader "Unlit/BlackHoleShader" {
    Properties {
        _center ("_center", Vector) = (0, 0, 0, 0)
		_singularityRadius ("_singularityRadius", Float) = 0
        _lensingRadius ("_lensingRadius", Float) = 0
        _mass ("_mass", Float) = 0
        _step ("_step", Float) = 1
        _maxSteps ("_maxSteps", Int) = 1000
        _diskWidth ("_diskWidth", Float) = 0.1
        _diskInner ("_diskInner", Range(0, 1)) = 0.0
        _diskOuter ("_diskOuter", Range(0, 1)) = 1.0
        _diskSpin ("_diskSpin", Float) = 0
        _accretionDiskBrightness ("_accretionDiskBrightness", Vector) = (0, 0, 0, 0)
        _accretionDiskTexture ("_accretionDiskTexture", 2D) = "white" {}
        _textureVars ("_textureVars", Vector) = (0, 0, 0, 0)
        _planetCount ("_planetCount", Int) = 0
        _starTexture ("_starTexture", 2D) = "black" {}
    }
    SubShader {
        //Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        //ZWrite Off
        //Blend SrcAlpha OneMinusSrcAlpha
        //Cull front 

        Pass {
            CGPROGRAM



            //#pragma debug
            //#pragma enable_d3d11_debug_symbols
            #pragma target 4.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Shaders/Math.cginc"

            struct Vertex {
                float4 vertex : POSITION;
            };

            struct Interpolator {
                float4 vertex : SV_POSITION;
                float4 world : TEXCOORD1;
                Sphere lensingSphere : TEXCOORD2;
                uint planetCount : TEXCOORD5;
            };

            struct Planet {
                float3 origin;
                float radius;
                //Texture2D<float4> texture;
            };
            
            float3 _center;
            float _singularityRadius;
            float _lensingRadius;
            float _mass;
            float _step;
            int _maxSteps;
            float _diskWidth;
            float _diskInner;
            float _diskOuter;
            float _diskSpin;
            float3 _accretionDiskBrightness;
            Texture2D<float4> _accretionDiskTexture;
            SamplerState sampler_accretionDiskTexture;
            float3 _textureVars;
            int _planetCount;
            sampler2D _starTexture;
            float _test;

            StructuredBuffer<Planet> _planets;
            
            Interpolator vert (Vertex v) {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex);

                o.lensingSphere = CreateSphere(_center, _lensingRadius);
                //uint numSpheres = 0;
                //uint stride = 0;
                //_planets.GetDimensions(numSpheres, stride);
                o.planetCount = _planetCount;
                return o;
            }

            float2 discUV(float3 planarDiscPos, float3 discDir, float3 centre, float radius) {
                float3 planarDiscPosNorm = normalize(planarDiscPos);
                float sampleDist01 = length(planarDiscPos) / radius;
             
                float3 tangentTestVector = float3(1,0,0);
                if(abs(dot(discDir, tangentTestVector)) >= 1)
                    tangentTestVector = float3(0,1,0);
             
                float3 tangent = normalize(cross(discDir, tangentTestVector));
                float3 biTangent = cross(tangent, discDir);
                float phi = atan2(dot(planarDiscPosNorm, tangent), dot(planarDiscPosNorm, biTangent)) / PI;
                phi = remap(phi, -1, 1, 0, 1);
             
                // Radial distance
                float u = sampleDist01;
                // Angular distance
                float v = phi;
             
                return float2(u,v);
            }
            
            float3 accretionDiskColor(float2 uv, float distance) {
                float col = _accretionDiskTexture.SampleLevel(sampler_accretionDiskTexture, uv * float2(1, 1), 0).r;
                float brightness = col * (1 - distance);
                return float3(1.0 * col, 0.6 * col, 0.0);
            }
            
            float3 frag(Interpolator vertex) : SV_Target {
                Ray ray = CreateRay(_WorldSpaceCameraPos, normalize(vertex.world - _WorldSpaceCameraPos));
                
                const float lenseDistance = _lensingRadius * _lensingRadius;
                const float singularityDistance = _singularityRadius * _singularityRadius;
                const float ringMin = _diskInner * _lensingRadius;
                const float ringMax = _diskOuter * _lensingRadius;
                
                int i = 0;
                const TraceResult result = IntersectSphere(ray, vertex.lensingSphere);
                bool escaped = false;
                
                while (result.collides && i < _maxSteps) {
                    
                    if (i == 0)
                        ray.origin = result.position;

                    float3 between = _center - ray.origin;
                    const float force = G * _mass / lengthSquared(between);
                    between = normalize(between) * force;

                    ray.direction = normalize(ray.direction + between * _step);
                    ray.origin += ray.direction * _step;

                    //TraceResult hit = IntersectBox(ray, accretionDisk, _step);
                    
                    float3 up = float3(0, 1, 0);
                    float3 p1 = _center - 0.5 * up * _diskWidth;
                    float3 p2 = _center + 0.5 * up * _diskWidth;
                    TraceResult hit = IntersectDisc(ray, p1, p2, up, ringMax, ringMin);
                    if (hit.collides && hit.distance < _step) {
                        float3 planarDiscPos = hit.position - dot(hit.position - _center, up) * up - _center;
                        float2 uv = discUV(planarDiscPos, up, _center, ringMax);
                        uv.y += _Time.x * _diskSpin;

                        return accretionDiskColor(uv, lengthSquared(hit.position, _center) / lenseDistance);
                        //return float4(uv.x, (uv.y + _Time.y * _diskSpin) % 1, 0.0, 1.0);
                    }

                    const float distance = lengthSquared(ray.origin, _center);
                    if (distance > lenseDistance) {
                        escaped = true;
                        break;
                    }
                    if (distance < singularityDistance)
                        break;
                    
                    i++;
                }

                // When the sphere's scale is larger then the lensing radius,
                // will appear. Let's color it a hideous color so we fix it
                // TODO consider skipping initial sphere collision 
                if (!result.collides) {
                    return float3(1, 0, 0.5);
                }

                // After a light ray escapes this black-hole, we need to check
                // if the ray will hit a planet. We should sample the color at
                // this hit UV point.
                if (escaped) {
                    TraceResult smallest = EmptyCollision();
                    smallest.distance = maxFloat;
                    
                    for (uint j = 0; j < vertex.planetCount; j++) {
                        const Planet planet = _planets[j]; 
                        const TraceResult hit = IntersectSphere(ray, CreateSphere(planet.origin, planet.radius));
                        if (hit.collides && hit.distance >= 0 && hit.distance < smallest.distance) {
                            smallest = hit;
                        }
                    }

                    // Show blue when we hit a planet todo
                    if (smallest.collides)
                        return float3(0, 0, 1);

                    const float theta = asin(-ray.direction.z);
                    const float phi = atan2(ray.direction.x, ray.direction.y);
                    return tex2D(_starTexture, float2(theta, phi));
                }

                // When the ray bounces around (seemingly) infinitely, the ray
                // is in the singularity, and thus returns pitch black.
                return float3(0, 0, 0);
            }

            
            
            ENDCG
        }
    }
}
