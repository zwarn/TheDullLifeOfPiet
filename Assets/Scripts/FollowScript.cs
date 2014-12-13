using UnityEngine;
using System.Collections;

public class FollowScript : MonoBehaviour {

	public GameObject toFollow;
	public float camSpeed = 0.6f;
	
	// Update is called once per frame
	void Update () {
		Vector3 target = new Vector3(toFollow.transform.position.x,
		                             toFollow.transform.position.y,
		                             transform.position.z);
		//transform.position = Vector3.Lerp (transform.position, target, camSpeed * Time.deltaTime);
		transform.position = target;
	}
}
