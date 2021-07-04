#ifndef MYRP_LIT_INPUT_INCLUDED
#define MYRP_LIT_INPUT_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
#define MAX_VISIBLE_LIGHTS 4
CBUFFER_START(_LightBuffer)
	float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

#endif
