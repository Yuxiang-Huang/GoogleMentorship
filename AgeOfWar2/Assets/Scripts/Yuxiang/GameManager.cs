using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject clubMan;

    Tile highlighted;

    ITroop playerSelected;

    string mode;

    List<ITroop> allTroops = new List<ITroop>();

    // Update is called once per frame
    void Update()
    {
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
                        playerSelected = highlighted.GetComponent<Tile>().unit.GetComponent<ITroop>();
                    }
                }
                //findPath
                else
                {
                    if (highlighted != null)
                    {
                        playerSelected.findPath(highlighted.GetComponent<Tile>());
                        playerSelected = null;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (ITroop troop in allTroops)
                {
                    troop.move();
                }
            }
        }
        else if (mode == "spawn")
        {
            if (highlighted != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (highlighted.unit == null)
                    {
                        GameObject newUnit = Instantiate(clubMan,
                        highlighted.gameObject.transform.position, Quaternion.identity);

                        highlighted.GetComponent<Tile>().unit = newUnit;
                        newUnit.GetComponent<ITroop>().tile = highlighted;

                        allTroops.Add(newUnit.GetComponent<ITroop>());

                        mode = "move";
                    }
                }
            }
        }
    }

    public void spawn()
    {
        if (mode == "spawn")
        {
            mode = "move";
        }
        else
        {
            mode = "spawn";
        }
    }
}
