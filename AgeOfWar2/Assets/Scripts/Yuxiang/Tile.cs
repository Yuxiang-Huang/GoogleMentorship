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

        //reveal land
        dark.SetActive(false);

        foreach (Tile tile in neighbors)
        {
            tile.removeDark();
        }

        //highlight if land
        if (terrain == "land")
        {
            lastColor = ownerColor[owner.id - 1];

            lastColor.SetActive(true);
        }
        else
        {
            lastColor = null;
        }
    }

    public void removeDark()
    {
        dark.SetActive(false);
    }
}
