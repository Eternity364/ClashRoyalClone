Shader "Custom/UnitButton"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 1
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionStrength ("Emission Strength", Range(0,1)) = 1
    }
    SubShader
    {
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
        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Progress;
            fixed4 _EmissionColor;
            float _EmissionStrength;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            float getProgress(float2 center, float2 pt) {
                float2 upwardVector = float2(0, 0.5);
                float2 pointVector = pt - center;
                float angleRad = acos(dot(normalize(pointVector), normalize(upwardVector)));
                if (pointVector.x < 0) {
                    angleRad = UNITY_PI * 2 - angleRad;
                }
                return angleRad / UNITY_PI / 2;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                float progress = getProgress(float2(0.5, 0.5), i.uv);
                if (_Progress < 1) {
                    float gray = dot(c.rgb, float3(0.299, 0.587, 0.114));
                    c.rgb = gray;
                    if (progress <= _Progress)
                        c.rgb += _EmissionColor.rgb * _EmissionStrength;
                }
                return c;
            }
            ENDCG
        }
    }
}