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


float GetDirectionalShadowAttenuation(DirectionalShadowData shadowdata,ShadowData global,Surface sur)
{
    if(shadowdata.strength <= 0)
    {
        return 1.0f;
    }
    //沿着法线偏移
    float3 normalBise = sur.normal*(shadowdata.normalBias* _CascadeData[global.cascedeindex].y);
    float3 postionSTS = mul(_DirShadowVPMatrix[shadowdata.tileindex],float4(sur.pos+normalBise ,1)).xyz;
    float shadow = FilterShadow(postionSTS);
    return lerp(1.0, shadow,shadowdata.strength);
}




DirectionalShadowData GetDirectionalShadowData(int index,ShadowData shadowData)
{
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData[index].x * shadowData.strength;
    data.tileindex = _DirectionalLightShadowData[index].y + shadowData.cascedeindex;
    data.normalBias = _DirectionalLightShadowData[index].z;
    return data;
}

Light GetDirectionalLight(int index,Surface surdata)
{
	Light light;
	light.color = _VisibleLightColors[index].rgb;
	light.direction = _VisibleLightDirections[index].xyz;
    ShadowData shadowdata = GetShadowData(surdata);
	DirectionalShadowData dirShadowData = GetDirectionalShadowData(index,shadowdata);
	light.shadowAttenuation = GetDirectionalShadowAttenuation(dirShadowData,shadowdata,surdata);
	return light;
}

Light GetMainLight(Surface sur)
{
	return GetDirectionalLight(0,sur);
}

float3 GetLighting (Light light,Surface sur)
{
	float diffue = saturate(dot(sur.normal, light.direction)*light.shadowAttenuation);
	return  diffue*light.color;
}

#endif