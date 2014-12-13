using UnityEngine;
using System.Collections;

public class ScreenResetScript : MonoBehaviour {

	public Rect bounds;
	public GameObject center;
	
	// Update is called once per frame
	void Update () {

		Vector3 delta = transform.position - center.transform.position; 

		if (delta.x < bounds.xMin) {
			transform.Translate(new Vector3(bounds.width,0,0));
		}
		if (delta.x > bounds.xMax) {
			transform.Translate(new Vector3(-bounds.width,0,0));
		}
		if (delta.y < bounds.yMin) {
			transform.Translate(new Vector3(0,bounds.height,0));
		}
		if (delta.y > bounds.yMax) {
			transform.Translate(new Vector3(0,-bounds.height,0));
		}
	}
}
