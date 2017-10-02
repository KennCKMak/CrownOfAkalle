using System.Collections;
using System.Collections.Generic;
using System.Linq; //for array & pathfinding
using UnityEngine;

/// <summary>
/// Map manager.
/// 
/// TODO: Where should I put the click logic commands??
/// Selected Units, commanding to attack, etc.
/// 
/// 
/// This holds the pathfinding, map building, and all the map information 
/// </summary>

public class MapManager : MonoBehaviour {


	protected UnitManager unitManager; //spawning
	protected ClickManager clickManager;

	//the object itself
	[SerializeField]public GameObject[,] tileGameObjectArray;
	//the script in the tile
	[SerializeField]public Tile[,] tileArray;

	Node[,] NodeList;
	List<Node> currentPath = null; //for pathfinding
	List<Tile> validTiles; //used for setting valid tiles 


	[SerializeField] int mapSizeX = 20;
	[SerializeField] int mapSizeY = 20;
	[SerializeField] protected GameObject tileEmpty;
	[SerializeField] protected GameObject tileGrasslandPrefab;
	[SerializeField] protected GameObject tileForestPrefab;
	[SerializeField] protected GameObject tileMountainPrefab;

	[SerializeField] protected Shader shaderStandard;
	[SerializeField] protected Shader shaderOutline;

	void Start(){
		clickManager = GetComponent<ClickManager> ();
		unitManager = GetComponent<UnitManager> ();

		shaderStandard = Shader.Find ("Standard");
		shaderOutline = Shader.Find ("Outlined/Custom");

		GenerateMapData ();
		GenerateMapVisuals ();

		GeneratePathfindingGraph ();

		validTiles = new List<Tile> ();

	}


	void GenerateMapData(){
		tileGameObjectArray = new GameObject[mapSizeX, mapSizeY];
		tileArray = new Tile[mapSizeX, mapSizeY];

		int x, y;
		//Populating the array with empty placeholder tiles
		for (x = 0; x < mapSizeX; x++) {
			for (y = 0; y < mapSizeY; y++) {
				tileGameObjectArray [x, y] = Instantiate (tileEmpty, Vector3.zero, Quaternion.identity);
				tileArray [x, y] = tileGameObjectArray [x, y].GetComponent<Tile> ();
			}
		}

		for (x = 0; x < mapSizeX; x++) {
			for (y = 0; y < mapSizeY; y++) {
				tileArray[x,y].tileType = Tile.TileType.Grassland;
				tileArray[x,y].setTileVisualPrefab(tileGrasslandPrefab);
			}
		}


		//Make a big forest area
		for (x = 3; x <= 5; x++) {
			for(y=0; y< 4; y++) {
				tileArray[x,y].setTileType(Tile.TileType.Forest);
				tileArray[x,y].setTileVisualPrefab(tileForestPrefab);
			}
		}
		//Making a mountain range
		for (y = 0; y < mapSizeY; y++) {
			tileArray [0, y].setTileType(Tile.TileType.Mountain);
			tileArray [0, y].setTileVisualPrefab(tileMountainPrefab);

			tileArray [mapSizeX-1, y].setTileType(Tile.TileType.Mountain);
			tileArray [mapSizeX-1, y].setTileVisualPrefab(tileMountainPrefab);
		}
	}


	void GeneratePathfindingGraph(){
		//Initializing the array!
		NodeList = new Node [mapSizeX, mapSizeY];
		//Initializing each Node in the array!!
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				NodeList [x, y] = new Node ();
			}
		}
		//Now that all is initiated, this should work
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				//we are adding connections. only 4-way for this
				NodeList[x,y].x = x;
				NodeList [x, y].y = y;
				//adding left, right, up and down
				if (x > 0)
					NodeList [x, y].neighbours.Add (NodeList [x - 1, y]);
				if (x < mapSizeX - 1) //still have 1 nieghbour to the right
					NodeList [x, y].neighbours.Add (NodeList [x + 1, y]);
				if (y > 0)
					NodeList [x, y].neighbours.Add (NodeList [x, y - 1]);
				if (y < mapSizeY - 1) //still have 1 nieghbour above us
					NodeList [x, y].neighbours.Add (NodeList [x, y + 1]);

			}
		}
	}


	void GenerateMapVisuals() {
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeX; y++) {
				GameObject newTile = Instantiate (tileArray[x,y].GetComponent<Tile>().getTileVisualPrefab(), new Vector3 (x, 0.5f, y), Quaternion.identity) as GameObject;
				newTile.GetComponent<Tile>().setTileX(x);
				newTile.GetComponent<Tile>().setTileY(y);
				newTile.GetComponent<Tile>().map = this;


				newTile.GetComponent<Tile>().setTileType(tileArray[x,y].getTileType());

				newTile.GetComponent<Tile>().setTileVisualPrefab(tileArray[x,y].getTileVisualPrefab());


				newTile.GetComponent<Tile>().clickManager = clickManager;
				newTile.GetComponent<Tile>().shaderNormal = shaderStandard;
				newTile.GetComponent<Tile>().shaderOutline = shaderOutline;


				newTile.GetComponent<Tile> ().Setup ();
				tileArray [x, y] = newTile.GetComponent<Tile> ();



				newTile.transform.parent = transform.FindChild ("MapTileGroup").transform;


				Destroy(tileGameObjectArray [x, y].gameObject);
				tileGameObjectArray [x, y] = newTile;
			}
		}

		//moves the transform group cubes somwhere else
		//transform.FindChild("MapTileGroup").transform.parent = null;	
	}

	public Vector3 TileCoordToWorldCoord(int x, int y){

//		TileType tt = tileTypes [tiles [x, y]];
		float ypos = 1.25f;//tt.playerYPosition;
		return new Vector3 (x, ypos, y);
	}

	public bool TileIsOccupied(int x, int y){
		return tileArray [x, y].isOccupied ();
	}

	public bool UnitCanEnterTile(int x, int y){
		//Here we can test a unit's walk/hover
		if (tileArray [x, y].isOccupied ())
			return false;

		return tileArray [x, y].isWalkable();
	}

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY){
		
		float cost = tileArray[sourceX, sourceY].getMovementCost();

		if (UnitCanEnterTile (targetX, targetY) == false)
			return Mathf.Infinity;
		//implementation for diagonal movement - 
		//prefer straight lines isntead
		if (sourceX != targetX && sourceY != targetY) {
			//fudges cost for tie-breaking
			cost += 0.0001f;
		}
		return cost;
	}

	public void GeneratePathTo(GameObject selectedUnit, Tile tile){
		if (selectedUnit != null) {
			currentPath = null;
			selectedUnit.GetComponent<Unit> ().setCurrentPath (null);

			if (!UnitCanEnterTile (tile.getTileX (), tile.getTileY ())) 
				return;
			if (TileIsOccupied(tile.getTileX(), tile.getTileY()))
				return;

			


			Dictionary<Node, float> dist = new Dictionary<Node, float> ();
			Dictionary<Node, Node> prev = new Dictionary<Node, Node> ();

			//Setup the 'Q' -- the list of nodes we haven't checked yet
			List<Node> unvisited = new List<Node> ();

			//Where are we starting from? Getting our node xy from the node list 'graph'
			Node source = NodeList [
				             selectedUnit.GetComponent<Unit> ().getTileX (), 
				             selectedUnit.GetComponent<Unit> ().getTileY ()];

			//Where are we going?
			Node target = NodeList [tile.getTileX (), tile.getTileY ()];
		
			//distance from us to... ourselves
			dist [source] = 0;
			prev [source] = null; //we didn't have one before us

			//initialize everything to have INFINITY distance, since
			// we don't know any better right now. Also it's possible
			// that some nodes CAN'T be reached from the source which
			// would make INFINITY a reasonable value.
			foreach (Node v in NodeList) {
				if (v != source) {
					dist [v] = Mathf.Infinity;
					prev [v] = null;
				}

				unvisited.Add (v);
			}
			//unviisted not empty yet...
			while (unvisited.Count > 0) {

				//u is going to be the unvisited node with the smallest distance
				Node u = null;

				foreach (Node possibleU in unvisited) {
					if (u == null || dist [possibleU] < dist [u]) {
						u = possibleU;
					}
				}

				//if we are at target
				if (u == target) {
					break;
				}


				unvisited.Remove (u);
				//Calculates Distance. Here you can add Modifiers
				foreach (Node v in u.neighbours) {
					//float alt = dist [u] + u.DistanceTo (v);
					float alt = dist [u] + CostToEnterTile (u.x, u.y, v.x, v.y);

					if (alt < dist [v]) { //found shorter path
						dist [v] = alt;
						prev [v] = u;
					}
				}
			}

			//If we get here, either we found the shortest route
			//to our target, or there is no route at all
	

			if (prev [target] == null) {
				//No route between our target and the source.
				return;
			}
	
			//what if we DO have a route.
			currentPath = new List<Node> ();
			Node curr = target;

			//step through the 'prev' chain and add it to our path
			while (curr != null) {
				currentPath.Add (curr);
				curr = prev [curr];
			}

			//Right now, currentPath describes a rotue from our target to our source.
			//So we need to inver it.

			currentPath.Reverse (); //Linq
			selectedUnit.GetComponent<Unit> ().setCurrentPath (currentPath);
		}
	}

	//gives a list of tiles to be highlighted (within range of the given tile)
	public void showValidMoves(GameObject myUnit, Tile tile, int range, string type){
		List<Node> validMoves = new List<Node> ();

		if (!tile.isOccupied ()) {
			validTiles.Add(tile); //add my current tiles
		} else {
			if (type == "Move" && tile.getOccupyingUnit () == myUnit)
				validTiles.Add(tile); //add my current tiles
			if (type == "Attack"){
				if(tile.getOccupyingUnit().GetComponent<Unit>().faction != myUnit.GetComponent<Unit>().faction
					|| tile.getOccupyingUnit() == myUnit)
					validTiles.Add (tile);
				
			}
		}

		Node source = NodeList [tile.getTileX (), tile.getTileY ()];
		validMoves.Add (source);

		for (int i = 0; i < source.neighbours.Count; i++) {
			//recursive into others while taking away movement points


			int nextMoveCost;
			if (type == "Move")
				nextMoveCost = range - (int)CostToEnterTile (source.x, source.y, source.neighbours [i].x, source.neighbours [i].y);
			else if (type == "Attack")
				nextMoveCost = range - 1;
			else {
				Debug.Log ("showValidMoves: Not a valid type - " + type);
				break;
			}


			if (nextMoveCost >= 0 && !validMoves.Contains (source.neighbours [i])) {
				showValidMoves (myUnit, tileArray [source.neighbours [i].x, source.neighbours [i].y], nextMoveCost, type);
			}
		}
	}

	public void MoveUnitTowards(Tile tile, GameObject selectedUnit){
		GeneratePathTo (selectedUnit, tile);
		//if you have enough movement...
		if (selectedUnit.GetComponent<Unit> ().hasEnoughMove()) {

			//change occupied space of the selected unit before moving
			tileArray [selectedUnit.GetComponent<Unit> ().getTileX (),
				selectedUnit.GetComponent<Unit> ().getTileY ()].setIsOccupied (false, null);
			//our new tile is now our selected one
			tile.setIsOccupied (true, selectedUnit);
			selectedUnit.GetComponent<Unit>().setState (Unit.State.Done);
			return;
		}
	}

	public void cleanValidMovesTilesList(){
		validTiles.Clear ();
	}

	public List<Tile> getValidMovesTilesList(){
		return validTiles;
	}

	public void cleanMap(){
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				//tileArray [x, y].setIsSelected (false);
				tileArray [x, y].setHighlighted (false);
			}
		}

	}


	public int GetTileDistance(Tile tile1, Tile tile2){
		int num = 0;
		Debug.Log ("Tile1 @ " + tile1.getTileX () + ", " + tile1.getTileY () + ". Tile2 @ " + tile2.getTileX () + ", " + tile2.getTileY ());

		return num += (Mathf.Abs (tile2.getTileX () - tile1.getTileX ()) + Mathf.Abs (tile2.getTileY () - tile1.getTileY ()));
	}



}
