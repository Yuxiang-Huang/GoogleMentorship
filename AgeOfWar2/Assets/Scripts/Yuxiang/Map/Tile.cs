using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Info")]
    public Vector2Int pos;

    public List<Tile> neighbors;
    public List<Tile> neighbors2;

    public IUnit unit;

    public string terrain;

    public int ownerID = -1;

    [Header("Highlight")]
    [SerializeField] GameObject highlightTile;

    public GameObject dark;
    public GameObject gray;

    [SerializeField] List<Color> ownerColor;

    [SerializeField] List<GameObject> borders;

    private void Awake()
    {
        //no one own this land in the beginning
        foreach (GameObject border in borders)
        {
            border.SetActive(false);
        }

        //covered in the beginning
        dark.SetActive(true);
    }

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }

    public void setGray(bool status)
    {
        gray.SetActive(status);
    }

    public override string ToString()
    {
        return pos.ToString();
    }

    public void updateStatus(int newOwnerID, IUnit newUnit)
    {
        if (ownerID != newOwnerID)
        {
            //remove from other player's territory
            if (ownerID != -1)
            {
                GameManager.instance.allPlayersOriginal[ownerID].territory.Remove(this);

                //update dark if I was the owner
                if (ownerID == PlayerController.instance.id)
                {
                    foreach (Tile tile in neighbors)
                    {
                        tile.updateDark();
                    }

                    updateDark();
                }
            }

            //add this land to new owner's territory
            ownerID = newOwnerID;
            GameManager.instance.allPlayersOriginal[ownerID].territory.Add(this);

            //territory color
            setTerritoryColor();
        }

        //reveal land only if mine
        if (ownerID == PlayerController.instance.id)
        {
            dark.SetActive(false);

            foreach (Tile tile in neighbors)
            {
                tile.setDark(false);
            }
        }

        this.unit = newUnit;
    }

    public void setDark(bool status)
    {
        dark.SetActive(status);
    }

    public void setTerritoryColor()
    {
        PlayerController owner = GameManager.instance.allPlayersOriginal[ownerID];

        for (int i = 0; i < borders.Count; i++)
        {
            borders[i].SetActive(false);
            borders[i].GetComponent<SpriteRenderer>().color = ownerColor[ownerID];

            //border disapper when two territories are adjacent
            if (! owner.territory.Contains(neighbors[i]))
            {
                borders[i].SetActive(true);
            }
        }
    }

    #region Run locally

    void updateDark()
    {
        //include in territory already
        if (PlayerController.instance.territory.Contains(this))
        {
            dark.SetActive(false);
            return;
        }

        //check if bound by other territory
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

    public void updateCanSpawn()
    {
        foreach (Tile neighbor in neighbors)
        {
            //if any neighbor has my team's building
            if (neighbor.unit != null && neighbor.unit.gameObject.CompareTag("Building") && neighbor.unit.ownerID == ownerID)
            {
                return;
            }
        }

        //can't be spawn anymore
        PlayerController.instance.canSpawn[pos.x, pos.y] = false;
    }

    public void updateVisibility()
    {
        //check myself too
        if (PlayerController.instance.territory.Contains(this) ||
            (unit != null && unit.ownerID == PlayerController.instance.id)) return;

        foreach (Tile neighbor in neighbors)
        {
            //if any neighbor is my team's territory or there is a my team's water unit on it
            if (PlayerController.instance.territory.Contains(neighbor) ||
                (neighbor.unit != null && neighbor.unit.ownerID == PlayerController.instance.id))
            {
                return;
            }
        }

        //can't be seem anymore
        dark.SetActive(true);
    }

    public void reset()
    {
        foreach(GameObject border in borders)
        {
            border.SetActive(false);
        }

        ownerID = -1;
    }

    #endregion
}
