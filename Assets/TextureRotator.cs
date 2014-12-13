using UnityEngine;
using System.Collections;

public class TextureRotator : MonoBehaviour {

	public ArrayList textures = new ArrayList();
	public float swapEverySeconds = 1f;

	private float timePassed;
	private int currentTexture;

	// Use this for initialization
	void Start () {
		timePassed = 0f;
		currentTexture = 0;
		RenderCurrentTexture();
	}
	
	// Update is called once per frame
	void Update () {
		timePassed+= Time.deltaTime;
		if (timePassed >= swapEverySeconds) {
			timePassed-= swapEverySeconds;
			NextTexture();
		}
		
	}

	void NextTexture() {
		currentTexture++;
		if (currentTexture >= textures.Count) {
			currentTexture = 0;
		}
		RenderCurrentTexture();
		return;
	}

	public void AddTexture(Texture2D texture) {
		textures.Add(texture);
	}

	void RenderCurrentTexture() {
		if (textures.Count > 0) {
			renderer.material.mainTexture = (Texture2D) textures[currentTexture];
		}
	}
}
