Shader "Custom/Terrain" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_NormalMap("Normals", 2D) = "bump" {}
		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
		[NoScaleOffset] _NormalStrength("NormalStrength", Range(0.1, 100)) = 0.5
		_BumpScale("_BumpScale", Range(0.1, 100)) = 1
	}
	SubShader 
	{
		Tags { "Queue"="Geometry"  "RenderType"="Opaque"  "IgnoreProjector"="True"}

		Pass
		{
			Tags {"LightMode"="ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight multi_compile_fog			
			#include "AutoLight.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;				
				float2 uvState : TEXCOORD1;								
				float4 pos : SV_POSITION;

				SHADOW_COORDS(2) 
				UNITY_FOG_COORDS(3)
			};

			float _NormalStrength;			
			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _NormalMap;
			float _BumpScale;

			sampler2D _StateTex;
			float2 _StateTex_TexelSize;

			#define WATER_HEIGHT(s) (s.g)
			#define TERRAIN_HEIGHT(s) (s.r)
			#define FULL_HEIGHT(s) (TERRAIN_HEIGHT(s) + WATER_HEIGHT(s))

			v2f vert(appdata_base v)
			{
				float4 state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				
				v.vertex.y += TERRAIN_HEIGHT(state) ;
				//discard;
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
				//o.uv = v.texcoord;
				o.uvState = v.texcoord;				
				
				TRANSFER_SHADOW(o)
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}			


			fixed4 frag(v2f i) : SV_Target
			{				
				float2 du = float2(_StateTex_TexelSize.x * 0.5, 0);
				float2 dv = float2(0, _StateTex_TexelSize.y * 0.5);
				float4 state_l = tex2D(_StateTex, i.uvState + du);
				float4 state_r = tex2D(_StateTex, i.uvState - du);
				float4 state_t = tex2D(_StateTex, i.uvState + dv);
				float4 state_b = tex2D(_StateTex, i.uvState - dv);

				half dhdu = _NormalStrength * 0.5 * (TERRAIN_HEIGHT(state_r) - TERRAIN_HEIGHT(state_l));
				half dhdv = _NormalStrength * 0.5 * (TERRAIN_HEIGHT(state_b) - TERRAIN_HEIGHT(state_t));

				float3 normal = float3(dhdu, 1, dhdv);
				float3 bump = UnpackScaleNormal(tex2D(_NormalMap, i.uv), _BumpScale);				
				
				float3 worldNormal = UnityObjectToWorldNormal(normalize(normal + bump));

				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				half diff = nl * _LightColor0.rgb;
				half ambient = ShadeSH9(half4(worldNormal, 1));

				fixed4 col = tex2D(_MainTex, i.uv);				
				
				// compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
				fixed shadow = SHADOW_ATTENUATION(i);
				// darken light's illumination with shadow, keep ambient intact
				fixed3 lighting = diff * shadow + ambient;
				col.rgb *= lighting;

				UNITY_APPLY_FOG(i.fogCoord, col);

				
				return col;
			}
			ENDCG
		}

		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}		
			LOD 300

			ZWrite On 
			ZTest Less
			Offset 1, 1
			Cull Off
			//ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			sampler2D _StateTex;

			v2f vert(appdata_base v)
			{
				float4 state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				v.vertex.y += state.r;

				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}	
}

//Shader "Custom/Terrain" {
//	Properties{
//		_Color("Color", Color) = (0, 0, 1, 0.8)
//		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
//		_NormalStrength("NormalStrength", Range(0.1, 100)) = 0.5
//		_Metallic("Metallic", Range(0, 1)) = 0.5
//		_Smoothness("Smoothness", Range(0, 1)) = 1
//		_DepthDecay("DepthDecay", float) = 1
//		_WaterMode("WaterMode",int) = 0
//		_MaxWaterDepth("MaxWaterDepth",float) = 10.0
//			_Tess("Tessellation",range(1,32)) = 32
//	}
//
//		SubShader
//		{
//			Tags { "Queue" = "Transparent"  "RenderType" = "Transparent"  "IgnoreProjector" = "True" }
//
//			Pass
//			{
//				Tags {"LightMode" = "ForwardBase" "IgnoreProjector" = "True" }
//				//LOD 300
//				Blend SrcAlpha OneMinusSrcAlpha
//			//Zwrite Off
//
//			CGPROGRAM
//			#pragma tessellate tessFixed 
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//			#include "Lighting.cginc"
//			#include "UnityPBSLighting.cginc"
//
//			// compile shader into multiple variants, with and without shadows
//			// (we don't care about any lightmaps yet, so skip these variants)
//			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight multi_compile_fog			
//			#include "AutoLight.cginc"
//
//			struct v2f
//			{
//				float2 uvState : TEXCOORD0;
//				float4 pos : SV_POSITION;
//				//	float3 normal:TEXCOORD0;
//					float3 worldPos : TEXCOORD1;
//					float4 screenPos : TEXCOORD2;
//					float3 normal:TEXCOORD3;
//					SHADOW_COORDS(3)
//					UNITY_FOG_COORDS(4)
//				};
//
//				sampler2D _StateTex;
//				float2 _StateTex_TexelSize;
//				float _NormalStrength;
//				float _Metallic;
//				float _Smoothness;
//				fixed4 _Color;
//				float _MaxWaterDepth;
//				sampler2D _CameraDepthTexture;
//				float4 _CameraDepthTexture_TexelSize;
//				float _DepthDecay;
//				int _WaterMode;
//				float _Tess;
//				#define WATER_HEIGHT(s) (s.g)
//				#define TERRAIN_HEIGHT(s) (s.r)
//				#define FULL_HEIGHT(s) (TERRAIN_HEIGHT(s) + WATER_HEIGHT(s))
//
//				float2 i2f(uint2 uv) {
//				float2 uv2;
//				uv2 = 1.0 / 4096 * uv;
//
//				return uv2;
//				}
//
//				float4 tessFixed()
//				{
//					return _Tess;
//				}
//
//
//
//
//
//				v2f vert(appdata_base v)
//				{
//					float4 state = tex2Dlod(_StateTex,v.texcoord);
//					float minH = FULL_HEIGHT(state),maxH = FULL_HEIGHT(state);
//					v.vertex.y += TERRAIN_HEIGHT(state);
//					v2f o;
//					o.normal = v.normal;
//					o.pos = UnityObjectToClipPos(v.vertex);
//					o.uvState = v.texcoord;
//					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
//					o.screenPos = ComputeScreenPos(o.pos);
//
//					TRANSFER_SHADOW(o)
//					UNITY_TRANSFER_FOG(o, o.pos);
//					return o;
//				}
//
//				//float4 sample(sampler2D tex, uint2 uv) {
//				//	float2 uv2;
//				//	uv2 = 1.0 / 4096 * uv;
//				//	return tex2D(tex, uv2);
//				//}
//				float interPlot(float aphla1, float aphla2, float dep1, float dep2, float dep) {
//					if (dep > dep2)
//						dep = dep2;
//					if (dep < dep1)
//						dep = dep1;
//					return (dep - dep1) / (dep2 - dep1) * (aphla2 - aphla1) + aphla1;
//				}
//
//				float4 SampleBilinear(sampler2D tex, float2 uv)
//				{
//					uv = uv * 4096;
//					float2 uva = floor(uv);
//					float2 uvb = ceil(uv);
//
//					uint2 id00 = (uint2)uva;  // 0 0
//					uint2 id10 = uint2(uvb.x, uva.y); // 1 0
//					uint2 id01 = uint2(uva.x, uvb.y); // 0 1	
//					uint2 id11 = (uint2)uvb; // 1 1
//
//					float2 d = uv - uva;
//
//					return
//						tex2D(tex, i2f(id00)) * (1 - d.x) * (1 - d.y) +
//						tex2D(tex, i2f(id10)) * d.x * (1 - d.y) +
//						tex2D(tex, i2f(id01)) * (1 - d.x) * d.y +
//						tex2D(tex, i2f(id11)) * d.x * d.y;
//				}
//
//
//				fixed4 frag(v2f i) : SV_Target
//				{
//					/*float4 state = tex2D(_StateTex, i.uvState);
//					float h = WATER_HEIGHT(state);
//					fixed4 c = fixed4(0, 0, h, 0.5);
//					return c;*/
//
//
//					float4 state = tex2D(_StateTex, i.uvState);
//
//
//					
//
//
//
//
//
//
//								float2 du = float2(_StateTex_TexelSize.x * 0.5, 0);
//								float2 dv = float2(0, _StateTex_TexelSize.y * 0.5);
//
//								float4 state_l = tex2D(_StateTex, i.uvState + du);
//								float4 state_r = tex2D(_StateTex, i.uvState - du);
//								float4 state_t = tex2D(_StateTex, i.uvState + dv);
//								float4 state_b = tex2D(_StateTex, i.uvState - dv);
//
//								half dhdu = _NormalStrength * 0.5 * (FULL_HEIGHT(state_r) - FULL_HEIGHT(state_l));
//								half dhdv = _NormalStrength * 0.5 * (FULL_HEIGHT(state_b) - FULL_HEIGHT(state_t));
//
//								float3 normal = float3(dhdu, 1, dhdv);
//								//float3 normal = float3(0, 1, 0);
//								float3 worldNormal = UnityObjectToWorldNormal(normalize(normal));
//
//
//
//								float3 lightDir = _WorldSpaceLightPos0.xyz;
//								float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
//
//								float3 lightColor = _LightColor0.rgb;
//								float3 albedo = _Color.rgb;
//
//								float3 specularTint;
//								float oneMinusReflectivity;
//								albedo = DiffuseAndSpecularFromMetallic(
//									albedo, _Metallic, specularTint, oneMinusReflectivity
//								);
//
//								UnityLight light;
//								light.color = lightColor;
//								light.dir = lightDir;
//								light.ndotl = DotClamped(normal, lightDir);
//								UnityIndirect indirectLight;
//								indirectLight.diffuse = 0;
//								indirectLight.specular = 0;
//								float3 reflectionDir = reflect(-viewDir, normal);
//								Unity_GlossyEnvironmentData envData;
//								envData.roughness = 1 - _Smoothness;
//								envData.reflUVW = reflectionDir;
//								indirectLight.specular = Unity_GlossyEnvironment(
//									UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
//								);
//
//								half4 c = UNITY_BRDF_PBS(
//									albedo, specularTint,
//									oneMinusReflectivity, _Smoothness,
//									normal, viewDir,
//									light, indirectLight
//								);
//
//								float2 uv = i.screenPos.xy / i.screenPos.w;
//								#if UNITY_UV_STARTS_AT_TOP
//								if (_CameraDepthTexture_TexelSize.y < 0) {
//									uv.y = 1 - uv.y;
//								}
//								#endif
//
//								float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
//								float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.screenPos.z);
//								float depthDifference = (backgroundDepth - surfaceDepth);
//								c.a = saturate(clamp(depthDifference * _DepthDecay * 4, 0, 0.8) + 0.4 * saturate(depthDifference * _DepthDecay * 0.5));
//								c.rgb *= 1.4;
//								c.r = 1;
//								c.g = 1;
//								c.b = 0;
//								c.a = 1;
//								
//
//								return c;
//							}
//							ENDCG
//						}
//		}
//								Fallback "Diffuse"
//}