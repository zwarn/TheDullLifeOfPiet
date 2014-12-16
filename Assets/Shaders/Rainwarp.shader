Shader "Rainwarp" {
	Properties {
		_MainTex ("Texture", 2D) = "white"
		_XS ("X Scale", Float) = 8.8
		_YS ("Y Scale", Float) = 0.08
		_Speed ("Speed", Float) = 1.0
        _TimeArg ("Time argument", Float) = 0
        _Intensity ("Intensity argument", Float) = 0
		_ColorSpeed	("Color Speed", Float) = 1.0	
	}	

	SubShader {
		Pass {			
			CGPROGRAM
			#include "Rainbow.cginc"
			#pragma fragment frag
			#pragma vertex vert
			#pragma target 3.0

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			float _XS;
			float _YS;	
            float _Intensity;
            float _TimeArg;		
			float _ColorSpeed;
							
			struct vIn {
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};
			
			struct vOut {
				float4 pos : SV_POSITION;
				float2 tex : TEXCOORD0;				
			};
			
			vOut vert (vIn i) {
				vOut o;
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
//				float distort = cos(i.vertex.x*_XS);
//				distort = 0.5 - 0.5 * distort;
				o.tex = i.tex.xy; // + float2(0.0, distort*_YS);				
				return o;
			}
			
			float4 frag (vOut i) : COLOR {                
				float2 xy = i.tex * _MainTex_ST.xy + _MainTex_ST.zw;
				float distort = cos(xy.x*_XS);
				float timeFactor = sin(radians(_TimeArg*360));
				float2 distorted = xy + float2(0.0, distort*_YS*timeFactor*_Intensity);
				float4 tex = tex2D(_MainTex, saturate(distorted));
				if (tex.a < 0.3 || 
					distorted.y<0.0 ||
					distorted.y>1.0 || 
					distorted.x<0.0 ||
					distorted.x>1.0 ) {
					discard;
				}
				return lerp(tex, float4(toRainbowRGB(_TimeArg*_ColorSpeed),1), _Intensity);
			}
																																	
			ENDCG
		}
	}
}