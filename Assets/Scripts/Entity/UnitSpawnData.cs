using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawnData {

	public int x;
	public int y;
	public int unitNum;
	public int factionNum;
	public UnitSpawnData(int newX, int newY, int unitType, int newFaction){
		x = newX;
		y = newY;
		unitNum = unitType;
		factionNum = newFaction;
	}
}
