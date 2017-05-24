using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
	public GameObject player;

	private Camera me;

	void Start() {
		me = GetComponent<Camera> ();
	}

	// Update is called once per frame
	void LateUpdate() {
		transform.position = new Vector3(transform.position.x, player.transform.position.magnitude * 5, transform.position.z);
	}
}
