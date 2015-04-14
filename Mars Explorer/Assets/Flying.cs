using UnityEngine;
using System.Collections;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class Flying : MonoBehaviour {

	public GameObject oculusLeftEye = null	;
	public GameObject myo = null;
	Rigidbody rr;
	
	float forwardSpeed = 2f;
	float rotationSpeed = 300f;
	
	Vector3 axis;
	float rotationY;
	float rotationX;
	float rotationZ;
	
	// A rotation that compensates for the Myo armband's orientation parallel to the ground, i.e. yaw.
	// Once set, the direction the Myo armband is facing becomes "forward" within the program.
	// Set by making the fingers spread pose or pressing "r".
	private Quaternion _antiYaw = Quaternion.identity;
	
	// A reference angle representing how the armband is rotated about the wearer's arm, i.e. roll.
	// Set by making the fingers spread pose or pressing "r".
	private float _referenceRoll = 0.0f;
	
	// Use this for initialization
	void Start () {
		rr = GetComponent<Rigidbody> ();
		//ThalmicHub hub = ThalmicHub.instance;
		//hub.lockingPolicy = LockingPolicy.None; // Disable locking
	}

	private Pose _last_pose = Pose.Unknown;

	// Update is called once per frame
	void Update () {
		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo>();

		//if (Input.GetKey (KeyCode.Space))

		// Update references. This anchors the joint on-screen such that it faces forward away
		// from the viewer when the Myo armband is oriented the way it is when these references are taken.
		if (thalmicMyo.pose == Pose.FingersSpread && _last_pose != Pose.FingersSpread) {
			print ("Zeroing");
			thalmicMyo.Vibrate(VibrationType.Short);
			// _antiYaw represents a rotation of the Myo armband about the Y axis (up) which aligns the forward
			// vector of the rotation with Z = 1 when the wearer's arm is pointing in the reference direction.
			_antiYaw = Quaternion.FromToRotation (
				new Vector3 (myo.transform.forward.x, 0, myo.transform.forward.z),
				new Vector3 (0, 0, 1)
				);
			
			// _referenceRoll represents how many degrees the Myo armband is rotated clockwise
			// about its forward axis (when looking down the wearer's arm towards their hand) from the reference zero
			// roll direction. This direction is calculated and explained below. When this reference is
			// taken, the joint will be rotated about its forward axis such that it faces upwards when
			// the roll value matches the reference.
			Vector3 referenceZeroRoll = computeZeroRollVector (myo.transform.forward);
			_referenceRoll = rollFromZero (referenceZeroRoll, myo.transform.forward, myo.transform.up);
		}
		
		if (thalmicMyo.pose == Pose.Fist) {
			//start = true;
			// Current zero roll vector and roll value.
			Vector3 zeroRoll = computeZeroRollVector (myo.transform.forward);
			float roll = rollFromZero (zeroRoll, myo.transform.forward, myo.transform.up);
			// Current pitch vector and pitch value.
			//Vector3 zeroPitch = computeZeroPitchVector (myo.transform.forward);
			float pitch = pitchFromZero (new Vector3(0,0,0), myo.transform.forward);
			pitch = pitch/90f;
			forwardSpeed = 10*Mathf.Pow (pitch,3)* (Mathf.Abs (oculusLeftEye.transform.position[1]/7.5f)+1f); //Mathf.Abs(pitch)*pitch;
			roll = normalizeAngle (roll - _referenceRoll);
			roll = roll/-90f;
			rotationSpeed = 500*Mathf.Pow(roll,3);
			//print (roll);
			//print (forwardSpeed);
			FlightMode();
		} else {
			//start = false;
			// STOP
			rr.velocity = new Vector3(0,0,0);
		}

		_last_pose = thalmicMyo.pose;
		//print();
//		if(start == true)
//		{
//			FlightMode();
//		}
	}
	
	void FlightMode(){
		//get rotation values for the leftEye
//		rotationX = oculusLeftEye.transform.localRotation.x / 2;
//		rotationY = oculusLeftEye.transform.localRotation.y / 2;
//		rotationZ = oculusLeftEye.transform.localRotation.z;
		
		//put them into a vector
//		axis = new Vector3 (rotationX, rotationY, rotationZ);
		axis = new Vector3 (0, 1, 0);

		//Rotate
		transform.Rotate (axis * Time.deltaTime * rotationSpeed);
		rr.velocity = oculusLeftEye.transform.forward * forwardSpeed; // * (Mathf.Abs(oculusLeftEye.transform.position[3])/20f+15f);
	}

	// Compute the angle of rotation clockwise about the forward axis relative to the provided zero roll direction.
	// As the armband is rotated about the forward axis this value will change, regardless of which way the
	// forward vector of the Myo is pointing. The returned value will be between -180 and 180 degrees.
	float rollFromZero (Vector3 zeroRoll, Vector3 forward, Vector3 up)
	{
		// The cosine of the angle between the up vector and the zero roll vector. Since both are
		// orthogonal to the forward vector, this tells us how far the Myo has been turned around the
		// forward axis relative to the zero roll vector, but we need to determine separately whether the
		// Myo has been rolled clockwise or counterclockwise.
		float cosine = Vector3.Dot (up, zeroRoll);
		
		// To determine the sign of the roll, we take the cross product of the up vector and the zero
		// roll vector. This cross product will either be the same or opposite direction as the forward
		// vector depending on whether up is clockwise or counter-clockwise from zero roll.
		// Thus the sign of the dot product of forward and it yields the sign of our roll value.
		Vector3 cp = Vector3.Cross (up, zeroRoll);
		float directionCosine = Vector3.Dot (forward, cp);
		float sign = directionCosine < 0.0f ? 1.0f : -1.0f;
		
		// Return the angle of roll (in degrees) from the cosine and the sign.
		return sign * Mathf.Rad2Deg * Mathf.Acos (cosine);
	}

	// Compute the angle of rotation clockwise about the forward axis relative to the provided zero roll direction.
	// As the armband is rotated about the forward axis this value will change, regardless of which way the
	// forward vector of the Myo is pointing. The returned value will be between -180 and 180 degrees.
	float pitchFromZero (Vector3 zeroRoll, Vector3 forward)
	{
		Vector3 antigrav = Vector3.up;
		float cosine = Vector3.Dot (forward, antigrav);

		return Mathf.Rad2Deg * Mathf.Acos (cosine) - 90f;
	}
	
	// Compute a vector that points perpendicular to the forward direction,
	// minimizing angular distance from world up (positive Y axis).
	// This represents the direction of no rotation about its forward axis.
	Vector3 computeZeroRollVector (Vector3 forward)
	{
		Vector3 antigravity = Vector3.up;
		Vector3 m = Vector3.Cross (myo.transform.forward, antigravity);
		Vector3 roll = Vector3.Cross (m, myo.transform.forward);
		
		return roll.normalized;
	}
	
	// Adjust the provided angle to be within a -180 to 180.
	float normalizeAngle (float angle)
	{
		if (angle > 180.0f) {
			return angle - 360.0f;
		}
		if (angle < -180.0f) {
			return angle + 360.0f;
		}
		return angle;
	}
}
