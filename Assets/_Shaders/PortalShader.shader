Shader "Custom/PortalShader"
{
	Properties
	{
		_InactiveColour("Inactive Colour", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UNITYCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos : TEX_COORD0;
			};

			sampler2D _MainTex;
			float4 _InactiveColour;
			int displayMask;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;
				fixed4 portalCol = tex2D(_MainTex, uv);
				return portalCol * displayMask + _InactiveColour * (1 - displayMask);
			}
			ENDCG
		}
	}

	Fallback "Standard"
}