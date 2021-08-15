#ifndef MYRP_LIT_INPUT_INCLUDED
#define MYRP_LIT_INPUT_INCLUDED
#define MAX_VISIBLE_LIGHTS 4
CBUFFER_START(_LightBuffer)
	int 	_VisibleLightCount;
	float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END
CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4x4 unity_worldToObject;
	float4x4 unity_WorldToObject;
	float4 unity_LODFade;
	float4 unity_WorldTransformParams;
CBUFFER_END
float4x4 unity_MatrixV;
float4x4 unity_MatrixVP;
float4x4 glstate_matrix_projection;
#endif
