using UnityEngine;
using System.Collections;

public class TextureRotator : MonoBehaviour
{

		public Texture2D[] textures = new Texture2D[0];
		public float swapEverySeconds = 1f;
		public bool rainbowMode = false;
		public Texture2D originalTexture;
		private float timePassed;
		private int currentTexture;

		public void EnterRainbowMode() {
		rainbowMode = true;
	    }

		public void LeaveRainbowMode() {
		rainbowMode = false;
		RenderCurrentTexture();
		}

		// Use this for initialization
		void Start ()
		{
				timePassed = 0f;
				currentTexture = 0;
				originalTexture = (Texture2D)renderer.material.mainTexture;
				RenderCurrentTexture ();			
		}
	
		// Update is called once per frame
		void Update ()
		{
				if (rainbowMode) {
						timePassed += Time.deltaTime;
						if (timePassed >= swapEverySeconds) {
								timePassed -= swapEverySeconds;
								NextTexture ();
						}
				}
		else RenderCurrentTexture();
		
		}

		void NextTexture ()
		{
				currentTexture++;
				if (currentTexture >= textures.Length) {
						currentTexture = 0;
				}
				RenderCurrentTexture ();
				return;
		}

		public void AddTexture (Texture2D texture)
		{
				Texture2D[] finalArray = new Texture2D[ textures.Length + 1 ];
				for (int i = 0; i < textures.Length; i ++) {
						finalArray [i] = textures [i];
				}
			
				finalArray [finalArray.Length - 1] = texture;
				textures = finalArray;
		}

		public void ClearTextures ()
		{
				textures = new Texture2D [0];
		}

		void RenderCurrentTexture ()
		{
				if (rainbowMode) {
						if (textures.Length > 0) {
								renderer.material.mainTexture = textures [currentTexture];
						}
				}
				else {
						renderer.material.mainTexture = originalTexture;
				}
		}
}
