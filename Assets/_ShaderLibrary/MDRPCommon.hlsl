#ifndef MRP_Lit_INCLUDE
#define MRP_Lit_INCLUDE


#include "Assets/_ShaderLibrary/MDRPMacro.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

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
CBUFFER_END
#endif