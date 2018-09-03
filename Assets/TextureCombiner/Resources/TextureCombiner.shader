Shader "Utils/TextureCombiner"
{
	Properties
	{
		
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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

			sampler2D _R;
			sampler2D _G;
			sampler2D _B;
			sampler2D _A;

			float4 _BlendR;
			float4 _BlendG;
			float4 _BlendB;
			float4 _BlendA;

			float4 _Invert;
			int _Preview;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			inline fixed4 cancat(fixed4 value)
			{
				return fixed4((value.x + value.y + value.z + value.w).rrrr);
			}

			inline fixed4 invertChannel(fixed4 value, float isInvert)
			{
				return lerp(value, 1-value, isInvert);
			}
						
			fixed4 frag (v2f i) : SV_Target
			{
				float4 r = invertChannel(cancat(tex2D(_R, i.uv) * _BlendR), _Invert.x);
				float4 g = invertChannel(cancat(tex2D(_G, i.uv) * _BlendG), _Invert.y);
				float4 b = invertChannel(cancat(tex2D(_B, i.uv) * _BlendB), _Invert.z);
				float4 a = invertChannel(cancat(tex2D(_A, i.uv) * _BlendA), _Invert.w); 
				
				if(_Preview == 1)
				{
					return r + g + b + a;
				}				
				return fixed4(r.r, g.g, b.b, a.a);
			}

			ENDCG
		}
	}
}
