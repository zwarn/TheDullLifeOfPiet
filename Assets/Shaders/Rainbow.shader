Shader "rainbow" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_PerSec ("Rainbows per second", Float) = 1
		_PerX ("Rainbows per x", Int) = 1
		_PerXb ("Rainbows per x (building)", Int) = -1
		_PerY ("Rainbows per y", Float) = 1.3
	}
	
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
		
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _PerSec;
			float _PerX;
			float _PerXb;
			float _PerY;
			
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
				f = f - floor(f);
				float a = (1.0f - f) * 6f;
				int X = a;
				float Y = a - X;
				float3 rgb;			
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
						rgb = float3(1.0,0.0,1.0-Y);
						break;
				case 6: 
						rgb = float3(1.0,0.0,0.0);
						break;
				}
		
				return rgb;
		}
		
		float4 frag(outV i) : COLOR {
				float2 uv = i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex = tex2D(_MainTex, saturate(uv));
				bool building = (tex.w > 0.8);
				float t = - _Time.y * _PerSec;
				float y = i.tex.y * _PerY;
				float x;
				if (building) {
					x = i.tex.x * _PerXb;
				}
				else  {
					x = i.tex.x * _PerX;
				}								
				float param = t + x + y;
				if (!building) {
					param -= 2*t + abs(sin(radians(x*360)));
				}
				float3 rgb = toRainbowRGB(param);
				if (building) {
					rgb*=0.7;
				}
				return float4(rgb, 1.0);
		}
			
			ENDCG
		}
	}
}