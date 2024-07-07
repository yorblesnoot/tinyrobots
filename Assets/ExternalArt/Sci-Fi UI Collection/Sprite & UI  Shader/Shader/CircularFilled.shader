Shader "UUIFX/CircularFilled"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayStrength ("Overlay Strength", Range(0, 5)) = 0.5
        _OverlayPercent ("Overlay Percent", Range(0, 1)) = 1
        _OverlayColor ("Overlay Color", Color) = (1, 1, 1, 1)
        _StartAngle ("Start Angle", Range(0, 360)) = 90
        _EndAngle ("End Angle", Range(0, 360)) = 180
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
                float2 overlayUV : TEXCOORD1; // 新增的用于Overlay的UV坐标
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float _OverlayStrength;
            float _OverlayPercent;
            fixed4 _OverlayColor;
            uniform float _StartAngle;
            uniform float _EndAngle;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);

                // 计算Overlay的UV坐标，根据方向和百分比
                float2 overlayUV = input.uv + float2(0, 1) * (1 - _OverlayPercent); // 默认方向为 (0, 1)

                output.uv = input.uv;
                output.overlayUV = overlayUV;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                // 从主纹理中采样基本颜色
                fixed4 baseColor = tex2D(_MainTex, input.uv);

                // 计算Overlay的UV坐标，根据方向和百分比
                float2 overlayUV = saturate(input.overlayUV);

                // 计算角度值
                float angle = atan2(overlayUV.y - 0.5, overlayUV.x - 0.5);
                angle = angle >= 0 ? angle : angle + 2 * 3.14159265358979323846; // 转换为正角度值

                // 计算起始角度和结束角度
                float startAngle = radians(_StartAngle);
                float endAngle = radians(_EndAngle);

                // 转换为 0 到 2 * Pi 的范围
                if (startAngle > endAngle)
                {
                    if (angle < startAngle)
                    {
                        angle += 2 * 3.14159265358979323846;
                    }
                    endAngle += 2 * 3.14159265358979323846;
                }

                // 计算遮罩
                float mask = 0.0;
                if (angle >= startAngle && angle <= endAngle)
                {
                    mask = 1.0;
                }

                // 从Overlay纹理中采样颜色
                fixed4 overlayColor = tex2D(_OverlayTex, overlayUV);

                // 应用Overlay颜色，保留基本颜色的alpha通道
                fixed4 finalColor = lerp(baseColor, baseColor + overlayColor * _OverlayColor * _OverlayStrength * (1 - baseColor.a), mask);

                // 保留基本颜色的alpha通道
                finalColor.a = baseColor.a;

                return finalColor;
            }
            ENDCG
        }
    }
}
