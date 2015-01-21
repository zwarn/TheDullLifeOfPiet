using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;

[ExecuteInEditMode]
public class LevelCreator : MonoBehaviour
{

	public GameObject map;
	public GameObject player;

    public int toCreate = 3;
    public float size = 1f;    
    public int range = 30;	
    public int minHeight = -15;
    public int maxHeight = 15;

	public List<Chunk> chunks;

    public bool recreate = false;
    
	private static LevelCreator _instance;
    private int lastHeight = 0;
	private List<int> heightmap;
	private int sumAbundance;

    void Awake ()
    {
		_instance = this;
    }

	public static LevelCreator Instance {
		get {return _instance;}
	}

    // Update is called once per frame
    void Update ()
    {
        if (recreate) {
            resetMap ();
            recreate = false;
        }

        while (player.transform.position.x + range > toCreate) {
			InstantiateChunk ();
        }
    }

	private void InstantiateChunk() {

		Chunk chunk = ChunkSelection ();

		GameObject newObj = (GameObject) Instantiate (chunk.instance, new Vector3(toCreate, lastHeight , 0), Quaternion.identity);
		newObj.transform.parent = map.transform;

		for (int i = 0; i < chunk.width; i++) {
			heightmap.Add(lastHeight);
		}

		lastHeight += chunk.deltaHeight + Random.Range(-1,2);
		toCreate += chunk.width;



	}

	private Chunk ChunkSelection() {
		int value = Random.Range (0, sumAbundance);
		float acc = 0;

		foreach (Chunk chunk in chunks) {
			acc += chunk.abundance;
			if (acc >= value) return chunk;
		}

		Debug.LogError ("Should not happen");
		return chunks.Last();
	}

    public void resetMap ()
    {
		while ( map.transform.childCount > 0) {
            DestroyImmediate (map.transform.GetChild (0).gameObject);
        }
        toCreate = 0;
        lastHeight = 0;
		heightmap = new List<int> ();
		sumAbundance = 0;
		foreach (Chunk chunk in chunks) {
			sumAbundance += chunk.abundance;
		}

	}

	public float GetTerrainHeight (int i)
	{
		if (i <= 0 || i >= heightmap.Count)
						return 0;
		else {
			return heightmap[i];
		}
	}

	[System.Serializable]
	public class Chunk {
		public int width = 1;
		public int deltaHeight = 0;
		public int abundance = 1;
		public GameObject instance;
	}
}
