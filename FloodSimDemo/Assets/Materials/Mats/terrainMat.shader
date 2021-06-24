// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
Shader "Infrared/terrainMat" {
	Properties {
		_Color ("Color Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Main Tex", 2D) = "white" {}
		_matType("MatType",float) =0
		_grayIndex("GrayIndex",Range(0,1)) = 0.5
		_textureRatio("TextureRatio",Range(0,1)) = 0.2

		_FocusPos("FocusPos1",Vector) = (-1,-1,-1)
		_a("a",Range(0,100)) = 2.5

		_Intensity("intensity",Range(0,1)) = 1
	}
	SubShader {		
		Pass { 
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile INFARE NOT_INFARE
			#include "Lighting.cginc"
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _matType;
			fixed _grayIndex;
			fixed _textureRatio;
			fixed _a;
			fixed3 _FocusPos;
			fixed _Intensity;
			struct a2v {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
			};
			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};
			v2f vert(a2v v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				return o;
			}
			bool isInCircle(fixed3 Pos)
			{
				return (sqrt((Pos.x - _FocusPos.x) * (Pos.x - _FocusPos.x) + (Pos.z - _FocusPos.z) * (Pos.z - _FocusPos.z)) - _a < 0);
	
				//if(sqrt((Pos.x-_FocusPos1.x)*(Pos.x-_FocusPos1.x)+(Pos.z-_FocusPos1.z)*(Pos.z-_FocusPos1.z))+sqrt((Pos.x-_FocusPos2.x)*(Pos.x-_FocusPos2.x)+(Pos.z-_FocusPos2.z)*(Pos.z-_FocusPos2.z))-2*_a<0)
				//	return true;
				//else
				//	return false;
			}
			fixed4 frag(v2f i) : SV_Target {
				#ifdef NOT_INFARE
				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(worldNormal, worldLightDir));
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 halfDir = normalize(worldLightDir + viewDir);
				fixed3 light = (0.1,0.1,0.1);

				return fixed4(ambient + diffuse+light, 1.0);
				#endif
				#ifdef INFARE
				fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
				fixed3 Tex = albedo.r*0.33+albedo.g*0.33+albedo.b*0.33;
				//fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
				fixed gray = Tex*_textureRatio+(1-_textureRatio)*_grayIndex;
				if (isInCircle(i.worldPos))
				{
					gray = _Intensity;
				}
				return fixed4(gray,gray,gray,1.0);
				#endif
				discard;
			}
			ENDCG
		}
	}
	FallBack "Specular"
}