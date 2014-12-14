using UnityEngine;
using System.Collections;

public class LevelCreator : MonoBehaviour {

	public int toCreate = 3;
	public float size = 1;
	public GameObject block;
	public GameObject player;

	public int range = 30;
	public int lastHeight = 0;


	// Use this for initialization
	void Start () {

		while (player.transform.position.x + range > toCreate) {
			createBlock();
		}

	}
	
	// Update is called once per frame
	void Update () {
		while (player.transform.position.x + range > toCreate) {
			createBlock();
		}
	}


	public void createBlock() {

		float value = Random.Range (0f, 1f);
		int delta = 0;

		if (value < 0.6f) {
			delta = 0;
		} else if (value < 0.8f) {
			delta = 1;
		} else { 
			delta = -1;
		}

		
		Instantiate (block, new Vector3 (toCreate, lastHeight + delta, 0), Quaternion.identity);
		lastHeight = lastHeight + delta;
		toCreate++;

	}
}
