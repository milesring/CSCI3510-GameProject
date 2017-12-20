using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour {
	Transform player;
	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag ("Player").transform;
	}
	

	void LateUpdate () {
		Vector3 newPosition = new Vector3 (player.position.x, transform.position.y, player.position.z);
		transform.position = newPosition;
		transform.rotation = Quaternion.Euler (90f, player.eulerAngles.y, 0f);
	}
}
