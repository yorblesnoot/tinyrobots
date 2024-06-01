Shader "UUIFX/AlphaBlend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlendTex ("Blend Texture", 2D) = "white" {}
        _Strength ("Strength", Range(0, 10)) = 0.5
        _FlowSpeed ("Flow Speed", Range(0, 1)) = 0.5
        _FlowDirection ("Flow Direction", Vector) = (1, 0, 0, 0) // Ĭ�����ⷽ��Ϊ����
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
                float2 flowUV : TEXCOORD1; // ����������_BlendTex ��UV����
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _BlendTex;
            float4 _MainTex_ST;
            float4 _BlendTex_ST;
            float _FlowSpeed;
            float _Strength;
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
                output.flowUV = TRANSFORM_TEX(flowOffset, _BlendTex); // ʹ���µ�UV����������_BlendTex

                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                 fixed4 mcolor = tex2D(_MainTex, input.uv);
                fixed4 color = tex2D(_BlendTex, input.flowUV);  // ʹ�ô��� Tiling �� Offset ����������

                // ��������͸����
                float alpha = color.a * _Strength;

                // ���ش�����͸���ȵ���ɫ
                return float4(mcolor.rgb, alpha * mcolor.a);
            }
            ENDCG
        }
    }
}