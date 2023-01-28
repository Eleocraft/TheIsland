Shader "Custom/Terrain/Grass"
{
	Properties
	{
		_ColorMap("BiomeMap", 2D) = "white" {}
		_TipColorDiff("Tip Color Difference", Range(0, 10)) = 0.1
		_ShadowLightness("Shadow Lightness", Range(0, 1)) = 0.5

		_BladeWidthMin("Blade Width (Min)", Range(0, 0.1)) = 0.02
		_BladeWidthMax("Blade Width (Max)", Range(0, 0.1)) = 0.05
		_BladeHeightMin("Blade Height (Min)", Range(0, 2)) = 0.1
		_BladeHeightMax("Blade Height (Max)", Range(0, 2)) = 0.2

		_BladeSegmentFalloff("Blade Segment Falloff", Range(0.01, 0.2)) = 0.1
		_BladeBendDistance("Blade Forward Amount", Float) = 0.38
		_BladeBendCurve("Blade Curvature Amount", Range(1, 4)) = 2

		_BendDelta("Bend Variation", Range(0, 1)) = 0.2

		_MaxTessellationGrassDistance("Tessellation Grass Strength", Range(0.01, 2)) = 0.1
		_TessellationGrassFalloff("Tessellation Grass Falloff", Range(0.01, 0.1)) = 0.08

		_GrassGenerationDistance("Grass Generation Distance", Range(1, 200)) = 50
		_GrassGenerationSteepness("Grass Generation Steepness", Range(0, 2)) = 1
		_GrassGenerationMinHeight("Grass Generation Height (Min)", Range(0, 100)) = 50

		_GrassMap("Grass Visibility Map", 2D) = "white" {}

		_WindMap("Wind Offset Map", 2D) = "bump" {}
		_WindFrequency("Wind Frequency", Float) = 0.2
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"
		}
		LOD 100
		Cull Off

		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT

			#define UNITY_PI 3.14159265359f
			#define UNITY_TWO_PI 6.28318530718f

			#define MAX_BLADE_SEGMENTS 5
			
			CBUFFER_START(UnityPerMaterial)
				sampler2D _ColorMap;
				float _TipColorDiff;
				float _ShadowLightness;

				float _BladeWidthMin;
				float _BladeWidthMax;
				float _BladeHeightMin;
				float _BladeHeightMax;

				float _BladeBendDistance;
				float _BladeBendCurve;
				float _BladeSegmentFalloff;

				float _BendDelta;

				float _MaxTessellationGrassDistance;
				float _TessellationGrassFalloff;

				float _GrassGenerationDistance;
				float _GrassGenerationSteepness;
				float _GrassGenerationMinHeight;
				
				sampler2D _GrassMap;
				float4 _GrassMap_ST;

				sampler2D _WindMap;
				float4 _WindMap_ST;
				float  _WindFrequency;

				float4 _ShadowColor;
			CBUFFER_END

			float4 _WindVelocity;

			struct VertexInput
			{
				float4 vertex  : POSITION;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
				float2 uv      : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 vertex  : SV_POSITION;
				float3 normal  : NORMAL;
				float4 tangent : TANGENT;
				float2 uv      : TEXCOORD0;
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside  : SV_InsideTessFactor;
			};

			struct GeomData
			{
				float4 pos : SV_POSITION;
				float2 uv  : TEXCOORD0;
				float3 normal : NORMAL;
				float3 worldPos : TEXCOORD1;
				float4 BaseColor : COLOR;
			};

			// Following functions from Roystan's code:
			// (https://github.com/IronWarrior/UnityGrassGeometryShader)

			// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
			// Extended discussion on this function can be found at the following link:
			// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
			// Returns a number in the 0...1 range.
			float rand(float3 co)
			{
				return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
			}

			// Construct a rotation matrix that rotates around the provided axis, sourced from:
			// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
			float3x3 angleAxis3x3(float angle, float3 axis)
			{
				float c, s;
				sincos(angle, s, c);

				float t = 1 - c;
				float x = axis.x;
				float y = axis.y;
				float z = axis.z;

				return float3x3
				(
					t * x * x + c, t * x * y - s * z, t * x * z + s * y,
					t * x * y + s * z, t * y * y + c, t * y * z - s * x,
					t * x * z - s * y, t * y * z + s * x, t * z * z + c
				);
			}

			// Vertex shader which just passes data to tessellation stage.
			VertexOutput tessVert(VertexInput v)
			{
				VertexOutput o;
				o.vertex = v.vertex;
				o.normal = v.normal;
				o.tangent = v.tangent;
				o.uv = v.uv;
				return o;
			}

			// Vertex shader which translates from object to world space.
			VertexOutput geomVert (VertexInput v)
            {
				VertexOutput o; 
				o.vertex = float4(TransformObjectToWorld(v.vertex.xyz), 1.0f);
				o.normal = TransformObjectToWorldNormal(v.normal);
				o.tangent = v.tangent;
				o.uv = TRANSFORM_TEX(v.uv, _GrassMap);
                return o;
            }

			// This function lets us derive the tessellation factor for an edge
			// from the vertices.
			// It also takes distance from the viewer
			// into account.
			float tessellationEdgeFactor(VertexInput vert0, VertexInput vert1)
			{
				float3 v0 = vert0.vertex.xyz;
				float3 v1 = vert1.vertex.xyz;
				float edgeLength = distance(v0, v1);

				float3 edgeCenter = (v0 + v1) * 0.5f;
				float viewDist = clamp(distance(edgeCenter, _WorldSpaceCameraPos), 1.0f, _GrassGenerationDistance) / 10.0f;

				float TessellationAmount = _TessellationGrassFalloff * viewDist;

				if (TessellationAmount < _MaxTessellationGrassDistance)
				{
					TessellationAmount = _MaxTessellationGrassDistance;
				}

				return edgeLength / TessellationAmount;
			}

			// Tessellation hull and domain shaders derived from Catlike Coding's tutorial:
			// https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/

			// The patch constant function is where we create new control
			// points on the patch. For the edges, increasing the tessellation
			// factors adds new vertices on the edge. Increasing the inside
			// will add more 'layers' inside the new triangle.
			TessellationFactors patchConstantFunc(InputPatch<VertexInput, 3> patch)
			{
				TessellationFactors f;

				f.edge[0] = tessellationEdgeFactor(patch[1], patch[2]);
				f.edge[1] = tessellationEdgeFactor(patch[2], patch[0]);
				f.edge[2] = tessellationEdgeFactor(patch[0], patch[1]);
				f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3.0f;

				return f;
			}

			// The hull function is the first half of the tessellation shader.
			// It operates on each patch (in our case, a patch is a triangle),
			// and outputs new control points for the other tessellation stages.
			//
			// The patch constant function is where we create new control points
			// (which are kind of like new vertices).
			[domain("tri")]
			[outputcontrolpoints(3)]
			[outputtopology("triangle_cw")]
			[partitioning("integer")]
			[patchconstantfunc("patchConstantFunc")]
			VertexInput hull(InputPatch<VertexInput, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			// In between the hull shader stage and the domain shader stage, the
			// tessellation stage takes place. This is where, under the hood,
			// the graphics pipeline actually generates the new vertices.

			// The domain function is the second half of the tessellation shader.
			// It interpolates the properties of the vertices (position, normal, etc.)
			// to create new vertices.
			[domain("tri")]
			VertexOutput domain(TessellationFactors factors, OutputPatch<VertexInput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
			{
				VertexInput i;

				#define INTERPOLATE(fieldname) i.fieldname = \
					patch[0].fieldname * barycentricCoordinates.x + \
					patch[1].fieldname * barycentricCoordinates.y + \
					patch[2].fieldname * barycentricCoordinates.z;

				INTERPOLATE(vertex)
				INTERPOLATE(normal)
				INTERPOLATE(tangent)
				INTERPOLATE(uv)

				return tessVert(i);
			}

			// Geometry functions derived from Roystan's tutorial:
			// https://roystan.net/articles/grass-shader.html

			// This function applies a transformation (during the geometry shader),
			// converting to clip space in the process.
			GeomData TransformGeomToClip(float3 pos, float3 offset, float3x3 transformationMatrix, float2 uv, float3 normal, float4 color)
			{
				GeomData o;

				o.pos = TransformObjectToHClip(pos + mul(transformationMatrix, offset));
				o.normal = normal;
				o.uv = uv;
				o.BaseColor = color;
				o.worldPos = TransformObjectToWorld(pos + mul(transformationMatrix, offset));

				return o;
			}

			// This is the geometry shader. For each vertex on the mesh, a leaf
			// blade is created by generating additional vertices.
			[maxvertexcount(MAX_BLADE_SEGMENTS * 2 + 1)]
			void geom(point VertexOutput input[1], inout TriangleStream<GeomData> triStream)
			{
				float grassVisibility = tex2Dlod(_GrassMap, float4(input[0].uv, 0, 0)).r;
				float terrainSteepnessRadient = acos(dot(input[0].normal,float3(0, 1, 0)));
				float distanceToCamera = distance(input[0].vertex.xyz, _WorldSpaceCameraPos);

				if (grassVisibility >= 0.5 && distanceToCamera < _GrassGenerationDistance && terrainSteepnessRadient < _GrassGenerationSteepness && input[0].vertex.y > _GrassGenerationMinHeight)
				{
					float3 pos = input[0].vertex.xyz;
					float4 BaseColor = tex2Dlod(_ColorMap, float4(input[0].uv, 0, 0));

					float3 normal = input[0].normal;
					float4 tangent = input[0].tangent;
					float3 bitangent = cross(normal, tangent.xyz) * tangent.w;

					float3x3 tangentToLocal = float3x3
					(
						tangent.x, bitangent.x, normal.x,
						tangent.y, bitangent.y, normal.y,
						tangent.z, bitangent.z, normal.z
					);

					// Rotate around the y-axis a random amount.
					float3x3 randRotMatrix = angleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1.0f));

					// Rotate around the bottom of the blade a random amount.
					float3x3 randBendMatrix = angleAxis3x3(rand(pos.zzx) * _BendDelta * UNITY_PI * 0.5f, float3(-1.0f, 0, 0));

					float2 windUV = pos.xz * _WindMap_ST.xy + _WindMap_ST.zw + normalize(_WindVelocity.xzy).xy * _WindFrequency * _Time.y;
					float2 windSample = (tex2Dlod(_WindMap, float4(windUV, 0, 0)).xy * 2 - 1) * length(_WindVelocity);

					float3 windAxis = normalize(float3(windSample.x, windSample.y, 0));
					float3x3 windMatrix = angleAxis3x3(UNITY_PI * windSample.x, windAxis);

					// Transform the grass blades to the correct tangent space.
					float3x3 baseTransformationMatrix = mul(tangentToLocal, randRotMatrix);
					float3x3 tipTransformationMatrix = mul(mul(mul(tangentToLocal, windMatrix), randBendMatrix), randRotMatrix);

					int widthMultiplier = (int)(_TessellationGrassFalloff * distanceToCamera);
					if (widthMultiplier < 1)
						widthMultiplier = 1;
					float width  = lerp(_BladeWidthMin, _BladeWidthMax, rand(pos.xzy)) * widthMultiplier;
					float height = lerp(_BladeHeightMin, _BladeHeightMax, rand(pos.zyx));
					float forward = rand(pos.yyz) * _BladeBendDistance;

					float bladeLODvalue = clamp(distanceToCamera * _BladeSegmentFalloff, 1, MAX_BLADE_SEGMENTS);
					int bladeSegments = (int)(MAX_BLADE_SEGMENTS / bladeLODvalue);

					// Create blade segments by adding two vertices at once.
					for (int i = 0; i < bladeSegments; ++i)
					{
						float t = i / (float)bladeSegments;
						float3 offset = float3(width * (1 - t), pow(abs(t), _BladeBendCurve) * forward, height * t);

						float3x3 transformationMatrix = (i == 0) ? baseTransformationMatrix : tipTransformationMatrix;

						triStream.Append(TransformGeomToClip(pos, float3( offset.x, offset.y, offset.z), transformationMatrix, float2(0, t), normal, BaseColor));
						triStream.Append(TransformGeomToClip(pos, float3(-offset.x, offset.y, offset.z), transformationMatrix, float2(1, t), normal, BaseColor));
					}

					// Add the final vertex at the tip of the grass blade.
					triStream.Append(TransformGeomToClip(pos, float3(0, forward, height), tipTransformationMatrix, float2(0.5, 1), normal, BaseColor));

					triStream.RestartStrip();
				}
			}
		ENDHLSL

		// This pass draws the grass blades generated by the geometry shader.
        Pass
        {
			Name "GrassPass"
			Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
			#pragma require geometry
			#pragma require tessellation tessHW

			//#pragma vertex vert
			#pragma vertex geomVert
			#pragma hull hull
			#pragma domain domain
			#pragma geometry geom
            #pragma fragment frag

			// The fragment shader uses the UniversalFragmentBlinnPhong Function to calculate lighting
            half4 frag (GeomData i) : SV_Target
            {
				float TipColorRDiff = _TipColorDiff * i.BaseColor.r;
				float TipColorGDiff = _TipColorDiff * i.BaseColor.g;
				float TipColorBDiff = _TipColorDiff * i.BaseColor.b;
				float4 TipColor = float4(i.BaseColor.r + TipColorRDiff, i.BaseColor.g + TipColorGDiff, i.BaseColor.b + TipColorBDiff, 1);
				float3 albedo = lerp(i.BaseColor.rgb, TipColor.rgb, i.uv.y);

				float3 worldSpaceViewDir = normalize(i.worldPos.xyz - _WorldSpaceCameraPos.xyz);

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

				return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
			}
			ENDHLSL
		}
    }
}
