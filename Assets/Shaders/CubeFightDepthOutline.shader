
Shader "Custom/CubeFightDepthOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _Thickness ("Outline Thickness (Pixels)", Range(1, 5)) = 1
        _DepthThreshold ("Depth Threshold", Range(0.00001, 0.0001)) = 0.0001
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "DepthOutline"

            Cull Off
            ZWrite Off
            ZTest Always

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _Thickness;
                float _DepthThreshold;
            CBUFFER_END

            // Get linear depth at a screen UV coordinate.
            float GetDepth(float2 uv)
            {
                float rawDepth = SampleSceneDepth(saturate(uv));

                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float2 uv = IN.texcoord;

                // Read the original screen color.
                half4 sceneColor = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                );

                // Convert pixel thickness to UV offsets.
                float2 texelSize = 1.0 / _ScaledScreenParams.xy;
                float2 offset = texelSize * _Thickness;

                // Sample depth at the current pixel.
                float center = GetDepth(uv);

                // Sample four neighboring pixels.
                float left  = GetDepth(uv - float2(offset.x, 0));
                float right = GetDepth(uv + float2(offset.x, 0));
                float up    = GetDepth(uv + float2(0, offset.y));
                float down  = GetDepth(uv - float2(0, offset.y));

                // Compare depth differences.
                float difference = max(
                    max(abs(center - left), abs(center - right)),
                    max(abs(center - up), abs(center - down))
                );

                // Use a relative threshold to reduce
                // sensitivity to distance from the camera.
                float relativeDifference =
                    difference / max(center, 1.0);

                // 1 = edge, 0 = non-edge.
                float edge = step(
                    _DepthThreshold,
                    relativeDifference
                );

                // Apply the outline to the original scene.
                half3 finalColor = lerp(
                    sceneColor.rgb,
                    _OutlineColor.rgb,
                    edge * _OutlineColor.a
                );

                return half4(finalColor, sceneColor.a);
            }

            ENDHLSL
        }
    }
}
