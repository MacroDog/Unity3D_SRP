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
float4 _LightSpotDirections[MDRP_VISIBLE_LIGHT_COUNT]

float4 DiffuseLightDir(int index,float4 worldPos){
    if(MDRP_VISIBLE_LIGHT_COUNT < index){
        return float4(0,0,1,1);
    }
    return _LightDirections[i]-worldPos*_LightDirections.w;
}

//返回的颜色包含衰减
float4 DiffuseLightColor(int index,float4 worldPos){
    float4 dirtion = _LightDirections[i]-worldPos*_LightDirections.w;
    float distancespr = max(dot(_LightDirections[i],_LightDirections[i]),0.00001);
    
    float spotfede = dot(_LightDirections[i] , _LightSpotDirections[i])*    
    return _LightColors[index];
}
CBUFFER_END
#endif