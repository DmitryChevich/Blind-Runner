Shader "Custom/Echolocation"
{
    Properties
    {
        _PingWidth ("Ping Width", Float) = 0.8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }  // добавь этот тег на Pass
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _EMISSION  // добавь эту строку
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Параметры пинга (устанавливаются через Shader.SetGlobal из PingSystem)
            float4 _PingOrigin;
            float _PingRadius, _PingWidth, _TrailIntensity, _TrailRadius;

            // Цвета зашиты в шейдер — меняй здесь если нужно
            static const float4 BASE_COLOR  = float4(0, 0, 0, 1);
            static const float4 PING_COLOR  = float4(2.0, 5.0, 10.0, 1);
            static const float4 TRAIL_COLOR = float4(0.3, 0.8, 2.0, 1);

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float dist = distance(IN.positionWS, _PingOrigin.xyz);
                
                // --- КОЛЬЦО ---
                float ringDist = abs(dist - _PingRadius);
                float ring = 1.0 - saturate(ringDist / _PingWidth);
                ring = pow(ring, 0.5);  // было 2.0 (резко) → 0.5 (мягко)
                float ringVisible = (_PingRadius > 0) ? ring : 0;

                // --- ОСТАТОЧНОЕ СВЕЧЕНИЕ ---
                float trailVisible = (_TrailRadius > 0 && dist < _TrailRadius) ? _TrailIntensity : 0;
                trailVisible *= saturate(dist / max(_TrailRadius, 0.001));

                // --- ФИНАЛЬНЫЙ ЦВЕТ ---
                float4 color = BASE_COLOR;
                color += PING_COLOR * ringVisible;
                color += TRAIL_COLOR * trailVisible;
                
                return float4(color.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}