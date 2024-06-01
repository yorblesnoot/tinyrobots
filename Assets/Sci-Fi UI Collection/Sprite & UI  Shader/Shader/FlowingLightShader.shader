Shader "UUIFX/FlowingLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlowSpeed ("Flow Speed", float) = 0.5
        _FlowStrength ("Flow Strength", Range(0, 10)) = 0.5
        _MinBrightness ("Min Brightness", Range(0, 1)) = 0.1
        _MaxBrightness ("Max Brightness", Range(0, 1)) = 0.9
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
            float4 _MainTex_ST;
            float _FlowSpeed;
            float _FlowStrength;
            float _MinBrightness;
            float _MaxBrightness;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv);
                float2 flowOffset = input.uv + _FlowSpeed * _Time.y;
                float flow = _FlowStrength * (sin(flowOffset.x) + cos(flowOffset.y));

                // 计算最终的流光效果
                flow = saturate(flow); // 将流光效果限制在[0, 1]范围内
                flow = lerp(_MinBrightness, _MaxBrightness, flow); // 根据最暗和最亮的限制设置流光的亮度范围

                color.rgb += color.a * flow;
                return color;
            }
            ENDCG
        }
    }
}