﻿using UnityEngine;
using System.Collections;

public class Attack : TileEffects
{
	public Attack(int strength) : base(strength)
	{
		color = TileEffects.ATTACK;
		//hakee värin paletista
	
	}
	public override void Action (GameObject player, GameObject enemy)
	{
		enemy.GetComponent<HealthController> ().TakeDamage (strength);
		//Debug.Log ("TakeDamage");
	}
}

