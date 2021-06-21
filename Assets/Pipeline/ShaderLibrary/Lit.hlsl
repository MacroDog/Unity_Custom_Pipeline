#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END
CBUFFER_START(UnityPerFrame)
	float4x4 UNITY_MATRIX_M;
CBUFFER_END
CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
CBUFFER_END
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4 , _Color)
UNITY_INSTANCING_BUFFER_END(Props)
struct VertexInput {
	float4 pos 			: POSITION;
	float3 normal 		: NORMAL;
	float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 clipPos 	: SV_POSITION;
	float3 normal 	:TEXCOORD0;
    float2 uv     	: TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
CBUFFER_END
TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);


VertexOutput UnlitPassVertex (VertexInput i) {
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, o);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(i.pos.xyz, 1.0));
	o.clipPos = mul(unity_MatrixVP, worldPos);
	o.normal = mul((float3x3)UNITY_MATRIX_M, i.normal);
	o.uv = TRANSFORM_TEX(i.texcoord,_MainTex);
	return o;
}

float4 UnlitPassFragment (VertexOutput i) : SV_TARGET {
	i.normal = normalize(i.normal);
	UNITY_SETUP_INSTANCE_ID(i);
	// blinphone
	float3 fincolor = float3(1,1,1);
	// fincolor* =  SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb;
	fincolor*= UNITY_ACCESS_INSTANCED_PROP(Props, _Color).rgb;
	// float3 fincolor =  diffuse * albedo;
	return  float4 (fincolor,1);
}

#endif