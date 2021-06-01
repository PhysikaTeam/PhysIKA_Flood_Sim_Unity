Shader "Custom/WaterTess" {
	Properties{		
		_Color("Color", Color) = (0, 0, 1, 0.8)
		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
		_NormalStrength("NormalStrength", Range(0.1, 100)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0.5
		_Smoothness("Smoothness", Range(0, 1)) = 1
		_DepthDecay("DepthDecay", float) = 1
		_WaterMode("WaterMode",int) = 0
		_MaxWaterDepth("MaxWaterDepth",float) = 10.0
			_Tess("Tessellation",range(1,32)) = 32
			_FilterMode("FilterMode",range(0,3)) = 0
			B("B",range(0,1)) = 1
			C("C",range(0,1)) = 0
			_AdaptiveTess("AdaptiveTess",range(0,1)) = 0
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
			#pragma vertex tessvert
			#pragma fragment frag
			#pragma hull hs
			#pragma domain ds
			#pragma target 4.6
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"

			// compile shader into multiple variants, with and without shadows
			// (we don't care about any lightmaps yet, so skip these variants)
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight multi_compile_fog			
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 texcoord:TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 tangent : TANGENT;
				float3 normal : NORMAL;
			};

			struct InternalTessInterp_appdata {
			  float4 vertex : INTERNALTESSPOS;
			  float4 tangent : TANGENT;
			  float3 normal : NORMAL;
			  float2 texcoord : TEXCOORD0;
			};

			
			sampler2D _StateTex;
			float4 _StateTex_ST;
			float2 _StateTex_TexelSize;
			fixed4 _Color;
			float _MaxWaterDepth;
			float _DepthDecay;
			float _Tess;
			int _FilterMode;
			float B;
			float C;
			int _AdaptiveTess;
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



			
			float interPlot(float aphla1, float aphla2, float dep1, float dep2, float dep) {
				if (dep > dep2)
					dep = dep2;
				if (dep < dep1)
					dep = dep1;
				return (dep - dep1) / (dep2 - dep1) * (aphla2 - aphla1) + aphla1;
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
				float2 d = uv - uva;



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
			InternalTessInterp_appdata tessvert(appdata v) {
				InternalTessInterp_appdata o;
				o.vertex = v.vertex;
				o.tangent = v.tangent;
				o.normal = v.normal;
				o.texcoord = v.texcoord;
				return o;
			}

			v2f vert(appdata v)
			{
				//float4 state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				//float4 state = SampleBilinear(_StateTex,float2(v.texcoord.x,v.texcoord.y));
				
				
				//float4 state = bicubicSample(_StateTex,v.texcoord.xy);

				//if (_FilterMode == 0)
				//{
				//	state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				//}
				//else if (_FilterMode == 1)
				//{
				//	state = SampleBilinear(_StateTex, float2(v.texcoord.x, v.texcoord.y));
				//}
				//else if (_FilterMode == 2)
				//{
				//	state = bicubicSample(_StateTex, float2(v.texcoord.x, v.texcoord.y));
				//}
				//else
				//{
				//	state = BiCubicSample(_StateTex, float2(v.texcoord.x, v.texcoord.y));
				//}
				//float minH= FULL_HEIGHT(state),maxH = FULL_HEIGHT(state);
				//v.vertex.y += minH;

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _StateTex);
				o.normal = v.normal;
				return o;
			}

			float factorByH(float H) {
				H = min(0, max(400, H));
				return H / 50;
			}
			//
			bool checkNeedTess(sampler2D _StateTex, float2 uv) {
				float4 state = tex2Dlod1(_StateTex, uv);
				float minH=FULL_HEIGHT(state), maxH = FULL_HEIGHT(state);
				for (int offx = -1; offx < 2; offx++)
					{
						for (int offy =-1; offy <2; offy++)
						{
							float4 tt = tex2Dlod(_StateTex, float4(uv.x + 1.0/4096*offx, uv.y + 1.0/4096*offy,0,0));
							float terrainH = TERRAIN_HEIGHT(tt);
							float waterH = WATER_HEIGHT(tt);
							float fullH = FULL_HEIGHT(tt);
							if (fullH < minH)
								minH = fullH;
							if (fullH > maxH)
								maxH = fullH;
						}
					}
				if (maxH - minH > 5)
					return true;
				return false;
			}
			UnityTessellationFactors hsconst(InputPatch<InternalTessInterp_appdata, 3> v) {
				UnityTessellationFactors o;
				
				float4 state1 = tex2Dlod1(_StateTex, float2(v[0].texcoord.x, v[0].texcoord.y));
				float4 state2 = tex2Dlod1(_StateTex, float2(v[1].texcoord.x, v[1].texcoord.y));
				float4 state3 = tex2Dlod1(_StateTex, float2(v[2].texcoord.x, v[2].texcoord.y));
				float minH = min(FULL_HEIGHT(state1), min(FULL_HEIGHT(state2), FULL_HEIGHT(state3)));
				float maxH = max(FULL_HEIGHT(state1), max(FULL_HEIGHT(state2), FULL_HEIGHT(state3)));
				float maxWaterH = max(WATER_HEIGHT(state1), max(WATER_HEIGHT(state2), WATER_HEIGHT(state3)));
				float4 tf=1;

				//tf = max(1, (maxH - minH) / 2);
				if (_AdaptiveTess == 1)
				{
					//if (maxH - minH > 2 && maxWaterH > 0.00001)
					if ((checkNeedTess(_StateTex, v[0].texcoord)|| checkNeedTess(_StateTex, v[1].texcoord)|| checkNeedTess(_StateTex, v[2].texcoord))&& maxWaterH > 0.00001)
					//if ((checkNeedTess(_StateTex, v[0].texcoord) || checkNeedTess(_StateTex, v[1].texcoord) || checkNeedTess(_StateTex, v[2].texcoord)))
					tf = 16;
				}
				else
					tf = _Tess;
				//tf = _Tess;
				o.edge[0] = tf.x;
				o.edge[1] = tf.y;
				o.edge[2] = tf.z;
				o.inside = tf.w;
				return o;
			}
			
			[UNITY_domain("tri")]
			[UNITY_partitioning("fractional_odd")]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_patchconstantfunc("hsconst")]
			[UNITY_outputcontrolpoints(3)]
			InternalTessInterp_appdata hs(InputPatch<InternalTessInterp_appdata, 3> v, uint id : SV_OutputControlPointID) {
				return v[id];
			}

			[UNITY_domain("tri")]
			v2f ds(UnityTessellationFactors tessFactors, const OutputPatch<InternalTessInterp_appdata, 3> vi, float3 bary : SV_DomainLocation) {
				appdata v;

				v.vertex = vi[0].vertex * bary.x + vi[1].vertex * bary.y + vi[2].vertex * bary.z;
				v.tangent = vi[0].tangent * bary.x + vi[1].tangent * bary.y + vi[2].tangent * bary.z;
				v.normal = vi[0].normal * bary.x + vi[1].normal * bary.y + vi[2].normal * bary.z;
				v.texcoord = vi[0].texcoord * bary.x + vi[1].texcoord * bary.y + vi[2].texcoord * bary.z;
				
				float4 state;
				state = tex2Dlod(_StateTex, float4(v.texcoord.x, v.texcoord.y, 0, 0));
				//state = SampleBilinear(_StateTex, v.texcoord);
				float minH = FULL_HEIGHT(state), maxH = FULL_HEIGHT(state);

				////float maxWaterH = WATER_HEIGHT(state);
				//for (int offx = -1; offx < 2; offx++)
				//{
				//	for (int offy =-1; offy <2; offy++)
				//	{
				//		float4 tt = tex2Dlod(_StateTex, float4(v.texcoord.x + 1.0/4096*offx, v.texcoord.y + 1.0/4096*offy,0,0));
				//		float terrainH = TERRAIN_HEIGHT(tt);
				//		float waterH = WATER_HEIGHT(tt);
				//		float fullH = FULL_HEIGHT(tt);
				//		if (fullH < minH)
				//			minH = fullH;
				//		if (fullH > maxH)
				//			maxH = fullH;
				//	}
				//}
				//if (maxH - minH > 4)
				//	v.vertex.y += minH;
				////else
				if(TERRAIN_HEIGHT(state)<10)
					v.vertex.y += FULL_HEIGHT(state);

				v2f o = vert(v);
				return o;
			}

		





			fixed4 frag(v2f i) : SV_Target
			{
				//return fixed4(0,1,1,1);
				float4 state = bicubicSample(_StateTex, i.texcoord);
				if (_FilterMode == 0)
				{
					state = tex2Dlod(_StateTex, float4(i.texcoord.x, i.texcoord.y, 0, 0));
				}
				else if (_FilterMode == 1)
				{
					state = SampleBilinear(_StateTex, i.texcoord);
				}
				else if (_FilterMode == 2)
				{
					state = bicubicSample(_StateTex, i.texcoord);
				}
				else
				{
					state = BiCubicSample(_StateTex, i.texcoord);
				}

				clip(WATER_HEIGHT(state) - 0.00001);

				/*if (  dot( normalize(UnityObjectToWorldNormal(normalize(i.normal))) , float3(0, 1, 0)) < 0.1 )
					discard;*/

				float4 stateNew= tex2Dlod(_StateTex, float4(i.texcoord.x, i.texcoord.y, 0, 0));
				float H = FULL_HEIGHT(stateNew);
				float minH = FULL_HEIGHT(stateNew), maxH = FULL_HEIGHT(stateNew);
				////float maxWaterH = WATER_HEIGHT(state);
				//for (int offx = -1; offx < 2; offx++)
				//{
				//	for (int offy =-1; offy <2; offy++)
				//	{
				//		float4 tt = tex2Dlod(_StateTex, float4(i.texcoord.x + 1.0/4096*offx, i.texcoord.y + 1.0/4096*offy,0,0));
				//		float terrainH = TERRAIN_HEIGHT(tt);
				//		float waterH = WATER_HEIGHT(tt);
				//		float fullH = FULL_HEIGHT(tt);
				//		if (fullH < minH)
				//			minH = fullH;
				//		if (fullH > maxH)
				//			maxH = fullH;
				//	}
				//}
				//if (maxH - minH > 20)
				//	discard;


				//float2 du = float2(_StateTex_TexelSize.x * 0.5, 0);
				//float2 dv = float2(0, _StateTex_TexelSize.y * 0.5);

				//float4 state_l = tex2D(_StateTex, i.texcoord + du);
				//float4 state_r = tex2D(_StateTex, i.texcoord - du);
				//float4 state_t = tex2D(_StateTex, i.texcoord + dv);
				//float4 state_b = tex2D(_StateTex, i.texcoord - dv);

				//half dhdu = 0.5 * (FULL_HEIGHT(state_r) - FULL_HEIGHT(state_l));
				//half dhdv = 0.5 * (FULL_HEIGHT(state_b) - FULL_HEIGHT(state_t));

				//float3 normal = float3(dhdu, 1, dhdv);
				////if (dot(normalize(normal), float3(0, 1, 0)) < 0.8)
				////	discard; 
				////float3 normal = float3(0, 1, 0);
				//float3 worldNormal = UnityObjectToWorldNormal(normalize(normal));

		

				//float3 lightDir = _WorldSpaceLightPos0.xyz;
				//float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

				//float3 lightColor = _LightColor0.rgb;
				//float3 albedo = _Color.rgb;

				//float3 specularTint;
				//float oneMinusReflectivity;
				//albedo = DiffuseAndSpecularFromMetallic(
				//	albedo, _Metallic, specularTint, oneMinusReflectivity
				//);

				//UnityLight light;
				//light.color = lightColor;
				//light.dir = lightDir;
				//light.ndotl = DotClamped(normal, lightDir);
				//UnityIndirect indirectLight;
				//indirectLight.diffuse = 0;
				//indirectLight.specular = 0;
				//float3 reflectionDir = reflect(-viewDir, normal);
				//Unity_GlossyEnvironmentData envData;
				//envData.roughness = 1 - _Smoothness;
				//envData.reflUVW = reflectionDir;
				//indirectLight.specular = Unity_GlossyEnvironment(
				//	UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
				//);

				//half4 c = UNITY_BRDF_PBS(
				//	albedo, specularTint,
				//	oneMinusReflectivity, _Smoothness,
				//	normal, viewDir,
				//	light, indirectLight
				//);

				//float2 uv = i.screenPos.xy / i.screenPos.w;
				//#if UNITY_UV_STARTS_AT_TOP
				//if (_CameraDepthTexture_TexelSize.y < 0) {
				//	uv.y = 1 - uv.y;
				//}
				//#endif

				//float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
				//float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(i.screenPos.z);
				//float depthDifference = (backgroundDepth - surfaceDepth);
				fixed4 c;
				//c.a = saturate(clamp(depthDifference * _DepthDecay * 4, 0, 0.8) + 0.4 * saturate(depthDifference * _DepthDecay * 0.5));	
				//c.rgb *= 1.4;
				c.r = 120.0 / 255;
				c.g = 152.0 / 255;
				c.b = 174.0 / 255;
				//c.a = 1.0;
				c.a =interPlot(0.5,1.0,0,6, WATER_HEIGHT(state));
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

		
}