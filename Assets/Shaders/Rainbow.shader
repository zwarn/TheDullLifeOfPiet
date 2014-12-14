Shader "rainbow" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Speed ("Speed", Range(0.1, 30.0)) = 5.0
		_RainbowLen ("Rainbow Length", Range(0.2, 5.0)) = 1.0
	}
	
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
		
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Speed;
			float _RainbowLen;
			
			struct inV {
				float4 v : POSITION;
				float4 tex : TEXCOORD0;
			};
			
			struct outV {
				float4 v : SV_POSITION;
				float4 tex : TEXCOORD0;
			};
			
			outV vert(inV i) {
				outV o;
				o.v = mul(UNITY_MATRIX_MVP, i.v);
				o.tex = i.tex;
				return o;
			}
			
			float3 toRainbowRGB (float f) {
				float a = (1.0f - f) * 5f;
				int X = a;
				float Y = a - X;
				float3 rgb = float3(0.5,0.5,0.5);			
				switch (X) {
				case 0:
						rgb = float3(1.0,Y,0.0);
						break;
				case 1:
						rgb = float3(1.0-Y,1.0,0.0);
						break;
				case 2:
						rgb = float3(0.0,1.0,Y);
						break;
				case 3:
						rgb = float3(0.0,1.0-Y,1.0);
						break;
				case 4:
						rgb = float3(Y,0.0,1.0);
						break;
				case 5:
						rgb = float3(1.0,0.0,1.0);
						break;
				}
		
				return rgb;
		}
		
		float4 frag(outV i) : COLOR {
				float4 tex = tex2D(_MainTex, i.tex.xy);
				bool building = (tex.w > 0.8);
				float param;
				float3 rgb;
				float xy;
				if (building) {
					param = 10+i.tex.y - i.tex.x;
				}
				else  {
					param = (i.tex.y + i.tex.x);	
				}								
				float realParam = param / _RainbowLen + _Time.x * _Speed;
				int p = realParam;
				rgb = toRainbowRGB(realParam-p);
				if (building) {
					rgb*=0.7;
				}
				return float4(rgb, 1.0);
		}
			
			ENDCG
		}
	}
}