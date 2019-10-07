Shader "Pipeline/GPUInstace/UnlitOpaque"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",color) = (1,1,1,1)
    }
    SubShader
    {
       
        LOD 100
        Tags { "RenderQueue" = "Geometry" }
        Pass
        {
            Tags { "LightMode" = "02BatchPipeline" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma multi_compile_instancing
            #include "Assets/_ShaderLibrary/MDRPLit.hlsl"
            #include "Assets/_ShaderLibrary/MDRPMacro.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            UNITY_INSTANCING_BUFFER_START(PerInstance)
                UNITY_DEFINE_INSTANCED_PROP(float4,_Color)
            UNITY_INSTANCING_BUFFER_END(PerInstance)
           struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
            CBUFFER_END
            v2f vert (appdata v)
            {
                v2f o;
               	UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = MRP_ObjectToClipPos(v.vertex);
                o.uv = _MainTex_ST.xy*v.uv+_MainTex_ST.zw;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4 _color = UNITY_ACCESS_INSTANCED_PROP(PerInstance,_Color);
                float4 col = tex2D(_MainTex, i.uv); 
                return _color;
            }
            ENDHLSL
        }
    }
}
