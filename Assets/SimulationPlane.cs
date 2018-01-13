using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationPlane : MonoBehaviour {
	private GameManager game;

	private GameObject[,] plane;
	private GameObject[,] tilePrefab;
	private Tile.TileType[,] tileType;
	private GameObject middleBarrier;


	//	0	1	2
	//	0	E	2
	//	0	A	2
	//	0	1	1
	//E = enemy, A = ally/myunit


	void Start () {
		game = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		game.simPlane = this;
		PopulateArray ();
		DisplayTerrain ();
	}

	void PopulateArray(){
		plane = new GameObject[3,4];
		tilePrefab = new GameObject[3, 4];
		tileType = new Tile.TileType[3,4];

		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 4; j++) {
				tileType [i, j] = Tile.TileType.Grassland;
			}
		}

		plane [0, 0] = transform.GetChild (0).gameObject;
		plane [1, 0] = transform.GetChild (1).gameObject;
		plane [2, 0] = transform.GetChild (2).gameObject;
		plane [0, 1] = transform.GetChild (3).gameObject;
		plane [1, 1] = transform.GetChild (4).gameObject;
		plane [2, 1] = transform.GetChild (5).gameObject;
		plane [0, 2] = transform.GetChild (6).gameObject;
		plane [1, 2] = transform.GetChild (7).gameObject;
		plane [2, 2] = transform.GetChild (8).gameObject;
		plane [0, 3] = transform.GetChild (9).gameObject;
		plane [1, 3] = transform.GetChild (10).gameObject;
		plane [2, 3] = transform.GetChild (11).gameObject;

		middleBarrier = transform.GetChild (12).gameObject;

	}

	public void DisplayTerrain(){
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 4; j++) {
				GameObject terrain = Instantiate (game.map.tilePrefabArray [(int)tileType [i, j]], plane [i, j].transform) as GameObject;
				terrain.transform.localScale = new Vector3 (10.0f, 10.0f, 10.0f);
				terrain.transform.localPosition = new Vector3 (0.0f, -terrain.transform.localScale.y / 2.0f, 0.0f);
				tilePrefab [i, j] = terrain;
			}
		}
	}

	public void ReplaceTerrain(Tile.TileType[,] newType){
		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 4; j++) {
				if (tileType [i, j] != newType [i, j]) {
					tileType [i, j] = newType [i, j];
					Destroy (tilePrefab [i, j]);
					GameObject terrain = Instantiate (game.map.tilePrefabArray [(int)tileType [i, j]], plane [i, j].transform) as GameObject;
					terrain.transform.localScale = new Vector3 (10.0f, 10.0f, 10.0f);
					terrain.transform.localPosition = new Vector3 (0.0f, -terrain.transform.localScale.y / 2.0f, 0.0f);
					tilePrefab [i, j] = terrain;
				}
			}
		}
		tileType = newType;
	}

	void DestroyTerrain(){		for (int i = 0; i < 3; i++) {
			for (int j = 0; j < 4; j++) {
				tileType [i, j] = Tile.TileType.Empty;
				Destroy (tilePrefab [i, j]);
				GameObject terrain = Instantiate (game.map.tilePrefabArray [(int)tileType [i, j]], plane [i, j].transform) as GameObject;
				terrain.transform.localScale = new Vector3 (10.0f, 10.0f, 10.0f);
				terrain.transform.localPosition = new Vector3 (0.0f, -terrain.transform.localScale.y / 2.0f, 0.0f);
				tilePrefab [i, j] = terrain;
				middleBarrier.SetActive (false);

			}
		}
	}

	public void GenerateTerrain(Unit myUnit, Unit enemyUnit, int dist){
		Tile.TileType[,] newTiles = new Tile.TileType [3, 4];
		int myX = myUnit.getTileX ();
		int myY = myUnit.getTileY ();
		int enemyX = enemyUnit.getTileX ();
		int enemyY = enemyUnit.getTileY ();


		int sign = 1;
		//melee
		if (myX == enemyX) { //vertically aligned 
			sign = Mathf.Clamp (enemyY - myY, -1, 1);
			GetTileTypeVerticallyAligned (myX, myY, enemyX, enemyY, sign, newTiles);
		} else if (myY == enemyY) { //horizontally aligned 
			sign = Mathf.Clamp (enemyX - myX, -1, 1);
			GetTileTypeHorizontallyAligned (myX, myY, enemyX, enemyY, sign, newTiles);
		} else {
			int xDiff = enemyX - myX;
			int yDiff = enemyY - myY;
			if (yDiff > xDiff) { //more vertical difference, low horizontal difference, vertically aligned
				sign = Mathf.Clamp(yDiff, -1, 1);
				GetTileTypeVerticallyAligned (myX, myY, enemyX, enemyY, sign, newTiles);
			} else {
				sign = Mathf.Clamp(xDiff, -1, 1);
				GetTileTypeHorizontallyAligned (myX, myY, enemyX, enemyY, sign, newTiles);
			}

		}

		if (dist == 1)
			middleBarrier.SetActive (false);
		else
			middleBarrier.SetActive (true);



		ReplaceTerrain (newTiles);
	}

	public void GetTileTypeVerticallyAligned(int myX, int myY, int enemyX, int enemyY, int sign, Tile.TileType[,] newTiles){
		newTiles [0, 3] = game.map.getTileType (enemyX - sign, enemyY + sign);
		newTiles [1, 3] = game.map.getTileType (enemyX, enemyY + sign);
		newTiles [2, 3] = game.map.getTileType (enemyX + sign, enemyY + sign);

		newTiles [0, 2] = game.map.getTileType (enemyX - sign, enemyY);
		newTiles [1, 2] = game.map.getTileType (enemyX, enemyY);//ENEMY
		newTiles [2, 2] = game.map.getTileType (enemyX + sign, enemyY);

		newTiles [0, 1] = game.map.getTileType (myX - sign, myY);
		newTiles [1, 1] = game.map.getTileType (myX, myY); //ALLY
		newTiles [2, 1] = game.map.getTileType (myX + sign, myY);

		newTiles [0, 0] = game.map.getTileType (myX - sign, myY - sign);
		newTiles [1, 0] = game.map.getTileType (myX, myY - sign);
		newTiles [2, 0] = game.map.getTileType (myX + sign, myY - sign);

	}

	public void GetTileTypeHorizontallyAligned(int myX, int myY, int enemyX, int enemyY, int sign, Tile.TileType[,] newTiles){
		newTiles [0, 3] = game.map.getTileType (enemyX + sign, enemyY + sign);
		newTiles [1, 3] = game.map.getTileType (enemyX + sign, enemyY);
		newTiles [2, 3] = game.map.getTileType (enemyX + sign, enemyY - sign);

		newTiles [0, 2] = game.map.getTileType (enemyX, enemyY + sign);
		newTiles [1, 2] = game.map.getTileType (enemyX, enemyY);//ENEMY
		newTiles [2, 2] = game.map.getTileType (enemyX, enemyY - sign);

		newTiles [0, 1] = game.map.getTileType (myX, myY + sign);
		newTiles [1, 1] = game.map.getTileType (myX, myY); //ALLY
		newTiles [2, 1] = game.map.getTileType (myX, myY - sign);

		newTiles [0, 0] = game.map.getTileType (myX - sign, myY + sign);
		newTiles [1, 0] = game.map.getTileType (myX - sign, myY);
		newTiles [2, 0] = game.map.getTileType (myX - sign, myY - sign);
	}



	
	// Update is called once per frame
	void Update () {
		
	}


}
