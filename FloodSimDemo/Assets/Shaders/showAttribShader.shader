Shader "Custom/showAttribShader" {
	Properties
	{
		//_Color("Color", Color) = (1,1,1,1)
		_attLength("AttLength",Range(0,10)) = 0.01
		_vertexOption("VertexOption",int) = 0
		_normalArrowColor("NormalArrowColor",color) =(0,1,0,1)
		_tangentArrowColor("TangentArrowColor",color) = (0,0,1,1)
		
		//_tag("showTag",int)=0
		//_MainTex("Texture", 2D) = "white" {}
		//_AlphaTest("Alpha Scale", Range(0, 1)) = 0.3
	}
		SubShader
	{
		// 透明度混合队列为Transparent，所以Queue=Transparent
		// RenderType标签让Unity把这个Shader归入提前定义的组中，以指明该Shader是一个使用了透明度混合的Shader
		// IgonreProjector为True表明此Shader不受投影器（Projectors）影响
		Tags { "Queue" = "Transparent+30" "RenderType" = "Transparent" }




		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			// 关闭深度写入
			ZWrite Off
			// 开启混合模式，并设置混合因子为SrcAlpha和OneMinusSrcAlpha

			Cull Back
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normal)
			//#pragma exclude_renderers d3d11
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;

				float4 tangent:TANGENT;
				float4 color :COLOR;
				float4 texcoord : TEXCOORD0;
			};

			struct v2g
			{
				float4 color:COLOR;
				float4 pos:POSITION;
				float2 uv :TEXCOORD0;//坐标纹理集
				float3 dir:TEXCOORD1;
				float3 nor:TEXCOORD2;
			};
			struct g2f
			{
				float4 pos : SV_POSITION;
				//float2 uv : TEXCOORD0;
				//float3 worldNormal : TEXCOORD1;
				//float3 worldPos : TEXCOORD2;
				//float3 normal :TEXCOORD3;
				float4 color:TEXCOORD0;
			};

			float _attLength;
			int _vertexOption;
			float4 _normalArrowColor, _tangentArrowColor;
			v2g vert(a2v v)
			{
				v2g o;
				o.pos =v.vertex;
				o.uv = v.texcoord;
				o.uv.y = 1.0 - o.uv.y;

				float3 N = UnityObjectToWorldNormal( v.normal);
				float3 T = UnityObjectToWorldDir((v.tangent.xyz));
				N = normalize(N);
				T = normalize(T);
				//float3 B = cross(N, T)*(-v.tangent.w);
			
				float3 B = cross(N, T);
				float3x3 tangentToWorld = transpose(float3x3(T, B, N));
				

				o.color = v.color;
				o.nor = v.normal;
				//o.dir = mul(tangentToWorld, normalize(v.color*2.0 - 1.0));

				if (_vertexOption == 0)//顶点色
				{
					o.dir = normalize(v.color*2.0 - 1.0);
					o.color = v.color;
				}
					
				else if (_vertexOption == 1)//法向
				{
					o.dir = normalize(v.normal);
					o.color = _normalArrowColor;
				}
					
				else if (_vertexOption == 2)//Tangent
				{
					o.dir = normalize(v.tangent.xyz);
					o.color = _tangentArrowColor;
				}
					
		


				return o;
			}

			[maxvertexcount(6)]
			void geom(triangle v2g p[3], inout LineStream<g2f> lineStream)
			{
				g2f o;

				for (int i = 0; i < 3; i++)
				{
					o.pos = p[i].pos+float4(normalize(p[i].nor),0)*0.0001f;
					o.pos = UnityObjectToClipPos(o.pos);

					o.color = p[i].color;

					lineStream.Append(o);
					
					o.pos = p[i].pos + float4(normalize(p[i].nor), 0)*0.0001f + _attLength * float4(p[i].dir, 0.0);
					
					//o.pos = UnityWorldToClipPos(o.pos);
					o.pos = UnityObjectToClipPos(o.pos);

					lineStream.Append(o);
					lineStream.RestartStrip();
				}

			}

			fixed4 frag(g2f i) : SV_Target
			{
				//return fixed4(i.color.xyz,1.0);
				return i.color;
			}
			ENDCG
		}


		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			// 关闭深度写入
			ZWrite Off
			// 开启混合模式，并设置混合因子为SrcAlpha和OneMinusSrcAlpha

			Cull Off
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles

			// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normal)
			//#pragma exclude_renderers d3d11
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;

				float4 tangent:TANGENT;
				float4 color :COLOR;
				float4 texcoord : TEXCOORD0;
			};

			struct v2g
			{
				float4 color:COLOR;
				float4 pos:POSITION;
				float2 uv :TEXCOORD0;//坐标纹理集
				float3 dir:TEXCOORD1;
				float3 nor:TEXCOORD2;
			};
			struct g2f
			{
				float4 pos : SV_POSITION;
				//float2 uv : TEXCOORD0;
				//float3 worldNormal : TEXCOORD1;
				//float3 worldPos : TEXCOORD2;
				//float3 normal :TEXCOORD3;
				float4 color:TEXCOORD0;
			};

			float _attLength;
			int _vertexOption;
			float4 _normalArrowColor, _tangentArrowColor;
			v2g vert(a2v v)
			{
				v2g o;
				o.pos = v.vertex;
				//o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv = v.texcoord;
				o.uv.y = 1.0 - o.uv.y;

				float3 N = UnityObjectToWorldNormal(v.normal);
				float3 T = UnityObjectToWorldDir((v.tangent.xyz));
				N = normalize(N);
				T = normalize(T);
				//float3 B = cross(N, T)*(-v.tangent.w);

				float3 B = cross(N, T);
				float3x3 tangentToWorld = transpose(float3x3(T, B, N));


				o.color = v.color;
				o.nor = v.normal;
				//o.dir = mul(tangentToWorld, normalize(v.color*2.0 - 1.0));

				if (_vertexOption == 0)//顶点色
				{
					o.dir = normalize(v.color*2.0 - 1.0);
					o.color = v.color;
				}

				else if (_vertexOption == 1)//法向
				{
					o.dir = normalize(v.normal);
					o.color = fixed4(0, 1, 0,1);
				}

				else if (_vertexOption == 2)//Tangent
				{
					o.dir = normalize(v.tangent.xyz);
					o.color = fixed4(0, 0, 1, 1);
				}




				return o;
			}


			[maxvertexcount(21)]
			void geom(triangle v2g p[3], inout TriangleStream<g2f> triangleStream)
			{
				g2f o;

				for (int i = 0; i < 3; i++)
				{
					if (_vertexOption == 0)
						o.color = p[i].color;
					else if (_vertexOption == 1)
						o.color = _normalArrowColor;
					else if (_vertexOption == 2)
						o.color = _tangentArrowColor;
					o.pos = p[i].pos + _attLength * float4(p[i].dir, 0.0);
					//o.pos = UnityObjectToClipPos(o.pos);
					//triangleStream.Append(o);

					float3 axisX = normalize(float3(-p[i].dir.y, p[i].dir.x, 0));
					float3 axisY = cross(p[i].dir, axisX);

					float3 t[5];
					float arrorwScale = 0.02;
					t[0] = p[i].pos + _attLength * 1.2 * float4(p[i].dir, 0.0);
					t[1] = p[i].pos + _attLength * float4(p[i].dir, 0.0) + float4(axisX, 0.0) * arrorwScale*_attLength + float4(axisY, 0)*arrorwScale*_attLength;
					t[2] = p[i].pos + _attLength * float4(p[i].dir, 0.0) - float4(axisX, 0.0) * arrorwScale*_attLength + float4(axisY, 0) * arrorwScale*_attLength;
					t[3] = p[i].pos + _attLength * float4(p[i].dir, 0.0) - float4(axisX, 0) * arrorwScale*_attLength - float4(axisY, 0) * arrorwScale*_attLength;
					t[4] = p[i].pos + _attLength * float4(p[i].dir, 0.0) + float4(axisX, 0) * arrorwScale*_attLength - float4(axisY, 0) * arrorwScale*_attLength;

					for (int j = 1; j <= 4; j++)
					{
						o.pos = UnityObjectToClipPos(t[0]);
						triangleStream.Append(o);
						o.pos = UnityObjectToClipPos(t[j]);
						triangleStream.Append(o);
						o.pos = UnityObjectToClipPos(t[j % 4 + 1]);
						triangleStream.Append(o);
						triangleStream.RestartStrip();
					}

					o.pos = UnityObjectToClipPos(t[1]);
					triangleStream.Append(o);
					o.pos = UnityObjectToClipPos(t[2]);
					triangleStream.Append(o);
					o.pos = UnityObjectToClipPos(t[3]);
					triangleStream.Append(o);
					triangleStream.RestartStrip();
					o.pos = UnityObjectToClipPos(t[3]);
					triangleStream.Append(o);
					o.pos = UnityObjectToClipPos(t[4]);
					triangleStream.Append(o);
					o.pos = UnityObjectToClipPos(t[1]);
					triangleStream.Append(o);
					triangleStream.RestartStrip();

				}
			}
			fixed4 frag(g2f i) : SV_Target
			{
				//return fixed4(i.color.xyz,1.0);
				return i.color;
			}
			ENDCG
		}
	}
}
