Shader "Custom/Water" {
	Properties{		
		_Color("Color", Color) = (0, 0, 1, 0.8)
		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
		_NormalStrength("NormalStrength", Range(0.1, 100)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0.5
		_Smoothness("Smoothness", Range(0, 1)) = 1
		_DepthDecay("DepthDecay", float) = 1
		_WaterMode("WaterMode",int) = 0
		_MaxWaterDepth("MaxWaterDepth",float) = 10.0
			//_Tess("Tessellation",range(1,32))=32
			_FilterMode("FilterMode",range(0,3))=0
			B("B",range(0,1))=1
			C("C",range(0,1)) = 0
			//_Tess("Tess",range(1,32))=4
	}
		
	SubShader
	{
		Tags { "Queue"="Transparent"  "RenderType"="Transparent"  "IgnoreProjector"="True" }

		Pass
		{
			Tags {"LightMode" = "ForwardBase" "IgnoreProjector" = "True" }
			//LOD 300
			Blend SrcAlpha OneMinusSrcAlpha
			//Zwrite Off

			CGPROGRAM
			//#pragma tessellate tessFixed 
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight multi_compile_fog			
			#include "AutoLight.cginc"




			struct v2f
			{				
				float2 uvState : TEXCOORD0;
				float4 pos : SV_POSITION;
			//	float3 normal:TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float3 normal:TEXCOORD3;
				//SHADOW_COORDS(3)
				//UNITY_FOG_COORDS(4)
			};
			
			sampler2D _StateTex;
			float2 _StateTex_TexelSize;
			float _NormalStrength;
			float _Metallic;
			float _Smoothness;
			fixed4 _Color;
			float _MaxWaterDepth;
			sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;
			float _DepthDecay;
			int _WaterMode;
			//float _Tess;
			int _FilterMode;
			float B;
			float C;

			#define WATER_HEIGHT(s) (s.g)
			#define TERRAIN_HEIGHT(s) (s.r)
			#define FULL_HEIGHT(s) (TERRAIN_HEIGHT(s) + WATER_HEIGHT(s))
#define fWidth 4096
#define fHeight 4096
			float2 i2f(uint2 uv) {
				uv.x = max(0,min(uv.x, 4096));
				uv.y = max(0, min(uv.y, 4096));
			float2 uv2;
			uv2 = 1.0 / 4096 * uv;
			
			return uv2;
			}

			//float4 tessFixed()
			//{
			//	return _Tess;
			//}

			
			float interPlot(float aphla1, float aphla2, float dep1, float dep2, float dep) {
				if (dep > dep2)
					dep = dep2;
				if (dep < dep1)
					dep = dep1;
				return (dep - dep1) / (dep2 - dep1) * (aphla2 - aphla1) + aphla1;
			}

			float4 getColor(sampler2D tex, uint2 uv) {
				float waterDep = WATER_HEIGHT(tex2D(tex, i2f(uv)));
				if (waterDep < 0.0001)
					return float4(1, 1, 1, 0);
				float4 c;
				c.r = 120.0 / 255;
				c.g = 152.0 / 255;
				c.b = 174.0 / 255;
				c.a = interPlot(0.3, 1.0, 0, 1, waterDep);
				return c;
			}
			float4 tex2Dlod1(sampler2D tex, float2 uv) {
				return tex2Dlod(tex, float4(uv.x, uv.y, 0, 0));
			}
			float4 SampleBilinear(sampler2D tex, float2 uv)
			{
				uv = uv * 4096;
				float2 uva = floor(uv);
				float2 uvb = ceil(uv);

				uint2 id00 = (uint2)uva;  // 0 0
				uint2 id10 = uint2(uvb.x, uva.y); // 1 0
				uint2 id01 = uint2(uva.x, uvb.y); // 0 1	
				uint2 id11 = (uint2)uvb; // 1 1
				//if (id11.x == id00.x+1&&id11.y==id00.y+1)
				//	return 0;
				float2 d = uv - uva;


				/*			return
								getColor(tex, id00)* (1 - d.x)* (1 - d.y) +
								getColor(tex,id10) * d.x * (1 - d.y) +
								getColor(tex, id01) * (1 - d.x) * d.y +
								getColor(tex, id11) * d.x * d.y;*/


				return
					tex2Dlod1(tex, i2f(id00)) * (1 - d.x) * (1 - d.y) +
					tex2Dlod1(tex, i2f(id10)) * d.x * (1 - d.y) +
					tex2Dlod1(tex, i2f(id01)) * (1 - d.x) * d.y +
					tex2Dlod1(tex, i2f(id11)) * d.x * d.y;

				/*return
					tex2D(tex, i2f(id00)) * (1 - d.x) * (1 - d.y) +
					tex2D(tex, i2f(id10)) * d.x * (1 - d.y) +
					tex2D(tex, i2f(id01)) * (1 - d.x) * d.y +
					tex2D(tex, i2f(id11)) * d.x * d.y;*/
			}

			//return p1 + (0.5f * (p2 - p0) * t) + 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
			//	0.5f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t;

			float kx(float x) {
				//float B, C;
				//B = 0, C = 0.5;//Catmull_Rom
				//B = 1, C = 0;//Cubic B Spline
				//B = 0, C = 2;//(0, C) is  one-parameter family of cardinal cubics
				//B = 0.5, C = 0;//(B, 0) are Duff's tensioned B-splines

				if (x < 0)
					x = -x;
				if (x <= 1)
					return ((12 - 9 * B - 6 * C) * x * x * x + (-18 + 12 * B + 6 * C) * x * x + (6 - 2 * B)) / 6.0;
				else if (x <= 2)
				{
					return ((-B - 6 * C) * x * x * x + (6 * B + 30 * C) * x * x + (-12 * B - 48 * C) * x + (8 * B + 24 * C)) / 6.0;
				}
				else
					return 0;
			}
			float4 cubicFilter(float4 p0, float4 p1, float4 p2, float4 p3, float t) {
				float w0, w1, w2, w3;
				w0 = kx(1 + t);
				w1 = kx(t);
				w2 = kx(1 - t);
				w3 = kx(2 - t);
				return w0 * p0 + w1 * p1 + w2 * p2 + w3 * p3;
			}
			float4 BiCubicSample(sampler2D tex, float2 uv) {
				uv = uv * 4096;
				float2 uva = floor(uv);
				uint2 id = (uint2)uva;
				float2 e = uv - uva;

				uint2 tmp;

				float4 a00 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y - 1)));
				float4 a01 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y)));
				float4 a02 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 1)));
				float4 a03 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 2)));
				float4 a10 = tex2Dlod1(tex, i2f(uint2(id.x, id.y - 1)));
				float4 a11 = tex2Dlod1(tex, i2f(uint2(id.x, id.y)));
				float4 a12 = tex2Dlod1(tex, i2f(uint2(id.x, id.y + 1)));
				float4 a13 = tex2Dlod1(tex, i2f(uint2(id.x, id.y + 2)));
				float4 a20 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y - 1)));
				float4 a21 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y)));
				float4 a22 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 1)));
				float4 a23 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 2)));
				float4 a30 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y - 1)));
				float4 a31 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y)));
				float4 a32 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y + 1)));
				float4 a33 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y + 2)));

			/*	float err = 0.000001;
				if (WATER_HEIGHT(a11) < err && WATER_HEIGHT(a12) < err && WATER_HEIGHT(a21) < err && WATER_HEIGHT(a22) < err)
					return 0;*/

				float4 a = cubicFilter(a00, a01, a02, a03, e.y);
				float4 b = cubicFilter(a10, a11, a12, a13, e.y);
				float4 c = cubicFilter(a20, a21, a22, a23, e.y);
				float4 d = cubicFilter(a30, a31, a32, a33, e.y);
				float4 res = cubicFilter(a, b, c, d, e.x);
				return res;
			}

			float4 CatmullRomPoint(float4 p0, float4 p1, float4 p2, float4 p3, float t) {
				float fac = 0.5;
				float4 res = p1 + (fac* (p2 - p0) * t) + ((p2-p1)*3.0-(p3-p1)*fac-(p2-p0)*2*fac) * t * t +
					((p2-p1)*(-2.0)+(p3-p1)*fac+(p2-p0)*fac) * t * t * t;
				
				return res;
			}

			float BSpline(float x)
			{
				float f = x;
				if (f < 0.0) {
					f = -f;
				}
				if (f >= 0.0 && f < -1.0) {
					return (2.0 / 3.0) + (0.5) * (f * f * f) - (f * f);
				}
				else if (f > 1.0 && f < -2.0)
				{
					return 1.0 / 6.0 * pow((2.0 - f), 3.0);
				}
				return 1.0;
			}
			float4 BiCubicB(sampler2D textureSampler, float2 TexCoord)
			{
				float texelSizeX = 1.0 / fWidth;
				float texelSizeY = 1.0 / fHeight;
				float4 nSum = float4(0.0, 0.0, 0.0, 0.0);
				float4 nDenom = float4(0.0, 0.0, 0.0, 0.0);
				
				int nX = int(TexCoord.x * fWidth);
				int nY = int(TexCoord.y * fHeight);

				float a =(TexCoord.x * fWidth)-nX;
				float b = (TexCoord.y * fHeight)-nY;

				float2 TexCoord1 = float2(float(nX) / fWidth, float(nY) / fHeight);

				for (int m = -1; m <= 2; m++) {
					for (int n = -1; n <= 2; n++) {
						float4 vecData = tex2Dlod1(textureSampler, float2(TexCoord1.x+texelSizeX * m, TexCoord1.y+ texelSizeY * n));
						float f = BSpline(float(m) - a);
						float4 vecCooef1 = float4(f, f, f, f);
						float f1 = BSpline(-(float(n)) - b);
						float4 vecCooef2 = float4(f1, f1, f1, f1);

						/*nSum = nSum + (vecData * vecCooef2 * vecCooef1);
						nDenom = nDenom + (vecCooef1 * vecCooef2);*/
						nSum = nSum + (vecData * f * f1);
						nDenom = nDenom + (f * f1);
					}
				}
				return nSum / nDenom;
			}
			

			float4 bicubicSample(sampler2D tex, float2 uv) {
				/*float4 array[4][4];
				float4 row[4];*/
				uv = uv * 4096;
				float2 uva = floor(uv);
				uint2 id = (uint2)uva;
				float2 e = uv - uva;

				uint2 tmp;

				float4 a00 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y - 1)));
				float4 a01 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y )));
				float4 a02 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 1)));
				float4 a03 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 2)));
				float4 a10 = tex2Dlod1(tex, i2f(uint2(id.x , id.y - 1)));
				float4 a11 = tex2Dlod1(tex, i2f(uint2(id.x , id.y )));
				float4 a12 = tex2Dlod1(tex, i2f(uint2(id.x , id.y + 1)));
				float4 a13 = tex2Dlod1(tex, i2f(uint2(id.x , id.y +2 )));
				float4 a20 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y - 1)));
				float4 a21 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y )));
				float4 a22 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 1)));
				float4 a23 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 2)));
				float4 a30 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y - 1)));
				float4 a31 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y )));
				float4 a32 = tex2Dlod1(tex, i2f(uint2(id.x +2, id.y + 1)));
				float4 a33 = tex2Dlod1(tex, i2f(uint2(id.x +2, id.y + 2)));

				float err = 0.00001;
				if (WATER_HEIGHT(a11) < err && WATER_HEIGHT(a12) < err && WATER_HEIGHT(a21) < err && WATER_HEIGHT(a22) < err)
					return 0;

				float4 a = CatmullRomPoint(a00, a01, a02, a03, e.y);
				float4 b = CatmullRomPoint(a10, a11, a12, a13, e.y);
				float4 c = CatmullRomPoint(a20, a21, a22, a23, e.y);
				float4 d = CatmullRomPoint(a30, a31, a32, a33, e.y);

				float4 res= CatmullRomPoint(a,b,c,d,e.x);

				//res = a11;

				//for (int i = 0; i < 4; i++)
				//{
				//	for (int j = 0; j < 4; j++)
				//	{
				//		tmp = (uint2)(id.x + i - 1, id.y + j - 1);
				//		array[i][j] = tex2Dlod1(tex, i2f(tmp));
				//	}
				//	row[i] = CatmullRomPoint(array[i][0], array[i][1], array[i][2], array[i][3], d.y);
				//}
				//float4 res = CatmullRomPoint(row[0], row[1], row[2], row[3], d.x);
				////float4 res = tex2Dlod1(tex,i2f(id));
				return res;
			}
			



		

			v2f vert(appdata_base v)
			{
				//float4 state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				//float4 state = SampleBilinear(_StateTex,float2(v.texcoord.x,v.texcoord.y));
				float4 state = bicubicSample(_StateTex,v.texcoord.xy);

				if (_FilterMode == 0)
				{
					state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				}
				else if (_FilterMode == 1)
				{
					state = SampleBilinear(_StateTex, float2(v.texcoord.x, v.texcoord.y));
				}
				else if (_FilterMode == 2)
				{
					state = bicubicSample(_StateTex, float2(v.texcoord.x, v.texcoord.y));
				}
				else
				{
					state = BiCubicSample(_StateTex, float2(v.texcoord.x, v.texcoord.y));
				}

				float minH= FULL_HEIGHT(state),maxH = FULL_HEIGHT(state);
			////float maxWaterH = WATER_HEIGHT(state);
			//for (int offx = -2; offx < 3; offx++)
			//{
			//	for (int offy =-2; offy <3; offy++)
			//	{
			//		float4 tt = tex2Dlod(_StateTex, float4(v.texcoord.x + 1.0/2048*offx, v.texcoord.y + 1.0/2048*offy,0,0));
			//		float terrainH = TERRAIN_HEIGHT(tt);
			//		float waterH = WATER_HEIGHT(tt);
			//		float fullH = FULL_HEIGHT(tt);
			//		if (fullH < minH)
			//			minH = fullH;
			//		
			//	}
			//}
				v.vertex.y += minH;

				////if (_WaterMode == 0)
				//	v.vertex.y += FULL_HEIGHT(state);
				////else if(_WaterMode==1)
				////	v.vertex.y += TERRAIN_HEIGHT(state);
				//v.vertex.y += 0.01;
				//v.vertex.y += FULL_HEIGHT(state);
				v2f o;
				o.normal = v.normal;
				o.pos = UnityObjectToClipPos(v.vertex);				
				o.uvState = v.texcoord;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);

//				TRANSFER_SHADOW(o)
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			//float4 sample(sampler2D tex, uint2 uv) {
			//	float2 uv2;
			//	uv2 = 1.0 / 4096 * uv;
			//	return tex2D(tex, uv2);
			//}
			
			
		
			fixed4 frag(v2f i) : SV_Target
			{

				//return SampleBilinear(_StateTex,i.uvState);

				//float4 state = tex2D(_StateTex, i.uvState);
				//float4 state = SampleBilinear(_StateTex,i.uvState);
				float4 state = bicubicSample(_StateTex, i.uvState);
				if (_FilterMode == 0)
				{
					state = tex2Dlod(_StateTex, float4(i.uvState.x, i.uvState.y, 0, 0));
				}
				else if (_FilterMode == 1)
				{
					state = SampleBilinear(_StateTex, i.uvState);
				}
				else if (_FilterMode == 2)
				{
					state = bicubicSample(_StateTex, i.uvState);
				}
				else
				{
					state = BiCubicSample(_StateTex, i.uvState);
				}

				clip(WATER_HEIGHT(state) - 0.00001);


				float2 du = float2(_StateTex_TexelSize.x * 0.5, 0);
				float2 dv = float2(0, _StateTex_TexelSize.y * 0.5);

				float4 state_l = tex2D(_StateTex, i.uvState + du);
				float4 state_r = tex2D(_StateTex, i.uvState - du);
				float4 state_t = tex2D(_StateTex, i.uvState + dv);
				float4 state_b = tex2D(_StateTex, i.uvState - dv);

				half dhdu = _NormalStrength * 0.5 * (FULL_HEIGHT(state_r) - FULL_HEIGHT(state_l));
				half dhdv = _NormalStrength * 0.5 * (FULL_HEIGHT(state_b) - FULL_HEIGHT(state_t));

				float3 normal = float3(dhdu, 1, dhdv);
				//float3 normal = float3(0, 1, 0);
				float3 worldNormal = UnityObjectToWorldNormal(normalize(normal));

		

				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

				float3 lightColor = _LightColor0.rgb;
				float3 albedo = _Color.rgb;

				float3 specularTint;
				float oneMinusReflectivity;
				albedo = DiffuseAndSpecularFromMetallic(
					albedo, _Metallic, specularTint, oneMinusReflectivity
				);

				UnityLight light;
				light.color = lightColor;
				light.dir = lightDir;
				light.ndotl = DotClamped(normal, lightDir);
				UnityIndirect indirectLight;
				indirectLight.diffuse = 0;
				indirectLight.specular = 0;
				float3 reflectionDir = reflect(-viewDir, normal);
				Unity_GlossyEnvironmentData envData;
				envData.roughness = 1 - _Smoothness;
				envData.reflUVW = reflectionDir;
				indirectLight.specular = Unity_GlossyEnvironment(
					UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
				);

				half4 c = UNITY_BRDF_PBS(
					albedo, specularTint,
					oneMinusReflectivity, _Smoothness,
					normal, viewDir,
					light, indirectLight
				);

				float2 uv = i.screenPos.xy / i.screenPos.w;
				#if UNITY_UV_STARTS_AT_TOP
				if (_CameraDepthTexture_TexelSize.y < 0) {
					uv.y = 1 - uv.y;
				}
				#endif

				float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
				float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.screenPos.z);
				float depthDifference = (backgroundDepth - surfaceDepth);
				c.a = saturate(clamp(depthDifference * _DepthDecay * 4, 0, 0.8) + 0.4 * saturate(depthDifference * _DepthDecay * 0.5));	
				//c.rgb *= 1.4;
				//c.r = 120.0 / 255;
				//c.g = 152.0 / 255;
				//c.b = 174.0 / 255;
				//c.a =interPlot(0.5,1.0,0,1, WATER_HEIGHT(state));
				//if (_WaterMode == 1)
				//{
				//	//c.rgb = depthDifference;
				//					float tmp=1.0f*WATER_HEIGHT(state)/ _MaxWaterDepth;
				//					if (tmp < 0.0001)
				//						discard;
				//					else if (tmp < 0.25)
				//						c = fixed4(0.0, 0.0, 1.0, 1.0);
				//					else if (tmp < 0.50)
				//						c = fixed4(0.0, 1.0, 0.0, 1.0);
				//					else if (tmp < 0.75)
				//						c = fixed4(0.7, 0, 0, 1.0);
				//					else
				//						c= fixed4(1.0, 0.0, 0.0, 1.0);
				//}
				return c;
			}
			ENDCG
		}
	}



		/*
		SubShader{
			Tags { "RenderType" = "Transparent" "Queue"="Transparent" }			
			LOD 200
			
			ZWrite On

			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma surface surf Standard vertex:vert alpha:blend
			#pragma target 3.0
			struct Input {
				float2 uv_MainTex;				
				float3 worldRefl;				
				float4 screenPos;
				INTERNAL_DATA
			};

			float _NormalStrength;
			float _SampleSize;
			fixed4 _Color;
			sampler2D _MainTex;
			sampler2D _StateTex;
			float2 _StateTex_TexelSize;
			float4 _InputControls;
			half _Metallic;
			half _Smoothness;
			sampler2D _CameraDepthTexture;
			float4 _CameraDepthTexture_TexelSize;
			float _DepthDecay;

			#define WATER_HEIGHT(s) (s.g)
			#define TERRAIN_HEIGHT(s) (s.r)
			#define FULL_HEIGHT(s) (TERRAIN_HEIGHT(s) + WATER_HEIGHT(s))


			void vert(inout appdata_full v) {
				float4 state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));	
				v.vertex.y += state.r + state.g;
			}
			
			void surf(Input IN, inout SurfaceOutputStandard  o) {
				// Normals 
				float4 state = tex2D(_StateTex, IN.uv_MainTex.x);
				float4 state_l = tex2D(_StateTex, float2(IN.uv_MainTex.x + _StateTex_TexelSize.x, IN.uv_MainTex.y));
				float4 state_r = tex2D(_StateTex, float2(IN.uv_MainTex.x - _StateTex_TexelSize.x, IN.uv_MainTex.y));
				float4 state_t = tex2D(_StateTex, float2(IN.uv_MainTex.x, IN.uv_MainTex.y + _StateTex_TexelSize.y));
				float4 state_b = tex2D(_StateTex, float2(IN.uv_MainTex.x, IN.uv_MainTex.y - _StateTex_TexelSize.y));

				half dhhx = 0.5 * (FULL_HEIGHT(state_r) - FULL_HEIGHT(state_l));
				half dhhy = 0.5 * (FULL_HEIGHT(state_t) - FULL_HEIGHT(state_b));

				
				//half dx = 0.5 * (FULL_HEIGHT(state_l) - FULL_HEIGHT(state)) - 0.5 * (FULL_HEIGHT(state_r) - FULL_HEIGHT(state));
				//half dz = 0.5 * (FULL_HEIGHT(state_t) - FULL_HEIGHT(state)) - 0.5 * (FULL_HEIGHT(state_b) - FULL_HEIGHT(state));
				half3 normal = half3(_NormalStrength * dhhx, 1, _NormalStrength * dhhy);
				//o.Normal = UnityObjectToWorldNormal(normalize(normal));
				//o.Normal = normalize(normal);


				// Brush
				float brushPresence = saturate(sign(abs(_InputControls.z) - length(IN.uv_MainTex - _InputControls.xy)));

				float2 uv = IN.screenPos.xy / IN.screenPos.w;
				#if UNITY_UV_STARTS_AT_TOP
					if (_CameraDepthTexture_TexelSize.y < 0) {
						uv.y = 1 - uv.y;
					}
				#endif

				float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
				float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(IN.screenPos.z);
				float depthDifference = (backgroundDepth - surfaceDepth);			

				
				//clip(state.g - 0.08 + brushPresence);

				o.Metallic = _Metallic;
				o.Smoothness = _Smoothness;
				o.Alpha = saturate(clamp(depthDifference * _DepthDecay * 4, 0, 0.8) + 0.4 * saturate(depthDifference * _DepthDecay * 0.5));
				//o.Alpha = _Color.a * saturate(2 * state.g);
				//o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb * (1 - d * d * 0.1);
				//o.Albedo = (o.Normal + 1) * 0.5;
				o.Albedo = _Color.rgb * (1 - clamp(depthDifference * _DepthDecay, 0, 0.6));

				//o.Alpha = 1;
				//o.Albedo = saturate(1 - depthDifference * _DepthDecay);				
				
				o.Alpha = max(o.Alpha, brushPresence);
				o.Albedo = (1 - brushPresence) * o.Albedo + brushPresence * float3(1, 0, 0);
				
			}
			ENDCG
		}
		*/
		Fallback "Diffuse"		
}