Shader "GAF/GAFObjectsGroup" 
{
	Properties 
	{
		_MainTex ("Particle Texture", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater .01
		Cull Off
		Zwrite Off
		Lighting Off
	
		CGPROGRAM
				
		#pragma surface surf Unlit noambient vertex:vert
		#pragma glsl_no_auto_normalization
				
		#include "UnityCG.cginc"
	
		sampler2D _MainTex;
				
		struct Input
		{
			float2 uv_MainTex;
			fixed4 color;
			fixed4 colorShift;
		};
	
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color;
			
			TANGENT_SPACE_ROTATION;
			o.colorShift = fixed4(mul(rotation, v.tangent.xyz), v.tangent.w);
		}
				
		fixed4 LightingUnlit(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			fixed4 c;
			c.rgb = s.Albedo; 
			c.a = s.Alpha;
			return c;
		}

		void surf (Input input, inout SurfaceOutput o)
		{
			fixed4 mainColor = tex2D(_MainTex, input.uv_MainTex );

			o.Albedo = mainColor.rgb * input.color.rgb + input.colorShift.rgb;
			o.Alpha  = mainColor.a   * input.color.a   + input.colorShift.a;
		}

		ENDCG 
	}
}
