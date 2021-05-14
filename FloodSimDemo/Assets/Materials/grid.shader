Shader "Custom/grid"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Size("Size",Range(0,4096)) = 4096
	}
		SubShader
		{
			Pass{
			Tags { "Queue" = "Transparent"  "RenderType" = "Transparent"  "IgnoreProjector" = "True" }
			LOD 200

			Blend SrcAlpha OneMinusSrcAlpha
			//Zwrite Off

			CGPROGRAM
			#pragma tessellate tessFixed 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight multi_compile_fog			
			#include "AutoLight.cginc"
			#define WATER_HEIGHT(s) (s.g)
			#define TERRAIN_HEIGHT(s) (s.r)
			#define FULL_HEIGHT(s) (TERRAIN_HEIGHT(s) + WATER_HEIGHT(s))
			int _Size;
		sampler2D _StateTex;
		float4 _Color;
			struct v2f
			{
				float2 uvState : TEXCOORD0;
				float4 pos : SV_POSITION;
				//	float3 normal:TEXCOORD0;
					float3 worldPos : TEXCOORD1;
					float4 screenPos : TEXCOORD2;
				};



				v2f vert(appdata_base v)
				{
					float4 state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
					//float4 state = SampleBilinear(_StateTex,v.texcoord.xy);
					float minH = FULL_HEIGHT(state), maxH = FULL_HEIGHT(state);
					//float maxWaterH = WATER_HEIGHT(state);
				/*	for (int offx = -2; offx < 3; offx++)
					{
						for (int offy =-2; offy <3; offy++)
						{
							float4 tt = tex2Dlod(_StateTex, float4(v.texcoord.x + 1.0/2048*offx, v.texcoord.y + 1.0/2048*offy,0,0));
							float terrainH = TERRAIN_HEIGHT(tt);
							float waterH = WATER_HEIGHT(tt);
							float fullH = FULL_HEIGHT(tt);
							if (fullH < minH)
								minH = fullH;

						}
					}*/
					v.vertex.y += minH;
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uvState = v.texcoord;
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					o.screenPos = ComputeScreenPos(o.pos);
					return o;
				}
				bool test(float t) {
					int tmp = t * _Size;
					t = t * _Size;
					if (t - tmp<0.1 && t - tmp>-0.1)
						return true;
					return false;
				}
				fixed4 frag(v2f i) : SV_Target
				{
					if (!test(i.uvState.x) && !test(i.uvState.y))
					//if (!test(i.uvState.x) )
						discard;
					
					return _Color;


				}
			ENDCG
					}
		}

    FallBack "Diffuse"
}
