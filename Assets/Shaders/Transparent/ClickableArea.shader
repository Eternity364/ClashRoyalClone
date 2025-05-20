Shader "Custom/ClickableArea"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _CornerRadius ("CornerRadius", Range(0,0.1)) = 0.01
        _EdgeThickness ("EdgeThickness", Range(0,0.3)) = 0.01
        _InsideAlpha ("InsideAlpha", Range(0,1)) = 0.1
        _ObjectScale ("ObjectScale", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
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
        float _CornerRadius;
        float _EdgeThickness;
        float _InsideAlpha;
        float4 _ObjectScale;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        float roundedRect(float2 uv, float2 size, float radius)
        {
            float2 halfSize = size * 0.5;
            float2 center = float2(0.5, 0.5);
            float2 p = abs(uv - center) - halfSize + radius;
            float d = length(max(p, 0.0)) - radius;
            return saturate(1.0 - step(0.0, d));
        }

        float roundedRectEdge(float2 uv, float2 size, float radius, float thickness, float2 scale)
        {
            float2 adjustedThickness = thickness / scale.xy;
            float outer = roundedRect(uv, size, radius);
            float inner = roundedRect(uv, size - adjustedThickness, max(0, radius - max(adjustedThickness.x, adjustedThickness.y) * 0.5));
            return saturate(outer - inner);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Rectangle parameters
            float2 rectSize = float2(1, 1); // width, height (0..1)
            float cornerRadius = _CornerRadius;           // (0..0.5)
            float edgeThickness = _EdgeThickness;         // thickness of the edge

            // Compensate for object scale (assume x and z for a flat rectangle)
            float2 scale = float2(_ObjectScale.x, _ObjectScale.z);

            float fillMask = roundedRect(IN.uv_MainTex, rectSize, cornerRadius);
            float edgeMask = roundedRectEdge(IN.uv_MainTex, rectSize, cornerRadius, edgeThickness, scale);

            float edgeAlpha = 1.0;   // Fully opaque edges
            float fillAlpha = _InsideAlpha;   // Less opaque inside

            // If edgeMask > 0, use edgeAlpha; else if fillMask > 0, use fillAlpha; else 0
            float alpha = lerp(fillAlpha, edgeAlpha, edgeMask) * fillMask;

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}