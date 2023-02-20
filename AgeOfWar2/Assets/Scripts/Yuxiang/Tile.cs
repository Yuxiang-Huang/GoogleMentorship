using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int pos;

    public List<Tile> neighbors;

    public GameObject unit;

    public string terrain;

    public PlayerController owner;

    [SerializeField] GameObject highlightTile;

    [SerializeField] GameObject dark;

    [SerializeField] List<GameObject> ownerColor;

    GameObject lastColor;

    private void Awake()
    {
        //no one own this land in the beginning and is dark
        foreach (GameObject highlight in ownerColor)
        {
            highlight.SetActive(false);
        }

        dark.SetActive(true);
    }

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }

    public override string ToString()
    {
        return pos.ToString();
    }

    public void updateStatus(PlayerController player, GameObject newUnit)
    {
        if (owner != player)
        {
            //remove from other player's land

            //add this land to the player's territory
            this.owner = player;
            owner.territory.Add(this);
        }
        this.unit = newUnit;

        //highlight if land
        if (terrain == "land")
        {
            Debug.Log(owner.id);

            lastColor = ownerColor[owner.id];

            lastColor.SetActive(true);
        }
        else
        {
            lastColor = null;
        }

        //reveal land only if mine
        if (owner == PlayerController.instance)
        {
            dark.SetActive(false);

            foreach (Tile tile in neighbors)
            {
                tile.removeDark();
            }
        }
    }

    public void removeDark()
    {
        dark.SetActive(false);
    }
}
