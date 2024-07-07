Shader "UUIFX/AngleFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DisplayAngle ("Display Angle", Range(0, 360)) = 0
        _RangeAngle ("Range Angle", Range(0, 360)) = 90
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
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
            float _DisplayAngle;
            float _RangeAngle;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                // �������ص������ͼƬ���ĵĽǶ�
                float2 dir = input.uv - 0.5;
                float angle = atan2(dir.y, dir.x) * (180.0 / 3.14159265358979323846);
                if (angle < 0.0) angle += 360.0;

                // �������ص�����ʾ�Ƕ�֮��ļн�
                float angleDiff = abs(angle - _DisplayAngle);
                angleDiff = min(angleDiff, 360.0 - angleDiff);

                // ����͸����
                float alpha;
                if(angleDiff < _RangeAngle)alpha =1;
                else
                    alpha = 1.0 - smoothstep(0.0, _RangeAngle, angleDiff- _RangeAngle);

                // ���������в�����ɫ�����
                fixed4 color = tex2D(_MainTex, input.uv);
                color.a *= alpha;
                return color;
            }
            ENDCG
        }
    }
}
