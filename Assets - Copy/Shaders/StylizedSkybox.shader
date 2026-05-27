Shader "Skybox/Stylized Fortnite Sky"
{
    Properties
    {
        [Header(Sky Colors)]
        _TopColor("Top Color", Color) = (0.2, 0.5, 0.9, 1)
        _HorizonColor("Horizon Color", Color) = (0.4, 0.6, 0.85, 1)
        _BottomColor("Bottom Color", Color) = (0.5, 0.65, 0.8, 1)
        
        [Header(Gradient Settings)]
        _HorizonOffset("Horizon Offset", Range(-1, 1)) = 0
        _HorizonSmoothness("Horizon Smoothness", Range(0.01, 2)) = 0.5
        _GradientPower("Gradient Power", Range(0.1, 5)) = 1.5
        
        [Header(Sun Settings)]
        _SunColor("Sun Color", Color) = (1, 0.95, 0.8, 1)
        _SunDirection("Sun Direction", Vector) = (0.3, 0.6, 0.5, 0)
        _SunSize("Sun Size", Range(0, 0.5)) = 0.02
        _SunSoftness("Sun Softness", Range(0.001, 0.5)) = 0.01
        _SunIntensity("Sun Intensity", Range(0, 5)) = 0.0
        
        [Header(Cloud Settings)]
        _CloudColor("Cloud Color", Color) = (1, 1, 1, 0.3)
        _CloudSpeed("Cloud Speed", Range(0, 1)) = 0.05
        _CloudScale("Cloud Scale", Range(0.1, 10)) = 2
        _CloudCoverage("Cloud Coverage", Range(0, 1)) = 0.6
        _CloudSoftness("Cloud Softness", Range(0.01, 1)) = 0.3
        
        [Header(Rotation Settings)]
        _Rotation("Rotation (Degrees)", Range(0, 360)) = 0
    }
    
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest LEqual
        
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
                float4 _HorizonColor;
                float4 _BottomColor;
                float _HorizonOffset;
                float _HorizonSmoothness;
                float _GradientPower;
                float4 _SunColor;
                float4 _SunDirection;
                float _SunSize;
                float _SunSoftness;
                float _SunIntensity;
                float4 _CloudColor;
                float _CloudSpeed;
                float _CloudScale;
                float _CloudCoverage;
                float _CloudSoftness;
                float _Rotation;
            CBUFFER_END
            
            float3 RotateAroundYAxis(float3 vertex, float degrees)
            {
                float radians = degrees * 0.01745329; // PI/180
                float sina, cosa;
                sincos(radians, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.viewDir = input.positionOS.xyz;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            float Hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.13);
                p3 += dot(p3, p3.yzx + 3.333);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = Hash(i);
                float b = Hash(i + float2(1.0, 0.0));
                float c = Hash(i + float2(0.0, 1.0));
                float d = Hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            float FBM(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                
                for (int i = 0; i < octaves; i++)
                {
                    value += amplitude * Noise(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(input.viewDir);
                viewDir = RotateAroundYAxis(viewDir, _Rotation);
                
                float y = viewDir.y;
                y = (y + _HorizonOffset) / (1.0 + abs(_HorizonOffset));
                
                float t = saturate(y * 0.5 + 0.5);
                t = pow(t, _GradientPower);
                
                float3 skyColor = lerp(_BottomColor.rgb, _TopColor.rgb, t);
                
                float3 lightDir = normalize(_SunDirection.xyz);
                float sunDot = dot(viewDir, lightDir);
                float sun = smoothstep(_SunSize - _SunSoftness, _SunSize + _SunSoftness, sunDot);
                float3 sunColor = sun * _SunColor.rgb * _SunIntensity;
                
                float2 cloudUV = viewDir.xz / max(abs(viewDir.y), 0.1) * _CloudScale;
                cloudUV += _Time.y * _CloudSpeed;
                
                float clouds = FBM(cloudUV, 4);
                clouds = smoothstep(_CloudCoverage - _CloudSoftness, _CloudCoverage + _CloudSoftness, clouds);
                clouds *= saturate(abs(viewDir.y) * 5.0);
                
                float3 finalColor = skyColor + sunColor;
                finalColor = lerp(finalColor, _CloudColor.rgb, clouds * _CloudColor.a);
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    
    Fallback Off
}
