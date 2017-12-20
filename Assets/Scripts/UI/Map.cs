using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {
	GameObject mapCam;
	GameObject mapImage;
	GameObject mapCircle;
	GameObject mapBorder;
	bool mapEnabled = false;
	GameStatus gameStatus;


	GameObject wall;
	TerrainData terrainData;
	float wallInitialProportion;
	float mapCircleInitialScale;
	float radius;
	Vector2 distances;


	void Start(){
		gameStatus = GameObject.Find ("GameStatus").GetComponent<GameStatus> ();
		mapCam = transform.GetChild(0).gameObject;
		mapImage = transform.GetChild (1).GetChild(0).gameObject;
		mapBorder = transform.GetChild (1).gameObject;
		mapCircle = transform.GetChild (1).GetChild(1).gameObject;
		mapCam.SetActive (mapEnabled);
		mapImage.SetActive (mapEnabled);
		mapCircle.SetActive (mapEnabled);

		//map circle
		wall = GameObject.Find ("BlueWall");
		terrainData = GameObject.FindGameObjectWithTag ("Ground").GetComponent<Terrain> ().terrainData;
		distances = new Vector2 (wall.transform.position.x, wall.transform.position.z);
		radius = wall.transform.localScale.x / 2;
		wallInitialProportion = radius / terrainData.size.x;
		mapCircleInitialScale = mapCircle.transform.localScale.x;
		mapCircle.transform.localPosition = new Vector3 (mapCircle.transform.localPosition.x + distances.x / terrainData.size.x * 500f,
			mapCircle.transform.localPosition.y + distances.y / terrainData.size.x * 500f,
			mapCircle.transform.localPosition.z);

		Debug.Log ("Wall init prop "+wallInitialProportion);
		Debug.Log (mapCircleInitialScale);
	}

	// Update is called once per frame
	void Update () {
		if (!gameStatus.GameOver) {
			if (Input.GetKeyDown (KeyCode.M)) {
				mapEnabled = !mapEnabled;
				mapCam.SetActive (mapEnabled);
				mapImage.SetActive (mapEnabled);
			}

			transform.localScale = new Vector3 (mapCircleInitialScale * wall.transform.localScale.x / 2 / terrainData.size.x / wallInitialProportion, mapCircleInitialScale * wall.transform.localScale.x / 2 / terrainData.size.x / wallInitialProportion, mapCircle.transform.localScale.z);
			//Debug.Log (wall.transform.localScale.x / 2 / terrainData.size.x / wallInitialProportion);
			Debug.Log ("fsdaf :" + wall.transform.localScale.x / 2 / terrainData.size.x);
		}



	}
}
