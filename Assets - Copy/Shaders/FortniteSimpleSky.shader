Shader "Skybox/Fortnite Simple Sky"
{
    Properties
    {
        [Header(Sky Colors)]
        _TopColor("Top Color", Color) = (0.2, 0.5, 0.9, 1)
        _BottomColor("Bottom Color", Color) = (0.6, 0.75, 0.9, 1)
        
        [Header(Sun Settings)]
        _SunColor("Sun Color", Color) = (1, 0.95, 0.8, 1)
        _SunDirection("Sun Direction", Vector) = (0, 0.4, 0.8, 0)
        _SunSize("Sun Size", Range(0, 0.1)) = 0.02
        _SunIntensity("Sun Intensity", Range(0, 3)) = 0.8
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;
                float4 _SunColor;
                float3 _SunDirection;
                float _SunSize;
                float _SunIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDir);
                
                float skyGradient = saturate(viewDir.y * 0.5 + 0.5);
                float3 skyColor = lerp(_BottomColor.rgb, _TopColor.rgb, skyGradient);
                
                float3 sunDir = normalize(_SunDirection);
                float sunDot = dot(viewDir, sunDir);
                float sun = smoothstep(1.0 - _SunSize, 1.0, sunDot);
                float3 sunColor = sun * _SunColor.rgb * _SunIntensity;
                
                float3 finalColor = skyColor + sunColor;
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
