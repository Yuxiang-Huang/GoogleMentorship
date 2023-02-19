using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    int id = 1;

    Tile highlighted;

    Troop playerSelected;

    public string mode;

    List<Troop> allTroops = new List<Troop>();

    [Header("Spawn")]
    public GameObject toSpawn;
    public int goldNeedToSpawn;

    [Header("Gold")]
    [SerializeField] int gold;
    [SerializeField] TextMeshProUGUI goldText;

    private void Start()
    {
        SpawnManager.instance.player = this;
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
                    GameObject newUnit = Instantiate(toSpawn,
                    highlighted.gameObject.transform.position, Quaternion.identity);

                    highlighted.GetComponent<Tile>().unit = newUnit;
                    newUnit.GetComponent<Troop>().tile = highlighted;

                    newUnit.GetComponent<Troop>().owner = id;

                    allTroops.Add(newUnit.GetComponent<Troop>());

                    gold -= goldNeedToSpawn;
                }

                mode = "move";
            }
        }
    }
}