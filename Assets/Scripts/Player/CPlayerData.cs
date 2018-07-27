using System;

[Serializable]
public class CPlayerData {

	public string id;
	public string name;
	public int icon;
	public int totalGold;
	public int gold;
	public bool active;
	public string status;
	public int turnIndex;
	public bool isOwner;
	public CCardData[] cardList;

	public CPlayerData()
	{
		this.id 		= string.Empty;
		this.name 		= string.Empty;
		this.icon 		= 0;
		this.totalGold	= 0;
		this.gold		= 0;
		this.active		= false;
		this.status		= "ALIVE";
		this.turnIndex 	= 0;
		this.isOwner	= false;
		this.cardList   = new CCardData[0];
	}

	public CPlayerData(CPlayerData value)
	{
		this.id 		= value.id;
		this.name 		= value.name;
		this.icon 		= value.icon;
		this.totalGold	= value.totalGold;
		this.gold		= value.gold;
		this.active		= value.active;
		this.status		= value.status;
		this.turnIndex	= value.turnIndex;
		this.isOwner	= value.isOwner;
		this.cardList	= value.cardList;
	}

	// override object.Equals
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		return base.Equals (obj);
	}
	
	// override object.GetHashCode
	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;
			hash = hash * 23 + this.id.GetHashCode();
			hash = hash * 23 + this.name.GetHashCode();
			hash = hash * 23 + this.icon.GetHashCode();
			hash = hash * 23 + this.status.GetHashCode();
			return hash;
		}
		// return base.GetHashCode();
	}

}
