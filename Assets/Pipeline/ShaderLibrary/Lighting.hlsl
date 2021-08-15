#ifndef MYRP_LIGHTING_INCLUDED
#define MYRP_LIGHTING_INCLUDED
CBUFFER_START(_CustomLight)
float4 _DirectionLightShadowID[MAX_VISIBLE_LIGHTS];
float4 _DirectionalLightShadowData[MAX_VISIBLE_LIGHTS];
CBUFFER_END
struct Light
{
	half3 	color;
	half3 	direction;
	half    distanceAttenuation;
    half    shadowAttenuation;
};


float GetDirectionalShadowAttenuation(DirectionalShadowData shadowdata,Surface sur)
{
    if(shadowdata.strength <= 0)
    {
        return 1.0f;
    }
    float3 postionSTS = mul(_DirShadowVPMatrix[shadowdata.tileindex],float4(sur.pos,1)).xyz;
    float3 shadow = SampleDirectionalShadowAtlas(postionSTS);
    return lerp(1.0, shadow,shadowdata.strength);
}

DirectionalShadowData GetDirectionalShadowData(int index)
{
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData[index].x;
    data.tileindex = _DirectionalLightShadowData[index].y;
    return data;
}

Light  GetDirectionalLight(int index,Surface surdata)
{
	Light light;
	light.color = _VisibleLightColors[index].rgb;
	light.direction = _VisibleLightDirections[index].xyz;
	DirectionalShadowData shadowdata = GetDirectionalShadowData(index);
	light.shadowAttenuation = GetDirectionalShadowAttenuation(shadowdata,surdata);
	return light;
}

Light GetMainLight(Surface sur)
{
	return GetDirectionalLight(0,sur);
}

//光照结果
// float3 GetLighting(Surface sur)
// {
// 	return sur.normal.z;
// }

float3 GetLighting (Light light,Surface sur)
{
	float diffue = saturate(dot(sur.normal, light.direction)*light.shadowAttenuation);
	return diffue*light.color;
}

#endif