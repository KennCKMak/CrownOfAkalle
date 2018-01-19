using System.Collections;
using System.Collections.Generic;
using System.Linq; //for array & pathfinding
using UnityEngine;
using System.IO;
using System;

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

	protected GameManager game;
	protected ClickManager clickManager;

	//the object itself
	[SerializeField]public GameObject[,] tileGameObjectArray;
	//the script in the tile
	[SerializeField]public Tile[,] tileArray;

	Node[,] NodeList;
	List<Node> currentPath = null; //for pathfinding
	List<Tile> validTiles; //used for setting valid tiles 
    public List<Tile> attackingTiles;

	[SerializeField] int mapSizeX = 20;
	[SerializeField] int mapSizeY = 20;
	[SerializeField] public GameObject[] tilePrefabArray;
	List<UnitSpawnData> unitSpawnData;


    protected float heightVariance = 0.00f;

	protected Shader shaderStandard;
	protected Shader shaderOutline;

	void Start(){
		game = GetComponent<GameManager> ();
		clickManager = GetComponent<ClickManager> ();

		shaderStandard = Shader.Find ("Standard");
		shaderOutline = Shader.Find ("Outline/Black");



		tileGameObjectArray = new GameObject[mapSizeX, mapSizeY];
		tileArray = new Tile[mapSizeX, mapSizeY];
		validTiles = new List<Tile> ();
		attackingTiles = new List<Tile>();	
		unitSpawnData = new List<UnitSpawnData> ();

		GenerateMap ();
		Invoke ("CreateUnits", 0.04f);

    }

	void GenerateMap(){
		//GenerateDefaultMapData ();
		//LoadFile ("Map1");
		LoadFile("Default");


		GenerateMapVisuals ();
		GeneratePathfindingGraph ();
	}

	void GenerateDefaultMapData(){

		int x, y;
        //making map
		for (x = 0; x < mapSizeX; x++) {
			for (y = 0; y < mapSizeY; y++) {
				tileArray [x, y].setTileType (Tile.TileType.Grassland);
			}
		}

        //making water
        for (y = 0; y < mapSizeY; y++)
        {
            tileArray[7, y].setTileType(Tile.TileType.Water);
            tileArray[8, y].setTileType(Tile.TileType.Water);
        }
        //bridge

        for (x=0; x<12; x++)
        {
            tileArray[x, 9].setTileType(Tile.TileType.Stone);
            tileArray[x, 10].setTileType(Tile.TileType.Stone);
        }

        tileArray[8, 10].setTileType(Tile.TileType.Bridge);
        tileArray[8, 9].setTileType(Tile.TileType.Bridge);
        tileArray[7, 10].setTileType(Tile.TileType.Bridge);
        tileArray[7, 9].setTileType(Tile.TileType.Bridge);

        for (x = 12; x < mapSizeX; x++)
        {
            tileArray[x, 10].setTileType(Tile.TileType.Stone);
            tileArray[x, 11].setTileType(Tile.TileType.Stone);
        }

        //Make a big forest area
        for (x = 3; x <= 5; x++) {
			for(y=0; y< 4; y++) {
				tileArray[x,y].setTileType(Tile.TileType.Forest);
			}
		}

        tileArray[8, 4].setTileType(Tile.TileType.Bridge);
        tileArray[8, 3].setTileType(Tile.TileType.Bridge);
        tileArray[7, 4].setTileType(Tile.TileType.Bridge);
        tileArray[7, 3].setTileType(Tile.TileType.Bridge);

        //Making a mountain range
        for (y = 0; y < mapSizeY; y++) {
			tileArray [0, y].setTileType(Tile.TileType.Mountain);

			tileArray [mapSizeX-1, y].setTileType(Tile.TileType.Mountain);
		}
	}

	void LoadFile (string s) {
		TextAsset textFile= (TextAsset)Resources.Load ("MapData/" + s);
		/*if (!File.Exists ("Assets/Assets/MapData/"+s+".txt")) {
			Debug.Log ("File " + s + " does not exist.");
			if(s != "Default")
				LoadFile ("Default");
			GenerateDefaultMapData();
			return;
		}*/
		if (!textFile)
			textFile = (TextAsset)Resources.Load ("MapData/Default");
		//else
			//Debug.Log (textFile.text);
		StringReader reader = new StringReader (textFile.text);
		Int32.TryParse (reader.ReadLine (), out mapSizeX);
		Int32.TryParse (reader.ReadLine (), out mapSizeY);

		for (int x = 0; x < mapSizeX; x++) { 
			for (int y = 0; y < mapSizeY; y++) {
				tileGameObjectArray [x, y] = Instantiate (tilePrefabArray[(int)Tile.TileType.Empty], Vector3.zero, Quaternion.identity);
				tileArray [x, y] = tileGameObjectArray [x, y].AddComponent<Tile> ();
				tileArray [x, y].map = this;
			}
		}

		string line; int tileNum, unitNum, factionNum;
		for (int x = 0; x < mapSizeX; x++) {
			for (int z = 0; z < mapSizeY; z++) {
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

	void CreateTile(int x, int y, int num){
		tileArray [x, y].setTileType ((Tile.TileType)num);
	}

	void CreateTileWithUnit(int x, int y, int tileNum, int unitNum, int factionNum){
		CreateTile(x, y, tileNum);
		unitSpawnData.Add(new UnitSpawnData(x, y, unitNum, factionNum));
	}

	void CreateUnits(){
		for (int i = 0; i < unitSpawnData.Count; i++) {
			game.unit.CreateUnit (
				(UnitManager.UnitName)(unitSpawnData[i].unitNum), 
				unitSpawnData[i].x, 
				unitSpawnData[i].y, 
				(UnitManager.Faction)(unitSpawnData[i].factionNum));
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
				GameObject newTile = Instantiate (tileArray[x,y].GetComponent<Tile>().getTileVisualPrefab(), 
                    new Vector3 (0.5f, 0.5f, 0.5f), Quaternion.identity) as GameObject;
				newTile.AddComponent<Tile> ();
				newTile.GetComponent<Tile>().setTileX(x);
				newTile.GetComponent<Tile>().setTileY(y);
				newTile.GetComponent<Tile>().map = this;
				newTile.GetComponent<Tile> ().heightVariance = 0;//Random.value * (int)Random.Range(-1, 1) * heightVariance;
                newTile.transform.position = new Vector3(x, 0.5f + newTile.GetComponent<Tile>().heightVariance, y);


				newTile.GetComponent<Tile>().setTileType(tileArray[x,y].getTileType());
				newTile.GetComponent<Tile>().setTileVisualPrefab(tileArray[x,y].getTileVisualPrefab());


				newTile.GetComponent<Tile>().clickManager = clickManager;
				newTile.GetComponent<Tile>().shaderNormal = shaderStandard;
				newTile.GetComponent<Tile>().shaderOutline = shaderOutline;


				newTile.GetComponent<Tile> ().Setup ();
				tileArray [x, y] = newTile.GetComponent<Tile> ();


				//maptilegroup
				newTile.transform.parent = transform.GetChild(0).transform;


				Destroy(tileGameObjectArray [x, y].gameObject);
				tileGameObjectArray [x, y] = newTile;
			}
		}

		//moves the transform group cubes somwhere else
		transform.GetChild(0).transform.parent = null;	
	}

	public Vector3 TileCoordToWorldCoord(int x, int y){

//		TileType tt = tileTypes [tiles [x, y]];
		float ypos = tileArray[x,y].heightVariance + 0.57f;//tt.playerYPosition;
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
		List<Node> validMoves = new List<Node> (); //neighbours

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
        if (tile == tileArray[selectedUnit.GetComponent<Unit>().getTileX(), selectedUnit.GetComponent<Unit>().getTileY()])
        {

            selectedUnit.GetComponent<Unit>().setCurrentPath(null);
            return;
        }

		GeneratePathTo (selectedUnit, tile);
        //if you have enough movement...
        if (selectedUnit.GetComponent<Unit>().hasEnoughMove()) {

            //change occupied space of the selected unit before moving
            tileArray[selectedUnit.GetComponent<Unit>().getTileX(),
                selectedUnit.GetComponent<Unit>().getTileY()].setIsOccupied(false, null);
            //our new tile is now our selected one
            tile.setIsOccupied(true, selectedUnit);
            return;
        }
        else
        {
            selectedUnit.GetComponent<Unit>().setCurrentPath(null);
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
		return (Mathf.Abs (tile2.getTileX () - tile1.getTileX ()) + Mathf.Abs (tile2.getTileY () - tile1.getTileY ()));
	}


    
    /// <summary>
    /// Used by the AI to check for what enemies are near it, first using movement range then atk range
    /// </summary>
    /// <param name="myUnit"></param>
    /// <param name="tile"></param>
    /// <param name="range"></param>
    /// <param name="atkRange"></param>
    /// <param name="unitsFound"></param>
    public void FindEnemiesInRange(Unit myUnit, Tile tile, int range, int atkRange, List<Unit> unitsFound)
    {
        List<Node> NodesVisited = new List<Node>();

        int nextMoveCost = range;

        Node source = NodeList[tile.getTileX(), tile.getTileY()];
        NodesVisited.Add(source);
        
        //recursive into others while taking away movement points
        for (int i = 0; i < source.neighbours.Count; i++)
        {
            nextMoveCost = range - (int)CostToEnterTile(source.x, source.y, source.neighbours[i].x, source.neighbours[i].y);

            if (nextMoveCost >= 0 && !NodesVisited.Contains(source.neighbours[i]))
            {
                FindEnemiesInRange(myUnit, tileArray[source.neighbours[i].x, source.neighbours[i].y], nextMoveCost, atkRange, unitsFound);
            }
        }

        if (nextMoveCost <= 0)
//            && GetTileDistance(tileArray[myUnit.getTileX(), myUnit.getTileY()], tile) >= myUnit.getWeaponRange())
        {
            FindEnemiesInAtkRange(myUnit, tile, atkRange, unitsFound);
        }
        

    }

    /// <summary>
    /// The second part. Now uses atk range and send units found back to the AI list
    /// </summary>
    /// <param name="myUnit"></param>
    /// <param name="tile"></param>
    /// <param name="atkRange"></param>
    /// <param name="unitsFound"></param>
    public void FindEnemiesInAtkRange(Unit myUnit, Tile tile, int atkRange, List<Unit> unitsFound)
    {
        List<Node> NodesVisited = new List<Node>();
        Node source = NodeList[tile.getTileX(), tile.getTileY()];
        NodesVisited.Add(source);

        if (tileArray[source.x, source.y].isOccupied())
        {
            if (tileArray[source.x, source.y].getOccupyingUnit().GetComponent<Unit>().faction != myUnit.faction
            && !unitsFound.Contains(tileArray[source.x, source.y].getOccupyingUnit().GetComponent<Unit>()))
            {
                unitsFound.Add(tileArray[source.x, source.y].getOccupyingUnit().GetComponent<Unit>());
            }
        }
        //recursive into others while taking away attack points
        for (int i = 0; i < source.neighbours.Count; i++)
        {
            int nextMoveCost = atkRange - 1;

            if (nextMoveCost >= 0 && !NodesVisited.Contains(source.neighbours[i]))
            {
                FindEnemiesInAtkRange(myUnit, tileArray[source.neighbours[i].x, source.neighbours[i].y], nextMoveCost, unitsFound);
            }
        }
    }




    public Tile FindAITileDestination(Unit myUnit, Unit enemyUnit)
    {
        attackingTiles.Clear();
        GetPositionsAvailableToAttack(myUnit, enemyUnit, tileArray[enemyUnit.getTileX(), enemyUnit.getTileY()], myUnit.getWeaponRange());
        //gets and stores into attacking tiles
        Tile closestTile = findClosestTileFromLocation(myUnit, attackingTiles);
        return closestTile;
    }


    /// <summary>
    /// Ai tries to find where to place the character so he can attack. It works backwards,
    /// getting atkRange from the enemy character, then we use the movement range to move our character
    /// to the appropriate tiles. Tiles are stored into attackingTiles
    /// </summary>
    public void GetPositionsAvailableToAttack(Unit myUnit, Unit enemyUnit, Tile tile, int range)
    {
        List<Node> NodesVisited = new List<Node>();
        Node source = NodeList[tile.getTileX(), tile.getTileY()];

        //if the tile isn't occupied, and we don't already have it
        if (!tileArray[tile.getTileX(), tile.getTileY()].isOccupied() && 
            !attackingTiles.Contains(tileArray[source.x, source.y]))
        {
            NodesVisited.Add(source);
            attackingTiles.Add(tileArray[tile.getTileX(), tile.getTileY()]);
        } else if (tileArray[tile.getTileX(), tile.getTileY()].isOccupied() && 
            tileArray[tile.getTileX(), tile.getTileY()].getOccupyingUnit().GetComponent<Unit>() == myUnit)
        {
            attackingTiles.Add(tileArray[tile.getTileX(), tile.getTileY()]);
            NodesVisited.Add(source);
        }

        //recursive into others while taking away attack points
        for (int i = 0; i < source.neighbours.Count; i++)
        {
            int nextMoveCost = range - 1; //its a weapon, distance are not affected
            if (nextMoveCost >= 0 && !NodesVisited.Contains(source.neighbours[i]))
            {
                GetPositionsAvailableToAttack(myUnit, enemyUnit, tileArray[source.neighbours[i].x, source.neighbours[i].y], nextMoveCost);
            }
        }
    }

    public Tile findClosestTileFromLocation(Unit myUnit, List<Tile> tiles)
    {

        Tile lowestTile = null;
        int lowestDist = 500;

        List<int> distance = new List<int>(); //sees which Tile we should take
        List<int> indexTrack = new List<int>();

        for(int i = 0; i < tiles.Count; i++)
        {
            //tiles[i].setHighlighted(true, "Red");

            if (tileArray[myUnit.getTileX(), myUnit.getTileY()] == tiles[i])
            {
               // Debug.Log("Already on it");
                return tiles[i];
            }


            GeneratePathTo(myUnit.gameObject, tiles[i]);
            if (currentPath != null)
            {
                distance.Add(currentPath.Count-1);
                indexTrack.Add(i);
            } 
        }

        for (int i = 0; i < distance.Count; i++)
        {
            if (distance[i] < lowestDist && distance[i] <= myUnit.getSpeed())
            {
                lowestDist = distance[i];
                lowestTile = tiles[indexTrack[i]];
            }
        }
        return lowestTile;
    }

    //Used by Simulation Plane to give it info on what terrain to build
	public Tile.TileType getTileType(int x, int y){
		if (x < 0 || y < 0 || x >= 20 || y >= 20) 
			return Tile.TileType.Empty;
		return tileArray [x, y].getTileType ();
	}
    


}
