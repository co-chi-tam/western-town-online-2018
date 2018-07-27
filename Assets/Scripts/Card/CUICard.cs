using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CUICard : MonoBehaviour {

	[SerializeField]	protected Text m_UIName;
	[SerializeField]	protected Image m_Icon;
	[SerializeField]	protected Text m_Value;
	[SerializeField]	protected Button m_SubmitButton;
	[SerializeField]	protected CCardData m_CurrentData;

	protected string m_CardFolder = "cards/{0}";

	protected int m_CardIndex;
	public int index {
		get { return this.m_CardIndex; }
	}
	
	public virtual void SetupItem(int i, CCardData data, string name, string icon, UnityAction callback) {
		// Index
		this.m_CardIndex = i;
		this.m_CurrentData = data;
		// JOB NAME
		this.m_UIName.text = name;
		// JOB ICON
		this.m_Icon.sprite = this.GetSprite(icon);
		// Value
		this.m_Value.text = data.cardValue == 0 ? data.goldValue.ToString() : data.cardValue.ToString();
		// SUBMIT
		if (this.m_SubmitButton != null && callback != null) {
			this.m_SubmitButton.onClick.RemoveAllListeners();
			this.m_SubmitButton.onClick.AddListener(callback);
		}
	}

	protected Sprite GetSprite(string name) {
		var spriteResource = Resources.Load<Sprite> (string.Format (this.m_CardFolder, name));
		return spriteResource;
	}

}
