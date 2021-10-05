#ifndef MYRP_LIT_INCLUDED
#define MYRP_LIT_INCLUDED
#include "../ShaderLibrary/Common.hlsl"

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
	UNITY_DEFINE_INSTANCED_PROP(float4 , _Color)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct VertexInput {
	float3 pos 			: POSITION;
	float3 normal 		: NORMAL;
	float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput 
{
	float4 pos 	: SV_POSITION;
	float3 worldpos :VAR_POSITION;
	float3 normal 	:VAR_NORMAL;
    float2 uv     	: VAR_BASE_UV;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput LitPassVertex (VertexInput i) 
{
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i, o);
	o.worldpos =TransformObjectToWorld(i.pos);
	o.pos = TransformWorldToHClip(o.worldpos);
	o.normal =TransformObjectToWorldNormal(i.normal);
	float4 st = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
	o.uv =  i.texcoord * st.xy + st.zw;
	return o;
}

float4 LitPassFragment (VertexOutput i) : SV_TARGET 
{
	UNITY_SETUP_INSTANCE_ID(i);
	Surface sur;
	InitLitSurfaceData(i.worldpos,i.uv,i.normal, sur);
	// blinphone
	float3 basecolor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Color).rgb;
	sur.color *= basecolor;
	float3 diffuselight = 0;
	for(int index = 0;index<_VisibleLightCount; index++)
	{
		Light light = GetDirectionalLight(index,sur);
		diffuselight += GetLighting(light,sur);
	}
	sur.color  = diffuselight * sur.color;
	return float4(sur.color,sur.alpha);
}
#endif