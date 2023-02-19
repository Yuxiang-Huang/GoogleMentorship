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

    List<Troop> allTroops = new List<Troop>();

    public bool[,] canSee;

    public List<Tile> territory = new List<Tile>();

    [SerializeField] GameObject castle;

    public GameObject myCastle;

    [Header("Spawn")]
    public GameObject toSpawn;
    public int goldNeedToSpawn;

    [Header("Gold")]
    [SerializeField]  int gold;
    [SerializeField] TextMeshProUGUI goldText;

    private void Start()
    {
        instance = this;

        TileManager.instance.makeGrid(10, 10);

        //canSee = new bool[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];

        myCastle = Instantiate(castle, new Vector3(0, 0, 0), Quaternion.identity);

        TileManager.instance.tiles[0, 0].GetComponent<Tile>().unit = myCastle;

        territory.Add(TileManager.instance.tiles[0, 0]);
        TileManager.instance.tiles[0, 0].owner = this;
        TileManager.instance.tiles[0, 0].updateHighlight();
    }

    // Update is called once per frame
    void Update()
    {
        //update gold
        goldText.text = "Gold: " + gold;

        //highlight
        Tile newHilighted = TileManager.instance.getTile();

        if (highlighted != newHilighted)
        {
            if (highlighted != null)
                highlighted.highlight(false);

            highlighted = newHilighted;

            if (newHilighted != null)
                highlighted.highlight(true);
        }

        //move
        if (mode == "move")
        {
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
            if (Input.GetMouseButtonDown(0))
            {
                if (highlighted != null && highlighted.unit == null)
                {
                    //spawn unit and relation tile and unit
                    GameObject newUnit = Instantiate(toSpawn,
                    highlighted.gameObject.transform.position, Quaternion.identity);

                    highlighted.GetComponent<Tile>().unit = newUnit;
                    newUnit.GetComponent<Troop>().tile = highlighted;

                    //owner
                    highlighted.owner = this;
                    territory.Add(highlighted);
                    highlighted.updateHighlight();
                    newUnit.GetComponent<Troop>().owner = this;   

                    allTroops.Add(newUnit.GetComponent<Troop>());

                    gold -= goldNeedToSpawn;
                }

                mode = "move";
            }
        }
    }
}