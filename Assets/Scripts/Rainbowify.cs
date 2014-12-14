using UnityEngine;
using System.Collections;

public class Rainbowify : MonoBehaviour
{

		public Material targetMaterial;
		public Material[] warpMaterials;
		private Shader normalShader;
		private Shader rainbowShader;
		private Shader warpShader;

		// Use this for initialization
		void Start ()
		{
				normalShader = Shader.Find ("Transparent/Diffuse");
				rainbowShader = Shader.Find ("rainbow");
				warpShader = Shader.Find ("Rainwarp");				
		}
	
		void OnDestroy ()
		{
				NoRainbows ();
		}
		
		public void Rainbows ()
		{					
				targetMaterial.shader = rainbowShader;			
				foreach (Material m in warpMaterials) {				
						m.shader = warpShader;
				}						
		}
		
		public void SetSpeed (float speed)
		{
				targetMaterial.SetFloat ("_Speed", speed);			
		}
	
		public void NoRainbows ()
		{
				targetMaterial.shader = normalShader;
				//targetMaterial.shader = rainbowShader;
				foreach (Material m in warpMaterials) {						
						m.shader = normalShader;
				}		
		}
		
		
}
