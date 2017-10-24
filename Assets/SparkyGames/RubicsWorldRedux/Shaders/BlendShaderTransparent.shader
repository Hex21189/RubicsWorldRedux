Shader "Sparky Games/Transparent Blend Shader"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Blend ("Blend", Range(0, 1)) = 0.0
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Texture2 ("Albedo 2 (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		
		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows alpha
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Texture2;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_Texture2;
		};

		half _Glossiness;
		half _Metallic;
		half _Blend;
		fixed4 _Color;

		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = (tex2D (_MainTex, IN.uv_MainTex) * (1 - _Blend) + tex2D (_Texture2, IN.uv_Texture2) * _Blend) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
