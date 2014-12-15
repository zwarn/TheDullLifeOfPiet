using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    public float baseSpeed = 200f;
    public float speedPerCactus = 200f;
    
    public float baseJumppower = 200f;
    public float jumppowerPerCactus = 200f;

    public float Speed {
        get {
            return baseSpeed + CactusController.CactusLevel * speedPerCactus;
        }
    }
    public float Jumppower {
        get {
            return baseJumppower + CactusController.CactusLevel * jumppowerPerCactus;
        }
    }
    public bool onGround = false;
	
    void FixedUpdate ()
    {
        rigidbody2D.velocity = new Vector2 (Input.GetAxis ("Horizontal") * Speed * Time.deltaTime,
		                                   rigidbody2D.velocity.y);

        RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector3.down, 1.45f, 1 << LayerMask.NameToLayer ("Map"));
        if (hit.collider != null) {
            onGround = true;
        } else {
            onGround = false;
        }

        if (Input.GetButtonDown ("Jump")) {
            if (onGround) {	
                rigidbody2D.AddForce (Vector3.up * Jumppower);
            }
        }
    }

    void EatCactus (GameObject cactus)
    {
        GameObject.Destroy (cactus);
        CactusController.CactusLevel += 1;        
    }

    void OnTriggerEnter2D (Collider2D coll)
    {
        if (coll.CompareTag ("cactus")) {
            EatCactus (coll.gameObject);
        }

    }
}
