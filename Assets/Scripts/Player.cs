using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviour {
	public GameObject pivot;
	public float speed;
	public Text text;
	public float minimalR;
	public float maximalR;
	public float rSpeed;

	private Rigidbody rb;
	private int direction;
	private int points;
	private float radiusModifier;

	private Vector2 touchCache;
	private bool touched;
//	private Vector3 side;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		direction = 1;
		points = 0;
		radiusModifier = 0;
		touched = false;
	}
	
	// Update is called once per frame
	void Update () {
//		text.text = "v: "+ Vector3.Dot (rb.velocity, transform.right);

		KeepRotate();
	}

	void UpdateMove(Vector2 oldpos, Vector2 newpos){
		Vector2 move = newpos-oldpos;
		radiusModifier = Math.Min(move.magnitude/500f, 1f);
	}

	void FixedUpdate() {
		if (Input.touchSupported) {
			if (Input.touchCount >= 1) {
				Touch t = Input.touches [0];
				if (touched) {
					UpdateMove (touchCache, t.position);
				} else {
					touched = true;
					touchCache = t.position;
				}
			} else {
				touched = false;
				radiusModifier = 0;
			}
		} else {
			if (Input.GetKey (KeyCode.F)) {
				radiusModifier += Time.fixedDeltaTime;
			} else {
				radiusModifier = 0;
			}
		}
	}

	void KeepRotate() {
		Vector3 dir = pivot.transform.position - transform.position;
		float oriRadius = dir.magnitude;
		Vector3 newV = transform.position + Vector3.Cross(Vector3.up, dir.normalized).normalized * direction * speed * Time.deltaTime; //tangent movement


		dir = pivot.transform.position - newV;
		float rFix = dir.magnitude - oriRadius;
		if (radiusModifier == 0) {
			rFix += Math.Min (oriRadius - minimalR, rSpeed * Time.deltaTime);
		} else {
			rFix += -Math.Min (maximalR - oriRadius, rSpeed * radiusModifier * Time.deltaTime);
		}
		newV += rFix*dir.normalized; // fix the movement error and adjust to target radius
		rb.MovePosition(newV);
	}

	void OnTriggerEnter(Collider other){
		if (other.CompareTag ("blue")) {
			Bounce ();
			other.gameObject.SetActive (false);
		} else if (other.CompareTag("red")){
			Dead ();
		} else if (other.CompareTag("yellow")){
			speed *= 0.5f;
			other.gameObject.SetActive (false);
		} else if (other.CompareTag("green")){
			speed *= 2f;
			other.gameObject.SetActive (false);
		} else if (other.CompareTag("point")){
			points += 1;
			UpdatePoints();
			other.gameObject.SetActive (false);
		}
	}

	void UpdatePoints(){
		text.text = "Points: " + points;
		if (points > 16) {
			Dead();
			text.text = "You Won!";
		}
	}

	void Dead(){
		speed = 0;
		rSpeed = 0;
		direction = 0;
		text.text = "YOU LOST";
	}

	void Bounce(){
		direction = -direction;
	}
}
