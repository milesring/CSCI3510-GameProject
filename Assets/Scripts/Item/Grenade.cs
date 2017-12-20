using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour {
	public float delay = 3f;
	public float radius = 10f;
	public float force = 700f;
	public float damage = 100;
	public GameObject explosionEffect;
	public AudioClip explosionSound;
	// Use this for initialization

	//Terrain



	public IEnumerator Detonate(GameObject thrower){
		yield return new WaitForSeconds (delay);
		GameObject expl = Instantiate (explosionEffect, transform.position, transform.rotation);
		Destroy (expl, 2f);
		AudioSource.PlayClipAtPoint (explosionSound, transform.position);
	



		Collider[] colliders = Physics.OverlapSphere (transform.position, radius);


		foreach (Collider nearbyObject in colliders) {
			Status charStatus = nearbyObject.GetComponent<Status> ();
			if (charStatus) {
				float sqrDistance = (nearbyObject.transform.position - transform.position).sqrMagnitude;
				float damageToDo = 0;

				if (sqrDistance < 20f) {
					damageToDo = damage;
				} else if (sqrDistance < 40f) {
					damageToDo = damage * 0.75f;
				} else if (sqrDistance < 60f) {
					damageToDo = damage * 0.5f;
				} else if (sqrDistance < 80f) {
					damageToDo = damage * 0.25f;
				} 

				if (charStatus.DamageHealthAndArmor (damageToDo)) {
					thrower.GetComponent<Status> ().IncreaseKills ();
				}
			}
		}

		colliders = Physics.OverlapSphere (transform.position, radius);
		foreach (Collider nearbyObject in colliders) {
			Rigidbody rb = nearbyObject.GetComponent<Rigidbody> ();

			if (rb) {
				rb.AddExplosionForce (force, transform.position, radius);
			}

		}

		Destroy (gameObject);


	}
						
}
