Shader "XinY/Par/AlphaBlend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        
        [HDR]_BaseColor ("BaseColor", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_ST;
            half4 _BaseColor;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct appdata
        {
            half4 posOS : POSITION;
            float2 uv0 : TEXCOORD0;
            half4 color : COLOR;
        };

        struct v2f
        {
            float2 uv0 : TEXCOORD0;
            half4 posCS : SV_POSITION;
            half4 color : TEXCOORD1;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.posCS = TransformObjectToHClip(v.posOS);
            o.uv0 = TRANSFORM_TEX(v.uv0.xy, _MainTex);
            o.color = v.color;
            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            half3 finalcolor = _BaseColor.rgb;
            half alpha = _BaseColor.a;

            half4 maintex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv0);
            
            finalcolor = finalcolor * maintex.rgb * i.color.rgb;
            alpha = alpha * maintex.a * i.color.a;
            return half4(finalcolor, alpha);
        }
        
        ENDHLSL
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}

