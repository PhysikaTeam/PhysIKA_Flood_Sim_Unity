Shader "Sprites/Refraction" {
Properties {
    _MainTex ("Sprite Texture", 2D) = "white" {}
    _Refraction ("Refraction", Range (0.00, 100.0)) = .1
    _DistortTex ("Distortion Map (RGB)", 2D) = "white" {}
    _RefractionLayer ("Refraction Render Texture (RGB)", 2D) = "white" {}
    _ScreenPadding ("Screen Padding", Range(0.00, 0.5)) = .1
    _PixelSize("Pixel Size", float) = 0.004166
}
 
SubShader {
Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
 
CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
#pragma exclude_renderers gles
#pragma surface surf NoLighting keepalpha
#pragma vertex vert
 
fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten){
        fixed4 c;
        c.rgb = s.Albedo;
        c.a = s.Alpha;
        return c;
  }
 
sampler2D _DistortTex;
sampler2D _RefractionLayer;
sampler2D _MainTex;
 
float _Refraction;
float _ScreenPadding;
float _PixelSize;
 
struct Input {
    float2 uv_DistortTex;
    float2 uv_MainTex;
    fixed4 color;
    float3 worldRefl;
    float4 screenPos;
    INTERNAL_DATA
};
 
void vert (inout appdata_full v, out Input o) {
  UNITY_INITIALIZE_OUTPUT(Input,o);
  o.color = v.color;
}
 
void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
    float3 distort = tex2D(_DistortTex, IN.uv_DistortTex).rgb;
    distort = (distort*float3(1.0, 1.0, 2.0)) - float3(0.0, 0.0, 1.0);
    distort = distort*_Refraction;
    float2 offset = float2(distort.b, distort.r-distort.g);
    offset = round(offset)*float2(_PixelSize, _PixelSize);
    IN.screenPos.xy = IN.screenPos.xy*float2(1-(_ScreenPadding+_ScreenPadding), 1-(_ScreenPadding+_ScreenPadding));
    IN.screenPos.xy = IN.screenPos.xy+float2(_ScreenPadding, _ScreenPadding);
    IN.screenPos.xy = offset + IN.screenPos.xy;
    float4 refrColor = tex2Dproj(_RefractionLayer, IN.screenPos);
 
    o.Emission = refrColor.rgb * c.a;
     o.Alpha = c.a;
}
ENDCG
}
Fallback "Transparent/VertexLit"
}