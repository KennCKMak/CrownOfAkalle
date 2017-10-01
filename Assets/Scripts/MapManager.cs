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


	public GameObject selectedUnit;
	protected CombatManager combatManager;
	protected UnitManager unitManager;

	//the object itself
	public GameObject[,] tileGameObjectArray;
	//the script in the tile
	public Tile[,] tileArray;

	Node[,] graph;
	List<Node> currentPath = null; //for pathfinding


	[SerializeField] int mapSizeX = 20;
	[SerializeField] int mapSizeY = 20;

	[SerializeField] protected GameObject tileGrasslandPrefab;
	[SerializeField] protected GameObject tileForestPrefab;
	[SerializeField] protected GameObject tileMountainPrefab;


	void Start(){
		combatManager = gameObject.GetComponent<CombatManager> ();
		unitManager = gameObject.GetComponent<UnitManager> ();
		GenerateMapData ();
		GeneratePathfindingGraph ();
		GenerateMapVisuals ();

		unitManager.CreateUnit (UnitManager.UnitName.TemplateUnit, 10, 10);
	
	}

	void GenerateMapData(){
		tileGameObjectArray = new GameObject[mapSizeX, mapSizeY];
		tileArray = new Tile[mapSizeX, mapSizeY];

		int x, y;
		for (x = 0; x < mapSizeX; x++) {
			for (y = 0; y < mapSizeX; y++) {
				tileGameObjectArray [x, y] = Instantiate (tileGrasslandPrefab, Vector3.zero, Quaternion.identity);
				tileArray [x, y] = tileGameObjectArray [x, y].GetComponent<Tile> ();
				tileArray[x,y].tileType = Tile.TileType.Grassland;
				tileArray[x,y].setTileVisualPrefab(tileGrasslandPrefab);
				tileArray [x,y].Setup ();
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
		graph = new Node [mapSizeX, mapSizeY];
		//Initializing each Node in the array!!
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				graph [x, y] = new Node ();
			}
		}
		//Now that all is initiated, this should work
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeY; y++) {
				//we are adding connections. only 4-way for this
				graph[x,y].x = x;
				graph [x, y].y = y;
				//adding left, right, up and down
				if (x > 0)
					graph [x, y].neighbours.Add (graph [x - 1, y]);
				if (x < mapSizeX - 1) //still have 1 nieghbour to the right
					graph [x, y].neighbours.Add (graph [x + 1, y]);
				if (y > 0)
					graph [x, y].neighbours.Add (graph [x, y - 1]);
				if (y < mapSizeY - 1) //still have 1 nieghbour above us
					graph [x, y].neighbours.Add (graph [x, y + 1]);

			}
		}
	}


	void GenerateMapVisuals() {
		for (int x = 0; x < mapSizeX; x++) {
			for (int y = 0; y < mapSizeX; y++) {
				GameObject newTile = Instantiate (tileArray[x,y].GetComponent<Tile>().getTileVisualPrefab(), new Vector3 (x, 0.5f, y), Quaternion.identity) as GameObject;
				Destroy(tileGameObjectArray [x, y].gameObject);
				tileGameObjectArray [x, y] = newTile;
				newTile.transform.parent = transform.FindChild ("MapTileGroup").transform;
				Tile tile = newTile.GetComponent<Tile> ();
				tile.setTileX(x);
				tile.setTileY(y);
				tile.map = this;
			}
		}

		//moves the transform group cubes somwhere else
		transform.FindChild("MapTileGroup").transform.parent = null;	
	}

	public Vector3 TileCoordToWorldCoord(int x, int y){

//		TileType tt = tileTypes [tiles [x, y]];
		float ypos = 1.25f;//tt.playerYPosition;
		return new Vector3 (x, ypos, y);
	}

	public bool TileIsOccupied(int x, int y){
		return tileArray [x, y].getIsOccupied ();
	}

	public bool UnitCanEnterTile(int x, int y){
		//Here we can test a unit's walk/hover
		return tileArray [x, y].getIsWalkable();
	}

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY){
		//TileType tt = tileTypes [tileArray [sourceX, sourceY]];
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

	public void GeneratePathTo(Tile tile){
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
			Node source = graph [
				             selectedUnit.GetComponent<Unit> ().getTileX (), 
				             selectedUnit.GetComponent<Unit> ().getTileY ()];

			//Where are we going?
			Node target = graph [tile.getTileX (), tile.getTileY ()];
		
			//distance from us to... ourselves
			dist [source] = 0;
			prev [source] = null; //we didn't have one before us

			//initialize everything to have INFINITY distance, since
			// we don't know any better right now. Also it's possible
			// that some nodes CAN'T be reached from the source which
			// would make INFINITY a reasonable value.
			foreach (Node v in graph) {
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


	public void ClickEvent (int x, int y, Tile tile){

	}
}
