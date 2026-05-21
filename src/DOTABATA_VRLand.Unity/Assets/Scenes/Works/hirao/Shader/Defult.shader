Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)

        _MainTex ("Albedo", 2D) = "white" {}

        [Normal]
        _BumpMap ("Normal Map", 2D) = "bump" {}

        _BumpScale ("Normal Scale", Range(0,2)) = 1.0

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // óºñ ï`âÊ
        Cull Off

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;

            // ï\ó†îªíË
            float facing : VFACE;
        };

        half _Glossiness;
        half _Metallic;

        fixed4 _Color;

        half _BumpScale;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Base Color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = c.rgb;

            // Normal Map
            float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

            normal.xy *= _BumpScale;

            // ó†ñ Ç»ÇÁñ@ê¸îΩì]
            if (IN.facing < 0)
            {
                normal.xy *= -1;
            }

            o.Normal = normalize(normal);

            // PBR
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            o.Alpha = c.a;
        }

        ENDCG
    }

    FallBack "Diffuse"
}