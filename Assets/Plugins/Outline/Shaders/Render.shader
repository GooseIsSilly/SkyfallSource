Shader "Hidden/Outline/Render"
{
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest LEqual
		Lighting Off

		Pass
		{
			HLSLPROGRAM

			#pragma target 3.0

			#pragma vertex   Vertex
			#pragma fragment Fragment

			#include "UnityCG.cginc"

			sampler2D _MainTex;

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
			};

			VertexOutput Vertex(VertexInput input)
			{
				VertexOutput output;
				output.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, input.vertex));
				output.uv     = input.uv;
				return output;
			}

			float4 Fragment(VertexOutput vertexOutput) : SV_TARGET
			{
				float4 color = tex2D(_MainTex, vertexOutput.uv);
				clip(color.a);
				return 1;
			}

			ENDHLSL
		}
	}
}
