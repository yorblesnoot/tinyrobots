Shader "UUIFX/FadeOut"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Direction ("Direction", Vector) = (1, 0, 0, 0) // Ĭ�����ҷ���
        _Width ("Width", Range(0, 1)) = 0.1 // Ĭ�Ͽ��Ϊ0.1
        _Position ("Position", Range(-3, 3)) = 0.5 // Ĭ��λ��ΪͼƬ����
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Direction;
            float _Width;
            float _Position;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                // ���㵱ǰ���ص㵽ֱ�ߵľ���
                float dist;
                if( _Direction.x == 0)
                
                     dist =input.uv.y - _Position;
                
                else
                    dist = dot(input.uv - float2(_Position, 0.5), _Direction.xy);
                
                // ���ݾ������͸����
                float alpha = 1.0 - smoothstep(0.0, _Width, abs(dist));

                // �ж����ص���ֱ�ߵ�����
                if (dist < 0.0) // ��ֱ����࣬��ȫ��ʾԭ����ɫ
                {
                    alpha = 1.0;
                }
                else if (dist > _Width) // ��ֱ���Ҳ೬����ȣ�ȫ͸��
                {
                    alpha = 0.0;
                }

                // �������������ɫ
                fixed4 color = tex2D(_MainTex, input.uv);
                color.a *= alpha;
                return color;
            }
            ENDCG
        }
    }
}
