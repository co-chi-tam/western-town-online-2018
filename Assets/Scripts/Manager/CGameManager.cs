using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleSingleton;
using SocketIO;

public class CGameManager : CMonoSingleton<CGameManager> {

	#region Fields

    protected CPlayer m_Player;

	#endregion

	#region MonoBehaviour Implementation

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance();
	}

	protected virtual void OnDestroy()
	{
		
	}

	#endregion

	#region Main methods

	public virtual void DrawCard() {
	
	}

	public virtual void UseCard(int index) {
		
	}

	#endregion

	#region State Game

	public virtual void OnStartGame() {
		
	}

	public virtual void OnEndGame() {
		
	}

	public virtual void OnResetGame() {
		
	}

	#endregion

	#region Getter && Setter

	public virtual bool IsLocalTurn() {
		return this.m_Player.turnIndex == this.m_Player.playerData.turnIndex;
	}

	#endregion

}
