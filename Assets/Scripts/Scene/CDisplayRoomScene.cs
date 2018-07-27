﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CDisplayRoomScene : MonoBehaviour {

	[SerializeField]	protected Transform m_RoomRoot;
	[SerializeField]	protected CUIRoom m_RoomPrefabs;

	protected CPlayer m_Player;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		this.RefreshRoomsStatus();

	}

	protected virtual void UpdateRoomUI(object value = null) {
		var rooms = value as CRoom[]; // this.m_Player.rooms;
		var childCount = this.m_RoomRoot.childCount;
		var maxIndex = Mathf.Max(childCount, rooms.Length);
		for (int i = 0; i < maxIndex; i++)
		{
			if (i >= rooms.Length) {
				this.m_RoomRoot.GetChild(i).gameObject.SetActive(false);
				continue;
			}
			var roomUI = i >= childCount
							? Instantiate(this.m_RoomPrefabs) 
							: this.m_RoomRoot.GetChild(i).GetComponent<CUIRoom>();
			var roomData = rooms[i];
			var roomName = roomData.roomName;
			var roomDisplay = roomData.roomDisplay;
			roomUI.transform.SetParent (this.m_RoomRoot.transform);
			roomUI.transform.localScale = Vector3.one;
			roomUI.SetRoom (i, roomName, roomDisplay, () => {
				this.JoinRoom (roomName);
				roomUI.DisableUI(false);
			});
			roomUI.gameObject.SetActive (true);
			roomUI.name = roomDisplay;
			roomUI.DisableUI(true);
		}
	}

	public virtual void RefreshRoomsStatus() {
		this.m_Player.GetRoomsStatus(this.UpdateRoomUI);
	}

	public virtual void JoinRoom(string name) {
		this.m_Player.JoinRoom(name);
	}

	public virtual void SubmitJoinOrCreateRoom() {;
		this.m_Player.JoinOrCreateRoom();
	}

}
