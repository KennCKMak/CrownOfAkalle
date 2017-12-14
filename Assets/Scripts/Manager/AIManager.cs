using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour {


    protected GameManager game;
    protected MapManager map;
    protected ClickManager click;
    protected UnitManager unitManager;

    public GameObject selectedUnit;
    public GameObject MainTarget;

    public Tile myTile;
    public Tile enemyTile;
    public Tile destinationTile;

    public int dist;

    public bool active; //is it my turn atm?
    protected bool canClick;

    [SerializeField] protected List<Unit> myUnits;
    [SerializeField]
    protected List<Unit> enemyUnits;

    protected List<Unit> availableTargets;

    private void Start()
    {
       
        active = false;
        canClick = false;

        game = GetComponent<GameManager>();
        map = game.map;
        click = game.click;
        unitManager = game.unit;

        myUnits = new List<Unit>();
        enemyUnits = new List<Unit>();

        availableTargets = new List<Unit>();

    }

    private void Update()
    {
        if (active)
        {
            AIUpdate();
        }

    }

    public void AIStart(UnitManager.Faction myFaction, UnitManager.Faction enemyFaction)
    {
        myUnits.Clear();
        enemyUnits.Clear();
        availableTargets.Clear();
        selectedUnit = null;
        destinationTile = null;
        MainTarget = null;

        //Get all available units
        Unit[] allUnitsArray = unitManager.returnUnitArray();

        for (int i = 0; i < allUnitsArray.Length; i++)
        {
            if (allUnitsArray[i] != null)
            {
                if (myFaction == allUnitsArray[i].faction)
                    myUnits.Add(allUnitsArray[i]);
                else if (enemyFaction == allUnitsArray[i].faction)
                    enemyUnits.Add(allUnitsArray[i]);
            }
        }

        active = true;
        canClick = true;

        if (myUnits.Count == 0 || enemyUnits.Count == 0)
            AIStop();


    }

    //this is called to go through everything
    private void AIUpdate()
    {
        //if there is still a unit we can use...
        if (active)
        {
            //active, we have no unit, and we can click to choose a unit
            if (!selectedUnit && canClick)
            {
                selectedUnit = GetReadyUnit();
                if (selectedUnit == null)
                {
                    if (CheckForReadyUnits())
                    {
                        return;
                    }
                    else
                    {
                        AIStop();
                    }
                    return;
                }

                map.cleanMap();
                //We have a unit! Next function will see if we can hit
                GetUnitActionDetails(selectedUnit.GetComponent<Unit>());
                if (MainTarget == null)
                {
                    selectedUnit.GetComponent<Unit>().setState(Unit.State.Done);
                    DeselectUnit();
                    return;
                }
                myTile = map.tileArray[selectedUnit.GetComponent<Unit>().getTileX(), selectedUnit.GetComponent<Unit>().getTileY()];
                enemyTile = map.tileArray[MainTarget.GetComponent<Unit>().getTileX(), MainTarget.GetComponent<Unit>().getTileY()];
                if (myTile == destinationTile)
                {

                }
                if (destinationTile == null)
                {
                    selectedUnit.GetComponent<Unit>().setState(Unit.State.Done);
                    DeselectUnit();
                    return;
                }
                map.MoveUnitTowards(destinationTile, selectedUnit);

                canClick = false;
                //UNIT IS MOVING
                selectedUnit.GetComponent<Unit>().setState(Unit.State.Action);
                
                return;
            } //unit is moving

            //Unit reached dest, initiate atack
            if (selectedUnit && !canClick)
            {
                //and then there is my destinationTile
                if (selectedUnit.GetComponent<Unit>().getCurrentPath() == null)
                {
                    if (selectedUnit.GetComponent<Unit>().isMelee())
                    {
                        dist = map.GetTileDistance(destinationTile, enemyTile);
                        selectedUnit.GetComponent<Unit>().setIsAttacking(true);
                        selectedUnit.GetComponent<Unit>().setTarget(MainTarget.GetComponent<Unit>());
                        selectedUnit.GetComponent<Unit>().setDist(dist);
                        
                            //map.MoveUnitTowards(destinationTile, selectedUnit);
                            myTile.setIsOccupied(false, null);
                            destinationTile.setIsOccupied(true, selectedUnit);
                        selectedUnit.GetComponent<Unit>().setState(Unit.State.Action);
                        DeselectUnit();

                        canClick = false;
                    }
                    else if (selectedUnit.GetComponent<Unit>().isRanged())
                    {
                        dist = map.GetTileDistance(destinationTile, enemyTile);
                        selectedUnit.GetComponent<Unit>().setIsAttacking(true);
                        selectedUnit.GetComponent<Unit>().setTarget(MainTarget.GetComponent<Unit>());
                        selectedUnit.GetComponent<Unit>().setDist(dist);


                            //map.MoveUnitTowards(destinationTile, selectedUnit);
                            myTile.setIsOccupied(false, null);
                            destinationTile.setIsOccupied(true, selectedUnit);
                        selectedUnit.GetComponent<Unit>().setState(Unit.State.Action);
                        DeselectUnit();
                        canClick = false;
                    }
                    return;
                }
            }//end of unit reaching dest,

        }
        
    }

    public void AIStop()
    {
        if (active)
        {
            active = false;
            canClick = false;
            myUnits.Clear();
            enemyUnits.Clear();
            availableTargets.Clear();
            selectedUnit = null;
            destinationTile = null;
            MainTarget = null;

            game.turn.switchTurn();

        }
    }

    public GameObject GetReadyUnit()
    {
        foreach(Unit unit in myUnits)
        {
            if (unit.getState() == Unit.State.Ready)
                return unit.gameObject;
        }
        return null;
    }

    public bool CheckForReadyUnits()
    {
        foreach(Unit unit in myUnits)
        {
            if (unit.getState() == Unit.State.Ready)
                return true;
        }

        return false;
    }

    //With the selected unit, we try to see if there are any enemies at all
    public void GetUnitActionDetails(Unit myUnit)// Unit myUnit)
    {
        //find available targets
        MainTarget = null;
        destinationTile = null;

        if (myUnit != null)
             MainTarget = GetTarget(myUnit);
        if (MainTarget == null)
            return;
        //find destination
        destinationTile = GetTargetDestination(myUnit, MainTarget.GetComponent<Unit>());
        if (destinationTile == null)
            return;
        

        //move
    }

    public GameObject GetTarget(Unit myUnit)
    {
        availableTargets.Clear();
        map.FindEnemiesInRange(myUnit,
            map.tileArray[myUnit.getTileX(), myUnit.getTileY()],
            myUnit.getSpeed(), myUnit.getWeaponRange(),
            availableTargets);

        if (availableTargets == null)
            return null;

        Unit newTarget = GetBestTarget(myUnit, availableTargets);
        if (newTarget == null)
            return null;
        return newTarget.gameObject;
    }

    public Tile GetTargetDestination(Unit myUnit, Unit target)
    {
        map.cleanValidMovesTilesList();
        return map.FindAITileDestination(myUnit, target);
    }

    //if we do have units, then we get the best target possible
    protected Unit GetBestTarget(Unit myUnit, List<Unit> enemyUnits)
    {

        string combatType;
        if (myUnit.isRanged())
            combatType = "Ranged";
        else
            combatType = "Melee";

        Unit currentTarget = null;
        int highestDamage = 0;

        foreach (Unit enemyUnit in enemyUnits)
        {
            int test1 = game.combat.CalculateDamage(myUnit, enemyUnit, combatType);
            int test2 = game.combat.CalculateDamage(myUnit, enemyUnit, combatType);
            int test3 = game.combat.CalculateDamage(myUnit, enemyUnit, combatType);
            int avg = (test1 + test2 + test3) / 3;
            if (avg > highestDamage)
            {
                highestDamage = avg;
                currentTarget = enemyUnit;
            }
        }
        return currentTarget;
    }



    public void selectUnit(GameObject unit)
    {
        selectedUnit = unit;
    }

    public void DeselectUnit()
    {
        selectedUnit = null;
    }

	public bool isActive()
    {
        return active;
    }

    public void setActive(bool b)
    {
        active = true;
    }

    public bool canAIClick()
    {
        return canClick;
    }

    public void setCanClick(bool b)
    {
        canClick = b;
    }

    private void setCanClickTrue()
    {
        canClick = true;
    }

    public void InvokeSetCanClick(float time)
    {
        Invoke("setCanClickTrue", time);
    }
}
