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

    public override string ToString()
    {
        return pos.ToString();
    }

    #region Three Overlays

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }

    public void setGray(bool status)
    {
        gray.SetActive(status);
    }

    public void setDark(bool status)
    {
        dark.SetActive(status);
    }

    #endregion

    public void updateStatus(int newOwnerID, IUnit newUnit)
    {
        if (ownerID != newOwnerID)
        {
            //remove from other player's territory
            if (ownerID != -1)
            {
                PlayerController prevOwner = GameManager.instance.allPlayersOriginal[ownerID];

                prevOwner.territory.Remove(this);

                if (terrain == "land")
                    prevOwner.landTerritory--;

                //update territory color
                foreach (Tile neighbor in neighbors)
                {
                    //show neighbor border
                    if (prevOwner.territory.Contains(neighbor))
                    {
                        int index = pos.x % 2 == 0 ?
                        TileManager.instance.neighborIndexEvenRow[pos - neighbor.pos] :
                        TileManager.instance.neighborIndexOddRow[pos - neighbor.pos];

                        neighbor.borders[index].SetActive(true);
                    }
                }

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

            if (terrain == "land")
                GameManager.instance.allPlayersOriginal[ownerID].landTerritory++;

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

    public void setTerritoryColor()
    {
        PlayerController owner = GameManager.instance.allPlayersOriginal[ownerID];

        foreach (GameObject border in borders)
        {
            border.SetActive(false);
            border.GetComponent<SpriteRenderer>().color = ownerColor[ownerID];
        }

        foreach (Tile neighbor in neighbors)
        {
            //border disapper when two territories are adjacent
            if (! owner.territory.Contains(neighbor))
            {
                //find correct border
                int index = pos.x % 2 == 0 ?
                    TileManager.instance.neighborIndexEvenRow[neighbor.pos - pos] :
                    TileManager.instance.neighborIndexOddRow[neighbor.pos - pos];

                borders[index].SetActive(true);
            }
            else
            {
                int index = neighbor.pos.x % 2 == 0 ?
                    TileManager.instance.neighborIndexEvenRow[pos - neighbor.pos] :
                    TileManager.instance.neighborIndexOddRow[pos - neighbor.pos];

                neighbor.borders[index].SetActive(false);
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
