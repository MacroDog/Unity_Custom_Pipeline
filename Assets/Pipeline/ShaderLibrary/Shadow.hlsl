#ifndef MYRP_SHADOW
#define MYRP_SHADOW
#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
// #include “Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl”
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);
CBUFFER_START(_CustomShadow)
float4x4 _DirShadowVPMatrix[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct DirectionalShadowData
{
    float strength;
    int tileindex;
};
float SampleDirectionalShadowAtlas(float3 pos)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,pos);
}
#endif



