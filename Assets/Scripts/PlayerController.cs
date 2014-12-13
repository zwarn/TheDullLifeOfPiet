using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed = 400f;
	public float jumppower = 1f;
	public bool onGround = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		rigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * speed * Time.deltaTime,
		                                   rigidbody2D.velocity.y);

		RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector3.down, 0.55f, 1 << LayerMask.NameToLayer("Map"));
		if (hit.collider != null) {
			onGround = true;
		} else {
			onGround = false;
		}

		if (Input.GetButtonDown("Jump")) {
			if (onGround) {	
				rigidbody2D.AddForce(Vector3.up*jumppower);
			}
		}


	}
}
