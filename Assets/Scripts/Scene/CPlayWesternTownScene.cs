using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPlayWesternTownScene : MonoBehaviour {

	[Header("Players")]
	[SerializeField]	protected float m_Radius = 250f;
	[SerializeField]	protected Transform m_PlayersRoot;
	[SerializeField]	protected CUIPlayerInGame m_PlayerPrefab;
	[SerializeField]	protected Sprite[] m_PlayerIcons;
	[Header("Bomb")]
	[SerializeField]	protected CUIBomb m_Bomb;
	[Header("Card")]
	[SerializeField]	protected Transform m_CardRoot;
	[SerializeField]	protected CUICard m_CardPrefab;

    protected CPlayer m_Player;
	protected Dictionary<string, CUIPlayerInGame> m_PlayersInGame;

    protected virtual void Start() {
        this.m_Player = CPlayer.GetInstance();
		// ROOM PLAYERS
		this.m_Player.RemoveListener ("UpdateRoomInGame", 	this.UpdateRoomInGame);
		this.m_Player.AddListener ("UpdateRoomInGame", 		this.UpdateRoomInGame);
		// PLAYER
		this.m_Player.RemoveListener ("UpdatePlayerInGame", this.UpdatePlayer);
		this.m_Player.AddListener ("UpdatePlayerInGame", 	this.UpdatePlayer);

		this.m_Player.RemoveListener ("updateRoomChat", 	this.UpdateChatRoom);
		this.m_Player.AddListener ("updateRoomChat", 		this.UpdateChatRoom);
		// BOMB
		this.m_Player.RemoveListener ("UpdateBombInGame", 	this.UpdateBombInGame);
		this.m_Player.AddListener ("UpdateBombInGame", 		this.UpdateBombInGame);

		this.m_Player.RemoveListener ("ExplosionPlayer", 	this.UpdateBombExplosion);
		this.m_Player.AddListener ("ExplosionPlayer", 		this.UpdateBombExplosion);
		// INIT
		this.UpdateRoomInGame (this.m_Player.room.roomPlayes);
		// BOMB
		this.m_Bomb.SetFitBomb (
			this.m_Player.bombTimer, 
			this.m_Player.bombIndex, 
			this.m_Player.room.roomPlayes.Length, 
			this.m_Radius
		);
		// YOUR TURN 
		this.DrawACard ();
    }

	protected virtual void OnDestroy() {
		// ROOM PLAYERS
		this.m_Player.RemoveListener ("UpdateRoomInGame", this.UpdateRoomInGame);
		// PLAYER
		this.m_Player.RemoveListener ("UpdatePlayerInGame", this.UpdatePlayer);
		this.m_Player.RemoveListener ("updateRoomChat", 	this.UpdateChatRoom);
		// BOMB
		this.m_Player.RemoveListener ("UpdateBombInGame", this.UpdateBombInGame);
		this.m_Player.RemoveListener ("ExplosionPlayer", this.UpdateBombExplosion);
	}

	protected virtual void UpdateRoomInGame(object obj) {
		var players = obj as CPlayerData[];
		var childCount = this.m_PlayersRoot.childCount;
		var maxIndex = Mathf.Max(childCount, players.Length);
		var segment = (2 * Mathf.PI) / players.Length;
		this.m_PlayersInGame = new Dictionary<string, CUIPlayerInGame> ();
		for (int i = 0; i < maxIndex; i++)
		{
			if (i >= players.Length) {
				this.m_PlayersRoot.GetChild(i).gameObject.SetActive(false);
				continue;
			}
			var playerUI = i >= childCount
				? Instantiate(this.m_PlayerPrefab) 
				: this.m_PlayersRoot.GetChild(i).GetComponent<CUIPlayerInGame>();
			var playerData = players[i];
			var playerName = playerData.name;
			var playerIcon = this.m_PlayerIcons [playerData.icon];
			playerUI.transform.SetParent (this.m_PlayersRoot);
			playerUI.transform.localScale = Vector3.one;
			playerUI.gameObject.SetActive (true);
			playerUI.name = playerName;
			playerUI.SetupItem(
				i, playerData, playerName, playerIcon, playerData.isOwner, playerData.turnIndex == this.m_Player.turnIndex
			);
			var x = Mathf.Sin (segment * i) * this.m_Radius;
			var y = Mathf.Cos (segment * i) * this.m_Radius;
			playerUI.SetPosition (new Vector2(x, y));
			// PLAY IDLE ANIMATION
			if (playerData.status == "ALIVE")
				playerUI.PlayIdle ();
			this.m_PlayersInGame.Add(playerName, playerUI);
		}
		// YOUR TURN 
		this.DrawACard ();
		// CHECK WIN OR LOSE
		this.CheckWinOrLose();
	}

	protected virtual void UpdatePlayer(object obj) {
		var player = obj as CPlayerData;
		// CARD
		var cards = player.cardList;
		this.UpdateCards (cards);
		// YOUR TURN 
		this.DrawACard ();
	}

	protected virtual void UpdateCards(object value) {
		var cards = value as CCardData[];
		var childCount = this.m_CardRoot.childCount;
		var maxIndex = Mathf.Max(childCount, cards.Length);
		for (int i = 0; i < maxIndex; i++)
		{
			if (i >= cards.Length) {
				this.m_CardRoot.GetChild(i).gameObject.SetActive(false);
				continue;
			}
			var cardUI = i >= childCount
				? Instantiate(this.m_CardPrefab) 
				: this.m_CardRoot.GetChild(i).GetComponent<CUICard>();
			var cardData = cards[i];
			var cardName = cardData.cardName;
			cardUI.transform.SetParent (this.m_CardRoot);
			cardUI.transform.localScale = Vector3.one;
			cardUI.gameObject.SetActive (true);
			cardUI.name = cardName;
			cardUI.SetupItem(i, cardData, cardName, cardData.cardIcon, () => {
				this.UseCard (cardUI.index);
			});
		}
	}

	protected virtual void UpdateChatRoom(object value) {
		var receiveMsgs = value.ToString ().Split (':');
		var playerName = receiveMsgs [0];
		var msg = receiveMsgs [1];
		if (this.m_PlayersInGame.ContainsKey (playerName)) {
			this.m_PlayersInGame [playerName].ReceiveChat (msg);
		}
	}

	protected virtual void UpdateBombInGame(object value) {
		this.m_Bomb.SetBomb (
			this.m_Player.bombTimer, 
			this.m_Player.bombIndex, 
			this.m_Player.room.roomPlayes.Length, 
			this.m_Radius
		);
	}

	protected virtual void UpdateBombExplosion(object value) {
		var explosionName = value.ToString();
		// EXPLOSION
		this.m_Bomb.SetExplosion (() => {
			if (this.m_PlayersInGame.ContainsKey(explosionName)) {
				this.m_PlayersInGame [explosionName].PlayDeath ();
				if (explosionName.Equals (this.m_Player.playerData.name)) {
					this.m_Player.ShowMessage ("YOU DEATH.", null);
					this.UpdateCards(new CCardData[] {
						new CCardData() {
							cardName = "Grave",
							cardIcon = "card_grave",
							cardValue = 0,
							cardDescription = "You receive this card wehn you death."
						}
					});
				}
			}
		});
	}

	public virtual void SendChat(InputField chat) {
		this.SendChat (chat.text);
		chat.text = string.Empty;
	}

	public virtual void SendChat(string chat) {
		if (string.IsNullOrEmpty (chat))
			return;
		this.m_Player.SendMessageRoomChat (chat, this.UpdateChatRoom);
	}

	public virtual void CheckWinOrLose() {
		StopCoroutine (this.HandleWinLose ());
		StartCoroutine (this.HandleWinLose ());
	}

	protected IEnumerator HandleWinLose() {
		yield return new WaitForSeconds (1f);
		if (this.m_Player.room.roomPlayes.Length == 1) {
			var winner = this.m_Player.room.roomPlayes [0];
			this.m_Player.ShowMessage ("WINNER IS " + winner.name, () => {
				this.m_Player.LeaveRoom ();
			});
		}
		yield return null;
	}

	protected void DrawACard() {
		if (this.m_Player.playerData.status == "DEATH")
			return;
		if (this.m_Player.playerData.turnIndex != this.m_Player.turnIndex)
			return;
		#if UNITY_DEBUG
		Debug.Log ("DrawACard");
		#endif
		this.m_Player.SendDrawACard ();
	}

	protected void UseCard(int index) {
		if (this.m_Player.playerData.status == "DEATH")
			return;
		if (this.m_Player.playerData.turnIndex != this.m_Player.turnIndex)
			return;
		#if UNITY_DEBUG
		Debug.Log ("Use card " + index);
		#endif
		this.m_Player.SendUseCard (index);
	}

	public void LeaveRoom() {
		#if UNITY_DEBUG
		Debug.Log ("Leave Room");
		#endif
		this.m_Player.LeaveRoom ();
	}

}
