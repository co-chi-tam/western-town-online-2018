using System;
using UnityEngine;

[Serializable]
public class CRoom {

	public string roomName;
	public int roomMaximumMember;
	public CPlayerData[] roomPlayes;
	public string roomDisplay;

	public CRoom()
	{
		this.roomName = string.Empty;
		this.roomMaximumMember = 0;
		this.roomPlayes = new CPlayerData[0];
		this.roomDisplay = string.Empty;
	}
	
}
