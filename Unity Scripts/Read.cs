using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;
//using UnityEngine.Experimental.Networking;


public class Read : MonoBehaviour {

	//Thingsboard URL
	string url = "http://ec2-18-217-252-69.us-east-2.compute.amazonaws.com";
	//Custom AWS API Url
	string urlAPI = "https://kwgr1v28nb.execute-api.us-east-2.amazonaws.com/dev/";

	//Access Token from ThingsBoard
	private tokenObject authToken;

	//Header with token to showcase authorization
	Dictionary<string,string> authHeader = new Dictionary<string,string> ();
	//Header for Post
	Dictionary<string,string> postHeader = new Dictionary<string,string> ();
	//Header to use Custom API
	Dictionary<string,string> customAPIHeader = new Dictionary<string,string> ();


	public JSONObject buildings; //Holds all Building Data
	public int numBuildings; // Number of Buildings
	public JSONObject[] rooms = new JSONObject[10]; // Holds all Room Data
	public int[] numRooms = new int[10]; // Number of Rooms
	public JSONObject devices;
	public int numDevices;
	public JSONObject deviceAttributes;
	public int numAttributes;
	public JSONObject deviceTelemetry;
	public string telemetryKey;

	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	// Use this for initialization
	IEnumerator Start () {

		Debug.Log("Start");
		//Instantiate authToken
		authToken = new tokenObject ();

		//User Credentials
		string userCred = "{\"username\":\"tenant@thingsboard.org\", \"password\":\"tenant\"}";

		postHeader.Add ("Content-Type", "application/json");

		var userCredBytes = System.Text.Encoding.UTF8.GetBytes(userCred);
		WWW login = new WWW (url + ":80/api/auth/login", userCredBytes, postHeader);
		yield return login;

		Debug.Log(login.text);

		authToken = JsonUtility.FromJson<tokenObject> (login.text);

		//Add the Access Token to the Header
		authHeader.Add("X-Authorization", "Bearer " + authToken.token);
		customAPIHeader.Add ("token", authToken.token);

		StartCoroutine (getBuildings());
		Invoke("menuStart", 0.5f);



	}

	// Update is called once per frame
	void Update () {

	}

	IEnumerator getBuildings()
	{
		WWW getBuildingsWWW = new WWW (urlAPI + "buildings", null, customAPIHeader);
		yield return getBuildingsWWW;
		buildings = new JSONObject (getBuildingsWWW.text);
		numBuildings = getNumBuildings (buildings);
		StartCoroutine (getRooms());
	}

	int getNumBuildings(JSONObject toCount)
	{
		int buildingCounter = 0;
		bool validCount = true;
		while (validCount) {
			try {
				Debug.Log (toCount[buildingCounter]["name"].str);	
			} catch (NullReferenceException ex) {
				Debug.Log ("Caught Exception: " + ex);
				buildingCounter--;
				validCount = false;
			}
			buildingCounter++;
		}

		return buildingCounter;
	}

	IEnumerator getRoomsByID(string id)
	{
		WWW getRoomsWWW = new WWW (urlAPI + id + "/rooms", null, customAPIHeader);
		yield return getRoomsWWW;
	}

	IEnumerator getRooms()
	{
		WWW getRoomsWWW;

		for (int i = 0; i < numBuildings; i++) 
		{
			getRoomsWWW = new WWW (urlAPI + buildings [i] ["id"].str + "/rooms", null, customAPIHeader);
			yield return getRoomsWWW;
			rooms [i] = new JSONObject (getRoomsWWW.text);
		}

		getNumRooms ();
	}

	void getNumRooms()
	{
		for (int i = 0; i < numBuildings; i++) 
		{
			int roomCounter = 0;
			bool validCount = true;
			while (validCount) {
				try {
					Debug.Log (rooms [i][roomCounter]["name"].str);	
				} catch (NullReferenceException ex) {
					Debug.Log ("Caught Exception: " + ex);
					roomCounter--;
					validCount = false;
				}
				roomCounter++;
			}

			numRooms [i] = roomCounter;
		}

		Debug.Log("--------------------------------------");
		Debug.Log("--------------------------------");
		Debug.Log("--------------------------");
		Debug.Log("--------------------");
		Debug.Log("All Systems Operational!");

	}

	IEnumerator getDeviceByRoom(string buildingID, string roomID)
	{
		Debug.Log ("Inside the get devices by room function");
		WWW getDevicesWWW = new WWW (urlAPI + buildingID + "/" + roomID + "/devices", null, customAPIHeader);
		yield return getDevicesWWW;
		devices = new JSONObject (getDevicesWWW.text);
		numDevices = getNumDevices (devices);
		deviceIterate();

	}

	int getNumDevices(JSONObject toCount)
	{
		int deviceCounter = 0;
		bool validCount = true;
		while (validCount) {
			try {
				Debug.Log (toCount[deviceCounter]["name"].str);	
			} catch (NullReferenceException ex) {
				Debug.Log ("Caught Exception: " + ex);
				deviceCounter--;
				validCount = false;
			}
			deviceCounter++;
		}

		return deviceCounter;
	}

	void deviceIterate()
	{
		Debug.Log("Number of Assets:" + numDevices);
		textBox.text = "Number of Assets:" + numDevices;
		textBox.text += "\n";

		for(int i = 0; i<numDevices; i++)
		{
			Debug.Log("Asset #" + i + " : " + devices[i]["name"].str);
			textBox.text += i+1 + ". " + devices[i]["name"].str;
			textBox.text += "\n";
		}

		/////////////////////////////////////////////////////////////////////////////
		//StartCoroutine(getAttributesByDevice(devices[0]["id"].str));
		/////////////////////////////////////////////////////////////////////////
	}

	public IEnumerator getAttributesByDevice(string deviceID)
	{
		Debug.Log("Inside the get attributes function");
		Debug.Log("Device ID: " + deviceID);
		Debug.Log("url: " + url + "/api/plugins/telemetry/DEVICE/" + deviceID + "/values/attributes");
		WWW getAttributesWWW = new WWW (url + "/api/plugins/telemetry/DEVICE/" + deviceID + "/values/attributes", null, authHeader);
		yield return getAttributesWWW;
		deviceAttributes = new JSONObject(getAttributesWWW.text);
		numAttributes = getNumAttributes(deviceAttributes);
		attributeIterate();
	}

	int getNumAttributes(JSONObject toCount)
	{
		int attributeCounter = 0;
		bool validCount = true;
		while (validCount) {
			try {
				Debug.Log (toCount[attributeCounter]["key"].str);	
			} catch (NullReferenceException ex) {
				Debug.Log ("Caught Exception: " + ex);
				attributeCounter--;
				validCount = false;
			}
			attributeCounter++;
		}

		return attributeCounter;
	}

	void attributeIterate()
	{
		Debug.Log("Number of Attributes:" + numAttributes);

		for(int i = 0; i<numAttributes; i++)
		{
			Debug.Log("Attribute #" + i + " : " + deviceAttributes[i]["key"].str);
		}

		/////////////////////////////////////////////////////////////////////////////
		//StartCoroutine(checkTelemetry(devices[0]["id"].str));
		/////////////////////////////////////////////////////////////

	}

	//http://ec2-13-57-49-61.us-west-1.compute.amazonaws.com/api/plugins/telemetry/DEVICE/5b2f3f00-2775-11e8-80b3-519f935cd99e/values/timeseries?keys=humidity,temperature
	public IEnumerator getTelemetryByDevice(string deviceID)
	{
		System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
		String curTime = cur_time + "000";
		Debug.Log("Current Time: " + curTime);

		Debug.Log("Inside the get telemetry function");
		Debug.Log("Device ID: " + deviceID);
		Debug.Log("url: " + url + "/api/plugins/telemetry/DEVICE/" + deviceID + "/values/timeseries?startTs=1552927404000&endTs=" + curTime + "&limit=10&keys=" + telemetryKey);
		WWW getTelemetryWWW = new WWW (url + "/api/plugins/telemetry/DEVICE/" + deviceID + "/values/timeseries?startTs=1552927404000&endTs=" + curTime + "&limit=10&agg=AVG&keys=" + telemetryKey, null, authHeader);
		yield return getTelemetryWWW;
		deviceTelemetry = new JSONObject(getTelemetryWWW.text);
		//telemetryIterate();
	}

	public IEnumerator checkTelemetry(string deviceID)
	{
		Debug.Log("Inside the check telemetry function");
		Debug.Log("Device ID: " + deviceID);
		Debug.Log("url: " + url + "/api/plugins/telemetry/DEVICE/" + deviceID + "/keys/timeseries");
		WWW getCheckTelemetryWWW = new WWW (url + "/api/plugins/telemetry/DEVICE/" + deviceID + "/keys/timeseries", null, authHeader);
		yield return getCheckTelemetryWWW;
		JSONObject deviceCheckTelemetry = new JSONObject(getCheckTelemetryWWW.text);

		try{
			telemetryKey = deviceCheckTelemetry[0].str;
			Debug.Log("Telemetry Key: " + telemetryKey);
		} catch (NullReferenceException ex) {
			Debug.Log ("Caught Exception: " + ex);
			Debug.Log ("No Telemetry Data");
			telemetryKey = "";
		}

		StartCoroutine(getTelemetryByDevice(deviceID));
	}

	void telemetryIterate()
	{
		Debug.Log("Telemetry Key:" + telemetryKey);

		if(telemetryKey != "")
		{
			for(int i = 0; i<10; i++)
			{
				Debug.Log("Telemetry #" + i + " : " + deviceTelemetry[telemetryKey][i]["value"].str);
			}
		}

	}

	//-----------------------------MENU CODE--------------------------------------------------------------------


	public Button prefab;
	public Canvas buildingCanvas;
	public Canvas roomCanvas;
	public Canvas deviceCanvas;
	public Text textBox;
	Vector3 nextLayoutPosition = new Vector3(0f,92f,0f);
	bool buildingCanvasSet = false;
	int buildingNumTracker;

	void menuStart()
	{
		Debug.Log("Menu Start");

		//Reset
		buildingCanvas.GetComponent<CanvasGroup>().alpha = 1f;
		buildingCanvas.GetComponent<CanvasGroup>().interactable = true;
		buildingCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
		roomCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
		roomCanvas.GetComponent<CanvasGroup>().interactable = false;
		roomCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
		deviceCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
		deviceCanvas.GetComponent<CanvasGroup>().interactable = false;
		deviceCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;


		foreach (Transform child in roomCanvas.transform) 
		{
			if (child.name == "Title" || child.name == "ExtraBox" || child.name == "Bar")
			{
				continue;			
			}
			GameObject.Destroy(child.gameObject);
		}

		if(buildingCanvasSet==false)
		{
			for(int i = 0; i < numBuildings; i++)
			{
				AddButton(prefab,buildings[i]["name"].str, i);
			}
			buildingCanvasSet = true;
		}

	}

	void AddButton(Button prefab, string name, int number)
	{
		Button button = Instantiate(prefab);
		button.name = name;
		button.onClick.AddListener(delegate{roomMenuStart(number);});

		Text text = button.GetComponentInChildren<Text>();
		if (text)
			text.text = name;

		RectTransform transform = button.GetComponent<RectTransform>();
		transform.SetParent(buildingCanvas.transform);
		transform.anchoredPosition = nextLayoutPosition;
		nextLayoutPosition += Vector3.down * 50;
	}

	void buttonTest(int number)
	{
		Debug.Log("Button #"+ number);
	}

	void roomMenuStart(int num)
	{
		//Reset Menu
		roomCanvas.GetComponent<CanvasGroup>().alpha = 1f;
		roomCanvas.GetComponent<CanvasGroup>().interactable = true;
		roomCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
		buildingCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
		buildingCanvas.GetComponent<CanvasGroup>().interactable = false;
		buildingCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
		deviceCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
		deviceCanvas.GetComponent<CanvasGroup>().interactable = false;
		deviceCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
		nextLayoutPosition = new Vector3(0f,92f,0f);
		buildingNumTracker = num;

		Debug.Log("Room Menu Start");
		for(int i = 0; i<numRooms[num]; i++)
		{
			AddButtonRoom(prefab,rooms[num][i]["name"].str, i, num);
		}
		AddButtonRoomMenuBack(prefab);
	}

	void AddButtonRoom(Button prefab, string name, int roomNum, int buildingNum)
	{
		Button button = Instantiate(prefab);
		button.name = name;
		//button.onClick.AddListener(delegate{buttonTest(roomNum);});
		button.onClick.AddListener(delegate{deviceMenuStart(buildings[buildingNum]["id"].str,rooms[buildingNum][roomNum]["id"].str);});


		Text text = button.GetComponentInChildren<Text>();
		if (text)
			text.text = name;

		RectTransform transform = button.GetComponent<RectTransform>();
		transform.SetParent(roomCanvas.transform);
		transform.anchoredPosition = nextLayoutPosition;
		nextLayoutPosition += Vector3.down * 50;
	}

	void AddButtonRoomMenuBack(Button prefab)
	{
		Button button = Instantiate(prefab);
		button.name = "Back";
		button.onClick.AddListener(delegate{menuStart();});

		Text text = button.GetComponentInChildren<Text>();
		if (text)
			text.text = "Back";

		RectTransform transform = button.GetComponent<RectTransform>();
		transform.SetParent(roomCanvas.transform);
		transform.anchoredPosition = nextLayoutPosition;
		nextLayoutPosition += Vector3.down * 50;
	}

	public void deviceBackButton()
	{
		roomMenuStart(buildingNumTracker);
	}

	void deviceMenuStart(string buildingID, string roomID)
	{
		StartCoroutine (getDeviceByRoom(buildingID,roomID));
		roomCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
		roomCanvas.GetComponent<CanvasGroup>().interactable = false;
		roomCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
		buildingCanvas.GetComponent<CanvasGroup>().alpha = 0.0f;
		buildingCanvas.GetComponent<CanvasGroup>().interactable = false;
		buildingCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
		deviceCanvas.GetComponent<CanvasGroup>().alpha = 1f;
		deviceCanvas.GetComponent<CanvasGroup>().interactable = true;
		deviceCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;

	}

	public void vrButton(string sceneName)
	{
		Application.LoadLevel (sceneName);
	}

}


//This is an object created to match the output of logging in to ThingsBoard
//When logging in ThingsBoard give a token to verify you have logged in
[Serializable]
public class tokenObject
{
	public string token;
	public string refreshToken;
}