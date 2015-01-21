using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIController : MonoBehaviour {

	public Canvas gui;

	private GameController gameController;
	private Text pointsLabel;

	void Start () {
		gameController = GameController.Instance;
		pointsLabel = gui.transform.FindChild ("Points").GetComponent<Text> ();
	}

	// Update is called once per frame
	void Update () {

		pointsLabel.text = ((int) gameController.points).ToString();
	}
}
