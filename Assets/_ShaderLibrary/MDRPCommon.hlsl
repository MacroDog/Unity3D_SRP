#ifndef MRP_Lit_INCLUDE
#define MRP_Lit_INCLUDE


#include "Assets/_ShaderLibrary/MDRPMacro.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

CBUFFER_START(UnityPerFrame)
float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
float4x4 unity_ObjectToWorld;

float4 MRP_ObjectToClipPos(float4 vertex){
    float4 TEMP= mul(unity_ObjectToWorld,vertex);
    return mul(unity_MatrixVP, TEMP);
}
// float4x4 unity_WorldToObject;
// float4x4 unity_MatrixV;
// float4x4 unity_MatrixInvV;
// float4x4 unity_MatrixVP;
// float4x4 _InvCameraViewProj;
// float4x4 glstate_matrix_projection;
CBUFFER_END


CBUFFER_START(UnityLightBuffer)
float4 _LightColors[MDRP_VISIBLE_LIGHT_COUNT];
float4 _LightDirections[MDRP_VISIBLE_LIGHT_COUNT]; 
float4 _LightAttenuations[MDRP_VISIBLE_LIGHT_COUNT];
float4 _LightSpotDirections[MDRP_VISIBLE_LIGHT_COUNT];

float4 DiffuseLightDir(int index, float4 worldpos){
    if (MDRP_VISIBLE_LIGHT_COUNT < index) {
        return float4(0, 0, 1, 1);
    }
    return _LightDirections[index] - worldpos * _LightDirections[index].w;
}

//返回的颜色包含衰减
//衰减 (1-(dir^2/range^2)^2)^2
float4 DiffuseLightColor(int index, float4 worldPos)
{
    float3 dir = _LightDirections[index].xyz - worldPos.xyz;
    float rangeFade = max(dot(dir, dir),0.00001) * _LightAttenuations[index].x;
    rangeFade = saturate(1.0-rangeFade*rangeFade);
    rangeFade *= rangeFade;


    //spot fade (lightPos*lightDir)-cos(r_out)/cos(r_in)-cos(r_out)
    float spotFade = dot(_LightDirections[index],_LightSpotDirections[index]);
    spotFade = saturate(spotFade*_LightAttenuations[index].z+_LightAttenuations[index].w);
    spotFade *= spotFade;

    float dirFade = max(dot(dir, dir),0.00001);

    return _LightColors[index]*spotFade*rangeFade/dirFade;
}
CBUFFER_END
#endif