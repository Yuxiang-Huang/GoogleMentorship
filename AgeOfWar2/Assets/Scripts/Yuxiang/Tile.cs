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
            //remove from other player's land
            if (owner != null)
            {
                owner.territory.Remove(this);

                //update dark if mine
                if (owner == PlayerController.instance)
                {
                    foreach (Tile tile in neighbors)
                    {
                        tile.updateDark();
                    }

                    updateDark();
                }
            }

            //add this land to new owner's territory
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
                tile.setDark(false);
            }
        }
    }

    void setDark(bool status)
    {
        dark.SetActive(status);
    }

    void updateDark()
    {
        //include in territory already
        if (PlayerController.instance.territory.Contains(this))
        {
            dark.SetActive(false);
            return;
        }

        //bound by other territory
        bool hidden = true;

        foreach (Tile tile in neighbors)
        {
            if (PlayerController.instance.territory.Contains(tile))
            {
                hidden = false;
            }
        }

        dark.SetActive(hidden);
    }
}
