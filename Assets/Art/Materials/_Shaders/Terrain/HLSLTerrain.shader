Shader "Custom/Terrain/Terrain"
{
    Properties
    {
		_BiomeMap("Texture", 2D) = "white" {}
		_CliffColor("Cliff Color", Color) = (0, 0, 0, 1)
		_OceanColor("Cliff Color", Color) = (0, 0, 0, 1)
        _ShadowLightness("Shadow Lightness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags 
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"
			"UniversalMaterialType" = "Lit"
        }
        LOD 300

        HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT

			#define GRADIENT_COLOR_COUNT 3
			#define BIOME_COUNT 6
			
			CBUFFER_START(UnityPerMaterial)
				sampler2D _BiomeMap;
				float4 _CliffColor;
				float4 _OceanColor;
				float4 _BiomeGradientColors[BIOME_COUNT][GRADIENT_COLOR_COUNT];
				float _BiomeGradientTimes[BIOME_COUNT][GRADIENT_COLOR_COUNT];
                float _ShadowLightness;
			CBUFFER_END

			struct Gradient
			{
				float4 _BiomeGradientColors[GRADIENT_COLOR_COUNT];
				float _BiomeGradientTimes[GRADIENT_COLOR_COUNT];
			};

			struct Attributes
			{
				float4 vertex  : POSITION;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
				float2 uv      : TEXCOORD0;
			};

			struct Varyings
			{
				float4 vertex  : SV_POSITION;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
				float2 uv      : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				half  fogFactor : TEXCOORD5;
			};

            // Vertex shader
            Varyings vert(Attributes v)
			{
				Varyings o;
				o.vertex = TransformObjectToHClip(v.vertex);
				o.normal = TransformObjectToWorldNormal(v.normal);
				o.tangent = v.tangent;
				o.uv = v.uv;
				o.worldPos = TransformObjectToWorld(v.vertex);
				o.fogFactor = ComputeFogFactor(o.vertex.z);
				return o;
			}
            ENDHLSL

        Pass
        {
            Name "MainPass"
			Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
			//#pragma vertex vert
			#pragma vertex vert
            #pragma fragment frag

			// The fragment shader uses the UniversalFragmentBlinnPhong Function to calculate lighting
            half4 frag (Varyings i) : SV_Target
            {
                float3 albedo = tex2Dlod(_BiomeMap, float4(i.uv, 0, 0));

                float3 worldSpaceViewDir = GetWorldSpaceNormalizeViewDir(i.worldPos.xyz);

				SurfaceData surfaceInput = (SurfaceData) 0;
				surfaceInput.albedo = albedo * (1 - _ShadowLightness);
				surfaceInput.alpha = 1;
				surfaceInput.emission = albedo * _ShadowLightness;
				surfaceInput.metallic = 0;
				surfaceInput.occlusion = 1;
				surfaceInput.smoothness = 0;
				surfaceInput.specular = 1;
				surfaceInput.clearCoatMask = 0;
				surfaceInput.clearCoatSmoothness = 1;


				InputData lightingInput = (InputData) 0;
				lightingInput.positionWS = i.worldPos;
				lightingInput.normalWS = i.normal; // No need to normalize, triangles share a normal
				lightingInput.viewDirectionWS = worldSpaceViewDir; // Calculate the view direction
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = i.worldPos;
				lightingInput.shadowCoord = GetShadowCoord(vertexInput);
				lightingInput.fogCoord = InitializeInputDataFog(float4(i.worldPos, 1.0), i.fogFactor);

				half4 color =  UniversalFragmentPBR(lightingInput, surfaceInput);
				color.rgb = MixFog(color.rgb, lightingInput.fogCoord);
				return color;
			}
			ENDHLSL
        }
    }
}
