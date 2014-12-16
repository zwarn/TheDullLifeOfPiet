Shader "rainbow" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_PerSec ("Rainbows per second", Float) = 1
		_PerX ("Rainbows per x", Int) = 1
		_PerXb ("Rainbows per x (building)", Int) = -1
		_PerY ("Rainbows per y", Float) = 1.3	
		_XS ("Warp X Scale", Float) = 6.29
		_YS ("Warp Y Scale", Float) = 1.0
        _TimeArg ("Time argument", Float) = 0
        _Intensity ("Intensity argument", Float) = 0
		_Speed ("Warp Speed", Float) = 1.0
	}
	 
	SubShader {
		Pass {
			CGPROGRAM
			#include "Rainbow.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0 
		
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _PerSec;
			float _PerX;
			float _PerXb;
			float _PerY;
			float _XS;
			float _YS;
			float _TimeArg;
            float _Intensity;
			
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
		
		float4 frag(outV i) : COLOR {
				float2 xy = i.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float distort = cos(xy.x*_XS);
				float timeFactor = sin(radians(_TimeArg*360));
				xy += float2(0.0, distort*_YS*timeFactor*_Intensity);
				float4 tex = tex2D(_MainTex, saturate(xy));
				bool building = (tex.w > 0.8);
				float t = _TimeArg * _PerSec;
				float y = i.tex.y * _PerY;
				float x;
				if (building) {
					x = i.tex.x * _PerXb;
				}
				else  {
					x = i.tex.x * _PerX;
				}								
				float param = 0.0;
				if (!building) {
					param = y - t - sin(radians(x*180));
				}
				else { 
					param = x + y + t;
				}
				float3 rgb = toRainbowRGB(param);               
                if (building) {
                    rgb*=0.7;
                }
                float fg = 0.4;
                float3 grey = building? float3(fg,fg,fg) : float3(0,0,0);				
				return float4(lerp(grey, rgb, _Intensity), 1.0);
		}
			
			ENDCG
		}
	}
}