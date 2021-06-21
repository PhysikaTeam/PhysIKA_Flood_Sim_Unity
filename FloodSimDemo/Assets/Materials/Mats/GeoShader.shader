Shader "Unlit/GeoShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_TestTex("TestTexture", 2D) = "white"{}
		[NoScaleOffset] _flowVelocity("flowVelocity", 2D) = "black" {}
		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
		_Weight("Weight", Range(0,1)) = 0.2
		_Height("Length", float) = 0.5
		_Offset("Offset", float) = 0.1

		_StripColor("StripColor", Color) = (1, 1, 1, 1)
		_OutColor("OutColor", Color) = (1, 1, 1, 1)
		_InColor("InColor", Color) = (1, 1, 1, 1)
		_Sparse("Sparse",range(1,32))=1
	}
	SubShader{
		Cull off

		Pass {
			Tags {
				//"RenderType" = "Opaque"
			"RenderType" = "Transparent"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
			};

			struct v2f {
				float4 vertex: SV_POSITION;
				float4 objPos: TEXCOORD1;
				float2 uv: TEXCOORD0;
			};

			sampler2D _MainTex, _StateTex;
			float4 _MainTex_ST;
			sampler2D _flowVelocity;
			float _Height;
			float _Offset;
			fixed4 _StripColor;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.objPos = v.vertex;
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				discard;
				return col;
			}
			ENDCG
		}

		pass {
			Tags {
				"RenderType" = "Opaque"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geome
			#pragma fragment frag
			#include "UnityCG.cginc"

			int _Sparse;
			fixed4 _OutColor;
			fixed4 _InColor;
			float _Height;
			float _Offset;
			float _Weight;
			float4 _MainTex_TexelSize;
			float4 _TestTex_TexelSize;
			sampler2D _flowVelocity, _StateTex;
			struct appdata {
				float4 vertex: POSITION;
				float2 uv: TEXCOORD0;
				float3 normal: NORMAL;
			};

			struct v2g {
				float4 objPos: TEXCOORD0;
				float3 normal: NORMAL;
				float2 flow: TEXCOORD1;
				float2 uv: TEXCOORD2;
				float speed : TEXCOORD3;
			};

			struct g2f {
				float4 vertex: SV_POSITION;
				
				fixed4 col : TEXCOORD0;

				
			};

			void ADD_VERT(float4 v, float sp, g2f o, inout TriangleStream < g2f > outstream) {
				o.vertex = UnityObjectToClipPos(v);
				o.col = float4(sp / 1.0, 1 - sp / 1.0, 0, 1);
				if (sp < 0.0001)
					o.col = float4(1, 1, 1, 0);
				//o.col = float4(0.8, 0.2, 0, 1);
				outstream.Append(o);
			}
			bool testSparse(float2 uv) {
				uint2 uuvv = (uint2)(uv * 4096);
				return (uuvv.x % _Sparse == 0 && uuvv.y % _Sparse == 0);
			}
			v2g vert(appdata v) {
				v2g o;
				//v.vertex.y += 100;
				o.objPos = v.vertex;
				//o.objPos = UnityObjectToClipPos(v.vertex);//坐标转换
				//o.objPos = mul(unity_ObjectToWorld, v.vertex);
				//o.objPos = ComputeScreenPos(o.objPos);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				float4 state = tex2Dlod(_StateTex, float4(v.uv, 0, 0));
				if (state.g > 0.0000001)
					o.flow = tex2Dlod(_flowVelocity, float4(v.uv, 0, 0)).rg;
				else
					o.flow = float2(0, 0);
				if (testSparse(v.uv) == false)
					o.flow = float2(0, 0);
				

				o.uv = v.uv;
				/*float w = _TestTex_TexelSize.x;
				float h = _TestTex_TexelSize.y;
				w = v.uv.x / w;
				h = v.uv.y / h;
				o.index = uint2(w, h);*/
				o.speed = sqrt(o.flow.x* o.flow.x+ o.flow.y* o.flow.y);
				return o;
			}

			[maxvertexcount(3)]
			void geome(triangle v2g input[3],uint primID : SV_PrimitiveID, inout TriangleStream < g2f > outStream) {
				g2f o;

				//if (input[0].speed < 0.00001)
				//	return;
				if (input[0].uv.y == input[1].uv.y)
				{
					if (input[2].uv.y < input[0].uv.y)
						return;
				}
				else if (input[1].uv.y == input[2].uv.y)
				{
					if (input[0].uv.y < input[1].uv.y)
						return;
				}
				else if (input[0].uv.y == input[2].uv.y)
				{
					if (input[1].uv.y < input[0].uv.y)
						return;
				}
				/*if (input[0].index.y % 4 == 1)
					return;*/
				/*if (primID % 2 != 0)
					return;*/
				//float4 vertex = input[0].objPos;
				float4 vertex = (input[0].objPos + input[1].objPos + input[2].objPos) / 3.0f;
				float2 flow = -normalize(input[0].flow);

				float2 f_flow = normalize(float2(-flow.y, flow.x));
				//float2 f_flow = normalize(float2(flow.y, flow.x));
				
				//float2 f_flow = float2(-0.5, 0.5);
				float w = vertex.w;

				
				//viewpos
				/*float4 offset_0 = float4(0, 0, 1 / w, 0);
				float4 offset_1 = float4(flow.x, -flow.y, 1 / w, 0);
				float4 offset_2 = float4(_Weight*f_flow.x, -_Weight*f_flow.y, 1 / w, 0);
				float4 offset_3 = float4(-_Weight*f_flow.x, _Weight*f_flow.y, 1 / w, 0);*/
				//worldpos
				//float4 offset_all = float4(-1, 0, -1, 0);
				float sp = (input[0].speed + input[1].speed + input[2].speed) / 3.0f;
				float weight = (abs(input[0].objPos.x - input[1].objPos.x) + abs(input[1].objPos.x - input[2].objPos.x) + abs(input[0].objPos.x - input[2].objPos.x)) / 3;

				flow = -flow * weight;
				f_flow = -f_flow * weight;

				float4 offset_all = float4(0, 0, 0, 0);
				float4 offset_0 = float4(0, 0, 0, 0);
				float4 offset_1 = float4(flow.x, 0, flow.y, 0);
				float4 offset_2 = float4(_Weight * f_flow.x, 0, _Weight * f_flow.y, 0);
				float4 offset_3 = float4(-_Weight * f_flow.x, 0, -_Weight * f_flow.y, 0);

				float4 vertex_1 = vertex + offset_1 + offset_all;
				float4 vertex_2 = vertex + offset_2 + offset_all;
				float4 vertex_3 = vertex + offset_3 + offset_all;
				//ADD_VERT(vertex + offset_0, o, outStream);
				ADD_VERT(vertex + (offset_2 + offset_all)* _Height, sp, o, outStream);
				ADD_VERT(vertex + (offset_1 + offset_all)* _Height, sp, o, outStream);
				ADD_VERT(vertex + (offset_3 + offset_all)* _Height, sp, o, outStream);
				/*ADD_VERT(input[0].objPos, o, outStream);
				ADD_VERT(input[1].objPos, o, outStream);
				ADD_VERT(input[2].objPos, o, outStream);*/
				

				/*float4 offset_0 = float4(0, 1/w, 0, 0);
				float4 offset_1 = float4(-0.5, 1/w, 0, 0);
				ADD_VERT(vertex + offset_0, o, outStream);
				ADD_VERT(vertex + offset_1, o, outStream);*/
			}

			fixed4 frag(g2f i) : SV_Target {
				fixed4 col = i.col;
			if (col.a < 0.000001)
				discard;
			return col;
			}
			ENDCG

		}
	}
}
