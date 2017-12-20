using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviour {
	public int enemyCount;
    public List<AIThreat> threats;

	private GameObject wall;
	public void Init(){
		Terrain ground = GameObject.FindGameObjectWithTag ("Ground").GetComponent<Terrain>();
		TerrainData groundData = ground.terrainData;
		Vector3 groundSize = groundData.size;
        threats = new List<AIThreat>();

		wall = GameObject.Find ("BlueWall");

		float randx = 0f;
		float randz = 0f;
		float height = 0f;
		GameObject enemy = (GameObject)Resources.Load ("Prefabs/Characters/Enemy");
		for (int i = 0; i < enemyCount; ++i) {
			randx = Random.Range (-groundSize.x / 2, groundSize.x / 2);
			randz = Random.Range (-groundSize.z / 2, groundSize.z / 2);
			height = ground.SampleHeight (new Vector3 (randx, 0f, randz));
			GameObject bruh = Instantiate (enemy, new Vector3(randx , height, randz), Quaternion.identity);

            AIThreat ethreat = bruh.GetComponent<AIThreat>();
            if (ethreat != null)
                threats.Add(ethreat);
		}
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		randx = Random.Range (-groundSize.x / 2, groundSize.x / 2);
		randz = Random.Range (-groundSize.z / 2, groundSize.z / 2);
		height = ground.SampleHeight (new Vector3 (randx, 0f, randz));
		player.transform.position = new Vector3(randx , height, randz);

        AIThreat pthreat = player.GetComponent<AIThreat>();
        if (pthreat != null)
            threats.Add(pthreat);

        wall.GetComponent<Wall> ().SetAllowedToMove (true);
	}
}
