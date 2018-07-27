using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLobbyScene : MonoBehaviour {

	[SerializeField]	protected Transform m_PlayersRoot;
	[SerializeField]	protected CUIPlayerInLobby m_PlayerPrefab;
	[SerializeField]	protected Sprite[] m_PlayerIcons;

	protected CPlayer m_Player;

    protected virtual void Start() {
        this.m_Player = CPlayer.GetInstance();
		this.UpdateLobby (this.m_Player.room.roomPlayes);
		// JOIN ROOM
		this.m_Player.RemoveListener("NewJoinRoom", this.UpdateLobby);
		this.m_Player.AddListener("NewJoinRoom", this.UpdateLobby);
		// LEAVE
		this.m_Player.RemoveListener("NewLeaveRoom", this.UpdateLobby);
		this.m_Player.AddListener("NewLeaveRoom", this.UpdateLobby);
    }

	protected virtual void UpdateLobby(object obj) {
		var players = obj as CPlayerData[];
		var childCount = this.m_PlayersRoot.childCount;
		var maxIndex = Mathf.Max(childCount, players.Length);
		for (int i = 0; i < maxIndex; i++)
		{
			if (i >= players.Length) {
				this.m_PlayersRoot.GetChild(i).gameObject.SetActive(false);
				continue;
			}
			var playerUI = i >= childCount
							? Instantiate(this.m_PlayerPrefab) 
							: this.m_PlayersRoot.GetChild(i).GetComponent<CUIPlayerInLobby>();
			var playerData = players[i];
			var playerName = playerData.name;
			var playerIcon = this.m_PlayerIcons [playerData.icon];
			playerUI.transform.SetParent (this.m_PlayersRoot.transform);
			playerUI.transform.localScale = Vector3.one;
			playerUI.gameObject.SetActive (true);
			playerUI.name = playerName;
			playerUI.SetupItem(playerData, playerName, playerIcon, null);
		}
	}

	public void LeaveRoom() {
		this.m_Player.LeaveRoom();
	}

}
