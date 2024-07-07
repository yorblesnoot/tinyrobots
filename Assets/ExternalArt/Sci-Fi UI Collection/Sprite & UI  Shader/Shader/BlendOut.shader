Shader "UUIFX/BlendOut"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Direction ("Direction", Vector) = (1, 0, 0, 0) // 默认向右方向
        _BlendTex ("Blend Texture", 2D) = "white" {}
        _Width ("Width", Range(0, 1)) = 0.1 // 默认宽度为0.1
        _Position ("Position", Range(-1, 2)) = 0.5 // 默认位置为图片中心
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
            sampler2D _BlendTex;
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
                // 计算当前像素点到直线的距离
                float dist;
                if( _Direction.x == 0)
                
                     dist =input.uv.y - _Position;
                
                else
                    dist = dot(input.uv - float2(_Position, 0.5), _Direction.xy);
                
                // 根据距离计算透明度
                float alpha = 1.0 -sqrt( smoothstep(0.0, _Width, abs(dist)));
                fixed4 color;
                fixed4 baseColor = tex2D(_MainTex, input.uv);
                // 判断像素点在直线的两侧
                if (dist < 0.0) // 在直线左侧，完全显示原本颜色
                {  
                    color = baseColor;
                    alpha = 1.0;
                    color.a *= alpha;
                }
                else if (dist > _Width) // 在直线右侧超出宽度，全透明
                {
                    color = baseColor;
                    alpha = 0.0;
                    color.a *= alpha;
                }
                else
                {
                    
                    color = tex2D(_BlendTex, input.uv);
                    color.a = color.a * baseColor.a * alpha; 
                }


                return color;
            }
            ENDCG
        }
    }
}
