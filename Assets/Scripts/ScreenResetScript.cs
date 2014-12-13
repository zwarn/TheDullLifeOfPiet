using UnityEngine;
using System.Collections;

public class ScreenResetScript : MonoBehaviour {

	public Rect bounds;
	
	// Update is called once per frame
	void Update () {
		if (transform.position.x < bounds.xMin) {
			transform.Translate(new Vector3(bounds.width,0,0));
		}
		if (transform.position.x > bounds.xMax) {
			transform.Translate(new Vector3(-bounds.width,0,0));
		}
		if (transform.position.y < bounds.yMin) {
			transform.Translate(new Vector3(0,bounds.height,0));
		}
		if (transform.position.y > bounds.yMax) {
			transform.Translate(new Vector3(0,-bounds.height,0));
		}
	}
}
