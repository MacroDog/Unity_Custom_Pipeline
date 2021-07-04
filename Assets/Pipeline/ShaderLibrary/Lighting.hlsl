#ifndef MYRP_LIGHTING_INCLUDED
#define MYRP_LIGHTING_INCLUDED

struct Light
{
	half3 color;
	half3 direction;
	half    distanceAttenuation;
    half    shadowAttenuation;
};

Light  GetPerObjectLightIndex(int index)
{
	Light light;
	light.color = _VisibleLightColors[index].rgb;
	light. direction = _VisibleLightDirections[index].xyz;
	return light;
}

Light GetMainLight()
{
	
	return GetPerObjectLightIndex(1);
}

//光照结果
float3 GetLighting(Surface sur)
{
	return sur.normal.z;
}
float3  DiffuseLight (Light light,Surface sur)
{
	float diffue =  saturate(dot(sur.normal, light.direction));
	return diffue*light.color;
}

#endif