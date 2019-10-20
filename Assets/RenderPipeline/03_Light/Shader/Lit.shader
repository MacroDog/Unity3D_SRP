//-----
//Created Date: Sunday, October 6th 2019, 4:24:44 pm
//Author: XieYiFeng
//-----


Shader "Pipeline/Lit/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass    
        {
            Tags{"LightMode" = "03LitPipeline"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Assets/_ShaderLibrary/MDRPCommon.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldNor : TEXCOORD1;
                float4 worldPos: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = MRP_ObjectToClipPos(v.vertex);
                o.worldNor  =  mul(UNITY_MATRIX_M,v.normal);
                o.worldPos = mul(UNITY_MATRIX_M,o.vertex);
                o.uv = v.uv*_MainTex_ST.xy+_MainTex_ST.zw;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 normal = normalize(i.worldNor);
                float4 lightDir = DiffuseLightDir(0,i.worldPos);
                float albedo = max(0,dot(lightDir.xyz,normal.xyz))*DiffuseLightColor(0,i.worldPos);
                float4 fcolor = albedo*_Color;
                return  fcolor;
            }
            ENDHLSL
        }
    }
}
