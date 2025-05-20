Shader "Custom/UnitOpaque"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _TeamColorBase ("TeamColorBase", Color) = (1,1,1,1)
        _TeamColor ("TeamColor", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Blend One Zero
        ZWrite On

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma shader_feature _ALPHABLEND_ON
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _TeamColorBase;
        fixed4 _TeamColor;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 getTeamColor(float3 col) {
            if (abs(col.r - _TeamColorBase.r) < 0.1 && abs(col.g - _TeamColorBase.g) < 0.1 && abs(col.b - _TeamColorBase.b) < 0.1)
            {
                return _TeamColor.rgb;
            }
            else
            {
                return col;
            }
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = getTeamColor(c.rgb);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}