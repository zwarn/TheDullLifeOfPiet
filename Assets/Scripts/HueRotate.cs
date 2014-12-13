using UnityEngine;
using System.Collections;

public class HueRotate : MonoBehaviour
{
		public float rotAngle;
		private Texture2D newTex;
		private Texture2D origTex;
		private float currAngle;
		private int frameCounter;

		void Start ()
		{
				print (renderer.material.mainTexture);
				origTex = (Texture2D)renderer.material.mainTexture;
				newTex = new Texture2D (origTex.width, origTex.height);
				currAngle = 0;
		}

		Color ToRainbowRGB (float f)
		{

				float a = (1.0f - f) / 0.2f;
				int X = Mathf.FloorToInt (a);
				int Y = Mathf.FloorToInt (255 * (a - X));
				float r = 0;
				float g = 0;
				float b = 0;
				switch (X) {
				case 0:
						r = 255;
						g = Y;
						b = 0;
						break;
				case 1:
						r = 255 - Y;
						g = 255;
						b = 0;
						break;
				case 2:
						r = 0;
						g = 255;
						b = Y;
						break;
				case 3:
						r = 0;
						g = 255 - Y;
						b = 255;
						break;
				case 4:
						r = Y;
						g = 0;
						b = 255;
						break;
				case 5:
						r = 255;
						g = 0;
						b = 255;
						break;
				}
		
				return new Color (r / 256f, g / 256f, b / 256f);
		}

		float PixelParam (int x, int y, float angle)
		{
				return ((x + y + angle) % 1000) / 1000;
		}

		void Update ()
		{
				currAngle += rotAngle * Time.deltaTime;
				frameCounter++;
				if (frameCounter % 100 == 0) {
						HueRot (origTex, newTex, currAngle);
						renderer.material.mainTexture = newTex;
				}
		}

		void HueRot (Texture2D inTex, Texture2D outTex, float angle)
		{
				Color[] pix = inTex.GetPixels (0, 0, inTex.width, inTex.height);
				for (int y = 0; y < inTex.height; y++) {		
						for (int x = 0; x < inTex.width; x++) {
								int i = y * inTex.width + x;
//								if ((i % 1000 == 0))
//										print ("pix: " + pix [i].a);
								if (pix [i].a < 0.8) {
										// background
										pix [i] = ToRainbowRGB (PixelParam (x, y, angle));		
								} else {
										// house
										Color col = ToRainbowRGB (PixelParam (inTex.width - x, y, angle));		 
										pix [i] = Color.Lerp (col, Color.black, 0.6f);
								}
						}
				
				}
    
				outTex.SetPixels (0, 0, inTex.width, inTex.height, pix);
				outTex.Apply ();

		}
}