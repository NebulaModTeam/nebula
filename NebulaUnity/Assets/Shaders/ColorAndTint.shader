Shader "Custom/ColorAndTint"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Emission ("Emission", 2D) = "black" {}
        _MSTex ("MS Texture", 2D) = "black" {}

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _AlphaClip ("Alpha Clip", Float) = 0
		_UseTint ("Use Tint", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _Emission;
        sampler2D _MSTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float2 uv_Emission;
            float2 uv_MSTex;
        };

        half _Glossiness;
        half _Metallic;
        half _AlphaClip;
		half _UseTint;

        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 e = tex2D (_Emission, IN.uv_Emission);
            fixed4 ms = tex2D (_MSTex, IN.uv_MSTex);

            if (_AlphaClip > 0.05f && ms.g < _AlphaClip) discard;

            fixed4 ct = _Color * c.w;
			
			if (_UseTint > 0)
				c = lerp(c, ct, c.w);

            o.Albedo = c.rgb + e.rgb;
            o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
