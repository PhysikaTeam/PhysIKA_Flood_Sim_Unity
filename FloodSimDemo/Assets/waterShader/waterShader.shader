//水波纹shader
Shader "Custom/waterShader" {

	Properties{
		_Color("Main Color", Color) = (0.4706, 0.596, 0.682, 1)
		//_MainColor("MainColor",Color)=(120.0/255,152.0/255,174.0/255,0.0)
		_MianTex("MianTex",2D) = ""{}//主纹理贴图
	_NormalTex("NormalTex",2D) = ""{}//主纹理贴图
		_F("F",Range(0,29)) = 10//周期

		_A("A",Range(0,0.1)) = 0.01//振幅

		_R("R",Range(0,1)) = 0.2//水波范围

	}

		SubShader{

			pass {

				CGPROGRAM

				#pragma vertex vert

				#pragma fragment frag

				#include "unitycg.cginc"


				float4 _Color;
				sampler2D _MianTex,_NormalTex;

				float4 _MianTex_ST;

				float _F;

				float _A;

				float _R;

				struct v2f {

					float4 pos:POSITION;

					float2 uv:TEXCOORD0;

				};

				v2f vert(appdata_full v) {

					v2f o;

					o.pos = UnityObjectToClipPos(v.vertex);

					o.uv = TRANSFORM_TEX(v.texcoord,_MianTex);//宏（#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

					return o;

				}



				fixed4 frag(v2f IN) :COLOR{

					// IN.uv+=0.01*sin(IN.uv*3.14*_F+_Time.y);

					//float dis = distance(IN.uv,float2(0.5f,0.5f));//固定点发散
					
					float dis = distance(IN.uv, float2(0.0f, IN.uv.y));//固定点发散
					
					float f = saturate(1 - dis / _R);//限定范围

					//IN.uv += f * _A * sin(IN.uv.y+ 2 * _Time.y);

					IN.uv += 1.0 * _A * sin(-dis * 3.14 * _F + 10*_Time.y);//衰减水波 按发散方向荡漾
					fixed3 bump1 = UnpackNormal(tex2D(_NormalTex, IN.uv)).rgb;

					fixed4 color = _Color;

					return color;

				}

				ENDCG

			}

		}

			FallBack "Diffuse"

}

