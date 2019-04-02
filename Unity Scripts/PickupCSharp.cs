using System;
using UnityEngine;
using System.Collections;  //JJ for left click raycasting
using System.Collections.Generic;  //JJ for left click raycasting
using UnityEngine.SceneManagement; //For changing scenes (Press esc to go back to main menu)


public class PickupCSharp : MonoBehaviour
{
	public Transform onHand;

	public OVRPlayerController_JunsuTest1 access; //attach this script to a game object and drag the player controller to this public class

	public GameObject currentObject;
	/****************************************************
	 * Variable Access Test that didn't work
	 * public GameObject access = OVRPlayerController;
	 * private OVRPlayerController_JunsuTest1 accessed_script;
	 * **************************************************/

	public float speed = 1000;

	public bool pickupStatus = false;
	//public bool rotateEnable = true;

	public GameObject assigner_onHand;
	public GameObject assigner_access;


	void Start()
	{
		assigner_onHand = GameObject.Find("CubeDragger");
		assigner_access = GameObject.Find("OVRPlayerController");
		//accessed_script = access.GetComponent<OVRPlayerCntroller_JunsuTest1> ();
		//Debug.Log (access.access_test);

		//Cube in layer 0
		//User in layer 9
		Physics.IgnoreLayerCollision (0, 9); //prevents user cube collision
		Physics.IgnoreLayerCollision (0, 0); //prevents cube cube collision

		onHand = assigner_onHand.GetComponent<Transform>();
		access = assigner_access.GetComponent<OVRPlayerController_JunsuTest1> ();

		//onHand = assigner_onHand;
		//access = assigner_access;


	}
	void Update()
	{
		
		if (Input.GetButtonDown("Fire1") && access.object_name == currentObject.name)
		{
			//Status Update
			Debug.Log ("Update - Object Picked Up");
			pickupStatus = true;

			//GetComponent<Rigidbody>().useGravity = false;

			//Change position to cube dragger
			this.transform.position = onHand.transform.position;
			this.transform.parent = GameObject.Find("CubeDragger").transform;


			//Restrict Movement
			GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionX;
			GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionY;
			GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionZ;

			//Pause Rotation
			//rotateEnable = false;

			//Set rotation to 0
			//transform.rotation = Quaternion.identity;

			/* ALI'S CODE
			//Translate
			if (Input.GetKey (KeyCode.DownArrow))
			{
				transform.Translate (new Vector3 (0,0,-speed*Time.deltaTime));
				//transform.Translate (new Vector3 (0,0,-speed*Time.deltaTime));
				Debug.Log("CubeMoveDownArrow");
			}
				
			if (Input.GetKey (KeyCode.UpArrow))
			{
				transform.Translate (new Vector3 (0,0,speed*Time.deltaTime));
				//transform.Translate (new Vector3 (0,0,speed*Time.deltaTime));
			}*/
		}


		if (Input.GetButtonDown ("Fire2"))
		{
			//Status update
			Debug.Log ("Update - Object Dropped");
			access.object_name = "fuckingbullshit";
			pickupStatus = false;

			//holdChecker = false;

			//Resume Rotation
			//rotateEnable = true;

			//Restrict Movement
			GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionX;
			GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionY;
			GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezePositionZ;

			//Remove object from the parent
			this.transform.parent = null;


		}


		// Translate Cube
		if (Input.GetAxis ("Mouse ScrollWheel") > 0f && pickupStatus == true)
		{
			Debug.Log ("Scroll Up");
			this.transform.parent.Translate (new Vector3 (0, 0, 1));

		}

		if (Input.GetAxis ("Mouse ScrollWheel") < 0f && pickupStatus == true)
		{
			Debug.Log ("Scroll Down");
			this.transform.parent.Translate (new Vector3 (0, 0, -1));
		}


	}
		
	/*
	void OnMouseDown()
	{
		Debug.Log ("Johnson is a yute");
		GetComponent<Rigidbody>().useGravity = false;
		this.transform.position = onHand.transform.position;
		this.transform.parent = GameObject.Find("OVRPlayerController").transform;
	}

	void OnMouseUp ()
	{
		Debug.Log ("Johnson is a scute");
		this.transform.parent = null;
		//GetComponent<Rigidbody>().useGravity = true;

		//if the object has a parent, use the following line --> this.transform.parent = GameObject.Find("Enter parent name here").transform;
		//Use this line if you want the object to fall back down to the floor --> GetComponent<Rigidbody>().useGravity = true;
	}
	*/

}


