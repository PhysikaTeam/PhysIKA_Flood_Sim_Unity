Shader "Custom/Tess"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Tess("Tess",Range(1,32)) = 1
	}
		SubShader
		{
			Pass{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
				#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma tessellate tessFixed 
				#pragma vertex vert
				#pragma fragment frag
		   // #pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

				struct v2f
				{
					float2 uvState : TEXCOORD0;
					float4 pos : SV_POSITION;
					//	float3 normal:TEXCOORD0;
						float3 worldPos : TEXCOORD1;
						float4 screenPos : TEXCOORD2;
						float3 normal:TEXCOORD3;

					};

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			float _Tess;


				float4 tessFixed() {
				return _Tess;
			}


			v2f vert(appdata_base v)
			{
				v.vertex.y += sin(v.vertex.x+v.vertex.z);
				v2f o;
				o.normal = v.normal;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvState = v.texcoord;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(1,0,0,1);
			}
				ENDCG
}
			

			

		}
}
