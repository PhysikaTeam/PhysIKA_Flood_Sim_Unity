Shader "Custom/InitHeightMap"
{
    Properties
    {
        _TerrainTex ("Texture", 2D) = "white" {}
		_WaterTex("Texture", 2D) = "black" {}
		[NoScaleOffset] _Hardness("Texture", 2D) = "white" {}
		_SeaLevel("SeaLevel", float) = 0
		_Scale("Scale", float) = 1
		_Bias("Bias", float) = 0
		_InitWaterHeight("WaterH",float)=7.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _TerrainTex;
			sampler2D _WaterTex;
			sampler2D _Hardness;
			float2 _MainTex_TexelSize;
			float _Scale;
			float _Bias;
			float _SeaLevel;
			float _InitWaterHeight;
			int f2i(float a) {
				return (int)(a * 255);
			}
            fixed4 frag (v2f i) : SV_Target
            {
                float terrainH = tex2D(_TerrainTex, i.uv);
				float waterH= tex2D(_WaterTex, i.uv);
				fixed4 color;
				// Terrain height				
				color.r = max(0, terrainH * _Scale + _Bias);

				// Water height
				//col.g = max(0, _SeaLevel - col.r);


				if (f2i(waterH)==255)
					color.g = max(0, _InitWaterHeight);
				else
					color.g = 0;
			
				//color.g = max(0, waterH * _Scale + _Bias);
			
				//	col.g = 0;

				// Suspended sediment
				color.b = 0;
				
				// Hardness
				half h = tex2D(_Hardness, i.uv);
				color.a = saturate(0.2 + color.r * 0.8 * h);
				
                return color;
            }
            ENDCG
        }
    }
}
