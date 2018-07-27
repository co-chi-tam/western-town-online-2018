using System;
using UnityEngine;

[Serializable]
public class CBombData {

	public int turnIndex;
	public int bombIndex;
	public int bombTimer;
	public int maxTimer;

	public CBombData ()
	{
		this.turnIndex = 0;
		this.bombIndex = 0;
		this.bombTimer = 0;
		this.maxTimer = 0;
	}

}
