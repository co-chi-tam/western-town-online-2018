using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SocketIO;

public class CLoadingScene : MonoBehaviour {

	[SerializeField]	protected Text m_VersionText;

	protected CPlayer m_Player;
	protected WaitForSeconds m_DelaySeconds = new WaitForSeconds(3f);
	protected float m_MaximumTimer = 30f;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		this.m_Player.Connect();
		this.m_Player.DisplayLoading (true);
		this.SendRequestConnect ();
	}

	protected virtual void SendRequestConnect() {
		this.m_MaximumTimer = 30f;
		StopAllCoroutines ();
		StartCoroutine (this.HandleSendRequestConnect ());
	}

	protected IEnumerator HandleSendRequestConnect() {
		while (this.m_MaximumTimer > 0f) {
			this.m_MaximumTimer -= 3f;
			yield return this.m_DelaySeconds;
			this.m_Player.Connect();
			if (this.m_Player.socket.IsConnected)
				break;
		}
		this.m_VersionText.text = string.Format ("Version {0}", this.m_Player.version);
		if (this.m_Player.socket.IsConnected == false) {
			this.m_Player.ShowMessage ("Can not connect server. Please try again.", () => {
				this.SendRequestConnect ();
			});
		} else {
			this.m_Player.SwithSceneTo<CSetupGameTask>();
		}
	}
	
}
