Shader "SRP/Lit"
{
    Properties
    {
        _Color ("Color",Color)=(1,1,1,1)
        _Cutoff ("_Cutoff",float)=0.1
        _BaseMap ("BaseMap",2D)="white"{}

    }
    SubShader
    {
        Pass
        {
            Name "ForwardLit"
            Tags {"LightMode" = "SRPDefaultLit"}
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
            #pragma vertex LitPassVertex
			#pragma fragment LitPassFragment
            #include "../ShaderLibrary/Surface.hlsl"
			#include "../ShaderLibrary/Common.hlsl"
            #include "../ShaderLibrary/UnityInput.hlsl"
            #include "../ShaderLibrary/Shadow.hlsl"
            #include "../ShaderLibrary/Lighting.hlsl"
			#include "../ShaderLibrary/Lit.hlsl"
            ENDHLSL
        }

        Pass
        {
            Tags 
            {
                "LightMode" = "ShadowCaster"
            }
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "../ShaderLibrary/Common.hlsl"
			#include "../ShaderLibrary/ShadowCaster.hlsl"
            ENDHLSL
        }
    }
}
