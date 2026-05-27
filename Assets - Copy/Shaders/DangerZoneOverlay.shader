Shader "UI/DangerZoneOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Danger Color", Color) = (1, 0, 0, 0.6)
        _ZoneCenter ("Zone Center", Vector) = (0.5, 0.5, 0, 0)
        _ZoneRadius ("Zone Radius", Range(0, 1)) = 0.3
        _MapSize ("Map Size", Vector) = (1, 1, 0, 0)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
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
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ZoneCenter;
            float _ZoneRadius;
            float4 _MapSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 normalizedZoneCenter = _ZoneCenter.xy;
                
                float2 diff = i.uv - normalizedZoneCenter;
                
                float aspectRatio = _MapSize.x / _MapSize.y;
                diff.x *= aspectRatio;
                
                float dist = length(diff);
                
                float alpha = step(_ZoneRadius, dist);
                
                float edge = 0.01;
                alpha = smoothstep(_ZoneRadius - edge, _ZoneRadius + edge, dist);
                
                fixed4 col = _Color;
                col.a *= alpha;
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
}
