Shader "UUIFX/FlowingLightWithOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlowTexture ("Flow Texture", 2D) = "white" {}
        _FlowSpeed ("Flow Speed", Range(0, 10)) = 0.5
        _FlowStrength ("Flow Strength", Range(0, 10)) = 0.5
        _MinBrightness ("Min Brightness", Range(0, 1)) = 0.1
        _MaxBrightness ("Max Brightness", Range(0, 1)) = 0.9
        _FlowColor ("Flow Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnityCG.cginc"

            struct appdata_t
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
            sampler2D _FlowTexture;
            float4 _MainTex_ST;
            float4 _FlowTexture_ST;
            float _FlowSpeed;
            float _FlowStrength;
            float _MinBrightness;
            float _MaxBrightness;
            float4 _FlowColor;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                
                // 修正UV坐标，确保正确的Tiling和Offset
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, input.uv);
                fixed4 flowColor = tex2D(_FlowTexture, input.uv) * _FlowColor; // 将FlowTexture的颜色乘以_FlowColor

                float2 flowOffset = input.uv + _FlowSpeed * _Time.y;
                float flow = _FlowStrength * (sin(flowOffset.x) + cos(flowOffset.y));

                // 计算最终的流光效果
                flow = saturate(flow); // 将流光效果限制在[0, 1]范围内
                flow = lerp(_MinBrightness, _MaxBrightness, flow); // 根据最暗和最亮的限制设置流光的亮度范围

                fixed4 finalColor = baseColor + flowColor * flow;
                finalColor.a = baseColor.a; // 保持透明度不变

                return finalColor;
            }
            ENDCG
        }
    }
}
