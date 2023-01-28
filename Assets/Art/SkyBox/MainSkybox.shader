Shader "Custom/Skybox"
{
    Properties
    {
        [NoScaleOffset] _SunZenithGrad ("Sun-Zenith gradient", 2D) = "white" {}
        [NoScaleOffset] _ViewZenithGrad ("View-Zenith gradient", 2D) = "white" {}
        [NoScaleOffset] _SunViewGrad ("Sun-View gradient", 2D) = "white" {}

        _ZenithDropoff ("Sun-Zenith dropoff", Range(1, 10)) = 4
        _SunColorDropoff ("Sun-Color dropoff", Range(1, 10)) = 4

        _SunRadius ("Sun radius", Range(0,0.5)) = 0.05
        _SunBloom ("Sun bloom", Range(0,0.11)) = 0.05
        _SunColor ("Sun color", Color) = (1, 1, 1, 1)
        _SunsetStart ("Sunset start angle", Range(0,1)) = 0.2

        _MoonRadius ("Moon radius", Range(0,1)) = 0.05
        _MoonExposure ("Moon exposure", Range(-16, 16)) = 0
        _MoonColor ("Moon color", Color) = (1, 1, 1, 1)
        _MoonriseStart ("Moonrise start angle", Range(0,1)) = 0.2

        [NoScaleOffset] _StarCubeMap ("Star cube map", Cube) = "black" {}
        _StarExposure ("Star exposure", Range(-16, 16)) = 0
        _StarPower ("Star power", Range(1,5)) = 1
        _StarLatitude ("Star latitude", Range(-90, 90)) = 0
        _StarSpeed ("Star speed", Float) = 0.001
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
				float _ZenithDropoff;
				float _SunColorDropoff;
                float _SunRadius;
                float _SunBloom;
                float4 _SunColor;
                float _SunsetStart;
                float _MoonRadius;
                float _MoonExposure;
                float4 _MoonColor;
                float _MoonriseStart;
                float _StarExposure;
                float _StarPower;
                float _StarLatitude;
                float _StarSpeed;
			CBUFFER_END

            struct Attributes
            {
                float4 posOS    : POSITION;
            };

            struct v2f
            {
                float4 posCS        : SV_POSITION;
                float3 viewDirWS    : TEXCOORD0;
            };

            v2f Vertex(Attributes IN)
            {
                v2f OUT = (v2f)0;
    
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.posOS.xyz);
    
                OUT.posCS = vertexInput.positionCS;
                OUT.viewDirWS = vertexInput.positionWS;

                return OUT;
            }

            float GetSunStrength(float sunViewDot, float rawSunRadius, float sunBloom)
            {
                // Preparating values
                float distFromSun = 1 - sunViewDot;
                float sunRadius = rawSunRadius * rawSunRadius;
                // falloff calculation
                return 1 - saturate((distFromSun - sunRadius) / sunBloom);
            }

            // From Inigo Quilez, https://www.iquilezles.org/www/articles/intersectors/intersectors.htm
            float sphIntersect(float3 rayDir, float3 spherePos, float radius)
            {
                float3 oc = -spherePos;
                float b = dot(oc, rayDir);
                float c = dot(oc, oc) - radius * radius;
                float h = b * b - c;
                if(h < 0.0) return -1.0;
                h = sqrt(h);
                return -b - h;
            }

            // Construct a rotation matrix that rotates around a particular axis by angle
            // From: https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
            float3x3 AngleAxis3x3(float angle, float3 axis)
            {
                float c, s;
                sincos(angle, s, c);

                float t = 1 - c;
                float x = axis.x;
                float y = axis.y;
                float z = axis.z;

                return float3x3(
                    t * x * x + c, t * x * y - s * z, t * x * z + s * y,
                    t * x * y + s * z, t * y * y + c, t * y * z - s * x,
                    t * x * z - s * y, t * y * z + s * x, t * z * z + c
                    );
            }

            // Rotate the view direction, tilt with latitude, spin with time
            float3 GetStarUVW(float3 viewDir, float latitude, float localSiderealTime)
            {
                // tilt = 0 at the north pole, where latitude = 90 degrees
                float tilt = PI * (latitude - 90) / 180;
                float3x3 tiltRotation = AngleAxis3x3(tilt, float3(1,0,0));

                // 0.75 is a texture offset for lST = 0 equals noon
                float spin = (0.75-localSiderealTime) * 2 * PI;
                float3x3 spinRotation = AngleAxis3x3(spin, float3(0, 1, 0));
                
                // The order of rotation is important
                float3x3 fullRotation = mul(spinRotation, tiltRotation);

                return mul(fullRotation,  viewDir);
            }

            TEXTURE2D(_SunZenithGrad);      SAMPLER(sampler_SunZenithGrad);
            TEXTURE2D(_ViewZenithGrad);     SAMPLER(sampler_ViewZenithGrad);
            TEXTURE2D(_SunViewGrad);        SAMPLER(sampler_SunViewGrad);
            TEXTURECUBE(_StarCubeMap);      SAMPLER(sampler_StarCubeMap);

            float3 _SunDir, _MoonDir;

            float4 Fragment (v2f IN) : SV_TARGET
            {
                float3 viewDir = normalize(IN.viewDirWS);

                // Main angles
                float sunViewDot = dot(_SunDir, viewDir);
                float sunZenithDot = _SunDir.y;
                float viewZenithDot = viewDir.y;
                float sunMoonDot = dot(_SunDir, _MoonDir);

                // Remapping to 01 range
                float sunViewDot01 = (sunViewDot + 1.0) * 0.5;
                float sunZenithDot01 = (sunZenithDot + 1.0) * 0.5;

                // Sky colours
                // General sky color
                float3 sunZenithColor = SAMPLE_TEXTURE2D(_SunZenithGrad, sampler_SunZenithGrad, float2(sunZenithDot01, 0.5)).rgb;

                // Zenith color (around the horizon)
                float3 viewZenithColor = SAMPLE_TEXTURE2D(_ViewZenithGrad, sampler_ViewZenithGrad, float2(sunZenithDot01, 0.5)).rgb;
                float vzMask = pow(saturate(1.0 - viewZenithDot), _ZenithDropoff);

                // Sun view color (around the sun)
                float3 sunViewColor = SAMPLE_TEXTURE2D(_SunViewGrad, sampler_SunViewGrad, float2(sunZenithDot01, 0.5)).rgb;
                float svMask = pow(saturate(sunViewDot), _SunColorDropoff);

                float3 skyColor = sunZenithColor + vzMask * viewZenithColor + svMask * sunViewColor;

                // The sun
                float dayStrength = saturate(sunZenithDot / _SunsetStart);
                float sunStrength = GetSunStrength(sunViewDot, _SunRadius, _SunBloom) * dayStrength;
                float3 sunColor = _SunColor.rgb * sunStrength;

                // The moon
                float moonIntersect = sphIntersect(viewDir, _MoonDir, _MoonRadius);
                float moonMask = moonIntersect > -1 ? 1 : 0;
                float3 moonNormal = normalize(_MoonDir - viewDir * moonIntersect);
                float moonNdotL = saturate(dot(moonNormal, -_SunDir));
                float moonStrength = (1 - sunViewDot01) * (1 - saturate(sunZenithDot / _MoonriseStart));
                float3 moonColor = moonMask * moonNdotL * exp2(_MoonExposure) * moonStrength * _MoonColor.rgb;

                // The stars
                float3 starUVW = GetStarUVW(viewDir, _StarLatitude, _Time.y * _StarSpeed % 1);
                float3 starColor = SAMPLE_TEXTURECUBE_BIAS(_StarCubeMap, sampler_StarCubeMap, starUVW, -1).rgb;
                starColor = pow(abs(starColor), _StarPower);
                float starStrength = (1 - sunViewDot01) * saturate(-sunZenithDot);
                starColor *= (1 - sunStrength) * (1 - moonMask) * exp2(_StarExposure) * starStrength;

                // Mixing everything together
                skyColor *= (1 - sunStrength);
                float3 col = skyColor + sunColor + moonColor + starColor;
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}