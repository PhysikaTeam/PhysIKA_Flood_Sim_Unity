// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/reslution"
{
	Properties
	{
		_backgroundColor("面板背景色",Color) = (1.0,1.0,1.0,1.0)
		_axesColor("坐标轴的颜色",Color) = (0.0,0.0,0.0)
		_gridColor("网格的颜色",Color) = (0.0,0.0,0.0)
		_tickWidth("网格的密集程度",Range(0.001953125,5)) = 0.001953125
		_gridWidth("网格的宽度",Range(0.0001,0.01)) = 0.0001
		_axesXWidth("x轴的宽度",Range(0.0001,0.01)) = 0.0001
		_axesYWidth("y轴的宽度",Range(0.0001,0.01)) = 0.0001

		 _Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Display("Display",int) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		int _Display;
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			if (_Display == 0)
				c = _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG

		//去掉遮挡和深度缓冲
		Cull Off
		ZWrite Off
		
		//不要深度测试
		//ZTest Always
		
		CGINCLUDE
		//添加一个计算方法
		float mod(float a,float b)
		{
			//floor(x)方法是Cg语言内置的方法，返回小于x的最大的整数
			return a - b * floor(a / b);
		}
		ENDCG

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float4 _backgroundColor;
			uniform float4 _axesColor;
			uniform float4 _gridColor;
			uniform float _tickWidth;
			uniform float _gridWidth;
			uniform float _axesXWidth;
			uniform float _axesYWidth;

			struct appdata
			{
				float4 vertex:POSITION;
				float2 uv:TEXCOORD0;
			};
			struct v2f
			{
				float2 uv:TEXCOORD0;
				float4 vertex:SV_POSITION;
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) :SV_Target
			{
				//将坐标的中心从左下角移动到网格的中心
				fixed2 r = 2.0 * fixed2(i.uv.x - 0.5,i.uv.y - 0.5);
				fixed3 backgroundColor = _backgroundColor.xyz;
				fixed3 axesColor = _axesColor.xyz;
				fixed3 gridColor = _gridColor.xyz;

				fixed3 pixel = backgroundColor;

				//定义网格的的宽度
				const float tickWidth = _tickWidth;
				if (mod(r.x, tickWidth) < _gridWidth)
				{
					pixel = gridColor;
				}
				if (mod(r.y, tickWidth) < _gridWidth)
				{
					pixel = gridColor;
				}

				//画两个坐标轴
				if (abs(r.x) < _axesXWidth)
				{
					pixel = axesColor;
				}
				if (abs(r.y) < _axesYWidth)
				{
					pixel = axesColor;
				}

				return fixed4(pixel, 1.0);
			}
			ENDCG
		}

	}
	FallBack "Diffuse"
}
