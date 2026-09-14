/*
 * Mục đích: Vẽ chữ trong thế giới 3D, có kiểm tra chiều sâu để không xuyên tường.
 * Hàm: vert đổi đỉnh từ không gian object sang clip; frag lấy alpha glyph và trả màu chữ.
 */
Shader "TheReturn/WorldText"
{
    Properties { _MainTex ("Font", 2D) = "white" {} _Color ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END
            // Nhận đỉnh và UV font; trả vị trí clip, UV và màu cho bước fragment.
            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv; o.color = v.color * _Color; return o;
            }
            // Nhận UV/màu đã nội suy; trả RGBA của ký tự với alpha từ texture font.
            half4 frag (Varyings i) : SV_Target
            {
                return half4(i.color.rgb, i.color.a * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a);
            }
            ENDHLSL
        }
    }
}
