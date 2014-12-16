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
	
    void Update ()
    {
        rigidbody2D.velocity = new Vector2 (Input.GetAxis ("Horizontal") * Speed * Time.deltaTime,
                                            rigidbody2D.velocity.y);

        RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector3.down, 0.9f, 1 << LayerMask.NameToLayer ("Map"));
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
        
        if (transform.position.y < -15) {
            Die ();
        }
    }

    void Die ()
    {
        transform.position = new Vector2 (transform.position.x - 2, 15);
        rigidbody2D.velocity = new Vector2 (0, 0);
        CactusController.CactusLevel = 0;
    }

    void EatCactus (GameObject cactus)
    {
        GameObject.Destroy (cactus);
        CactusController.IncreaseCactusLevel ();
    }

    void OnTriggerEnter2D (Collider2D coll)
    {
        if (coll.CompareTag ("cactus")) {
            EatCactus (coll.gameObject);
        }

    }
}
