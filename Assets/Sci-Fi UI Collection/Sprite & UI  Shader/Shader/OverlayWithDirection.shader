Shader "UUIFX/OverlayWithDirection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayStrength ("Overlay Strength", Range(0, 5)) = 0.5
        _OverlayDirection ("Overlay Direction", Vector) = (1, 0, 0, 0)
        _OverlayPercent ("Overlay Percent", Range(0, 1)) = 1
        _OverlayColor ("Overlay Color", Color) = (1, 1, 1, 1)
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
            float4 _OverlayDirection;
            float _OverlayPercent;
            fixed4 _OverlayColor;

            v2f vert(appdata_t input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);

                // 计算Overlay的UV坐标，根据方向和百分比
                float2 overlayUV = input.uv + _OverlayDirection.xy * (1- _OverlayPercent);

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

    // 从Overlay纹理中采样颜色，如果UV超出范围则为全透明
    fixed4 overlayColor;
    if (overlayUV.x < 1 && overlayUV.y < 1)
    {
        overlayColor = tex2D(_OverlayTex, overlayUV);
    }
    else
    {
        overlayColor = fixed4(0, 0, 0, 0); // 超出范围的部分设为全透明
    }

    // 应用Overlay颜色，保留基本颜色的alpha通道
    fixed4 finalColor = lerp(baseColor, baseColor + overlayColor * _OverlayColor * _OverlayStrength * (1 - baseColor.a), overlayColor.a);

    // 保留基本颜色的alpha通道
    finalColor.a = baseColor.a;

    return finalColor;
}

            ENDCG
        }
    }
}
