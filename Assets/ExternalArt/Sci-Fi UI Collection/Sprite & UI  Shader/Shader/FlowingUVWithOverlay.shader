Shader "UUIFX/FlowingUVWithOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlowTexture ("Flow Texture", 2D) = "white" {}
        _FlowSpeed ("Flow Speed", Range(0, 1)) = 0.5
        _FlowStrength ("Flow Strength", Range(0, 1)) = 0.5
        _MinBrightness ("Min Brightness", Range(0, 1)) = 0.1
        _MaxBrightness ("Max Brightness", Range(0, 1)) = 0.9
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0) // Ĭ�����ⷽ��Ϊ����
        _FlowColor ("Flow Color", Color) = (1, 1, 1, 1) // ��� Flow Color �������ڵ��� _FlowTexture ����ɫ
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
                float2 flowUV : TEXCOORD1; // ����������_FlowTexture ��UV����
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
            float4 _FlowDirection;
            fixed4 _FlowColor; // ��� _FlowColor ����

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                float2 flowOffset ;
                // ���������ٶȺͷ������UV����
                float speed = _FlowSpeed * _Time.y;
                float2 direction = _FlowDirection.xy;
                float2 floorValue = float2(floor(speed * direction.x), floor(speed * direction.y));
          
                flowOffset = input.uv + (speed * direction - float2(floorValue.x, floorValue.y)) + float2(sign(floorValue.x),sign(floorValue.y));
            
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.flowUV = TRANSFORM_TEX(flowOffset, _FlowTexture); // ʹ���µ�UV����������_FlowTexture

                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, input.uv);
                fixed4 flowColor = tex2D(_FlowTexture, input.flowUV) * _FlowColor; // ʹ�� _FlowColor ���������� _FlowTexture ����ɫ

                float flow = _FlowStrength * (sin(input.flowUV.x) + cos(input.flowUV.y));

                // �������յ�����Ч��
                flow = saturate(flow); // ������Ч��������[0, 1]��Χ��
                flow = lerp(_MinBrightness, _MaxBrightness, flow); // �����������������������������ȷ�Χ

                fixed4 finalColor = baseColor + flowColor * flow;
                finalColor.a = baseColor.a; // ����͸���Ȳ���

                return finalColor;
            }
            ENDCG
        }
    }
}