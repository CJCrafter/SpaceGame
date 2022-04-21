Shader "Unlit/BlackHoleShader" {
    Properties {
        _center ("_center", Vector) = (0, 0, 0, 0)
		_singularityRadius ("_singularityRadius", Float) = 0
        _lensingRadius ("_lensingRadius", Float) = 0
        _mass ("_mass", Float) = 0
        _step ("_step", Float) = 1
        _maxSteps ("_maxSteps", Int) = 1000
        _boxDimensions ("_boxDimensions", Vector) = (0, 0, 0, 0)
        _accretionDiskMinMax ("_accretionDiskMinmax", Vector) = (0, 0, 0, 0)
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        //Cull front 

        Pass {
            CGPROGRAM

            #pragma enable_d3d11_debug_symbols
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
            };
            
            float3 _center;
            float _singularityRadius;
            float _lensingRadius;
            float _mass;
            float _step;
            int _maxSteps;
            float3 _boxDimensions;
            float2 _accretionDiskMinMax;
            
            Interpolator vert (Vertex v) {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float4 frag(Interpolator vertex) : SV_Target {
                const Sphere singularitySphere = CreateSphere(_center, _singularityRadius, CreateMaterial(0, 0));
                const Sphere lensingSphere = CreateSphere(_center, _lensingRadius, CreateMaterial(0, 0));
                const Box accretionDisk = CreateBox(_center - _boxDimensions, _center + _boxDimensions, CreateMaterial(float3(1, 0.5, 0), 0));

                Ray ray = CreateRay(_WorldSpaceCameraPos, normalize(vertex.world - _WorldSpaceCameraPos));
                
                float lenseDistance = _lensingRadius * _lensingRadius;
                float singularityDistance = _singularityRadius * _singularityRadius;
                
                int i = 0;
                const TraceResult result = IntersectSphere(ray, lensingSphere);
                bool escaped = false;
                while (result.collides && i < _maxSteps) {

                    if (i == 0)
                        ray.origin = result.position;

                    float3 between = _center - ray.origin;
                    const float force = G * _mass / lengthSquared(between);
                    between = normalize(between) * force;

                    ray.direction = normalize(ray.direction + between * _step);
                    ray.origin += ray.direction * _step;

                    TraceResult hit = IntersectBox(ray, accretionDisk, _step);
                    if (hit.collides) {
                        const float l = length(hit.position - _center) / length(_boxDimensions);

                        if (l > _accretionDiskMinMax.x && l < _accretionDiskMinMax.y)
                            return float4(1.0 * (1 - l), 0.5 * (1 - l), 0.0, 1.0);
                    }
                    
                    if (lengthSquared(ray.origin, _center) > lenseDistance) {
                        escaped = true;
                        break;
                    } 
                    
                    i++;
                }

                if (escaped)
                    return float4(0, 0, 0, 0);
                
                return float4(0, 0, 0, 1);
            }
            
            ENDCG
        }
    }
}
