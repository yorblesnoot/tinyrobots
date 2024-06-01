Shader "UUIFX/EnhancedHighlight" {
    Properties {
        _BaseColor ("Base Color", Color) = (0.5,0.5,0.5,1)
        _HighlightColor ("Highlight Color", Color) = (0.5,0.5,0.5,1)
        _Tint ("Tint Color", Color) = (1,1,1,1)
        _TintIntensity ("Tint Intensity", Float) = 1
        _HighlightIntensity ("Highlight Intensity", Float) = 1
        _FlareTexture ("Flare Texture", 2D) = "white" {}
        _FlareIntensity ("Flare Intensity", Float) = 1
        _FlareSpeedV ("Flare Vertical Speed", Float) = 1
        _FlareScaleU ("Flare Horizontal Scale", Float) = 1
        _FlareScaleV ("Flare Vertical Scale", Float) = 1
        _MainTexture ("Main Texture", 2D) = "black" {}
        _RandomSeed ("Random Seed", Float) = 0
    }
    SubShader {
        Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "Transparent" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            ZClip Off
            ZWrite Off
            Cull Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 color : COLOR;
            };

            struct v2f {
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4 _BaseColor;
            float4 _HighlightColor;
            float4 _Tint;
            float _TintIntensity;
            float _HighlightIntensity;
            sampler2D _FlareTexture;
            float _FlareIntensity;
            float _FlareSpeedV;
            float _FlareScaleU;
            float _FlareScaleV;
            sampler2D _MainTexture;
            float _RandomSeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = v.uv0; // 直接使用原始 UV 坐标
                o.uv1 = v.uv1; // 直接使用原始 UV 坐标
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float offset = _RandomSeed + _Time[0];
                float4 uvFlare = float4(i.uv1.x * _FlareScaleU + offset * 0.05, 
                                        tex2D(_MainTexture, i.uv0).r * _HighlightIntensity * 1.2 * _FlareScaleV + offset * _FlareSpeedV, 0, 0);
                float flare = tex2D(_FlareTexture, uvFlare.xy).r * i.color.r + tex2D(_MainTexture, i.uv0).r;
                flare *= _FlareIntensity * 2;
                float3 mColor = _HighlightColor.rgb * flare + (-_BaseColor.rgb);
                mColor = flare * mColor + _BaseColor.rgb;
                float4 color = float4(mColor * _TintIntensity, flare * i.color.r) * _Tint;
                color.a =  tex2D(_MainTexture, i.uv0).a;
                return color;
            }
            ENDCG
        }
    }
}
