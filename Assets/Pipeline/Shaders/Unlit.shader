Shader "SRP/Unlit"
{
    Properties
    {
        _Color ("Color",Color)=(1,1,1,1)
        // _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            // Tags{"LightMode" = "SRPDefaultUnlit"}
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
            #pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "../ShaderLibrary/Common.hlsl"
			#include "../ShaderLibrary/Unlit.hlsl"

            ENDHLSL
        }
    }
}
