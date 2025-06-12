Shader "Custom/UnitOpaque_URP_PBR"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _TeamColorBase ("TeamColorBase", Color) = (1,1,1,1)
        _TeamColor ("TeamColor", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionStrength ("Emission Strength", Range(0,1)) = 1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Blend One Zero
        ZWrite On

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile  _MAIN_LIGHT_SHADOWS
            #pragma multi_compile  _SHADOWS_SOFT
            #pragma multi_compile  _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/BSDF.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv: TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv: TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Glossiness;
            float _Metallic;
            float4 _TeamColorBase;
            float4 _TeamColor;
            float4 _EmissionColor;
            float _EmissionStrength;

            float3 getTeamColor(float3 col) {
                if (abs(col.r - _TeamColorBase.r) < 0.1 && abs(col.g - _TeamColorBase.g) < 0.1 && abs(col.b - _TeamColorBase.b) < 0.1)
                    return _TeamColor.rgb;
                else
                    return col;
            }

            Varyings vert(Attributes v)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(v.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(v.normalOS);
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                return OUT;
            }

            float4 frag(Varyings i) : SV_Target
            {
                // Albedo and team color logic
                float4 albedoTex = tex2D(_MainTex, i.uv) * _Color;
                float3 albedo = getTeamColor(albedoTex.rgb);

                // PBR inputs
                float metallic = _Metallic;
                float smoothness = _Glossiness;
                float3 normalWS = normalize(i.normalWS);
                float3 positionWS = i.positionWS;
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - positionWS);

                // Setup PBRData
                SurfaceData surfaceData;
                surfaceData.albedo = albedo;
                surfaceData.metallic = metallic;
                surfaceData.specular = 0; // Not used in metallic workflow
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalWS;
                surfaceData.emission = _EmissionColor.rgb * _EmissionStrength;
                surfaceData.occlusion = 1;
                surfaceData.alpha = 1;
                surfaceData.clearCoatMask = 0;
                surfaceData.clearCoatSmoothness = 0;

                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = i.shadowCoord;
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = 0;

                // Lighting calculation (GGX, shadows, ambient, etc.)
                float4 color = UniversalFragmentPBR(inputData, surfaceData);

                return color;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}