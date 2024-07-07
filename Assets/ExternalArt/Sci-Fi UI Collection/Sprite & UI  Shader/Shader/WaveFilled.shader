Shader "UUIFX/WaveFilled"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayStrength ("Overlay Strength", Range(0, 5)) = 0.5
        //_OverlayDirection ("Overlay Direction", Vector) = (1, 0, 0, 0) // ɾ��������
        _OverlayPercent ("Overlay Percent", Range(0, 1)) = 1
        _OverlayColor ("Overlay Color", Color) = (1, 1, 1, 1)
        _Amplitude ("Amplitude", Range(0, 1)) = 1
        _Band ("Band Count", float) = 2
        _AnimSpeed ("Animation Speed", float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "RenderType"="Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 overlayUV : TEXCOORD1; // ����������Overlay��UV����
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float _OverlayStrength;
            //float4 _OverlayDirection; // ɾ��������
            float _OverlayPercent;
            fixed4 _OverlayColor;
            float _Amplitude;
            float _AnimSpeed;
            float _Band;
            float _W;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);

                // ����Overlay��UV���꣬���ݷ���Ͱٷֱ�
                float2 overlayUV = input.uv ;//+ float2(0, 1) * (1 - _OverlayPercent); // Ĭ�Ϸ���Ϊ (0, 1)

                output.uv = input.uv;
                output.overlayUV = overlayUV;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                // ���������в���������ɫ
                fixed4 baseColor = tex2D(_MainTex, input.uv);

                // ����Overlay��UV���꣬���ݷ���Ͱٷֱ�
                float2 overlayUV = saturate(input.overlayUV);

                // �� w ��ʱ���� (-1, 1) ֮�����ر仯
                float timeVar = sin(_Time.y *_AnimSpeed *  3.14159265358979323846); // _Time.y ����ʱ��
                _W = timeVar;

                // ��Overlay�����в�����ɫ�����UV������Χ��������������Ϊȫ͸��
                fixed4 overlayColor;
                if (overlayUV.x < 1 && overlayUV.y < 1 && cos((overlayUV.x* 2 * 3.14159265358979323846 + _AnimSpeed * _Time.y) * _Band ) * _W * _Amplitude  + _OverlayPercent > overlayUV.y)
                {
                    overlayColor = tex2D(_OverlayTex, overlayUV);
                }
                else
                {
                    overlayColor = fixed4(0, 0, 0, 0); // ������Χ�����������Ĳ�����Ϊȫ͸��
                }

                // Ӧ��Overlay��ɫ������������ɫ��alphaͨ��
                fixed4 finalColor = lerp(baseColor, baseColor + overlayColor * _OverlayColor * _OverlayStrength * (1 - baseColor.a), overlayColor.a);

                // ����������ɫ��alphaͨ��
                finalColor.a = baseColor.a;

                return finalColor;
            }
            ENDCG
        }
    }
}
