using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CUIPlayerInLobby : MonoBehaviour {

	[SerializeField]	protected Text m_UIName;
	[SerializeField]	protected Image m_Icon;
	[SerializeField]	protected Button m_SubmitButton;

	[SerializeField]	protected CPlayerData m_CurrentData;
	public CPlayerData currentData {
		get { return this.m_CurrentData; }
		set { this.m_CurrentData = new CPlayerData (value); }
	}

	public virtual void SetupItem(CPlayerData value, string name, Sprite icon, UnityAction callback) {
		// INDEX
		this.currentData = value;
		// JOB NAME
		this.m_UIName.text = name;
		// JOB ICON
		this.m_Icon.sprite = icon;
		// SUBMIT
		if (this.m_SubmitButton != null && callback != null) {
			this.m_SubmitButton.onClick.RemoveAllListeners();
			this.m_SubmitButton.onClick.AddListener(callback);
		}
	}
	
}
