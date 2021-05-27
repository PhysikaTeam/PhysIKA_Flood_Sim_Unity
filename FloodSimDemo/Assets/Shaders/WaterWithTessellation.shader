Shader "Custom/WaterWithTessellation"
{
    Properties
    {
	   _Color("Color", Color) = (0, 0, 1, 0.8)
		[NoScaleOffset] _StateTex("State", 2D) = "black" {}
		[NoScaleOffset] _StateTex2("State2", 2D) = "black" {}
		_NormalStrength("NormalStrength", Range(0.1, 100)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0
		_Glossiness("Smoothness", Range(0,1)) = 0
		_Smoothness("Smoothness", Range(0, 1)) = 1
		_DepthDecay("DepthDecay", float) = 1
		_WaterMode("WaterMode",int) = 0
		_MaxWaterDepth("MaxWaterDepth",float) = 10.0
			_Tess("Tessellation",range(1,32)) = 32
			_FilterMode("FilterMode",range(0,3)) = 0
			B("B",range(0,1)) = 1
			C("C",range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent"  "RenderType" = "Transparent"  "IgnoreProjector" = "True" }
        //LOD 200

		Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
		//#pragma tessellate tessFixed 
		//#pragma vertex disp        
		#pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessFixed 
			//#pragma surface surf Blinn-Phong
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			#include "Tessellation.cginc"
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.6
		sampler2D _StateTex;
		sampler2D _StateTex2;
		float2 _StateTex_TexelSize;
		float _NormalStrength;
		float _Metallic;
		float _Smoothness;
		float _Glossiness;
		fixed4 _Color;
		float _MaxWaterDepth;
		int _WaterMode;
		float _Tess;
		int _FilterMode;
		float B;
		float C;
        struct Input
        {
            float2 uv_MainTex;
        };
		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
		};

#define WATER_HEIGHT(s) (s.g)
#define TERRAIN_HEIGHT(s) (s.r)
#define FULL_HEIGHT(s) (TERRAIN_HEIGHT(s) + WATER_HEIGHT(s))
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


		float2 i2f(uint2 uv) {
			uv.x = max(0, min(uv.x, 4096));
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
			float4 res = p1 + (fac * (p2 - p0) * t) + ((p2 - p1) * 3.0 - (p3 - p1) * fac - (p2 - p0) * 2 * fac) * t * t +
				((p2 - p1) * (-2.0) + (p3 - p1) * fac + (p2 - p0) * fac) * t * t * t;

			return res;
		}
#define fWidth 4096
#define fHeight 4096
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

			float a = (TexCoord.x * fWidth) - nX;
			float b = (TexCoord.y * fHeight) - nY;

			float2 TexCoord1 = float2(float(nX) / fWidth, float(nY) / fHeight);

			for (int m = -1; m <= 2; m++) {
				for (int n = -1; n <= 2; n++) {
					float4 vecData = tex2Dlod1(textureSampler, float2(TexCoord1.x + texelSizeX * m, TexCoord1.y + texelSizeY * n));
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

			float err = 0.00001;
			if (WATER_HEIGHT(a11) < err && WATER_HEIGHT(a12) < err && WATER_HEIGHT(a21) < err && WATER_HEIGHT(a22) < err)
				return 0;

			float4 a = CatmullRomPoint(a00, a01, a02, a03, e.y);
			float4 b = CatmullRomPoint(a10, a11, a12, a13, e.y);
			float4 c = CatmullRomPoint(a20, a21, a22, a23, e.y);
			float4 d = CatmullRomPoint(a30, a31, a32, a33, e.y);

			float4 res = CatmullRomPoint(a, b, c, d, e.x);

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


		float4 tessFixed() {
			return _Tess;
		}

		void disp(inout appdata v)
		{
			float4 d = tex2Dlod(_StateTex, float4(v.texcoord.xy, 0, 0));
			//v.vertex.y += FULL_HEIGHT(d);
			//o.uv_MainTex = v.texcoord;
			v.vertex.y += 100;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			
			float4 state = tex2Dlod(_StateTex2, float4(IN.uv_MainTex.xy, 0, 0));


			if (_FilterMode == 0)
			{
				state = tex2Dlod(_StateTex2, float4(IN.uv_MainTex.xy, 0, 0));
			}
			else if (_FilterMode == 1)
			{
				state = SampleBilinear(_StateTex2, IN.uv_MainTex.xy);
			}
			else if (_FilterMode == 2)
			{
				state = bicubicSample(_StateTex2, IN.uv_MainTex.xy);
			}
			else
			{
				state = BiCubicSample(_StateTex2, IN.uv_MainTex.xy);
			}
			state = tex2D(_StateTex2, IN.uv_MainTex);
			/*if (state.g < 0.00000000000000000000000000001)
				discard;*/
			//clip(WATER_HEIGHT(state) - 0.0000001);
			
            // Albedo comes from a texture tinted by color
            fixed4 c =0 ;
			c.r = 120.0 / 255.0;
			c.g = 152.0 / 255.0;
			c.b = 174.0 / 255.0;
			c.a = interPlot(0.3, 1.0, 0, 1, WATER_HEIGHT(state));

			//c.rg = IN.uv_MainTex;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
