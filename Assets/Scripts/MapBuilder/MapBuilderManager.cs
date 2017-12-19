using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class MapBuilderManager : MonoBehaviour {

	public TextAsset textAsset;
	public string mapData;

	private int mapSizeX = 20;
	private int mapSizeZ = 20;


	[SerializeField] protected GameObject TileBuilderPrefab;
	protected GameObject[,] TileObjArray;

	protected enum PlacementType { None, TilePlacement, UnitPlacement}
	protected PlacementType placementType;
	protected Tile.TileType currentTileType;
	protected UnitManager.UnitName currentUnitType;
	protected UnitManager.Faction currentUnitFaction;

	[SerializeField] protected GameObject[] tilePrefabArray;
	[SerializeField] protected GameObject[] unitObjArray;
	[SerializeField] protected Material[] FactionColours;
	[SerializeField] protected Material HorseMaterial;


	Dropdown tileDropdown;
	Dropdown unitDropdown;
	//record map size x, map size z,
	//then build map data
	//then units



	// Use this for initialization
	void Start () {
		placementType = PlacementType.TilePlacement;
		currentTileType = Tile.TileType.Stone;
		currentUnitType = UnitManager.UnitName.SwordsmanUnit;
		currentUnitFaction = UnitManager.Faction.Player;

		tileDropdown = GameObject.Find ("TileDropdown").GetComponent<Dropdown> ();
		unitDropdown = GameObject.Find ("UnitDropdown").GetComponent<Dropdown> ();

		InitiateTileArray ();


	}

	void LateStart(){

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.R)) {
			SaveFile (mapData);
		}
		if (Input.GetKeyDown (KeyCode.F)) {
			LoadFile (mapData);
		}
		if (Input.GetKeyDown (KeyCode.Space)) {
			SwitchPlacementType ();
		}

		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			SwitchUnitType (1);
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			SwitchUnitType (2);
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			SwitchUnitType (3);
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			SwitchUnitType (4);
		}
		if(Input.GetKeyDown(KeyCode.T))
			SwitchFaction ();

	}

	void CreateMap(){
		//SetTileArray (TileObjArray, Tile.TileType.Grassland);


	}

	void ClearMap(){

	}


	//Builds the array and generates visuals
	public void InitiateTileArray (){
		TileObjArray = new GameObject[mapSizeX, mapSizeZ];
		for (int i = 0; i < mapSizeX; i++) {
			for (int j = 0; j < mapSizeZ; j++) {
				GameObject newObj = Instantiate (TileBuilderPrefab, 
					new Vector3 (i, 0, j), Quaternion.identity) as GameObject;

				newObj.GetComponent<TileBuilder> ().mapBuilder = this;
				newObj.GetComponent<TileBuilder>().setTileX(i);
				newObj.GetComponent<TileBuilder>().setTileY(j);

				TileObjArray [i, j] = newObj;
			}
		}

		SetTileArray (TileObjArray, Tile.TileType.Grassland);
		GenerateAllTileVisuals (TileObjArray);

	}

	public void DeleteTileArray (){
		for (int i = 0; i < mapSizeX; i++) {
			for (int j = 0; j < mapSizeZ; j++) {
				Destroy(TileObjArray[mapSizeX, mapSizeZ]);
			}
		}
		TileObjArray = null;
	}

	void SetTile(GameObject tile, Tile.TileType tileType, GameObject tilePrefab){
		tile.GetComponent<TileBuilder> ().setTileType (tileType, tilePrefab);
	}

	void SetTileArray(GameObject[,] tileArray, Tile.TileType tileType){
		for (int x = 0; x < mapSizeX; x++){
			for(int z = 0; z < mapSizeZ; z++){
				SetTile(tileArray[x, z], tileType, tilePrefabArray[(int)tileType]);
			}
		}
	}

	void GenerateTileVisuals(GameObject tile){
		tile.GetComponent<TileBuilder> ().DestroyVisual ();
		tile.GetComponent<TileBuilder>().GenerateVisual ();
	}

	void GenerateAllTileVisuals(GameObject[,] tileArray){
		for (int x = 0; x < mapSizeX; x++){
			for(int z = 0; z < mapSizeZ; z++){
				GenerateTileVisuals (tileArray [x,z]);
			}
		}
	}

	void DestroyTileVisuals(GameObject tile){
		tile.GetComponent<TileBuilder>().DestroyVisual ();
	}

	void DestroyAllTileVisuals(GameObject[,] tileArray){
		for (int x = 0; x < mapSizeX; x++){
			for(int z = 0; z < mapSizeZ; z++){
				DestroyTileVisuals (tileArray [x,z]);
			}
		}
	}

	public void ClickEvent(GameObject tile){

		if (placementType == PlacementType.TilePlacement) {
			ChangeTile (tile);
		} else if (placementType == PlacementType.UnitPlacement && !tile.GetComponent<TileBuilder>().isOccupied()) {
			SpawnUnitAt (tile);
		}

	}

	public void ChangeTile(GameObject tile){
		SetTile (tile, currentTileType, tilePrefabArray [(int)currentTileType]);
		GenerateTileVisuals (tile);
	}

	public void SpawnUnitAt(GameObject tile){
		GameObject newUnit = Instantiate(unitObjArray[(int)currentUnitType], transform.position, Quaternion.identity) as GameObject;

		newUnit.AddComponent<UnitBuilder> ();
		newUnit.GetComponent<UnitBuilder> ().Parts = newUnit.GetComponent<Unit> ().Parts;
		newUnit.GetComponent<Unit> ().enabled = false;

		newUnit.GetComponent<UnitBuilder>().tileX = tile.GetComponent<TileBuilder>().getTileX();
		newUnit.GetComponent<UnitBuilder>().tileY = tile.GetComponent<TileBuilder>().getTileY();


		newUnit.GetComponent<UnitBuilder>().factionColour = FactionColours[(int)currentUnitFaction];
		newUnit.GetComponent<UnitBuilder>().horseMaterial = this.HorseMaterial;

		newUnit.GetComponent<UnitBuilder> ().unitType = currentUnitType;
		newUnit.GetComponent<UnitBuilder> ().faction = currentUnitFaction;
		newUnit.GetComponent<UnitBuilder> ().SetUpUnit ();

		tile.GetComponent<TileBuilder> ().setIsOccupied (true, newUnit);
	}

	public void SpawnUnitAt(GameObject tile, int unitType, int faction){
		GameObject newUnit = Instantiate(unitObjArray[unitType], transform.position, Quaternion.identity) as GameObject;

		newUnit.AddComponent<UnitBuilder> ();
		newUnit.GetComponent<UnitBuilder> ().Parts = newUnit.GetComponent<Unit> ().Parts;
		newUnit.GetComponent<Unit> ().enabled = false;

		newUnit.GetComponent<UnitBuilder>().tileX = tile.GetComponent<TileBuilder>().getTileX();
		newUnit.GetComponent<UnitBuilder>().tileY = tile.GetComponent<TileBuilder>().getTileY();


		newUnit.GetComponent<UnitBuilder>().factionColour = FactionColours[(int)faction];
		newUnit.GetComponent<UnitBuilder>().horseMaterial = this.HorseMaterial;

		newUnit.GetComponent<UnitBuilder> ().unitType = (UnitManager.UnitName)unitType;
		newUnit.GetComponent<UnitBuilder> ().faction = (UnitManager.Faction)faction;
		newUnit.GetComponent<UnitBuilder> ().SetUpUnit ();

		tile.GetComponent<TileBuilder> ().setIsOccupied (true, newUnit);
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

	void SaveFile(string name){
		StreamWriter writer;

		if(!File.Exists("Assets/Assets/MapData/"+name+".txt"))
			writer = File.CreateText("Assets/Assets/MapData/" + name+".txt");
		else 
			writer = new StreamWriter ("Assets/Assets/MapData/" + name+".txt");

		ConvertMapDataToWriter (writer);



		writer.Close ();
	}

	void ConvertMapDataToWriter(StreamWriter writer){
		Debug.Log ("Saving Data...");
		writer.WriteLine (mapSizeX); 
		writer.WriteLine (mapSizeZ);

		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeZ; z++) {
				writer.Write (TileObjArray [x, z].GetComponent<TileBuilder>().getTileType ());
				if(TileObjArray[x,z].GetComponent<TileBuilder>().isOccupied()){
					writer.Write (" ");
					writer.Write (TileObjArray [x, z].GetComponent<TileBuilder>().getUnitType ());
					writer.Write (" ");
					writer.Write (TileObjArray [x, z].GetComponent<TileBuilder>().getUnitFaction ());
				}
				writer.Write ("\r\n");
			}
		} //reads tile type, then the unit info

		Debug.Log ("Save Complete");
	}

	void LoadFile (string s) {
		if (!File.Exists ("Assets/Assets/MapData/"+s+".txt")) {
			Debug.Log ("File " + s + " does not exist.");
			//if(s != "Default")
			//LoadFile ("Default");
			return;
		}

		StreamReader reader = new StreamReader ("Assets/Assets/MapData/" + s + ".txt");
		Int32.TryParse (reader.ReadLine (), out mapSizeX);
		Int32.TryParse (reader.ReadLine (), out mapSizeZ);

		string line; int tileNum, unitNum, factionNum;
		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeZ; z++) {
				line = reader.ReadLine ();
				if (!line.Contains (" ")){// 1
					Int32.TryParse (line, out tileNum);
					CreateTile (x, z, tileNum);
				} else { // 1 1 2
					string t = line.ToCharArray()[0].ToString();
					string u = line.ToCharArray()[2].ToString();
					string f = line.ToCharArray()[4].ToString();
					Int32.TryParse(t, out tileNum);
					Int32.TryParse(u, out unitNum);
					Int32.TryParse(f, out factionNum);
					CreateTileWithUnit (x, z, tileNum, unitNum, factionNum);
				}
			}
		}

		reader.Close();
	}

	void CreateTile(int x, int z, int num){
		TileObjArray [x, z].GetComponent<TileBuilder> ().DestroyVisual ();
		TileObjArray [x, z].GetComponent<TileBuilder>().setTileType ((Tile.TileType)num, tilePrefabArray[num]);
		TileObjArray [x, z].GetComponent<TileBuilder> ().GenerateVisual ();
	}

	void CreateTileWithUnit(int x, int z, int tileNum, int unitNum, int factionNum){
		CreateTile(x, z, tileNum);
		SpawnUnitAt (TileObjArray [x, z], unitNum, factionNum);
	}


	public void SwitchPlacementType(){
		if (placementType == PlacementType.TilePlacement)
			placementType = PlacementType.UnitPlacement;
		else
			placementType = PlacementType.TilePlacement;
	}

	public void SwitchFaction(){
		if (currentUnitFaction == UnitManager.Faction.Player)
			currentUnitFaction = UnitManager.Faction.Enemy;
		else if (currentUnitFaction == UnitManager.Faction.Enemy)
			currentUnitFaction = UnitManager.Faction.Player;
	}

	public void TileDropdownUpdate(){
		Debug.Log ("tile dropdown = " + tileDropdown.value);
		SwitchTileType(tileDropdown.value+1);
	}

	public void SwitchTileType(int num){
		currentTileType = (Tile.TileType)num;
	}

	public void UnitDropdownUpdate(){

		Debug.Log ("unit dropdown = " + unitDropdown.value);
		SwitchUnitType(unitDropdown.value+1);
	}

	public void SwitchUnitType(int num){
		currentUnitType = (UnitManager.UnitName)num;
	}

}
