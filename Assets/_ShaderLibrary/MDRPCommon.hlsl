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
float4 _LightColors[MRP_VISIBLE_LIGHT_COUNT];
float4 _LightDirections[MRP_VISIBLE_LIGHT_COUNT]; 
float4 _LightAttenuations[MRP_VISIBLE_LIGHT_COUNT];

float4 DiffuseLightDir(int index, float4 worldpos){
    if (MRP_VISIBLE_LIGHT_COUNT < index) {
        return float4(0, 0, 1, 1);
    }
    return _LightDirections[index] - worldpos * _LightDirections[index].w;
}

//返回的颜色包含衰减
//衰减 (1-(d^2/r^2)^2)^2
float4 DiffuseLightColor(int index, float4 worldPos)
{
    float3 dir = _LightDirections[index].xyz - worldPos.xyz;
    float att = max(dot(dir, dir),0.00001) * _LightAttenuations[index].x;
    att *= att;
    att = 1 - att;
    return _LightColors[index]*att;
}
CBUFFER_END
#endif