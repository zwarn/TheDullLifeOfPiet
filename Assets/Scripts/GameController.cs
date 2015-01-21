using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	private static GameController _instance;

	private LevelCreator levelCreator;
	private PlayerController playerController;

	void Awake ()
	{
		_instance = this;
	}
	
	public GameController Instance {
		get {return _instance;}
	}
	// Use this for initialization
	void Start () {
		levelCreator = LevelCreator.Instance;
		levelCreator.resetMap ();

		playerController = PlayerController.Instance;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
