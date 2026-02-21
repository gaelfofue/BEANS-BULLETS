// CRTScanlines.shader
// Crear: Assets > Create > Shader > Unlit Shader, reemplazar contenido completo

Shader "RetroFX/CRTScanlines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _ScanlineCount ("Scanline Count", Float) = 300
        _ScanlineSpeed ("Scanline Scroll Speed", Float) = 0.5
        _ScreenBend ("Screen Curvature", Range(0, 0.1)) = 0.02
        _FlickerIntensity ("Flicker", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "CRTPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _ScanlineIntensity;
                float _ScanlineCount;
                float _ScanlineSpeed;
                float _ScreenBend;
                float _FlickerIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Curvatura de pantalla CRT
                float2 uv = input.uv;
                float2 centered = uv - 0.5;
                float2 bent = centered * (1.0 + dot(centered, centered) * _ScreenBend);
                uv = bent + 0.5;

                // Si sale del rango, negro (borde CRT)
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return float4(0, 0, 0, 1);

                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Scanlines horizontales con scroll
                float scanline = sin((uv.y + _Time.y * _ScanlineSpeed) * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                scanline = lerp(1.0, scanline, _ScanlineIntensity);
                color.rgb *= scanline;

                // Flicker sutil
                float flicker = 1.0 - _FlickerIntensity * sin(_Time.y * 60.0) * 0.5;
                color.rgb *= flicker;

                // Separación RGB sutil (fake chromatic en scanlines)
                float rgbOffset = 0.001;
                float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(rgbOffset, 0)).r;
                float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(rgbOffset, 0)).b;
                color.r = lerp(color.r, r, 0.3);
                color.b = lerp(color.b, b, 0.3);

                return color;
            }
            ENDHLSL
        }
    }
}