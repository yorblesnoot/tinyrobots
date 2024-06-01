Shader "UUIFX/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 100)) = 0.05
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
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half alpha = tex2D(_MainTex, i.uv).a;
                half edge = 0.0;
                
                // Calculate outline width based on _OutlineWidth
                float2 dxdy = _OutlineWidth * _MainTex_TexelSize.xy;
                
                // Check surrounding pixels for transparent areas
                for (float x = -1; x <= 1; x += 2)
                {
                    for (float y = -1; y <= 1; y += 2)
                    {
                        edge += tex2D(_MainTex, i.uv + float2(x, y) * dxdy).a;
                    }
                }
                
                // Determine if it's an edge pixel
                if (alpha > 0 && edge < 4)
                {
                    // Apply outline color to edge pixels
                    return _OutlineColor;
                }
                else
                {
                    // Display original texture color for non-edge pixels
                    return tex2D(_MainTex, i.uv);
                }
            }
            ENDCG
        }
    }
}
