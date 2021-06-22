#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
CBUFFER_END
#define MAX_VISIBLE_LIGHTS 4
CBUFFER_START(_LightBuffer)
	float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END
TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
CBUFFER_END
#define UNITY_MATRIX_M unity_ObjectToWorld
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
	float4 pos 	: SV_POSITION;
	float3 normal 	:TEXCOORD0;
    float2 uv     	: TEXCOORD1;
	float3 worldpos :TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
// TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);


VertexOutput UnlitPassVertex (VertexInput i) {
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, o);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(i.pos.xyz, 1.0));
	o.pos = mul(unity_MatrixVP, worldPos);
	o.worldpos = worldPos.xyz;
	o.normal = mul((float3x3)UNITY_MATRIX_M, i.normal);
	o.uv = TRANSFORM_TEX(i.texcoord,_BaseMap);
	return o;
}

float3  DiffuseLight (int index,float3 normal,float3 worldpos)
{
	float3 lightColor = _VisibleLightColors[index].rgb;
	float3 lightDir = _VisibleLightDirections[index].xyz  -(worldpos*_VisibleLightDirections[index].w);
	lightDir = normalize(lightDir);
	float diffue =  saturate(dot(normal, lightDir));
	return diffue*lightColor;
}

float4 UnlitPassFragment (VertexOutput i) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(i);
	i.normal = normalize(i.normal);
	// blinphone
	float3 albedo = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
	float3 basecolor = UNITY_ACCESS_INSTANCED_PROP(Props,_Color).rgb;
	albedo *= basecolor;
	float3 diffuselight = 0;
	for(int index=0;index<MAX_VISIBLE_LIGHTS;index++)
	{
		diffuselight += DiffuseLight(index,i.normal,i.worldpos);
	}
	float3 color = diffuselight*albedo;
	// float3 fincolor =  diffuse * albedo;
	return  float4 (color,1);
}
#endif