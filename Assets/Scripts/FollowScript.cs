using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowScript : MonoBehaviour {

	public GameObject toFollow;
	public float camSpeed = 0.6f;

	public List<ParallexScrollingEntry> parallex;

	[System.Serializable]
	public class ParallexScrollingEntry {
		public GameObject plane;
		public Vector3 scrollSpeed;

	}
	
	// Update is called once per frame
	void Update () {
		Vector3 target = new Vector3(toFollow.transform.position.x,
		                             toFollow.transform.position.y,
		                             transform.position.z);
		//transform.position = Vector3.Lerp (transform.position, target, camSpeed * Time.deltaTime);
		transform.position = target;

		foreach (ParallexScrollingEntry entry in parallex) {
			entry.plane.transform.position = toFollow.transform.position - Vector3.Scale (toFollow.transform.position, entry.scrollSpeed);
		}
	}
}
