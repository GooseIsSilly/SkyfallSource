Shader "Custom/StylizedWater"
{
    Properties
    {
        [Header(Colors)]
        _ShallowColor("Shallow Color", Color) = (0.2, 0.6, 0.8, 1)
        _DeepColor("Deep Color", Color) = (0.05, 0.1, 0.3, 1)
        _DepthDistance("Depth Distance", Float) = 5.0

        [Header(Foam)]
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamWidth("Foam Width", Range(0.001, 5)) = 0.5
        _FoamHardness("Foam Hardness", Range(0, 1)) = 0.5

        [Header(Waves)]
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _WaveSpeed("Wave Speed", Vector) = (0.1, 0.1, 0, 0)
        _WaveScale("Wave Scale", Float) = 0.01
        _WaveHeight("Wave Height", Float) = 0.5

        [Header(Rendering)]
        _Smoothness("Smoothness", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                float _DepthDistance;
                half4 _FoamColor;
                float _FoamWidth;
                float _FoamHardness;
                float4 _WaveSpeed;
                float _WaveScale;
                float _WaveHeight;
                half _Smoothness;
            CBUFFER_END

            TEXTURE2D(_SurfaceNoise);
            SAMPLER(sampler_SurfaceNoise);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                // Physical waves
                float wave = sin(worldPos.x * _WaveScale * 10.0 + _Time.y * _WaveSpeed.x) * 
                             cos(worldPos.z * _WaveScale * 10.0 + _Time.y * _WaveSpeed.y);
                worldPos.y += wave * _WaveHeight;

                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = input.screenPos.xy / max(0.0001, input.screenPos.w);
                
                // Safe depth sampling
                float depth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(depth, _ZBufferParams);
                float surfaceEyeDepth = input.screenPos.w;
                float depthDifference = max(0.0, sceneEyeDepth - surfaceEyeDepth);

                // Color Gradient
                float depthFactor = saturate(depthDifference / max(0.001, _DepthDistance));
                half4 waterColor = lerp(_ShallowColor, _DeepColor, depthFactor);

                // Surface Ripples
                float2 uv1 = input.positionWS.xz * _WaveScale + _Time.y * _WaveSpeed.xy * 0.5;
                float2 uv2 = input.positionWS.xz * _WaveScale * 1.2 - _Time.y * _WaveSpeed.xy * 0.3;
                
                half noise1 = SAMPLE_TEXTURE2D(_SurfaceNoise, sampler_SurfaceNoise, uv1).r;
                half noise2 = SAMPLE_TEXTURE2D(_SurfaceNoise, sampler_SurfaceNoise, uv2).r;
                half noise = (noise1 + noise2) * 0.5;
                waterColor.rgb += noise * 0.05;

                // Foam
                float foamEdge = saturate(depthDifference / max(0.001, _FoamWidth));
                float foam = 1.0 - smoothstep(_FoamHardness, 1.0, foamEdge);
                waterColor.rgb = lerp(waterColor.rgb, _FoamColor.rgb, foam * _FoamColor.a);
                waterColor.a = lerp(waterColor.a, 1.0, foam);

                // Apply Fog
                waterColor.rgb = MixFog(waterColor.rgb, input.fogFactor);

                return waterColor;
            }
            ENDHLSL
        }
    }
}
