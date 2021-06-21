Shader "Custom/WaterTessWithWave 2" {
	Properties{		
		_Color("Color", Color) = (0, 0, 1, 0.8)
		_StateTex("State", 2D) = "black" {}
		_NormalStrength("NormalStrength", Range(0.1, 100)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0.5
		_Smoothness("Smoothness", Range(0, 1)) = 1
		_DepthDecay("DepthDecay", float) = 1
		_WaterMode("WaterMode",int) = 0
		_MaxWaterDepth("MaxWaterDepth",float) = 6.0
			_Tess("Tessellation",range(1,32)) = 32
			_FilterMode("FilterMode",range(0,3)) = 0
			B("B",range(0,1)) = 1
			C("C",range(0,1)) = 0
			_AdaptiveTess("AdaptiveTess",range(0,1)) = 0
		_MainTex("_MainTex",2D) = "grey"{}
		_WaterNormalMap("Water normal", 2D) = "blue" {}
		_SkyBox("SkyBox", CUBE) = "" {}
		_FlowSpeed("Flow speed", float) = 1.0
		_FlowTileScale("Flow tile scale", float) = 35.0
		_NormalTileScale("Normal tile scale", float) = 10.0
	    _flowVelocity("flowVelocity", 2D) = "black" {}
		 _riverFlow("riverFlow", 2D) = "black" {}
		 _river("river",2D)="black"{}
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
				float4 vertex2 : TEXCOORD1;
	
			};

			struct InternalTessInterp_appdata {
			  float4 vertex : INTERNALTESSPOS;
			  float4 tangent : TANGENT;
			  float3 normal : NORMAL;
			  float2 texcoord : TEXCOORD0;
			  float4 vertex2 : TEXCOORD1;

			};

			
			sampler2D _StateTex, _riverFlow,_river;
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

			sampler2D _MainTex, _FlowMap, _WaterNormalMap, _flowVelocity;
			samplerCUBE _SkyBox;
			float _FlowSpeed, _FlowTileScale, _NormalTileScale;
			float4 _WaterNormalMap_ST;
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

			float2 classify(float2 flowdir, float2 texcoord) {
				float checkRiver = tex2D(_river, texcoord);
				if (checkRiver > 0.8)
				{
					float4 checkRiverColor = tex2D(_riverFlow, texcoord);
					checkRiverColor = floor(checkRiverColor * 255 + 0.1);
					uint cR = (uint)(checkRiverColor.r);

					if (cR < 40)
						flowdir = float2(-1, 1.5);
					//flowdir = float2(1, 0);
					else if (cR < 60)
						flowdir = float2(-1.2, 1.4);
					//flowdir = float2(1, 0);
					else if (cR < 80)
						flowdir = float2(0.2, 1);
					else if (cR < 100)
						flowdir = float2(2.5, 1);
					else if (cR < 120)
						flowdir = float2(4, 1);
					else
						flowdir = float2(1, 0);

				}

				flowdir = normalize(flowdir);

				//flowdir = float2(1, 0);

				float2 minV = float2(0, 1);
				int cnt = 36;
				for (int i = 0; i < cnt; i++)
				{
					float theta = 2 * 3.1415926 / cnt * (i + 1);
					float x = sin(theta);
					float y = cos(theta);
					if (dot(flowdir, float2(x, y)) > dot(flowdir, minV))
						minV = float2(x, y);
				}
				return minV;
				//return float2(0,1);
			}

			float4 getColor(float4 state, v2f i) {
				if (WATER_HEIGHT(state) < 0.00001)
					return float4(1, 1, 1, 0);
				float texScale = _FlowTileScale;
				float texScale2 = _NormalTileScale;
				float myangle;
				float transp;
				float3 myNormal;
				float2 mytexFlowCoord = i.texcoord * texScale;
				float2 ff = abs(2.0 * (frac(mytexFlowCoord)) - 1.0) - 0.5;
				ff = 0.5 - 4.0 * ff * ff * ff;
				float2 ffscale = sqrt(ff * ff + (1 - ff) * (1 - ff));
				float2 tt = TRANSFORM_TEX(i.texcoord, _WaterNormalMap);
				float2 Tcoord = tt * texScale2;
				float2 _offset = float2(_Time.x * _FlowSpeed, 0);
				float2 uv2 = i.texcoord * texScale;
				float2 flowdir = tex2D(_flowVelocity, floor(uv2) / texScale).rg;
				flowdir = classify(flowdir, i.texcoord);
				float2x2 rotmat = float2x2(flowdir.x, -flowdir.y, flowdir.y, flowdir.x);
				float2 NormalT0 = tex2D(_WaterNormalMap, mul(rotmat, Tcoord) - _offset).rg;

				flowdir = tex2D(_flowVelocity, floor(uv2 + float2(0.5, 0)) / texScale).rg;
				flowdir = classify(flowdir, i.texcoord);
				rotmat = float2x2(flowdir.x, -flowdir.y, flowdir.y, flowdir.x);
				float2 NormalT1 = tex2D(_WaterNormalMap, mul(rotmat, Tcoord) - _offset * 1.06 + 0.62).rg;

				float2 NormalTAB = ff.x * NormalT0 + (1.0 - ff.x) * NormalT1;





				flowdir = tex2D(_flowVelocity, floor(uv2 + float2(0, 0.5)) / texScale).rg;
				flowdir = classify(flowdir, i.texcoord);
				rotmat = float2x2(flowdir.x, -flowdir.y, flowdir.y, flowdir.x);
				NormalT0 = tex2D(_WaterNormalMap, mul(rotmat, Tcoord) - _offset * 1.33 + 0.27).rg;


				flowdir = tex2D(_flowVelocity, floor(uv2 + float2(0.5, 0.5)) / texScale).rg;
				flowdir = classify(flowdir, i.texcoord);
				rotmat = float2x2(flowdir.x, -flowdir.y, flowdir.y, flowdir.x);
				NormalT1 = tex2D(_WaterNormalMap, mul(rotmat, Tcoord) - _offset * 1.24).rg;

				float2 NormalTCD = ff.x * NormalT0 + (1.0 - ff.x) * NormalT1;

				float2 NormalT = ff.y * NormalTAB + (1.0 - ff.y) * NormalTCD;


				NormalT = (NormalT - 0.5) / (ffscale.y * ffscale.x);
				NormalT *= 1.0;


				transp = interPlot(0.3, 1.0, 0, 8, WATER_HEIGHT(state));;


				// and scale the normals with the transparency
				NormalT *= transp * transp;
				// assume normal of plane is 0,0,1 and produce the normalized sum of adding NormalT to it
				myNormal = float3(NormalT, sqrt(1 - NormalT.x * NormalT.x - NormalT.y * NormalT.y));

				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, i.vertex2).xyz);
				//float3 viewDir = normalize(float3(0,1,0));
				float3 reflectDir = reflect(viewDir, myNormal);
				float3 envColor = texCUBE(_SkyBox, reflectDir).rgb;

				// very ugly version of fresnel effect
				// but it gives a nice transparent water, but not too transparent
				myangle = dot(myNormal, normalize(viewDir));
				myangle = 0.95 - 0.6 * myangle * myangle;


				float4 base = _Color;

				base = float4(lerp(base.rgb, envColor, myangle * transp), 1.0);
				base.a = 1.0;
				return base;
			}


			float4 tex2Dlod1(sampler2D tex, float2 uv,v2f i) {
				float4 state=tex2Dlod(tex, float4(uv.x, uv.y, 0, 0));
				return getColor(state, i);
			}


			float4 SampleBilinear(sampler2D tex,v2f i)
			{
				float2 uv = i.texcoord;
				uv = uv * 4096;
				float2 uva = floor(uv);
				float2 uvb = ceil(uv);

				uint2 id00 = (uint2)uva;  // 0 0
				uint2 id10 = uint2(uvb.x, uva.y); // 1 0
				uint2 id01 = uint2(uva.x, uvb.y); // 0 1	
				uint2 id11 = (uint2)uvb; // 1 1
				float2 d = uv - uva;

				return
					tex2Dlod1(tex, i2f(id00),i) * (1 - d.x) * (1 - d.y) +
					tex2Dlod1(tex, i2f(id10),i) * d.x * (1 - d.y) +
					tex2Dlod1(tex, i2f(id01),i) * (1 - d.x) * d.y +
					tex2Dlod1(tex, i2f(id11),i) * d.x * d.y;
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
			float4 BiCubicSample(sampler2D tex, v2f i) {
				float2 uv = i.texcoord;
				uv = uv * 4096;
				float2 uva = floor(uv);
				uint2 id = (uint2)uva;
				float2 e = uv - uva;

				uint2 tmp;

				float4 a00 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y - 1)),i);
				float4 a01 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y)),i);
				float4 a02 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 1)),i);
				float4 a03 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 2)),i);
				float4 a10 = tex2Dlod1(tex, i2f(uint2(id.x, id.y - 1)),i);
				float4 a11 = tex2Dlod1(tex, i2f(uint2(id.x, id.y)),i);
				float4 a12 = tex2Dlod1(tex, i2f(uint2(id.x, id.y + 1)),i);
				float4 a13 = tex2Dlod1(tex, i2f(uint2(id.x, id.y + 2)),i);
				float4 a20 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y - 1)),i);
				float4 a21 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y)),i);
				float4 a22 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 1)),i);
				float4 a23 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 2)),i);
				float4 a30 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y - 1)),i);
				float4 a31 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y)),i);
				float4 a32 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y + 1)),i);
				float4 a33 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y + 2)),i);



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
			float4 bicubicSample(sampler2D tex, float2 uv,v2f i) {
				/*float4 array[4][4];
				float4 row[4];*/
				uv = uv * 4096;
				float2 uva = floor(uv);
				uint2 id = (uint2)uva;
				float2 e = uv - uva;

				uint2 tmp;

				float4 a00 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y - 1)),i);
				float4 a01 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y )),i);
				float4 a02 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 1)),i);
				float4 a03 = tex2Dlod1(tex, i2f(uint2(id.x - 1, id.y + 2)),i);
				float4 a10 = tex2Dlod1(tex, i2f(uint2(id.x , id.y - 1)),i);
				float4 a11 = tex2Dlod1(tex, i2f(uint2(id.x , id.y )),i);
				float4 a12 = tex2Dlod1(tex, i2f(uint2(id.x , id.y + 1)),i);
				float4 a13 = tex2Dlod1(tex, i2f(uint2(id.x , id.y +2 )),i);
				float4 a20 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y - 1)),i);
				float4 a21 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y )),i);
				float4 a22 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 1)),i);
				float4 a23 = tex2Dlod1(tex, i2f(uint2(id.x + 1, id.y + 2)),i);
				float4 a30 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y - 1)),i);
				float4 a31 = tex2Dlod1(tex, i2f(uint2(id.x + 2, id.y )),i);
				float4 a32 = tex2Dlod1(tex, i2f(uint2(id.x +2, id.y + 1)),i);
				float4 a33 = tex2Dlod1(tex, i2f(uint2(id.x +2, id.y + 2)),i);

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
				o.vertex2 = v.vertex;
				//o.texcoord1 = v.texcoord1;
				//o.texcoord2 = v.texcoord2;
				//o.texcoord3 = v.texcoord3;
				return o;
			}

			v2f vert(appdata v)
			{
				

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex2 = v.vertex;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _StateTex);
			//	o.texcoord2 = TRANSFORM_TEX(v.texcoord, _WaterNormalMap);
				o.normal = v.normal;

				return o;
			}

			float factorByH(float H) {
				H = min(0, max(400, H));
				return H / 50;
			}

			bool checkNeedTess(sampler2D _StateTex, float2 uv) {
				float4 state = tex2Dlod(_StateTex, float4(uv.xy,0,0));
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
				
				float4 state1 = tex2Dlod(_StateTex, float4(v[0].texcoord.x, v[0].texcoord.y,0,0));
				float4 state2 = tex2Dlod(_StateTex, float4(v[1].texcoord.x, v[1].texcoord.y,0,0));
				float4 state3 = tex2Dlod(_StateTex, float4(v[2].texcoord.x, v[2].texcoord.y,0,0));
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
				if(TERRAIN_HEIGHT(state)<8)
					v.vertex.y += FULL_HEIGHT(state);

				v2f o = vert(v);
				return o;
			}

		



			fixed4 frag(v2f i) : SV_Target
			{
				//return fixed4(0,1,1,1);
				float4 state = 0;
				if (_FilterMode == 0)
				{
					state = tex2Dlod1(_StateTex,i.texcoord,i);
				}
				else if (_FilterMode == 1)
				{
					state = SampleBilinear(_StateTex, i);
				}
				else if (_FilterMode == 2)
				{
					state = bicubicSample(_StateTex, i.texcoord, i);
				}
				else
				{
					state = BiCubicSample(_StateTex,i);
				}

				return state;

			}
			ENDCG
		}
	}	

		
}