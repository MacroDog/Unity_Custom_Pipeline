#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4 , _Color)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
struct VertexInput {
	float4 pos : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 clipPos : SV_POSITION;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
VertexOutput UnlitPassVertex (VertexInput input) {
	VertexOutput output;
	UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(UNITY_MATRIX_VP, worldPos);
	return output;
}
float4 UnlitPassFragment (VertexOutput input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);
	return UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
}
#endif