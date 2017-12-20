using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Player;

public class Status : MonoBehaviour {
	public float health;
	public float maxHealth;
	public float armor, maxArmor;
    public bool isDead;
	private int kills;

	GameStatus gameStatus;

	void Start(){
        isDead = false;
		GameObject gameStatusObj = GameObject.Find ("GameStatus");
		if(gameStatusObj){
			gameStatus = gameStatusObj.GetComponent<GameStatus>();
		}
	}

	//armor absorbs damage, if armor runs out, health is damaged
	public bool DamageHealthAndArmor(float damage){
		if (armor > 0) {
			armor -= damage;
			if (armor < 0) {
				health += armor;
				armor = 0;
			}
		} else {
			health -= damage;
		}

		//apply camera shake
		if (CompareTag ("Player")) {
			Camera.main.GetComponent<CameraManager> ().Damage ();
		}

		if (health <= 0 && !isDead) {
            Death();
			return true;
		}

		return false;
	}

	public void DamageHealth(float damage){
		health -= damage;
		if (health <= 0 && !isDead) {
            Death();
		}
			
	}



	//restore health to limit
	public IEnumerator RestoreHealth(int heal, float healTime){
		//Debug.Log ("Healing...");
		//Debug.Log ("Health: " + health);
		yield return new WaitForSeconds (healTime);
		health += heal;
		if (health > maxHealth) {
			health = maxHealth;
		}
	}

	//restore armor
	public void IncreaseArmor(int amount){
		armor += amount;
        if(armor > maxArmor)
        {
            armor = maxArmor;
        }
	}

    public void DecreaseArmor(int amount)
    {
        armor -= amount;
        if (armor < 0)
        {
            armor = 0;
        }
    }

    void Death(){
        GameObject ragdoll = transform.GetChild(2).gameObject;
        ragdoll.SetActive(true);
        ragdoll.transform.SetParent(null);

        GetComponent<Inventory>().DropAllInventory();

		if (gameObject.tag.Equals ("Player")) {
			Camera.main.GetComponent<CameraManager> ().Death (transform);
			gameStatus.GameOver = true;
		}

        gameObject.SetActive(false);
        isDead = true;

		if(!gameObject.tag.Equals("Player") && gameStatus){
			gameStatus.enemyDied ();
		}
    }

	public void IncreaseKills(){
		kills++;
	}

	public int Kills(){
		return kills;
	}

}
