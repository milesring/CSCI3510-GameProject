using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Player;

public class UpdateUI : MonoBehaviour {
    Inventory inventory;
    WeaponManager weaponmanager;
    Text ammoCounter;
	Text enemyCount;
    Slider playerHealth, playerArmor;
    Status status;
	GameStatus gameStatus;
	GameObject endUI;

	// Use this for initialization
	void Start () {
		GameObject gameStatusObj = GameObject.Find ("GameStatus");
		if(gameStatusObj){
			gameStatus = gameStatusObj.GetComponent<GameStatus> ();
		}
		status = GameObject.FindGameObjectWithTag("Player").GetComponent<Status>();
		inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
		weaponmanager = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponManager>();
        ammoCounter = transform.Find("AmmoCounter").GetComponent<Text>();
        playerHealth = transform.Find("HealthSlider").GetComponent<Slider>();
        playerArmor = transform.Find("ArmorSlider").GetComponent<Slider>();
		enemyCount = transform.Find ("EnemyCount").GetComponent<Text> ();
		endUI = transform.Find ("EndUI").gameObject;
		endUI.SetActive (false);

    }
	
	// Update is called once per frame
	void Update () {
		if (weaponmanager.GetEquippedWeapon () != null) {
			ammoCounter.text = weaponmanager.GetEquippedWeapon ().GetComponent<Gun> ().GetCurrentAmmo () + " / ";
			switch (weaponmanager.GetEquippedWeapon ().name) {
			case "M4_Carbine":
			case "M4_Carbine(Clone)":
				ammoCounter.text += inventory.GetAmmo () [0];
				break;
			case "AK-47":
			case "AK-47(Clone)":
				ammoCounter.text += inventory.GetAmmo () [1];
				break;
			case "L96_Sniper_Rifle":
			case "L96_Sniper_Rifle(Clone)":
				ammoCounter.text += inventory.GetAmmo () [2];
				break;
			}
		} else {
			ammoCounter.text = "";
		}
        playerHealth.value = status.health;
        playerArmor.value = status.armor;
		if (gameStatus) {
			enemyCount.text = "Enemy Count: " + gameStatus.aiAlive;
		}
		if (status.health <= 0 || gameStatus && gameStatus.aiAlive <= 0) {
			EndGame ();
		}
	}

	void EndGame(){
		StartCoroutine (EndGameSeq ());

	}

	IEnumerator EndGameSeq(){
		yield return new WaitForSeconds (4);
		StartCoroutine (SlowTime ());
		endUI.SetActive (true);
		Text endText = endUI.transform.Find ("EndText").GetComponent<Text>();
		Text endKills = endUI.transform.Find ("EndKills").GetComponent<Text>();
		if (gameStatus.aiAlive <= 0) {
			endText.text = "Winner winner!";
		} else {
			endText.text = "You lose.";
		}

		endKills.text = "Kills: "+status.Kills ();

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	IEnumerator SlowTime(){
		float timer = 0f;
		float time = 0.5f;
		float origTimeScale = Time.timeScale;
		while (timer < time) {
			Time.timeScale = Mathf.Lerp (origTimeScale, 0f, timer / time);
			timer += Time.deltaTime;
			yield return new WaitForEndOfFrame ();
			if (Time.timeScale < 0.05) {
				break;
			}
		}
		Time.timeScale = 0f;
	}
}
