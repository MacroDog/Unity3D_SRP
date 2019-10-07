//-----
//Created Date: Tuesday, October 1st 2019, 3:15:19 pm
//Author: XieYiFeng
//-----
Shader "Pipeline/Batch/UnlitOpaque"
{
    Properties
    {
        _Color("color",color) = (1,1,1,1)
    }
    SubShader
    {
       	Tags { "RenderQueue" = "Geometry" }
        Pass
        {   
            Tags{"LightMode" = "02BatchPipeline"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Assets/_ShaderLibrary/MDRPLit.hlsl"
            float4 _Color;
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz,1));
                o.vertex = mul(unity_MatrixVP,worldPos);
                return o;
            }
            float4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
