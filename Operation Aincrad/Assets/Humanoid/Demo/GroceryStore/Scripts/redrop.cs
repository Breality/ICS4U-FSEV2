using UnityEngine;

public class redrop : MonoBehaviour {

	private Rigidbody thisRigidbody;
	private Vector3 startPosition;
	private Quaternion startRotation;

	// Use this for initialization
	void Start () {

		startPosition = transform.position;
		startRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position.y < 0.2f) {
            thisRigidbody = transform.GetComponent<Rigidbody>();
            if (thisRigidbody != null) {
                thisRigidbody.MovePosition(new Vector3(startPosition.x, 2, startPosition.z));
                thisRigidbody.MoveRotation(startRotation);
                thisRigidbody.velocity = Vector3.zero;
                thisRigidbody.angularVelocity = Vector3.zero;
            }
		}
	}
}
