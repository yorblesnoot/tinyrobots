Shader "UUIFX/Vortex"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Blinds("Blinds Count", Range(1, 20)) = 10
        _Progress("Progress", Range(0, 1)) = 0
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
                float visibility : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _Blinds;
            float _Progress;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // 百叶窗效果
                float blindHeight = 1.0 / _Blinds;
                float blindIndex = floor(o.uv.y / blindHeight);
                float progress = _Progress * _Blinds;
                float visibility = step(fmod(blindIndex, 2.0), progress); // 使用 fmod 替代 mod
                o.visibility = visibility;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);
                color.a *= i.visibility; // 应用透明度
                return color;
            }
            ENDCG
        }
    }
}
