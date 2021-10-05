#ifndef MYRP_SHADOW
#define MYRP_SHADOW
#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#if defined(_DIRECTIONAL_PCF3)
#define DIRECTIONAL_FILTER_SAMPLES 4 
#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
#define DIRECTIONAL_FILTER_SAMPLES 9 
#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
#define DIRECTIONAL_FILTER_SAMPLES 16 
#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadow)
int _CascadeCount;
float4x4 _DirShadowVPMatrix[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
//联级数据
float4 _CascadeData[MAX_CASCADE_COUNT];
float4 _ShadowDistanceFade;
float2 _ShadowAtlasSize;
CBUFFER_END

struct DirectionalShadowData
{
    int tileindex;
    float strength;
    float normalBias;

};

struct ShadowData
{
    int cascedeindex;
    float strength;
};

float FadedShadowStrength(float distance,float scale,float fade)
{
    return saturate(( 1.0-distance*scale )* fade);
}

float DistanceSquard(float3 pa,float3 pb)
{
    return dot(pa-pb,pa-pb);
}

float SampleDirectionalShadowAtlas(float3 pos)
{
    return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas,SHADOW_SAMPLER,pos);
}

// float PCFShadow(float3 posSTS)
// {
//     float shadow=0;
//     float weights[DIRECTIONAL_FILTER_SAMPLES];
//     float2 positions[DIRECTIONAL_FILTER_SAMPLES];
//     float4 size= _ShadowAtlasSize.yyxx;
//     DIRECTIONAL_FILTER_SETUP(size,posSTS.xy,weights,positions);
//     for(int i=0;i<DIRECTIONAL_FILTER_SAMPLES;i++)
//     {
//         shadow += (weights[i]*SampleDirectionalShadowAtlas(float3(positions[i],posSTS.z)));
//     }
//     return shadow;
// }

float FilterShadow(float3 posSTS)
{
    #if defined(DIRECTIONAL_FILTER_SETUP)
     float shadow=0;
    float weights[DIRECTIONAL_FILTER_SAMPLES];
    float2 positions[DIRECTIONAL_FILTER_SAMPLES];
    float4 size= _ShadowAtlasSize.yyxx;
    DIRECTIONAL_FILTER_SETUP(size,posSTS.xy,weights,positions);
    for(int i=0;i<DIRECTIONAL_FILTER_SAMPLES;i++)
    {
        shadow += (weights[i]*SampleDirectionalShadowAtlas(float3(positions[i],posSTS.z)));
    }
    return shadow;
    #else
    return SampleDirectionalShadowAtlas(posSTS);
    #endif 
}



//得到世界空间的表面阴影数据
ShadowData GetShadowData(Surface sfdata)
{
    ShadowData data;
    int i = 0;
            data.cascedeindex = 1;
    for(i=0;i<_CascadeCount;i++)
    {
        float4 sphere = _CascadeCullingSpheres[i];
        float distance = DistanceSquard(sfdata.pos,sphere.xyz); 
        if(distance < sphere.w)
        {
            data.strength = FadedShadowStrength(sfdata.depth,_ShadowDistanceFade.x,_ShadowDistanceFade.y);
            data.cascedeindex = i;
            break;
        }
        if(i == _CascadeCount)
        {
            data.cascedeindex = i;
            data.strength = 0.0;
        }
    }
    return data;
}

#endif