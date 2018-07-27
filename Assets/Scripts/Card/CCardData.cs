using System;
using UnityEngine;

[Serializable]
public class CCardData {

    public string cardName;
	public string cardIcon;
	public string cardDescription;
	public bool active;
	public int cardValue;
	public int goldValue;

	public CCardData()
	{
		this.cardName 	= string.Empty;
		this.cardIcon 	= string.Empty;
		this.cardDescription = string.Empty;
		this.active 	= true;
		this.cardValue 	= 0;
		this.goldValue	= 0;
	} 

	public CCardData(string name, string icon, string desc, bool act) 
	{
		this.cardName 	= name;
		this.cardIcon 	= icon;
		this.cardDescription = desc;
		this.active 	= act;
		this.cardValue 	= 0;
		this.goldValue	= 0;
	}

	public CCardData (CCardData value) 
	{
		this.cardName 	= value.cardName;
		this.cardIcon 	= value.cardIcon;
		this.cardDescription = value.cardDescription;
		this.active 	= value.active;
		this.cardValue 	= value.cardValue;
		this.goldValue	= value.goldValue;
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
			hash = hash * 23 + this.cardName.GetHashCode();
			hash = hash * 23 + this.cardIcon.GetHashCode();
			hash = hash * 23 + this.cardDescription.GetHashCode();
			hash = hash * 23 + this.active.GetHashCode();
			return hash;
		}
		// return base.GetHashCode();
	}

}