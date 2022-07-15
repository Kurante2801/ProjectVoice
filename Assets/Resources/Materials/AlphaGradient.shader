Shader "UI/AlphaGradient"
{
    Properties
    {
		_Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "IgnoreProjector" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

			float4 _Color;
            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = i.color;
				col.a = i.uv.x;
                return col;
            }
            ENDCG
        }
    }
}