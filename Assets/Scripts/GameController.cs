using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	private static GameController _instance;

	public float points;

	private LevelCreator levelCreator;
	private PlayerController playerController;
	private CactusController cactusController;

	private float highestX = 0;

	void Awake ()
	{
		_instance = this;
	}
	
	public static GameController Instance {
		get {return _instance;}
	}
	// Use this for initialization
	void Start () {
		levelCreator = LevelCreator.Instance;
		levelCreator.resetMap ();

		playerController = PlayerController.Instance;

		cactusController = CactusController.Instance;
	}
	
	// Update is called once per frame
	void Update () {
		float x = playerController.gameObject.transform.position.x;
		if (x > highestX) {
			points += (x - highestX) * Mathf.Pow(2,1+cactusController.cactusLevel);
			highestX = x;
		}
	}
}
