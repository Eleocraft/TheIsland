Shader "Custom/Default"
{
    Properties
    {
		_MainColor("Main Color", Color) = (0, 0, 0, 1)
        _MainTex("Texture", 2D) = "white" {}
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
        LOD 100

        HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTERED_RENDERING

			#pragma multi_compile_fog
			
			CBUFFER_START(UnityPerMaterial)
				float4 _MainColor;
				sampler2D _MainTex;
                float _ShadowLightness;
			CBUFFER_END

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
                float3 albedo = tex2Dlod(_MainTex, float4(i.uv, 0, 0)) * _MainColor;

                float3 worldSpaceViewDir = GetWorldSpaceNormalizeViewDir(i.worldPos.xyz);
				
				InputData lightingInput = (InputData) 0;
				lightingInput.positionWS = i.worldPos;
				lightingInput.normalWS = i.normal; // No need to normalize, triangles share a normal
				lightingInput.viewDirectionWS = worldSpaceViewDir; // Calculate the view direction
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = i.worldPos;
				lightingInput.shadowCoord = GetShadowCoord(vertexInput);


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

				return UniversalFragmentPBR(lightingInput, surfaceInput);
			}
			ENDHLSL
        }
    }
}
