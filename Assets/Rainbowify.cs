using UnityEngine;
using System.Collections;

public class Rainbowify : MonoBehaviour
{

		public Material targetMaterial;

		private Shader normalShader;
		private Shader rainbowShader;

		// Use this for initialization
		void Start ()
		{
				normalShader = Shader.Find ("Sprites/Default");
				rainbowShader = Shader.Find ("rainbow");
		}	
	
		void OnDestroy ()
		{
				NoRainbows ();
		}
		
		public void Rainbows ()
		{
				targetMaterial.shader = rainbowShader;					
		}
		
		public void SetSpeed (float speed)
		{
				targetMaterial.SetFloat ("_Speed", speed);			
		}
	
		public void NoRainbows ()
		{
				targetMaterial.shader = normalShader;
		}
		
		
}
