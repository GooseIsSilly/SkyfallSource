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
        _FoamWidth("Foam Width", Range(0, 5)) = 0.5
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

            sampler2D _SurfaceNoise;

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                // Simple physical waves
                float wave = sin(worldPos.x * _WaveScale * 10.0 + _Time.y * _WaveSpeed.x) * 
                             cos(worldPos.z * _WaveScale * 10.0 + _Time.y * _WaveSpeed.y);
                worldPos.y += wave * _WaveHeight;

                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float depth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(depth, _ZBufferParams);
                float surfaceEyeDepth = input.screenPos.w;
                float depthDifference = sceneEyeDepth - surfaceEyeDepth;

                // Color Gradient
                float depthFactor = saturate(depthDifference / _DepthDistance);
                half4 waterColor = lerp(_ShallowColor, _DeepColor, depthFactor);

                // Surface Ripples
                float2 uv1 = input.positionWS.xz * _WaveScale + _Time.y * _WaveSpeed.xy * 0.5;
                float2 uv2 = input.positionWS.xz * _WaveScale * 1.2 - _Time.y * _WaveSpeed.xy * 0.3;
                half noise = (tex2D(_SurfaceNoise, uv1).r + tex2D(_SurfaceNoise, uv2).r) * 0.5;
                waterColor.rgb += noise * 0.1;

                // Foam
                float foamEdge = saturate(depthDifference / _FoamWidth);
                float foam = 1.0 - smoothstep(_FoamHardness, 1.0, foamEdge);
                waterColor.rgb = lerp(waterColor.rgb, _FoamColor.rgb, foam * _FoamColor.a);
                waterColor.a = lerp(waterColor.a, 1.0, foam);

                return waterColor;
            }
            ENDHLSL
        }
    }
}
