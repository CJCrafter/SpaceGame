Shader "Hidden/Atmosphere"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Math.cginc"
			//

			struct appdata {
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
			};

			struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float3 viewVector : TEXCOORD1;
			};

			v2f vert(appdata v) {
					v2f output;
					output.pos = UnityObjectToClipPos(v.vertex);
					output.uv = v.uv;
					// Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
					// (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
					float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
					output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
					return output;
			}

			float2 squareUV(float2 uv) {
				float width = _ScreenParams.x;
				float height = _ScreenParams.y;
				//float minDim = min(width, height);
				float scale = 1000;
				float x = uv.x * width;
				float y = uv.y * height;
				return float2 (x / scale, y / scale);
			}



			sampler2D _BlueNoise;
			sampler2D _MainTex;
			sampler2D _BakedOpticalDepth;
			sampler2D _CameraDepthTexture;

			float3 _DirToSun;

			float3 _PlanetCentre;
			float _AtmosphereRadius;
			float _OceanRadius;
			float _PlanetRadius;

			// Paramaters
			int _ScatteringPoints;
			int _OpticalDepthPoints;
			float _Intensity;
			float4 _ScatteringCoefficients;
			float _DitherStrength;
			float _DitherScale;
			float _DensityFalloff;


			float densityAtPoint(float3 densitySamplePoint) {
				float heightAboveSurface = length(densitySamplePoint - _PlanetCentre) - _PlanetRadius;
				float height01 = heightAboveSurface / (_AtmosphereRadius - _PlanetRadius);
				float localDensity = exp(-height01 * _DensityFalloff) * (1 - height01);
				return localDensity;
			}

			float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength) {
				float3 densitySamplePoint = rayOrigin;
				float stepSize = rayLength / (_OpticalDepthPoints - 1);
				float opticalDepth = 0;

				for (int i = 0; i < _OpticalDepthPoints; i++) {
					float localDensity = densityAtPoint(densitySamplePoint);
					opticalDepth += localDensity * stepSize;
					densitySamplePoint += rayDir * stepSize;
				}
				return opticalDepth;
			}

			float opticalDepthBaked(float3 rayOrigin, float3 rayDir) {

				
				float height = length(rayOrigin - _PlanetCentre) - _PlanetRadius;
				float height01 = saturate(height / (_AtmosphereRadius - _PlanetRadius));

				float uvX = 1 - (dot(normalize(rayOrigin - _PlanetCentre), rayDir) * .5 + .5);
				return tex2Dlod(_BakedOpticalDepth, float4(uvX, height01,0,0));
			}

			float opticalDepthBaked2(float3 rayOrigin, float3 rayDir, float rayLength) {
				float3 endPoint = rayOrigin + rayDir * rayLength;
				float d = dot(rayDir, normalize(rayOrigin - _PlanetCentre));
				float opticalDepth = 0;

				const float blendStrength = 1.5;
				float w = saturate(d * blendStrength + .5);

				float d1 = opticalDepthBaked(rayOrigin, rayDir) - opticalDepthBaked(endPoint, rayDir);
				float d2 = opticalDepthBaked(endPoint, -rayDir) - opticalDepthBaked(rayOrigin, -rayDir);

				opticalDepth = lerp(d2, d1, w);
				return opticalDepth;
			}

			float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 originalCol, float2 uv) {
				float blueNoise = tex2Dlod(_BlueNoise, float4(squareUV(uv) * _DitherScale,0,0));
				blueNoise = (blueNoise - 0.5) * _DitherStrength;

				float3 inScatterPoint = rayOrigin;
				float stepSize = rayLength / (_ScatteringPoints - 1);
				float3 inScatteredLight = 0;
				float viewRayOpticalDepth = 0;

				for (int i = 0; i < _ScatteringPoints; i++) {
					float sunRayLength = raySphere(_PlanetCentre, _AtmosphereRadius, inScatterPoint, _DirToSun).y;
					float sunRayOpticalDepth = opticalDepthBaked(inScatterPoint + _DirToSun * _DitherStrength, _DirToSun);
					float localDensity = densityAtPoint(inScatterPoint);
					viewRayOpticalDepth = opticalDepthBaked2(rayOrigin, rayDir, stepSize * i);
					float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * _ScatteringCoefficients);

					inScatteredLight += localDensity * transmittance;
					inScatterPoint += rayDir * stepSize;
				}
				inScatteredLight *= _ScatteringCoefficients * _Intensity * stepSize / _PlanetRadius;
				inScatteredLight += blueNoise * 0.01;

				// Attenuate brightness of original col (i.e light reflected from planet surfaces)
				// This is a hacky mess, TODO: figure out a proper way to do this
				const float brightnessAdaptionStrength = 0.15;
				const float reflectedLightOutScatterStrength = 3;
				float brightnessAdaption = dot(inScatteredLight,1) * brightnessAdaptionStrength;
				float brightnessSum = viewRayOpticalDepth * _Intensity * reflectedLightOutScatterStrength + brightnessAdaption;
				float reflectedLightStrength = exp(-brightnessSum);
				float hdrStrength = saturate(dot(originalCol,1) / 3 - 1);
				reflectedLightStrength = lerp(reflectedLightStrength, 1, hdrStrength);
				float3 reflectedLight = originalCol * reflectedLightStrength;

				float3 finalCol = reflectedLight + inScatteredLight;


				return finalCol;
			}


			float4 frag(v2f i) : SV_Target
			{
				float4 originalCol = tex2D(_MainTex, i.uv);
				float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

				float3 rayOrigin = _WorldSpaceCameraPos;
				float3 rayDir = normalize(i.viewVector);

				float dstToOcean = raySphere(_PlanetCentre, _OceanRadius, rayOrigin, rayDir);
				float dstToSurface = min(sceneDepth, dstToOcean);

				float2 hitInfo = raySphere(_PlanetCentre, _AtmosphereRadius, rayOrigin, rayDir);
				float dstToAtmosphere = hitInfo.x;
				float dstThroughAtmosphere = min(hitInfo.y, dstToSurface - dstToAtmosphere);

				if (dstThroughAtmosphere > 0) {
					const float epsilon = 0.0001;
					float3 pointInAtmosphere = rayOrigin + rayDir * (dstToAtmosphere + epsilon);
					float3 light = calculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere - epsilon * 2, originalCol, i.uv);
					return float4(light, 1);
				}
				return originalCol;
			}


			ENDCG
		}
	}
}
