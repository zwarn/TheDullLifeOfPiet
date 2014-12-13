using UnityEngine;
using System.Collections;

public class HueRotate : MonoBehaviour {
	public float rotAngle;

	private Texture2D newTex;
	private Texture2D origTex;
	private float currAngle;


void Start() {
		print (renderer.material.mainTexture);
	origTex = (Texture2D) renderer.material.mainTexture;
	newTex = new Texture2D(origTex.width, origTex.height);
	currAngle = 0;
}


void Update () {
		print ("tex: "+renderer.material.mainTexture);
	currAngle += rotAngle * Time.deltaTime;
		while (currAngle>=1)
						currAngle -= 1;
	HueRot(origTex, newTex, currAngle);
	renderer.material.mainTexture = newTex;
}

	void HueRot(Texture2D inTex, Texture2D outTex, float angle) {
	Color[] pix = inTex.GetPixels(0, 0, inTex.width, inTex.height);
	
	for (int i = 0; i < pix.Length; i++) {
		HSBColor hsb = HSBColor.FromColor(pix[i]);
			if (i== 50) print (pix[i]);
			hsb.h = hsb.h + angle;
		pix[i] = hsb.ToColor();		
			if (i== 50) print (pix[i]);
	}
    
	outTex.SetPixels(0, 0, inTex.width, inTex.height, pix);
	outTex.Apply();

}
}