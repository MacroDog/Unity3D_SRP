﻿Shader "Pipeline/Base/UnlitOpaque"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
       	Tags { "RenderQueue"="Geometry" }
        Pass
        {   
            Tags{"LightMode" = "01BasePipeline"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/_ShaderLibrary/MDRPCommon.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = MRP_ObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
