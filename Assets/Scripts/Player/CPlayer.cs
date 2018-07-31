using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SocketIO;
using SimpleSingleton;

public class CPlayer : CMonoSingleton<CPlayer> {

	#region Fields

	[SerializeField]	protected SocketIOComponent m_Socket;
	public SocketIOComponent socket { 
		get { return this.m_Socket; } 
		set { this.m_Socket = value; }
	}

	[SerializeField]	protected CPlayerData m_Data;
	public CPlayerData playerData { 
		 get { return this.m_Data; }
		 set { this.m_Data = value; }
	}

	[SerializeField]	protected CRoom m_Room;
	public CRoom room { 
		 get { return this.m_Room; }
		 set { this.m_Room = value; }
	}

	[SerializeField]	protected CSwitchScene m_SwitchScene;
	[Header("UI")]
	[SerializeField]	protected GameObject m_LoadingGOPanel;
	[SerializeField]	protected GameObject m_MessagePanel;
	[SerializeField]	protected Text m_MessageText;
	[SerializeField]	protected Button m_MessageOKButton;

	protected Dictionary<string, Action<object>> m_SimpleEvent;

	protected CRoom[] m_Rooms = new CRoom[0];
	public CRoom[] rooms { 
		get { return this.m_Rooms; }
		set { this.m_Rooms = value; }
	}
	[Header("Game")]
	[SerializeField]	protected CBombData m_Bomb;
	public int turnIndex {
		get { return this.m_Bomb.turnIndex; }
		set { this.m_Bomb.turnIndex = value; }
	}
	public int bombIndex {
		get { return this.m_Bomb.bombIndex; }
		set { this.m_Bomb.bombIndex = value; }
	}
	public int bombTimer {
		get { return this.m_Bomb.bombTimer; }
		set { this.m_Bomb.bombTimer = value; }
	}
	public int maxTimer {
		get { return this.m_Bomb.maxTimer; }
		set { this.m_Bomb.maxTimer = value; }
	}
	[SerializeField]	protected string m_Version;
	public string version {
		get { return this.m_Version; }
		protected set { this.m_Version = value; }
	}

	// Delay 3 second
	protected WaitForSeconds m_DelaySeconds = new WaitForSeconds(3f);
	protected float m_DelayShowErrorMsg = 30f;

	#endregion

	#region Implementation MonoBehaviour

	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad(this.gameObject);
		this.m_SimpleEvent = new Dictionary<string, Action<object>>();
		this.m_Bomb = new CBombData ();
	}

	protected virtual void Start()
	{
		// TEST SERVER
 		socket.On("open", ReceiveOpenMsg);
		socket.On("boop", ReceiveBoop);
		socket.On("error", ReceiveErrorMsg);
		socket.On("msgError", ReceiveErrorMsg);
		socket.On("close", ReceiveCloseMsg);
		socket.On("disconnect", ReceiveCloseMsg);
		// GAME DATA
		socket.On("gameInit", this.ReceiveGameData);
		// ROOM
		socket.On("newJoinRoom", this.JoinRoomCompleted);
		socket.On("joinRoomFailed", this.JoinRoomFailed);
		socket.On("newLeaveRoom", this.LeaveRoomCompleted);
		socket.On("updateRoomStatus", this.UpdateRoomStatus);
		socket.On("clearRoom", this.ReceiveClearRoom);
		socket.On("msgChatRoom", this.ReceiveRoomChat);
		socket.On("playerGameSet", this.ReceivePlayerSetting);
		// GAME
		socket.On("enterGame", this.ReceiveEnterGame);
		socket.On("updatePlayer", this.ReceiveUpdatePlayer);
		socket.On("updateTurn", this.ReceiveUpdateTurn);
		socket.On ("explosionPlayer", this.ReceiveExplosionPlayer);
		socket.On ("msgError", this.ReceiveMessageError);
		// Beep to server
		StartCoroutine(this.BeepBoop());
		// Close loading
		this.m_SwitchScene.OnLoading.AddListener ((value) => {
			this.DisplayLoading (value);
		});
	}

	protected virtual void Update() {
		if (Input.GetKeyDown(KeyCode.Home) 
			|| Input.GetKeyDown(KeyCode.Escape)
			|| Input.GetKeyDown(KeyCode.Menu)) {
			this.Disconnect ();
			this.SwithSceneTo<CLoadingTask>();
		}
	}

	protected virtual void OnApplicationQuit()
	{
		this.Disconnect();
	}

	#endregion

	#region Main methods

	public virtual void Connect() {
		if (this.m_Socket != null) {
			this.m_Socket.Connect();
		}
	}

	public virtual void Disconnect() {
		if (this.m_Socket != null) {
			this.m_Socket.Close();
		}
	}

	private IEnumerator BeepBoop()
	{
		while (true) {
			// wait 3 seconds and continue
			yield return this.m_DelaySeconds;
			this.Connect();
			this.m_Socket.Emit("beep");
			this.m_DelayShowErrorMsg -= 3f;
			if (this.m_DelayShowErrorMsg < 0f) {
				this.Disconnect ();
				this.SwithSceneTo<CLoadingTask> ();
				yield break;
			}
		}
	}

	public virtual void AddListener(string name, Action<object> eventCallback) {
		if (this.m_SimpleEvent.ContainsKey(name))
			return;
		this.m_SimpleEvent.Add (name, eventCallback);
	}

	public virtual void CallbackEvent(string name, object value = null) {
		if (this.m_SimpleEvent.ContainsKey(name) == false)
			return;
		this.m_SimpleEvent[name].Invoke(value);
	}  

	public virtual void RemoveListener(string name, Action<object> eventCallback) {
		if (this.m_SimpleEvent.ContainsKey(name) == false)
			return;
		this.m_SimpleEvent.Remove (name);
	}
	
	public virtual void RemoveAllListener() {
		this.m_SimpleEvent.Clear ();
	}

	public virtual void CancelUI() {
		if (this.m_LoadingGOPanel != null) {
			this.m_LoadingGOPanel.SetActive (false);
		}
		if (this.m_MessagePanel != null) {
			this.m_MessagePanel.SetActive (false);
		}
	}

	public virtual void DisplayLoading(bool value) {
		if (this.m_LoadingGOPanel != null) {
			this.m_LoadingGOPanel.SetActive (value);
		}
	}

	public virtual void ShowMessage(string text, UnityAction callback = null) {
		if (this.m_MessagePanel != null && this.m_MessageText != null) {
			this.m_MessagePanel.SetActive (true);
			this.m_MessageText.text = text;
			if (callback != null) {
				this.m_MessageOKButton.onClick.RemoveListener(callback);
				this.m_MessageOKButton.onClick.AddListener (callback);
			}
		}
	}

	public virtual void SwithSceneTo(string name, float after = 0f) {
		if (after <= 0) {
			// this.m_SwitchScene.LoadScene (name);
			this.m_SwitchScene.LoadSceneAfterSeconds (name, 1f);
		} else {
			this.m_SwitchScene.LoadSceneAfterSeconds (name, after);
		}
	}

	public virtual void SwithSceneTo<T>(float after = 0f) where T : CTask, new() {
		var task = new T ();
		if (after <= 0) {
			this.SwithSceneTo (task.sceneName, 2f);
		} else {
			this.SwithSceneTo (task.sceneName, after);
		}
	}

	#endregion

	#region Send

	/// <summary> 
	/// Emit message.
	/// </summary>
	public virtual void Emit(string ev) {
		if (this.m_Socket != null) {
			this.m_Socket.Emit(ev);
		}
	}

	/// <summary> 
	/// Emit message.
	/// </summary>
	public virtual void Emit(string ev, JSONObject data) {
		if (this.m_Socket != null) {
			this.m_Socket.Emit(ev, data);
		}
	}

	/// <summary>
	/// Emit Set player setup.
	/// Necessary emit first.
	/// </summary>
	public void SetPlayerSetup(string name, int icon) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var sendData = new JSONObject();
		sendData.AddField("playerName", name);
		sendData.AddField("playerIcon", icon);
		this.Emit("setGameSetup", sendData);
		this.DisplayLoading (true);
		#if UNITY_DEBUG
		Debug.Log ("SetGameSetup " + name + " / " + dayJob.jobName);
		#endif
	}

	/// <summary>
	/// Emit Join random room.
	/// </summary>
	public void JoinOrCreateRoom() {
		var random = UnityEngine.Random.Range (0, this.m_Rooms.Length);
		this.JoinRoom (this.m_Rooms [random].roomName);
	}

	/// <summary>
	/// Emit join a room by name.
	/// </summary>
	public void JoinRoom(string roomName) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var sendData = new JSONObject();
		sendData.AddField("roomName", roomName);
		this.Emit("joinOrCreateRoom", sendData);
		this.DisplayLoading (true);
		#if UNITY_DEBUG
		Debug.Log ("JoinOrCreateRoom");
		#endif
	}

	/// <summary>
	/// Emit to get room info.
	/// </summary>
	public void GetRoomsStatus(Action<object> callback = null) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		this.Emit("getRoomsStatus");
		this.DisplayLoading (true);
		this.m_Rooms = new CRoom[0];
		this.RemoveListener("updateRoomsComplete", callback);
		this.AddListener("updateRoomsComplete", callback);
		#if UNITY_DEBUG
		Debug.Log ("GetRoomsStatus");
		#endif
	}

	/// <summary>
	/// Send room message chat.
	/// </summary>
	public void SendMessageRoomChat(string msg, Action<object> callback) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var sendData = new JSONObject();
		sendData.AddField("message", msg);
		this.Emit("sendRoomChat", sendData);
        this.RemoveListener ("updateRoomChat", callback);
        this.AddListener ("updateRoomChat", callback);
		#if UNITY_DEBUG
		Debug.Log ("SendMessageRoomChat");
		#endif
	}

	/// <summary>
	/// SEND DRAW A CARD TO SERVER
	/// </summary>
	public virtual void SendDrawACard() {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		this.Emit("drawACard");
		#if UNITY_DEBUG
		Debug.Log ("SendDrawACard");
		#endif
	}

	/// <summary>
	/// Sends the use card to server.
	/// </summary>
	public virtual void SendUseCard(int index) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var sendData = new JSONObject();
		sendData.AddField("cardIndex", index);
		this.Emit("useCard", sendData);
		#if UNITY_DEBUG
		Debug.Log ("SendUseCard");
		#endif
	}

	/// <summary>
	/// Emit leave room.
	/// </summary>
	public void LeaveRoom() {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		this.Emit("leaveRoom");
		this.SwithSceneTo<CDisplayRoomTask> ();
		#if UNITY_DEBUG
		Debug.Log ("leaveRoom");
		#endif
	}

	#endregion

	#region Receive

	/// <summary>
	/// Receive open connect message.
	/// </summary>
	public void ReceiveOpenMsg(SocketIOEvent e)
	{
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
		#endif
	}

	/// <summary>
	/// Receive beep and boop message.
	/// Keep connect between client and server.
	/// </summary>
	public void ReceiveBoop(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Boop received: " + e.name + " " + e.data);
		#endif
		this.m_DelayShowErrorMsg = 30f;
	}
	
	/// <summary>
	/// Receive error.
	/// </summary>
	public void ReceiveErrorMsg(SocketIOEvent e)
	{
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
		#endif
		this.ShowMessage (e.data.GetField("msg").ToString());
		// this.DisplayLoading (false);
	}
	
	/// <summary>
	/// Receive close connect message.
	/// </summary>
	public void ReceiveCloseMsg(SocketIOEvent e)
	{	
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
		#endif
	}

	/// <summary>
	/// Receive game init data.
	/// </summary>
	public void ReceiveGameData(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[Receive game data ] " + e.data);
		#endif
		// Version
		this.m_Version = e.data.GetField("version").ToString().Replace ("\"","");
		// Switch scene
		// this.SwithSceneTo<CSetupGameTask>();
	}

	/// <summary>
	/// Receive player name message.
	/// Emit from SetPlayerName.
	/// </summary>
	public void ReceivePlayerSetting (SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[SOCKET IO] Player name receive " + e.name + e.data);
		#endif
		this.m_Data.id 		= e.data.GetField("id").ToString().Replace ("\"","");
		this.m_Data.name 	= e.data.GetField("name").ToString().Replace("\"", "");
		this.m_Data.icon 	= int.Parse (e.data.GetField("icon").ToString().Replace("\"", ""));
		this.SwithSceneTo<CDisplayRoomTask> ();
	}

	/// <summary>
	/// Receive Join a room complete message.
	/// Change PlayCaro7x7Scene
	/// Emit from JoinOrCreateRoom or JoinRoom.
	/// </summary>
	public void JoinRoomCompleted(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Join room received: " + e.name + " " + e.data);
		#endif
		var room = e.data.GetField("roomInfo");
		this.m_Room = new CRoom();
		this.m_Room.roomName = room.GetField("roomName").ToString().Replace ("\"","");
		this.m_Room.roomMaximumMember = int.Parse (room.GetField("maxPlayer").ToString().Replace ("\"",""));
		var players = room.GetField("players").list;
		// ROOM PLAYERS
		this.UpdateRoomPlayers (players);
		this.SwithSceneTo<CLobbyTask> ();
		this.CallbackEvent("NewJoinRoom", this.m_Room.roomPlayes);
	}

	/// <summary>
	/// Updates the room players.
	/// </summary>
	protected void UpdateRoomPlayers(List<JSONObject> players) {
		#if UNITY_DEBUG
		Debug.Log ("[UpdateRoomPlayers]: " + players.Count);
		#endif
		if (this.m_Room.roomPlayes == null
			|| this.m_Room.roomPlayes.Length != players.Count) {
			this.m_Room.roomPlayes = new CPlayerData[players.Count];
		}
		for (int i = 0; i < players.Count; i++)
		{
			var tmpPlayer = players[i];
			if (this.m_Room.roomPlayes[i] == null) {
				this.m_Room.roomPlayes[i] = new CPlayerData();
			}
			this.m_Room.roomPlayes[i].turnIndex = int.Parse (tmpPlayer.GetField("turnIndex").ToString().Replace ("\"",""));
			this.m_Room.roomPlayes[i].name 		= tmpPlayer.GetField("playerName").ToString().Replace ("\"","");
			this.m_Room.roomPlayes[i].icon 		= int.Parse (tmpPlayer.GetField("playerIcon").ToString().Replace ("\"",""));
			this.m_Room.roomPlayes[i].gold 		= int.Parse (tmpPlayer.GetField("gold").ToString().Replace ("\"",""));
			this.m_Room.roomPlayes[i].status 	= tmpPlayer.GetField("status").ToString().Replace ("\"","");
			this.m_Room.roomPlayes[i].isOwner	= this.m_Data == null ? false : this.m_Room.roomPlayes [i].name == this.m_Data.name;
		}
		// UPDATE CALLBACK
		this.CallbackEvent("UpdateRoomInGame", this.m_Room.roomPlayes);
	}

	/// <summary>
	/// Receive Join a room fail message.
	/// Emit from JoinOrCreateRoom or JoinRoom.
	/// </summary>
	public void JoinRoomFailed(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Join room failed received: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
		this.ShowMessage (e.data.GetField("msg").ToString(), () => {
			this.LeaveRoom();
		});
	}

	/// <summary>
	/// Receive leave message.
	/// Emit from LeaveRoom.
	/// </summary>
	public void ReceiveClearRoom(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Received clear room: " + e.name + " " + e.data);
		#endif
		this.m_Room = new CRoom ();
		this.SwithSceneTo<CDisplayRoomTask> ();
		this.ShowMessage (e.data.GetField("msg").ToString());
	}

	/// <summary>
	/// Receive leave message.
	/// Emit from LeaveRoom.
	/// </summary>
	public void LeaveRoomCompleted(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Leave received: " + e.name + " " + e.data);
		#endif
		var room = e.data.GetField("roomInfo");
		this.m_Room = new CRoom();
		this.m_Room.roomName = room.GetField("roomName").ToString().Replace ("\"","");
		this.m_Room.roomMaximumMember = int.Parse (room.GetField("maxPlayer").ToString().Replace ("\"",""));
		// ROOM PLAYERS
		var players = room.GetField("players").list;
		this.UpdateRoomPlayers (players);
		this.CallbackEvent("NewLeaveRoom", this.m_Room.roomPlayes);
		this.CallbackEvent ("UpdateRoomInGame", this.m_Room.roomPlayes);
	}

	/// <summary>
	/// Receive Update Room Status message.
	/// Emit from GetRoomStatus.
	/// </summary>
	public void UpdateRoomStatus(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Received update room status: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
		var receiveRooms = e.data.GetField("rooms").list;
		this.m_Rooms = new CRoom[receiveRooms.Count];
		for (int i = 0; i < receiveRooms.Count; i++)
		{
			var tmpRoom = receiveRooms[i];
			var tmpRoomName = tmpRoom.GetField("roomName").ToString().Replace("\"", "");
			var tmpRoomDisplay = tmpRoom.GetField("roomDisplay").ToString().Replace("\"", "");
			this.m_Rooms[i] = new CRoom();
			this.m_Rooms[i].roomName = tmpRoomName;
			this.m_Rooms[i].roomDisplay = tmpRoomDisplay;
		}
		this.CallbackEvent("updateRoomsComplete", this.m_Rooms);
	}

	/// <summary>
	/// Receive enter game message.
	/// Switch scene to PlayWolvesGameScene.
	/// </summary>
	public void ReceiveEnterGame(SocketIOEvent e) {
		// PLAYER
		this.ReceiveUpdatePlayer(e);
		// TURN
		this.ReceiveUpdateBomb(e);
		// ROOM
		this.UpdateRoomPlayers(e.data.GetField("town").GetField("townMembers").list);
		#if UNITY_DEBUG
		Debug.Log ("[Receive enter game]: " + e.data);
		#endif
		this.SwithSceneTo<CPlayWesternTownTask>();
	}

	/// <summary>
	/// Receives the update player.
	/// </summary>
	public void ReceiveUpdatePlayer(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[Receive Update Player]: " + e.data);
		#endif
		var player = e.data.GetField("player");
		this.m_Data.turnIndex = int.Parse (player.GetField("turnIndex").ToString().Replace("\"", ""));
		this.m_Data.name = player.GetField("playerName").ToString().Replace("\"", "");
		this.m_Data.icon = int.Parse (player.GetField("playerIcon").ToString().Replace("\"", ""));
		this.m_Data.totalGold = int.Parse (player.GetField("totalGold").ToString().Replace("\"", ""));
		this.m_Data.gold = int.Parse (player.GetField("gold").ToString().Replace("\"", ""));
		this.m_Data.status = player.GetField("status").ToString().Replace("\"", "");
		this.m_Data.isOwner = true;
		var cardListParse = player.GetField ("cardList").list;
		if (this.m_Data.cardList == null
			|| this.m_Data.cardList.Length != cardListParse.Count) {
			this.m_Data.cardList = new CCardData[cardListParse.Count];
		}
		for (int i = 0; i < cardListParse.Count; i++) {
			if (this.m_Data.cardList [i] == null) {
				this.m_Data.cardList [i] = new CCardData ();
			}
			this.m_Data.cardList [i].cardName = cardListParse [i].GetField ("cardName").ToString().Replace("\"", "");
			this.m_Data.cardList [i].cardIcon = cardListParse [i].GetField ("cardIcon").ToString().Replace("\"", "");
			this.m_Data.cardList [i].cardDescription = cardListParse [i].GetField ("cardDescription").ToString().Replace("\"", "");
			this.m_Data.cardList [i].cardValue = int.Parse (cardListParse [i].GetField("cardValue").ToString().Replace("\"", ""));
			this.m_Data.cardList [i].goldValue = int.Parse (cardListParse [i].GetField("goldValue").ToString().Replace("\"", ""));
		}
		this.CallbackEvent("UpdatePlayerInGame", this.m_Data);
	}

	/// <summary>
	/// Receives the update bomb.
	/// </summary>
	public void ReceiveUpdateTurn(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[Receive Update Turn]: " + e.data);
		#endif
		// INDEX
		this.ReceiveUpdateBomb (e);
		// ROOM
		this.UpdateRoomPlayers(e.data.GetField("town").GetField("townMembers").list);
		// PLAYER
		this.ReceiveUpdatePlayer(e);
	}

	/// <summary>
	/// Receives the update bomb.
	/// </summary>
	public void ReceiveUpdateBomb(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[Receive Update Bomb]: " + e.data);
		#endif
		// INDEX
		this.turnIndex = int.Parse (e.data.GetField("turnIndex").ToString().Replace("\"", ""));
		this.bombIndex = int.Parse (e.data.GetField("bombIndex").ToString().Replace("\"", ""));
		this.bombTimer = int.Parse (e.data.GetField("bombTimer").ToString().Replace("\"", ""));
		this.maxTimer = int.Parse (e.data.GetField("maxTimer").ToString().Replace("\"", ""));
		this.CallbackEvent("UpdateBombInGame", this.bombIndex);
	}

	/// <summary>
	/// Receives the explosion player.
	/// </summary>
	public void ReceiveExplosionPlayer(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[Receive Explosion Player]: " + e.data);
		#endif
		// PLAYER
		this.ReceiveUpdatePlayer(e);
		// EXPLOSION PLAYER
		var explosionPlayer = e.data.GetField ("explosionPlayer");
		var explosionName = explosionPlayer.GetField ("playerName").ToString().Replace("\"", "");
		// ROOM
		StartCoroutine (this.HandleWaitUpdateRoom(e));
		// EVENTS
		this.CallbackEvent ("ExplosionPlayer", explosionName);
	}

	protected IEnumerator HandleWaitUpdateRoom(SocketIOEvent e) {
		yield return new WaitForSeconds (2f);
		// ROOM
		this.UpdateRoomPlayers(e.data.GetField("town").GetField("townMembers").list);
	}

	/// <summary>
	/// Receves the message error.
	/// </summary>
	public void ReceiveMessageError(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[Receive Message Error]: " + e.data);
		#endif
		var msg = e.data.GetField ("msg").ToString().Replace("\"", "");
		this.ShowMessage (msg, null);
	}

	/// <summary>
	/// Receive Chat message.
	/// Emit from SendMessageRoomChat.
	/// </summary>
	public void ReceiveRoomChat (SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[SOCKET IO] Room chat receive " + e.name + e.data);
		#endif
		var playerName = e.data.GetField("playerName").ToString().Replace("\"", "");
		var message = e.data.GetField("message").ToString().Replace("\"", "");
		this.CallbackEvent("updateRoomChat", string.Format("{0}:{1}", playerName, message));
	}

	#endregion

	#region Utilities

	private bool ActionComparer<T>(Action<T> firstAction, Action<T> secondAction) {
		if(firstAction.Target != secondAction.Target)
			return false;

		var firstMethodBody = firstAction.Method.GetMethodBody().GetILAsByteArray();
		var secondMethodBody = secondAction.Method.GetMethodBody().GetILAsByteArray();

		if(firstMethodBody.Length != secondMethodBody.Length)
			return false;

		for(var i = 0; i < firstMethodBody.Length; i++)
		{
			if(firstMethodBody[i] != secondMethodBody[i])
				return false;
		}
		return true;
	}

	#endregion

}
