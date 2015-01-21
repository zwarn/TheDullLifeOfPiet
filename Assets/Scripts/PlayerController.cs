using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	private static PlayerController _instance;

    public float baseSpeed = 200f;
    public float speedPerCactus = 200f;
    
    public float baseJumppower = 200f;
    public float jumppowerPerCactus = 200f;

	private float chargeTime = 0;

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

	public static PlayerController Instance 
	{
		get {return _instance;}
	}

	void Awake () {
		if (_instance == null) {
			_instance = this;
		} else {
			Debug.LogError("More than one PlayerController");
		}
	}

    void Update ()
    {
        rigidbody2D.velocity = new Vector2 (Input.GetAxis ("Horizontal") * Speed * Time.deltaTime,
                                            rigidbody2D.velocity.y);

		int rayNumbers = 3;
		Vector3 shift = new Vector3 (0.15f,0,0);

		bool foundGround = false;

		for (int i = 0; i < rayNumbers; i++) {
			var hit = Physics2D.Raycast (transform.position + shift * (i-1), Vector3.down, 0.8f, 1 << LayerMask.NameToLayer ("Map"));

			if (hit.collider != null) {
				foundGround = true;
			}

			Debug.DrawLine(transform.position + shift * (i-1), (transform.position + shift * (i-1)) + Vector3.down * 0.8f);
		}

		if (foundGround) {
			onGround = true;
		} else {
			onGround = false;
		}


		if (Input.GetButton ("Jump")) {
			if (onGround) {	
				chargeTime += Time.deltaTime;
			} else {
				chargeTime = 0;
			}
		}

        if (Input.GetButtonUp ("Jump")) {
			if (onGround) {	
				if (chargeTime >= 1) {
					rigidbody2D.AddForce (Vector3.up * Jumppower*1.4f);
				} else {
                	rigidbody2D.AddForce (Vector3.up * Jumppower);
				}
			}
			chargeTime = 0;
        }
        
		if (transform.position.y +10 < LevelCreator.Instance.GetTerrainHeight((int)transform.position.x)) {
			Die ();
        }
    }

    void Die ()
    {
		transform.position = new Vector2 (transform.position.x - 2, LevelCreator.Instance.GetTerrainHeight((int)(transform.position.x - 2)) + 7);
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
