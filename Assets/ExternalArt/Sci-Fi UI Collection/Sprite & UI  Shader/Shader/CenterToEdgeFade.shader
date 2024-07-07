Shader "UUIFX/CenterToEdgeFade"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Fade("Fade", Range(0,1)) = 0
        _Speed("Speed", Float) = 1
        _EdgeWidth("Edge Width", Float) = 0.1
        _Invert("Invert", Int) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
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
            float _Fade;
            float _Speed;
            float _EdgeWidth;
            int _Invert;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.uv, float2(0.5, 0.5));
                float alpha = _Invert == 0 ? 1 - smoothstep(_Fade - _EdgeWidth, _Fade + _EdgeWidth, dist)
                                           : smoothstep(_Fade - _EdgeWidth, _Fade + _EdgeWidth, dist);
                return tex2D(_MainTex, i.uv) * alpha;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}