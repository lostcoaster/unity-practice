using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System.Text;

public class ScenarioManager : MonoBehaviour {

//	void onDrawGizmos(){
//		Gizmos.DrawIcon (transform.position, "Light Gizmo.tiff", true);
//	}

	private SocketIOComponent socket;

	private Dictionary<string, List<GameObject>> others;
	public GameObject ball;

	public float updateInterval;
	private float updateCountdown;

	private string[] colors;

	void Awake() {
		others = new Dictionary<string, List<GameObject>>();
		updateCountdown = updateInterval;
		colors = new string[]{"red", "blue", "yellow", "green"};
	}

	void Start() {
		Screen.orientation = ScreenOrientation.Landscape;

		socket = GameObject.Find ("SocketIO").GetComponent<SocketIOComponent> ();
		socket.On ("open", onSocketOpen);

		ScanObjects ();
	}

	public void onMessage(){
		string message = socket.lastMessage;
		string[] segments = message.Split (new []{';'});

		if (segments [0] == "point") {
			GameObject obj = Instantiate (others ["point"] [0]);
			obj.SetActive (true);
			obj.transform.position = new Vector3 (float.Parse (segments [1]), obj.transform.position.y, float.Parse (segments [2]));

			others ["point"].Add (obj);
		} else {
			GameObject obj = Instantiate (others [segments [0]] [0]);
			obj.SetActive (true);
			Vector2 start = new Vector2(float.Parse(segments [1]), float.Parse(segments [2])), 
				end = new Vector2(float.Parse(segments [3]), float.Parse(segments [4]));
			Vector2 mid = (start + end) / 2f;
			obj.transform.position = new Vector3 (mid.x, obj.transform.position.y, mid.y);
			obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, (start - mid).magnitude);
			obj.transform.eulerAngles = new Vector3(0, Mathf.Rad2Deg * Mathf.Atan ((start.x-end.x)/(start.y-end.y)), 0);

			others [segments [0]].Add (obj);
		}
	}

	void ScanObjects() {
		// scan all gimmicks into store
		foreach (string x in colors) {
			others [x] = new List<GameObject> (GameObject.FindGameObjectsWithTag (x));
		}

		others ["point"] = new List<GameObject> (GameObject.FindGameObjectsWithTag ("point"));
	}

	public void onSocketOpen(SocketIOEvent ev){
		socket.EmitRawString ("roleplayer");
	}

	void FixedUpdate(){

		if (socket.lastMessage.Length > 0) {
			onMessage ();
		}
		socket.lastMessage = "";

		updateCountdown -= Time.fixedDeltaTime;
		if (updateCountdown <= 0) {
			sendUpdate ();
			updateCountdown = updateInterval;
		}
	}

	public void sendUpdate() {
		var sb = new StringBuilder ();
		sb.Append ("d");
		sb.Append (ball.transform.position.x);
		sb.Append (";");
		sb.Append (ball.transform.position.z);
		sb.Append (";");

		// the things are not many, we can afford 4x runtime, whatsoever
		foreach (string x in colors){
			foreach (GameObject obj in others[x]) {
				if (!obj.activeInHierarchy) {
					continue;
				}
				float rot = obj.transform.eulerAngles.y * Mathf.Deg2Rad;
				Vector2 orient = new Vector2 (Mathf.Sin (rot), Mathf.Cos (rot)) * obj.transform.localScale.z * 0.5f;

				sb.Append (obj.transform.localPosition.x - orient.x); sb.Append (";");
				sb.Append (obj.transform.localPosition.z - orient.y); sb.Append (";");
				sb.Append (obj.transform.localPosition.x + orient.x); sb.Append (";");
				sb.Append (obj.transform.localPosition.z + orient.y); sb.Append (";");
			}
			sb.Append (";");
		}

		foreach (GameObject obj in others["point"]) {
			if (!obj.activeInHierarchy) {
				continue;
			}
			sb.Append (obj.transform.localPosition.x); sb.Append (";");
			sb.Append (obj.transform.localPosition.z); sb.Append (";");
		}
		sb.Append (";");

		socket.EmitRawString (sb.ToString ());
	}
//	public void addGimmick(GameObject ob){
//		others.Add (ob);
//	}
}

