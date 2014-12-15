using UnityEngine;
using System.Collections;

public class LevelCreator : MonoBehaviour {

	public int toCreate = 3;
	public float size = 1f;
	public GameObject block;
	public GameObject player;
	public GameObject map;
	public GameObject cactus;

	public int range = 30;
	public int lastHeight = 0;
	public float cactusProp = 0.01f;
	public float gapProp = 0.1f;

	public bool reset = false;


	// Use this for initialization
	void Start () {
		resetMap ();
		while (player.transform.position.x + range > toCreate) {
			createBlock();
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (reset) {
			resetMap();
			reset = false;
		}
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

		value = Random.Range (0f, 1f);
		if (value < gapProp) {

		} else {
			GameObject blockObject = (GameObject) Instantiate (block, new Vector3 (toCreate, lastHeight + delta, 0), Quaternion.identity);
			blockObject.name = "Block";
			blockObject.transform.parent = map.transform;
			
			value = Random.Range (0f, 1f);
			if (value < cactusProp) {
				GameObject cactusObject = (GameObject) Instantiate (cactus, new Vector3 (toCreate, lastHeight + delta + 1 , 0), Quaternion.identity);
				cactusObject.name = "Block";
				cactusObject.transform.parent = map.transform;
			}
		}

		lastHeight = lastHeight + delta;
		toCreate++;

	}

	public void resetMap() {
		for (int i = 0; i < map.transform.childCount; i++) {
			Destroy(map.transform.GetChild(i).gameObject);
		}
		toCreate = 0;
		lastHeight = 0;
	}
}
