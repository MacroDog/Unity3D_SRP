Shader "Pipeline/Base/UnlitTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha("Alpha",Range(0,1)) = 0.6
    }
    SubShader
    {
        Tags { "RenderQueue" = "Transparent" }
        Cull Off Zwrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags{"LightMode"="01BasePipeline"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Alpha;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = float4(tex2D(_MainTex, i.uv).rgb,_Alpha) ;
                return col;
            }
            ENDCG
        }
    }
}
