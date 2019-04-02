/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using System;
using UnityEngine;
using System.Collections;  //JJ for left click raycasting
using System.Collections.Generic;  //JJ for left click raycasting
using UnityEngine.SceneManagement; //For changing scenes (Press esc to go back to main menu)
using UnityEngine.UI;

/// <summary>
/// Controls the player's movement in virtual reality.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController_JunsuTest1 : MonoBehaviour
{
	/// <summary>
	/// The rate acceleration during movement.
	/// </summary>
	public float Acceleration = 0.1f;

	/// <summary>
	/// The rate of damping on movement.
	/// </summary>
	public float Damping = 0.3f;

	/// <summary>
	/// The rate of additional damping when moving sideways or backwards.
	/// </summary>
	public float BackAndSideDampen = 0.5f;

	/// <summary>
	/// The force applied to the character when jumping.
	/// </summary>
	public float JumpForce = 0.3f;

	/// <summary>
	/// The rate of rotation when using a gamepad.
	/// </summary>
	public float RotationAmount = 1.5f;

	/// <summary>
	/// The rate of rotation when using the keyboard.
	/// </summary>
	public float RotationRatchet = 45.0f;

	/// <summary>
	/// The player will rotate in fixed steps if Snap Rotation is enabled.
	/// </summary>
	[Tooltip("The player will rotate in fixed steps if Snap Rotation is enabled.")]
	public bool SnapRotation = true;

	/// <summary>
	/// How many fixed speeds to use with linear movement? 0=linear control
	/// </summary>
	[Tooltip("How many fixed speeds to use with linear movement? 0=linear control")]
	public int FixedSpeedSteps;

	/// <summary>
	/// If true, reset the initial yaw of the player controller when the Hmd pose is recentered.
	/// </summary>
	public bool HmdResetsY = true;

	/// <summary>
	/// If true, tracking data from a child OVRCameraRig will update the direction of movement.
	/// </summary>
	public bool HmdRotatesY = true;

	/// <summary>
	/// Modifies the strength of gravity.
	/// </summary>
	public float GravityModifier = 0.379f;

	/// <summary>
	/// If true, each OVRPlayerController will use the player's physical height.
	/// </summary>
	public bool useProfileData = true;

	/// <summary>
	/// The CameraHeight is the actual height of the HMD and can be used to adjust the height of the character controller, which will affect the
	/// ability of the character to move into areas with a low ceiling.
	/// </summary>
	[NonSerialized]
	public float CameraHeight;

	/// <summary>
	/// This event is raised after the character controller is moved. This is used by the OVRAvatarLocomotion script to keep the avatar transform synchronized
	/// with the OVRPlayerController.
	/// </summary>
	public event Action<Transform> TransformUpdated;

	/// <summary>
	/// This bool is set to true whenever the player controller has been teleported. It is reset after every frame. Some systems, such as 
	/// CharacterCameraConstraint, test this boolean in order to disable logic that moves the character controller immediately 
	/// following the teleport.
	/// </summary>
	[NonSerialized] // This doesn't need to be visible in the inspector.
	public bool Teleported;

	/// <summary>
	/// This event is raised immediately after the camera transform has been updated, but before movement is updated.
	/// </summary>
	public event Action CameraUpdated;

	/// <summary>
	/// This event is raised right before the character controller is actually moved in order to provide other systems the opportunity to 
	/// move the character controller in response to things other than user input, such as movement of the HMD. See CharacterCameraConstraint.cs
	/// for an example of this.
	/// </summary>
	public event Action PreCharacterMove;

	/// <summary>
	/// When true, user input will be applied to linear movement. Set this to false whenever the player controller needs to ignore input for
	/// linear movement.
	/// </summary>
	public bool EnableLinearMovement = true;

	/// <summary>
	/// When true, user input will be applied to rotation. Set this to false whenever the player controller needs to ignore input for rotation.
	/// </summary>
	public bool EnableRotation = true;

	protected CharacterController Controller = null;
	protected OVRCameraRig CameraRig = null;

	private float MoveScale = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float FallSpeed = 0.0f;
	private OVRPose? InitialPose;
	public float InitialYRotation { get; private set; }
	private float MoveScaleMultiplier = 1.0f;
	private float RotationScaleMultiplier = 1.0f;
	private bool  SkipMouseRotation = true; // It is rare to want to use mouse movement in VR, so ignore the mouse by default.
	private bool  HaltUpdateMovement = false;
	private bool prevHatLeft = false;
	private bool prevHatRight = false;
	private float SimulationRate = 60f;
	private float buttonRotation = 0f;
	private bool ReadyToSnapTurn; // Set to true when a snap turn has occurred, code requires one frame of centered thumbstick to enable another snap turn.




	/*---------------------------------------------------------
	 * 						JJ Variables
	 * -------------------------------------------------------*/

	// JJ left click raycasting
	public float range = 100f;
	public Camera fpsCam;

	// JJ object pull
	public int access_test = 13;
	public string object_name;
	public string interact_name;

	// Creating New Prefabs
	public GameObject myPrefab;
	public GameObject spawner; //Set this to cube dragger
	Vector3 spawnPosition;
	int cubeNamer = 0;
	string cubeName = "Ayy lmao";

	// Canvas
	public Canvas jointsCanvas;

	void Start()
	{
		// Add eye-depth as a camera offset from the player controller
		var p = CameraRig.transform.localPosition;
		p.z = OVRManager.profile.eyeDepth;
		CameraRig.transform.localPosition = p;


		Debug.Log("Start"); // JJ Test
		aliStart();
	}

	void Awake()
	{
		Controller = gameObject.GetComponent<CharacterController>();

		if(Controller == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");

		// We use OVRCameraRig to set rotations to cameras,
		// and to be influenced by rotation
		OVRCameraRig[] CameraRigs = gameObject.GetComponentsInChildren<OVRCameraRig>();

		if(CameraRigs.Length == 0)
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		else if (CameraRigs.Length > 1)
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		else
			CameraRig = CameraRigs[0];

		InitialYRotation = transform.rotation.eulerAngles.y;
	}

	void OnEnable()
	{
		OVRManager.display.RecenteredPose += ResetOrientation;

		if (CameraRig != null)
		{
			CameraRig.UpdatedAnchors += UpdateTransform;
		}
	}

	void OnDisable()
	{
		OVRManager.display.RecenteredPose -= ResetOrientation;

		if (CameraRig != null)
		{
			CameraRig.UpdatedAnchors -= UpdateTransform;
		}
	}

	void Update()
	{
		//Use keys to ratchet rotation
		if (Input.GetKey(KeyCode.Q))
		{
			buttonRotation -= RotationRatchet;
			Debug.Log("Q Detection");  // JJ 
		}


		if (Input.GetKey(KeyCode.E))
		{
			buttonRotation += RotationRatchet;
			Debug.Log("E Detection");  // JJ
		}


		// JJ for left click raycasting
		if (Input.GetButtonDown("Fire1")) //fire1 = left mouse button 
		{
			
			Debug.Log("Left Click Detection");
			Shoot();
		}

		/*
		// JJ press Esc to main menu
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SceneManager.LoadScene(0);
		}*/

		// JJ ESC to hide canvas
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			HideCanvas (jointsCanvas);
		}

		if(Input.GetKeyDown(KeyCode.B))
		{
			canvasActive = true;
			devicePull ();
			ShowCanvas (jointsCanvas);
		}

		// JJ create prefab
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			Application.LoadLevel ("PrimaryScene");

		}

		// JJ alternate interactive button
		if (Input.GetKeyDown (KeyCode.V))
		{
			Debug.Log ("Alternate interaction");

			RaycastHit hit;
			if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range)) //info in hit variable
			{
				interact_name = hit.transform.name;
				interact_name = interact_name.Replace ("(Clone)","");

				for(int i = 0; i<9; i++)
				{
					if (interact_name == devices [i] ["id"].str) {
						titleBox.text = devices [i] ["name"].str;
					}
				}

				StartCoroutine (getAttributes (interact_name));
				ShowCanvas (jointsCanvas);
			}


			//Interact (jointsCanvas);
		}

		/*
		// JJ Save
		if (Input.GetKeyDown (KeyCode.J))
		{
			SavePlayer ();
		}

		// JJ Load
		if (Input.GetKeyDown (KeyCode.K))
		{
			LoadPlayer ();
		}
		*/

		if (canvasActive) {
			
			if (Input.GetKeyDown (KeyCode.Alpha1)) {
				if (deviceSelected [0] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [0] = true;
					titleBox.text = devices [0] ["name"].str;
					InstantiateCube (devices [0] ["id"].str);
				}
			}

			if (Input.GetKeyDown (KeyCode.Alpha2)) {
				if (deviceSelected [1] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [1] = true;
					titleBox.text = devices [1] ["name"].str;
					InstantiateCube (devices [1] ["id"].str);
				}
			}

			if (Input.GetKeyDown (KeyCode.Alpha3)) {
				if (deviceSelected [2] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [2] = true;
					titleBox.text = devices [2] ["name"].str;
					InstantiateCube (devices [2] ["id"].str);
				}
			}

			if (Input.GetKeyDown (KeyCode.Alpha4)) {
				if (deviceSelected [3] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [3] = true;
					titleBox.text = devices [3] ["name"].str;
					InstantiateCube (devices [3] ["id"].str);
				}
			}

			if (Input.GetKeyDown (KeyCode.Alpha5)) {
				if (deviceSelected [4] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [4] = true;
					titleBox.text = devices [4] ["name"].str;
					InstantiateCube (devices [4] ["id"].str);
				}
			}

			if (Input.GetKeyDown (KeyCode.Alpha6)) {
				if (deviceSelected [5] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [5] = true;
					titleBox.text = devices [5] ["name"].str; 
					InstantiateCube (devices [5] ["id"].str);
				}
			}
			if (Input.GetKeyDown (KeyCode.Alpha7)) {
				if (deviceSelected [6] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [6] = true;
					titleBox.text = devices [6] ["name"].str; 
					InstantiateCube (devices [6] ["id"].str);
				}
			}
			if (Input.GetKeyDown (KeyCode.Alpha8)) {
				if (deviceSelected [7] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [7] = true;
					titleBox.text = devices [7] ["name"].str; 
					InstantiateCube (devices [7] ["id"].str);
				}
			}
			if (Input.GetKeyDown (KeyCode.Alpha9)) {
				if (deviceSelected [8] == false) {
					HideCanvas (jointsCanvas);
					deviceSelected [8] = true;
					titleBox.text = devices [8] ["name"].str; 
					InstantiateCube (devices [8] ["id"].str);
				}
			}
				
		}
	}


	// JJ for left click raycasting
	void Shoot()
	{
		RaycastHit hit;
		if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range)) //info in hit variable
		{
			object_name = hit.transform.name;
			Debug.Log (object_name);
			Debug.Log("Hit Detection");
		}
	}

	/*
	// JJ for interacting
	void Interact(Canvas myCanvas)
	{
		RaycastHit hit;
		if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range)) //info in hit variable
		{
			ShowCanvas (myCanvas);
		}
	}*/

	// Instantiating cubes
	void InstantiateCube(string cubeName)
	{
		Debug.Log ("Given Name :: " + cubeName);
		myPrefab.name = cubeName;
		//set spawn location
		spawnPosition = spawner.transform.position; 

		//make cube
		Instantiate (myPrefab, spawnPosition, Quaternion.identity);  

		//change cube name from previous ones

	}

	// Showing Canvas
	void ShowCanvas(Canvas myCanvas)
	{
		myCanvas.GetComponent<CanvasGroup>().alpha = 1f;
		myCanvas.GetComponent<CanvasGroup>().interactable = true;
		myCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
	}
	// Hiding Canvas
	void HideCanvas(Canvas myCanvas)
	{
		myCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
		myCanvas.GetComponent<CanvasGroup>().interactable = false;
		myCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
		canvasActive = false;
	}

	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////AJ
	public GameObject scriptObj;
	public Read readScript;
	JSONObject devices;
	JSONObject attributes;
	JSONObject telemetry;
	string telemetryKey;
	int numDevices;
	int numAttributes;
	public Text textBox;
	public Text titleBox;
	bool canvasActive = false;
	bool[] deviceSelected = new bool[15];

	void aliStart()
	{
		Debug.Log ("Entered Ali Start");
		scriptObj = GameObject.Find ("ScriptsGameObj");
		readScript = scriptObj.GetComponent<Read> ();
		Invoke("devicePull", 4.0f);
		for(int i = 0; i<15; i++)
		{
			deviceSelected [i] = false;
		}
	}

	void devicePull()
	{
		Debug.Log ("Entered Device Pull");
		devices = readScript.devices;
		numDevices = readScript.numDevices;
		Debug.Log("Num Devices: " + numDevices);

		titleBox.text = "Asset / Device List";
		textBox.text = "";

		for(int i = 0; i<numDevices; i++)
		{
			if(deviceSelected[i] == false)
			{
				Debug.Log("Device: " + devices[i]["name"].ToString());
				textBox.text += "[" + (i+1) + "] " + devices[i]["name"].ToString() + "  ";
				textBox.text += "\n";
			}
		}
	}

	IEnumerator getAttributes(string deviceID)
	{
		textBox.text = "[Loading....]";
		StartCoroutine (readScript.getAttributesByDevice (deviceID));
		StartCoroutine (readScript.checkTelemetry (deviceID));
		Invoke ("attributesCanvas", 0.5f);
		yield return new WaitForSeconds (0.5f);

		if (telemetryKey != "") 
		{
			StartCoroutine (readScript.getTelemetryByDevice(deviceID));
		}

		Invoke ("telemetryCanvas", 0.5f);

	}

	void attributesCanvas()
	{
		attributes = readScript.deviceAttributes;
		numAttributes = readScript.numAttributes;
		telemetryKey = readScript.telemetryKey;
		textBox.text = "";

		for(int i = 0; i<numAttributes; i++)
		{

			Debug.Log(attributes[i]["key"].ToString() + ": " + attributes[i]["value"].ToString());
			textBox.text += attributes[i]["key"].str + ": " + attributes[i]["value"].ToString();
			textBox.text += "\n";
		}
			
	}

	void telemetryCanvas()
	{
		telemetry = readScript.deviceTelemetry;
		float trunc;

		double unixTimeStamp;
		// Unix timestamp is seconds past epoch
		System.DateTime dtDateTime;


		//textBox.text += telemetryKey + "\n";
		for(int i = 0; i<10; i++)
		{
			try{
			//This is Temp
			Debug.Log(i + ": " + telemetry[telemetryKey][i]["value"].ToString());
			trunc = float.Parse(telemetry[telemetryKey][i]["value"].str);
			trunc = Mathf.Round(trunc *100f)/100f;
			Debug.Log("Timestamp: " + telemetry[telemetryKey][i]["ts"].ToString());
			
			//Timestamp
			unixTimeStamp = Double.Parse(telemetry[telemetryKey][i]["ts"].ToString());
			unixTimeStamp = unixTimeStamp/1000;
			dtDateTime = new System.DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();

			//write to canvas
			
				if(i == 0 && telemetryKey == "Temperature")
				{
					textBox.text += "\nLatest " + telemetryKey + ": " + trunc + "\x00B0C\n\nHistorical " + telemetryKey + ":\n";
				}
				else if(i == 0 && telemetryKey == "Voltage")
				{
					textBox.text += "\nLatest " + telemetryKey + ": " + trunc + " V\n\nHistorical " + telemetryKey + ":\n";
				}
				else{
					textBox.text += dtDateTime + ": " + trunc;
					textBox.text += "\n";
				}

			} catch(NullReferenceException ex)
			{
				Debug.Log ("Caught Exception: " + ex);
				break;
			}

		}
	}


	/*
	// JJ for saving
	public void SavePlayer()
	{
		SaveSystem.SavePlayer (this);
		Debug.Log ("Saved");
	}

	public void LoadPlayer()
	{
		PlayerData data = SaveSystem.LoadPlayer ();

		Vector3 position;
		position.x = data.playerPosition [0];
		position.y = data.playerPosition [1];
		position.z = data.playerPosition [2];

		transform.position = position;

		Debug.Log ("Loaded");
	}
	*/

	protected virtual void UpdateController()
	{
		if (useProfileData)
		{
			if (InitialPose == null)
			{
				// Save the initial pose so it can be recovered if useProfileData
				// is turned off later.
				InitialPose = new OVRPose()
				{
					position = CameraRig.transform.localPosition,
					orientation = CameraRig.transform.localRotation
				};
			}

			var p = CameraRig.transform.localPosition;
			if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.EyeLevel)
			{
				p.y = OVRManager.profile.eyeHeight - (0.5f * Controller.height) + Controller.center.y;
			}
			else if (OVRManager.instance.trackingOriginType == OVRManager.TrackingOrigin.FloorLevel)
			{
				p.y = - (0.5f * Controller.height) + Controller.center.y;
			}
			CameraRig.transform.localPosition = p;
		}
		else if (InitialPose != null)
		{
			// Return to the initial pose if useProfileData was turned off at runtime
			CameraRig.transform.localPosition = InitialPose.Value.position;
			CameraRig.transform.localRotation = InitialPose.Value.orientation;
			InitialPose = null;
		}

		CameraHeight = CameraRig.centerEyeAnchor.localPosition.y;

		if (CameraUpdated != null)
		{
			CameraUpdated();
		}

		UpdateMovement();

		Vector3 moveDirection = Vector3.zero;

		float motorDamp = (1.0f + (Damping * SimulationRate * Time.deltaTime));

		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;

		moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

		// Gravity
		if (Controller.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * SimulationRate * Time.deltaTime);

		moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;


		if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
		{
			// Offset correction for uneven ground
			float bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
			moveDirection -= bumpUpOffset * Vector3.up;
		}

		if (PreCharacterMove != null)
		{
			PreCharacterMove();
			Teleported = false;
		}

		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));

		// Move contoller
		Controller.Move(moveDirection);
		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
	}

	public virtual void UpdateMovement()
	{
		if (HaltUpdateMovement)
			return;

		if (EnableLinearMovement)
		{
			bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
			bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
			bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
			bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

			bool dpad_move = false;

			if (OVRInput.Get(OVRInput.Button.DpadUp))
			{
				moveForward = true;
				dpad_move = true;

			}

			if (OVRInput.Get(OVRInput.Button.DpadDown))
			{
				moveBack = true;
				dpad_move = true;
			}

			MoveScale = 1.0f;

			if ((moveForward && moveLeft) || (moveForward && moveRight) ||
				(moveBack && moveLeft) || (moveBack && moveRight))
				MoveScale = 0.70710678f;

			// No positional movement if we are in the air
			if (!Controller.isGrounded)
				MoveScale = 0.0f;

			MoveScale *= SimulationRate * Time.deltaTime;

			// Compute this for key movement
			float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

			// Run!
			if (dpad_move || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				moveInfluence *= 2.0f;

			Quaternion ort = transform.rotation;
			Vector3 ortEuler = ort.eulerAngles;
			ortEuler.z = ortEuler.x = 0f;
			ort = Quaternion.Euler(ortEuler);

			if (moveForward)
				MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * Vector3.forward);
			if (moveBack)
				MoveThrottle += ort * (transform.lossyScale.z * moveInfluence * BackAndSideDampen * Vector3.back);
			if (moveLeft)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.left);
			if (moveRight)
				MoveThrottle += ort * (transform.lossyScale.x * moveInfluence * BackAndSideDampen * Vector3.right);



			moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

			#if !UNITY_ANDROID // LeftTrigger not avail on Android game pad
			moveInfluence *= 1.0f + OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
			#endif

			Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

			// If speed quantization is enabled, adjust the input to the number of fixed speed steps.
			if (FixedSpeedSteps > 0)
			{
				primaryAxis.y = Mathf.Round(primaryAxis.y * FixedSpeedSteps) / FixedSpeedSteps;
				primaryAxis.x = Mathf.Round(primaryAxis.x * FixedSpeedSteps) / FixedSpeedSteps;
			}

			if (primaryAxis.y > 0.0f)
				MoveThrottle += ort * (primaryAxis.y * transform.lossyScale.z * moveInfluence * Vector3.forward);

			if (primaryAxis.y < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.y) * transform.lossyScale.z * moveInfluence *
					BackAndSideDampen * Vector3.back);

			if (primaryAxis.x < 0.0f)
				MoveThrottle += ort * (Mathf.Abs(primaryAxis.x) * transform.lossyScale.x * moveInfluence *
					BackAndSideDampen * Vector3.left);

			if (primaryAxis.x > 0.0f)
				MoveThrottle += ort * (primaryAxis.x * transform.lossyScale.x * moveInfluence * BackAndSideDampen *
					Vector3.right);
		}

		if (EnableRotation)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			float rotateInfluence = SimulationRate * Time.deltaTime * RotationAmount * RotationScaleMultiplier;

			bool curHatLeft = OVRInput.Get(OVRInput.Button.PrimaryShoulder);

			if (curHatLeft && !prevHatLeft)
				euler.y -= RotationRatchet;

			prevHatLeft = curHatLeft;

			bool curHatRight = OVRInput.Get(OVRInput.Button.SecondaryShoulder);

			if (curHatRight && !prevHatRight)
				euler.y += RotationRatchet;

			prevHatRight = curHatRight;

			euler.y += buttonRotation;
			buttonRotation = 0f;


			#if !UNITY_ANDROID || UNITY_EDITOR
			if (!SkipMouseRotation)
				euler.y += Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
			#endif

			if (SnapRotation)
			{

				if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft))
				{
					if (ReadyToSnapTurn)
					{
						euler.y -= RotationRatchet;
						ReadyToSnapTurn = false;
					}
				}
				else if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight))
				{
					if (ReadyToSnapTurn)
					{
						euler.y += RotationRatchet;
						ReadyToSnapTurn = false;
					}
				}
				else
				{
					ReadyToSnapTurn = true;
				}
			}
			else
			{
				Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				euler.y += secondaryAxis.x * rotateInfluence;
			}

			transform.rotation = Quaternion.Euler(euler);
		}
	}


	/// <summary>
	/// Invoked by OVRCameraRig's UpdatedAnchors callback. Allows the Hmd rotation to update the facing direction of the player.
	/// </summary>
	public void UpdateTransform(OVRCameraRig rig)
	{
		Transform root = CameraRig.trackingSpace;
		Transform centerEye = CameraRig.centerEyeAnchor;

		if (HmdRotatesY && !Teleported)
		{
			Vector3 prevPos = root.position;
			Quaternion prevRot = root.rotation;

			transform.rotation = Quaternion.Euler(0.0f, centerEye.rotation.eulerAngles.y, 0.0f);

			root.position = prevPos;
			root.rotation = prevRot;
		}

		UpdateController();
		if (TransformUpdated != null)
		{
			TransformUpdated(root);
		}
	}

	/// <summary>
	/// Jump! Must be enabled manually.
	/// </summary>
	public bool Jump()
	{
		if (!Controller.isGrounded)
			return false;

		MoveThrottle += new Vector3(0, transform.lossyScale.y * JumpForce, 0);

		return true;
	}

	/// <summary>
	/// Stop this instance.
	/// </summary>
	public void Stop()
	{
		Controller.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}

	/// <summary>
	/// Gets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = MoveScaleMultiplier;
	}

	/// <summary>
	/// Sets the move scale multiplier.
	/// </summary>
	/// <param name="moveScaleMultiplier">Move scale multiplier.</param>
	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		MoveScaleMultiplier = moveScaleMultiplier;
	}

	/// <summary>
	/// Gets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = RotationScaleMultiplier;
	}

	/// <summary>
	/// Sets the rotation scale multiplier.
	/// </summary>
	/// <param name="rotationScaleMultiplier">Rotation scale multiplier.</param>
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	/// <summary>
	/// Gets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">Allow mouse rotation.</param>
	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = SkipMouseRotation;
	}

	/// <summary>
	/// Sets the allow mouse rotation.
	/// </summary>
	/// <param name="skipMouseRotation">If set to <c>true</c> allow mouse rotation.</param>
	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		SkipMouseRotation = skipMouseRotation;
	}

	/// <summary>
	/// Gets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">Halt update movement.</param>
	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = HaltUpdateMovement;
	}

	/// <summary>
	/// Sets the halt update movement.
	/// </summary>
	/// <param name="haltUpdateMovement">If set to <c>true</c> halt update movement.</param>
	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		HaltUpdateMovement = haltUpdateMovement;
	}

	/// <summary>
	/// Resets the player look rotation when the device orientation is reset.
	/// </summary>
	public void ResetOrientation()
	{
		if (HmdResetsY && !HmdRotatesY)
		{
			Vector3 euler = transform.rotation.eulerAngles;
			euler.y = InitialYRotation;
			transform.rotation = Quaternion.Euler(euler);
		}
	}
}

