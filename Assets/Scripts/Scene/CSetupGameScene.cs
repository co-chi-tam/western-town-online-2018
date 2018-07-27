using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSetupGameScene : MonoBehaviour {

	[SerializeField]	protected InputField m_DisplayName;

	protected const string SAVE_PLAYER_NAME = "PLAYER_NAME";
	protected const string SAVE_PLAYER_ICON = "PLAYER_ICON";
	protected CPlayer m_Player;
	protected static string PLAYER_NAME = string.Empty;
	protected static int PLAYER_ICON = 0;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		var savedPlayerName = PlayerPrefs.GetString(SAVE_PLAYER_NAME, string.Empty);
		// SAVE NAME
		this.m_DisplayName.text = savedPlayerName;
	}

	public virtual void SubmitSetting () {
		PLAYER_NAME = this.m_DisplayName.text;
		this.SubmitAllSetting (PLAYER_NAME, PLAYER_ICON);
	}

	public void SetIcon(int index) {
		PLAYER_ICON = index;
	}

	public virtual void SubmitAllSetting(string playerName, int icon) {
		if (string.IsNullOrEmpty (playerName)) {
			this.m_Player.ShowMessage("User name must not empty.");
			return;
		}
		if (playerName.Length < 5) {
			this.m_Player.ShowMessage("User name must greater 5 character.");
			return;
		}
		this.m_Player.SetPlayerSetup (playerName, icon);
		PlayerPrefs.SetString(SAVE_PLAYER_NAME, playerName);
		PlayerPrefs.SetInt(SAVE_PLAYER_ICON, icon);
	}

}
