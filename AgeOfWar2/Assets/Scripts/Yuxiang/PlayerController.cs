using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public int id = 1;

    Tile highlighted;

    Troop playerSelected;

    public string mode;

    public List<Troop> allTroops = new List<Troop>();
    public List<Tile> territory = new List<Tile>();

    [SerializeField] GameObject castle;
    public GameObject myCastle;

    [Header("Spawn")]
    public bool[,] canSpawn;
    public GameObject toSpawn;
    public int goldNeedToSpawn;

    [Header("Gold")]
    [SerializeField] int gold;
    [SerializeField] TextMeshProUGUI goldText;

    private void Start()
    {
        instance = this;

        TileManager.instance.makeGrid(10, 10);

        canSpawn = new bool[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];

        //spawn castle
        myCastle = Instantiate(castle, new Vector3(0, 0, 0), Quaternion.identity);
        castle.GetComponent<Building>().Init(TileManager.instance.tiles[0, 0], canSpawn);
        TileManager.instance.tiles[0, 0].updateStatus(this, myCastle);
    }

    // Update is called once per frame
    void Update()
    {
        //update gold
        goldText.text = "Gold: " + gold;

        //move
        if (mode == "move")
        {
            //highlight
            Tile newHighlighted = TileManager.instance.getTile();

            if (highlighted != newHighlighted)
            {
                if (highlighted != null)
                    highlighted.highlight(false);

                highlighted = newHighlighted;

                if (newHighlighted != null)
                    highlighted.highlight(true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                //select player
                if (playerSelected == null)
                {
                    if (highlighted != null && highlighted.GetComponent<Tile>().unit != null)
                    {
                        playerSelected = highlighted.GetComponent<Tile>().unit.GetComponent<Troop>();
                        playerSelected.highlight(true);
                    }
                }
                //findPath
                else
                {
                    if (highlighted != null)
                    {
                        playerSelected.findPath(highlighted.GetComponent<Tile>());
                        playerSelected.highlight(false);
                        playerSelected = null;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                gold += territory.Count;

                foreach (Troop troop in allTroops)
                {
                    troop.move();
                }
            }
        }
        else if (mode == "spawn")
        {
            //highlight
            Tile newHighlighted = TileManager.instance.getTile();

            if (highlighted != newHighlighted)
            {
                if (highlighted != null) 
                    highlighted.highlight(false);

                highlighted = newHighlighted;

                if (highlighted != null)
                {
                    //can only spawn on spawnable tiles and no unit and terrain is not water
                    if (canSpawn[highlighted.pos.x, highlighted.pos.y] && highlighted.unit == null
                        && highlighted.terrain != "water")
                    {
                        highlighted.highlight(true);
                    }
                    else
                    {
                        highlighted = null;
                    }
                }
            }

            //click to spawn
            if (Input.GetMouseButtonDown(0))
            {
                if (highlighted != null)
                {
                    gold -= goldNeedToSpawn;

                    //spawn unit and relation tile and unit
                    GameObject newUnit = Instantiate(toSpawn,
                    highlighted.gameObject.transform.position, Quaternion.identity);

                    highlighted.updateStatus(this, newUnit);

                    if (newUnit.GetComponent<Troop>() != null)
                    {
                        newUnit.GetComponent<Troop>().Init(this, highlighted);
                    }

                    //building code here 
                }

                mode = "move";
            }
        }
    }

    public void nextTurn()
    {
        gold += territory.Count;

        foreach (Troop troop in allTroops)
        {
            troop.move();
        }
    }
}