#ifndef MYRP_SURFACE_INCLUDED
#define MYRP_SURFACE_INCLUDED
#include "../ShaderLibrary/Common.hlsl"
struct Surface
{
	half3 normal;
	half3 color;
	half alpha;
};
TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);

void InitLitSurfaceData(float2 uv, float3 normal,out Surface outSurfaceData)
{
    float4 sim = SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv);
	outSurfaceData.color = sim.rgb;
	outSurfaceData.alpha = sim.a;
	outSurfaceData.normal = normalize(normal);
}
#endif