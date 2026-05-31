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
        _FoamWidth("Foam Width", Range(0, 2)) = 0.2
        _FoamHardness("Foam Hardness", Range(0, 1)) = 0.5

        [Header(Waves)]
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _WaveSpeed("Wave Speed", Vector) = (0.1, 0.1, 0, 0)
        _WaveScale("Wave Scale", Float) = 1.0

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
                half _Smoothness;
            CBUFFER_END

            sampler2D _SurfaceNoise;

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 screenUV = input.screenPos.xy / max(0.0001, input.screenPos.w);
                float depth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(depth, _ZBufferParams);
                float surfaceEyeDepth = input.screenPos.w;
                float depthDifference = sceneEyeDepth - surfaceEyeDepth;

                // For Orthographic Cameras (like the Map Screenshot Tool)
                // depthDifference can be 0 or negative because sceneEyeDepth/surfaceEyeDepth 
                // calculations change in ortho mode. We add a fallback:
                #if defined(UNITY_REVERSED_Z)
                    if (sceneEyeDepth < 0.001) depthDifference = 1000.0; 
                #endif

                // Color Gradient
                float depthFactor = saturate(depthDifference / max(0.001, _DepthDistance));
                half4 waterColor = lerp(_ShallowColor, _DeepColor, depthFactor);

                // If we are in Ortho and sceneEyeDepth is not valid, fallback to deep color
                if (unity_OrthoParams.w > 0.5 && depthDifference < 0.0) {
                    waterColor = _DeepColor;
                }

                // Waves / Noise
                float2 uv = input.positionWS.xz * _WaveScale + _Time.y * _WaveSpeed.xy;
                half noise = tex2D(_SurfaceNoise, uv).r;
                waterColor.rgb += noise * 0.05;

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
