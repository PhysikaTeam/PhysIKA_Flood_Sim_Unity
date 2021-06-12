Shader "Custom/InitFlowInOut"
{
    Properties
    {
        _FlowOutTex ("FlowOut", 2D) = "black" {}
		_FlowInTex("FlowIn", 2D) = "black" {}

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

            sampler2D _FlowOutTex;
			sampler2D _FlowInTex;


            fixed4 frag (v2f i) : SV_Target
            {
                float In = tex2D(_FlowInTex, i.uv);
				if (In < 0.95)
					In = 0;
				float Out= tex2D(_FlowOutTex, i.uv);
				fixed4 color;
				
				color = fixed4(Out, In, 0, 0);
				
                return color;
            }
            ENDCG
        }
    }
}
