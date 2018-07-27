using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CUIPlayerInGame : MonoBehaviour {

	[Header("UI")]
	[SerializeField]	protected Text m_UIName;
	[SerializeField]	protected Image m_Icon;
	[SerializeField]	protected Text m_Gold;
	[SerializeField]	protected GameObject m_Owner;
	[SerializeField]	protected GameObject m_Turn;
	[Header("Chat")]
	[SerializeField]	protected Animator m_ChatAnimator;
	[SerializeField]	protected Text m_ChatText;
	[Header("Animation")]
	[SerializeField]	protected Sprite[] m_IdleSprites;
	[SerializeField]	protected Sprite[] m_DeathSprites;
	[SerializeField]	protected Sprite[] m_AttackSprites;

	[SerializeField]	protected int m_Index;
	public int index {
		get { return this.m_Index; }
		set { this.m_Index = value; }
	}
	[SerializeField]	protected CPlayerData m_CurrentData;
	public CPlayerData currentData {
		get { return this.m_CurrentData; }
		set { this.m_CurrentData = new CPlayerData (value); }
	}

	protected RectTransform m_RectTransform;
	protected string m_CharacterFolder = "characters/{0}";
	protected WaitForSeconds m_WaitFor;

	protected virtual void Awake() {
		this.m_RectTransform = this.transform as RectTransform;
		this.m_WaitFor = new WaitForSeconds(Random.Range(0.2f, 0.7f));
	}

	public virtual void SetupItem(int index, CPlayerData value, string name, Sprite icon, bool isOwner, bool isTurn) {
		// INDEX
		this.m_Index = index;
		// DATA
		this.currentData = value;
		// JOB NAME
		this.m_UIName.text = name;
		// JOB ICON
		this.m_Icon.sprite = icon;
		// GOLD
		this.m_Gold.text = value.gold.ToString();
		// ANIMATION
		this.m_IdleSprites 	= this.GetCharacterSprites ((value.icon + 1).ToString(), 0, 4);
		this.m_DeathSprites = this.GetCharacterSprites ((value.icon + 1).ToString(), 4, 2);
		this.m_AttackSprites = this.GetCharacterSprites ((value.icon + 1).ToString(), 6, 1);
		// OWNER
		this.m_Owner.SetActive (isOwner);
		// TURN
		this.m_Turn.SetActive (isTurn);
	}

	public void SetPosition(Vector2 newPos) {
		this.m_RectTransform = this.transform as RectTransform;
		this.m_RectTransform.anchoredPosition = newPos;
	}

	public virtual void PlayIdle() {
		StopAllCoroutines ();
		StartCoroutine (this.HandlePlayAnimation(this.m_IdleSprites, true));
	}

	public virtual void PlayDeath() {
		StopAllCoroutines ();
		StartCoroutine (this.HandlePlayAnimation(this.m_DeathSprites, false));
	}

	public virtual void PlayAttack() {
		StopAllCoroutines ();
		StartCoroutine (this.HandlePlayAnimation(this.m_AttackSprites, false));
	}

	public virtual void ReceiveChat(string chat) {
		if (string.IsNullOrEmpty (chat) == false) {
			this.m_ChatAnimator.SetTrigger ("isShow");
			this.m_ChatText.text = chat;
		}
	}

	protected IEnumerator HandlePlayAnimation(Sprite[] anim, bool loop) {
		var index = 0;
		while(index < anim.Length) {
			this.m_Icon.sprite = anim[index];
			yield return this.m_WaitFor;
			index = loop ? (index + 1) % anim.Length : index + 1;
		}
	}

	protected Sprite[] GetCharacterSprites(string name, int start, int length) {
		var spriteResource = Resources.LoadAll<Sprite> (string.Format (this.m_CharacterFolder, name));
		var result = new Sprite[length];
		for (int i = 0; i < result.Length; i++) {
			result [i] = spriteResource [start + i];
		}
		return result;
	}

}
