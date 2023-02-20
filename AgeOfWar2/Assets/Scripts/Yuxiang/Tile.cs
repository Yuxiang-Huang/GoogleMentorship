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

    public void updateStatus(PlayerController newOwner, GameObject newUnit)
    {
        if (owner != newOwner)
        {
            if (owner != null)
            {
                //remove from other player's land
                owner.territory.Remove(this);
            }

            //add this land to the player's territory
            owner = newOwner;
            owner.territory.Add(this);
        }
        this.unit = newUnit;

        //highlight if land
        if (terrain == "land")
        {
            //replace the color if different
            if (lastColor != ownerColor[owner.id])
            {
                if (lastColor != null)
                {
                    lastColor.SetActive(false);
                }

                lastColor = ownerColor[owner.id];

                lastColor.SetActive(true);
            }
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
