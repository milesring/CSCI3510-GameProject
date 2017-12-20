using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wall : MonoBehaviour {
	private float sizeChangeAmount;
	public int sizeChangeInterval;
	public float sizeChangeSpeed;
	public float minSize;
	public int numStages;
	public float damage;

	private int currentStage;
	private float gameTime;
	private float goalSize;
	private Vector3 center;
	List<GameObject> charactersToDamage;
	public bool useTerrainNeighbors;
	private bool allowedToMove;
	private float totalTime;
	private float lastUpdate;
	// Use this for initialization

	private bool init = false;
	public void Init () {
		//initial size

		GameObject ground = GameObject.FindGameObjectWithTag("Ground");
		Vector3 terrain = ground.GetComponent<Terrain> ().terrainData.size;
		//x and z stay equal
		//transform.localScale = new Vector3(ground.transform.localScale.x,transform.localScale.y,ground.transform.localScale.x);
		//used to touch all 4 corners
		float multiplier = Mathf.Sqrt(2);
		if (useTerrainNeighbors) {
			transform.localScale = new Vector3 (terrain.x * multiplier, transform.localScale.y, terrain.x * multiplier)*2;
		} else {
			transform.localScale = new Vector3(terrain.x, transform.localScale.y, terrain.x);
		}

		float buffer = terrain.x / 4;
		//center location
		center = new Vector3(Random.Range(-terrain.x/2+buffer, terrain.x/2-buffer),transform.position.y, Random.Range(-terrain.z/2+buffer, terrain.z/2-buffer));
		transform.position = center;


		charactersToDamage = new List<GameObject> ();
		//add all enemies
		GameObject[] characters = GameObject.FindGameObjectsWithTag ("Enemy");
		foreach (GameObject character in characters) {
			charactersToDamage.Add (character);
		}
		//add player
		charactersToDamage.Add(GameObject.FindGameObjectWithTag("Player"));
		//render the cylinder inside out
		Mesh mesh = GetComponent<MeshFilter> ().mesh;
		mesh.triangles = mesh.triangles.Reverse ().ToArray();

		goalSize = transform.localScale.x;

		//divide up the size into stages
		sizeChangeAmount = transform.localScale.x / numStages;

		currentStage = 0;
		lastUpdate = 0f;
		init = true;
		allowedToMove = false;
	}
	
	// Update is called once per frame
	void Update () {
		//TODO make this change scale in chunks;
		if (init && allowedToMove) {
			if (transform.localScale.x > minSize) {
				if (gameTime > sizeChangeInterval) {
					//begin moving wall
					goalSize -= sizeChangeAmount;
					gameTime = 0f;
					currentStage++;
				}
				if (goalSize < transform.localScale.x) {
					//change the scale of the wall
					transform.localScale -= new Vector3 (1, 0, 1) * Time.deltaTime * sizeChangeAmount / sizeChangeSpeed;
				} else {
					//avoids tracking time while the wall is shrinking
					gameTime += Time.deltaTime;
				}
			}
            
			//applies damage to any character in wall
			foreach (GameObject character in charactersToDamage) {
				Status charStatus = character.GetComponent<Status> ();
				if (charStatus) {
					float damagetodo = currentStage * damage * Time.deltaTime;
					charStatus.DamageHealth (damagetodo);
				}
			}

		}
	}

	void OnTriggerExit(Collider other){
		charactersToDamage.Add (other.gameObject);
	}

	void OnTriggerEnter(Collider other){
        charactersToDamage.Remove(other.gameObject);
	}

	public void SetAllowedToMove(bool val){
		allowedToMove = val;
	}
}
