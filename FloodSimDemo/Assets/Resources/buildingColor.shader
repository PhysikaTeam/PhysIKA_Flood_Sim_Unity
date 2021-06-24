Shader "Custom/buildingColor"
{
    Properties
    {
        _Color ("Color", Color) = (0.8,0.8,0.8,1)
		 //_Color("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_StateTex("State", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Index("Index",Range(1,77))=1
		_Size("Size",Range(0,100000))=4096
			//_EarlyWarning("EarlyWarning",int)=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		//RWTexture2D<float4>_StateTex;
		sampler2D _StateTex;
        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		int _Index;
		int _Size;
		int _EarlyWarning;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

			float4 interP(float4 color1, float4 color2, float st, float ed, float cur) {
			return color1 + (color2 - color1) / (ed - st) * (cur - st);
		}
		float4 classify(float cur) {
			if (cur < 0.2)
				return float4(248, 243, 1,0) / 255.0;
			else if (cur < 0.4)
				return float4(246, 179, 8, 0) / 255.0;
			else if (cur < 0.6)
				return float4(244, 123, 0, 0) / 255.0;
			else if (cur < 0.8)
				return float4(191, 81, 49, 0) / 255.0;
			else
				return float4(255, 0, 0, 0) / 255.0;
	/*		if (cur < 0.2)
				return float4(0.8, 0.8, 0.8, 0);
			else if (cur < 0.4)
				return float4(0.6, 0.6, 0.6, 0);
			else if (cur < 0.6)
				return float4(0.4, 0.4, 0.4, 0);
			else if (cur < 0.8)
				return float4(0.2, 0.2, 0.2, 0);
			else
				return float4(0, 0, 0, 0);*/
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			_Index = _Index - 1;
			float a = 1.0*(_Index % _Size) / _Size;
			float b = 1.0*(_Index / _Size) /_Size;
			float ff =tex2D(_StateTex,float2(a,1.0-b));

			//float4 ff = _StateTex[uint2(_Index / _Size, _Index % _Size)];
			//////fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;


			fixed4 c = _Color;

			

			if (ff > 0.00001) {
				
				//c = interP(float4(1.0, 1.0, 0, 0), float4(1.0, 0, 0, 0), 0, 1, ff);
				//ff = 0.376 * sqrt(ff);
				c = classify(ff);
			}

			/*if (ff.r < 0.01)
				c = fixed4(1, 1, 1, 1);
			else if (ff.r < 0.5)
				c = fixed4(1.0,165.0/ 255, 0,1);
			else
				c = fixed4(1.0, 0, 0, 1);*/

			//	;
			//else if (ff.r < 0.5)
			//	c = fixed4(0.6, 0, 0, 1);
			//else
			//	c = fixed4(1, 0, 0, 1);
   ////         // Albedo comes from a texture tinted by color
           
			
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
