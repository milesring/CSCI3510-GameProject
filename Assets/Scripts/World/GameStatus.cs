using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatus : MonoBehaviour {
	public int aiAlive;
	private CameraManager cameraManager;
	public bool GameOver;
	//TODO this will store all data relating to the game, ie ai left alive. when 0 end game.

	void Start(){
		aiAlive = GameObject.Find ("CharacterSpawner").GetComponent<CharacterSpawner>().enemyCount;
		cameraManager = Camera.main.GetComponent<CameraManager> ();
		GameOver = false;
		Time.timeScale = 1f;
	}

	public void enemyDied(){
		aiAlive--;
		if (aiAlive <= 0) {
			cameraManager.Win (GameObject.FindGameObjectWithTag("Player").transform);
			GameOver = true;
		}
	}



}
