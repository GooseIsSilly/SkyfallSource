// SimpleWaterURP.shader
// Simple, lightweight stylized water shader for URP. Built for a free release.
//
// FEATURES: animated waves, world-position-based shallow/deep color + transparency,
// panning normal-map ripples, fresnel reflection, shoreline foam.
//
// HOW DEPTH WORKS (world-position based, not a raw depth subtraction):
// The camera depth texture tells us how far the camera is from whatever is behind
// the water (the lake bed, a submerged rock, etc). We use that to reconstruct the
// actual WORLD POSITION of that point, then compare its height (Y) to the water
// surface's own height. The vertical gap between them is the "depth" of the water
// at that pixel. This stays accurate at any camera angle, unlike simply subtracting
// screen-space depth values, which stretches near the shore when viewed at a
// grazing angle.
//
// SETUP:
// 1. In your URP Asset, enable "Depth Texture" (Rendering section). That's the
//    only setting needed — no Opaque Texture required, and the water mesh can be
//    a plain flat plane (no special shaping needed).
// 2. Assign a tileable normal map + a soft noise texture for foam.
// 3. Set _WaterDepth to roughly how many world units deep your water body is
//    before it should read as "fully deep" colored.

Shader "Custom/SimpleWaterURP"
{
    Properties
    {
        [Header(Waves)]
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 1.0
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.15
        _WaveScale ("Wave Scale", Range(0.1, 10)) = 1.0

        [Header(Water Color)]
        _ShallowColor ("Shallow Color", Color) = (0.42, 0.75, 0.75, 0.55)
        _DeepColor ("Deep Color", Color) = (0.02, 0.18, 0.32, 0.95)
        _WaterDepth ("Water Depth (max, world units)", Range(0.1, 20)) = 3.0

        [Header(Ripples)]
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalTiling ("Normal Tiling", Float) = 1.0
        _NormalStrength ("Normal Strength", Range(0, 2)) = 0.5
        _NormalSpeed ("Normal Speed", Range(0, 2)) = 0.1

        [Header(Reflection)]
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 3.0
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.6

        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamDistance ("Foam Distance (world units)", Range(0.01, 3)) = 0.4
        _FoamNoiseTex ("Foam Noise Texture", 2D) = "white" {}
        _FoamTiling ("Foam Tiling", Float) = 1.0
        _FoamSpeed ("Foam Speed", Range(0, 2)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }

        Pass
        {
            Name "ForwardWater"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float _WaveSpeed;
                float _WaveStrength;
                float _WaveScale;

                float4 _ShallowColor;
                float4 _DeepColor;
                float _WaterDepth;

                float _NormalTiling;
                float _NormalStrength;
                float _NormalSpeed;

                float _FresnelPower;
                float _ReflectionStrength;

                float4 _FoamColor;
                float _FoamDistance;
                float _FoamTiling;
                float _FoamSpeed;
            CBUFFER_END

            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
            TEXTURE2D(_FoamNoiseTex); SAMPLER(sampler_FoamNoiseTex);

            // Local version of Unity's classic ComputeScreenPos, kept in-shader
            // so it doesn't depend on which URP version's helper macros exist.
            float4 ComputeScreenPosition(float4 positionCS)
            {
                float4 o = positionCS * 0.5;
                o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
                o.zw = positionCS.zw;
                return o;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                // Two simple sine waves summed = cheap, natural-looking motion
                float t = _Time.y * _WaveSpeed;
                float wave1 = sin(positionWS.x * _WaveScale + t);
                float wave2 = sin((positionWS.z + positionWS.x * 0.5) * _WaveScale * 0.8 - t * 1.3);
                positionWS.y += (wave1 + wave2) * 0.5 * _WaveStrength;

                OUT.positionWS = positionWS;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPosition(OUT.positionHCS);
                OUT.fogFactor = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float surfaceEyeDepth = IN.screenPos.w;

                // ---- Reconstruct the world position behind the water from the depth ----
                // ---- texture, then compare its height to the water surface's height. ----
                float rawSceneDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(rawSceneDepth, _ZBufferParams);

                float3 viewVector = GetCameraPositionWS() - IN.positionWS;
                float3 viewDir = normalize(viewVector);
                float3 scenePositionWS = GetCameraPositionWS() - viewVector * (sceneEyeDepth / surfaceEyeDepth);
                float depthY = max(IN.positionWS.y - scenePositionWS.y, 0.0);

                // ---- Ripple normal: one texture, panned in two directions ----
                float2 uvA = IN.uv * _NormalTiling + float2(1.0, 0.5) * _NormalSpeed * _Time.y;
                float2 uvB = IN.uv * _NormalTiling * 1.5 - float2(0.5, 1.0) * _NormalSpeed * _Time.y;
                float3 rippleA = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvA), 1.0);
                float3 rippleB = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvB), 1.0);
                float2 rippleXY = (rippleA.xy + rippleB.xy) * _NormalStrength;
                float3 normalWS = normalize(float3(rippleXY.x, 1.0, rippleXY.y));

                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelPower);

                // ---- Depth-based color and transparency ----
                // Exponential falloff (like light absorption through water) instead of a
                // linear ramp — fades in quickly then eases off, reading as more natural.
                float depthFactor = 1.0 - saturate(exp(-depthY / max(_WaterDepth, 0.0001)));
                float3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);
                float alpha = lerp(_ShallowColor.a, _DeepColor.a, depthFactor);

                // ---- Reflection (nearest reflection probe / skybox) ----
                float3 reflectDir = reflect(-viewDir, normalWS);
                half4 reflectionRaw = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, reflectDir);
                half3 reflectionColor = DecodeHDREnvironment(reflectionRaw, unity_SpecCube0_HDR);
                float3 colorWithReflection = lerp(waterColor, reflectionColor, fresnel * _ReflectionStrength);

                // ---- Foam near shorelines / underwater objects ----
                float2 foamUV = IN.uv * _FoamTiling + float2(1.0, 1.0) * _FoamSpeed * _Time.y;
                float foamNoise = SAMPLE_TEXTURE2D(_FoamNoiseTex, sampler_FoamNoiseTex, foamUV).r;
                float foamLine = smoothstep(_FoamDistance, 0.0, depthY) * foamNoise;
                float3 finalColor = lerp(colorWithReflection, _FoamColor.rgb, saturate(foamLine));
                alpha = lerp(alpha, 1.0, saturate(foamLine));

                finalColor = MixFog(finalColor, IN.fogFactor);
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
