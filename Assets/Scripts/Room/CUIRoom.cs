using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CUIRoom : MonoBehaviour {

	[SerializeField]	protected string m_RoomName = string.Empty;
	public string roomName { 
		get { return this.m_RoomName; } 
	}
	[SerializeField]	protected Button m_RoomButton;
	[SerializeField]	protected Color[] m_BackgroundColors;
	[SerializeField]	protected Image m_RoomBackground;
	[SerializeField]	protected Text m_RoomDisplay;

	public virtual void SetRoom (int i, string name, string display, UnityAction callback = null) {
		this.m_RoomName = name;
		this.m_RoomDisplay.text = display;
		if (callback != null) {
			this.m_RoomButton.onClick.RemoveAllListeners();	
			this.m_RoomButton.onClick.AddListener (callback);
		}
		this.m_RoomBackground.color = this.m_BackgroundColors[i % this.m_BackgroundColors.Length];
	}

	public virtual void DisableUI(bool value) {
		if (this.m_RoomButton != null) {
			this.m_RoomButton.interactable = value;
		}
	}
	
}
