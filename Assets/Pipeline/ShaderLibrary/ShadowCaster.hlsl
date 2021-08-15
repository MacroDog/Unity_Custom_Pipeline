#ifndef MYRP_SHADOW_CASTER_INCLUDED
#define MYRP_SHADOW_CASTER_INCLUDED
// #define MAX_VISIBLE_LIGHTS 4
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
UNITY_DEFINE_INSTANCED_PROP(float4 , _Color)
UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct VertexInput {
	float4 pos 			: POSITION;
	float2 uv       : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 pos 	: SV_POSITION;
	float2 uv	: TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Light
{
	//direction or position
    half3   direction;
    half3   color;
    half    distanceAttenuation;
    half    shadowAttenuation;
};

VertexOutput ShadowCasterPassVertex (VertexInput i) 
{
	VertexOutput o;
	UNITY_SETUP_INSTANCE_ID(i);
	UNITY_TRANSFER_INSTANCE_ID(i,o);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(i.pos.xyz, 1.0));
	o.pos = mul(UNITY_MATRIX_VP, worldPos);
	float4 st= UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
	o.uv = i.uv*st.xy+st.zw;
	return o;
}

//片元函数
void ShadowCasterPassFragment(VertexOutput input) {
	UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
#if defined(_CLIPPING) 
	clip(baseMap.a-UNITY_DEFINE_INSTANCED_PROP(UnityPerMaterial,_Cutoff))
#endif
}
#endif
