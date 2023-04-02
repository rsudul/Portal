Shader "Custom/SliceShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glosiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		sliceNormal("normal", Vector) = (0,0,0,0)
		sliceCenter("center", Vector) = (0,0,0,0)
		sliceOffsetDst("offset", Float) = 0
	}

		SubShader
		{
		Tags { "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Geometry"}
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard addshadow
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glosiness;
		half _Metallic;
		fixed4 _Color;

		float3 sliceNormal;
		float3 sliceCenter;
		float sliceOffsetDst;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float3 adjustedCenter = sliceCenter + sliceNormal * sliceOffsetDst;
			float3 offsetToSliceCenter = adjustedCenter - IN.worldPos;
			clip(dot(offsetToSliceCenter, sliceNormal));

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

			o.Metallic = _Metallic;
			o.Smoothness = _Glosiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Vertex Lit"
}
