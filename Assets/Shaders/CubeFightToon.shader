
Shader "Custom/CubeFightToon"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.3, 0.7, 1, 1)
        _Threshold ("Light Threshold", Range(0, 1)) = 0.5
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Threshold;
                float _ShadowStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS =
                    TransformObjectToHClip(IN.positionOS.xyz);

                OUT.normalWS =
                    TransformObjectToWorldNormal(IN.normalOS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light light = GetMainLight();

                float3 N = normalize(IN.normalWS);
                float3 L = normalize(light.direction);

                float brightness = saturate(dot(N, L));

                float shade = brightness >= _Threshold
                    ? 1.0
                    : (1.0 - _ShadowStrength);

                return half4(_BaseColor.rgb * shade, 1);
            }

            ENDHLSL
        }
    }
}
