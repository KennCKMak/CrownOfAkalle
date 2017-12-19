using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class MapBuilderManager : MonoBehaviour {

	public TextAsset textAsset;
	public string mapData;

	private int mapSizeX;
	private int mapSizeZ;

	private Tile[,] TileArray;
	private int[,] unitType;


	[SerializeField] protected GameObject tileEmpty; //0
	[SerializeField] protected GameObject tileGrasslandPrefab; //1
	[SerializeField] protected GameObject tileForestPrefab; //2
	[SerializeField] protected GameObject tileStonePrefab;//3
	[SerializeField] protected GameObject tileMountainPrefab;//4
	[SerializeField] protected GameObject tileWaterPrefab;//5
	[SerializeField] protected GameObject tileBridgePrefab;//6

	int unitTypes;
	[SerializeField] protected GameObject[] unitObjArray;

	//record map size x, map size z,
	//then build map data
	//then units

	// Use this for initialization
	void Start () {


		unitObjArray = new GameObject[unitTypes];



	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void CreateMap(){
		ClearMap ()


	}

	void ClearMap(){
		for (int i = 0; i < mapSizeX; i++) {
			for (int j = 0; j < mapSizeZ; j++) {
				SetTileArray (TileArray [i] [j], tileEmpty);
			}
		}
	}

	

	void SetTile(Tile tile, GameObject tilePrefab){
		tile.setTileVisualPrefab (tilePrefab);
	}

	void SetTileArray(Tile[,] tileArray, GameObject tilePrefab){
		for (int x = 0; x < mapSizeX; x++){
			for(int z = 0; z < mapSizeZ; z++){
				SetTile(tileArray[x][z], tilePrefab);
			}
		}
	}

	void DestroyTile(Tile tile){
		Destroy (tile.gameObject);
	}

	void DestroyTileArray(Tile[,] tileArray){
		for (int x = 0; x < mapSizeX; x++){
			for(int z = 0; z < mapSizeZ; z++){
				DestroyTile (Tile [x] [z]);
			}
		}
	}

	void SaveFile(string name){
		StreamWriter writer;

		if(!File.Exists("Assets/MapData/"+name))
			writer = File.CreateText("Assets/MapData/" + name);
		else 
			writer = new StreamWriter ("Assets/MapData/" + name);

		ConvertMapDataToWriter (writer);



		writer.Close ();
	}

	void ConvertMapDataToWriter(StreamWriter writer){
		writer.WriteLine (mapSizeX);
		writer.WriteLine (mapSizeZ);

		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeZ; z++) {
				return;
			}
		}



	}

	void LoadFile (string s) {
		if (!File.Exists (s)) {
			Debug.Log ("File " + s + " does not exist.");
			if(s != "Default")
				LoadFile ("Default");
			return;
		}
	}

	void setMapSizeX(int num){
		CreateMap ();
		mapSizeX = num;
	}

	void setMapSizeZ(int num){
		CreateMap ();
		mapSizeZ = num;
	}

	public int getMapSizeX(){
		return mapSizeX;
	}

	public int getMapSizeZ(){
		return mapSizeZ;
	}




}
