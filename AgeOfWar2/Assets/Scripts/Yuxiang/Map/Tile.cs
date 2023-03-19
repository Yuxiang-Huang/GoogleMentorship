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
                        int index = neighbor.pos.x % 2 == 0 ?
                        TileManager.instance.neighborIndexEvenRow[pos - neighbor.pos] :
                        TileManager.instance.neighborIndexOddRow[pos - neighbor.pos];

                        neighbor.borders[index].SetActive(true);
                    }
                }

                //update visibility if I was the previous owner
                if (ownerID == PlayerController.instance.id)
                {
                    foreach (Tile neighbor in neighbors)
                    {
                        neighbor.updateVisibility();
                    }

                    updateVisibility();
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
            PlayerController.instance.visibleTiles.Add(this);

            foreach (Tile tile in neighbors)
            {
                tile.setDark(false);
                PlayerController.instance.visibleTiles.Add(tile);
            }
        }

        this.unit = newUnit;
    }

    public void setTerritoryColor()
    {
        PlayerController owner = GameManager.instance.allPlayersOriginal[ownerID];

        foreach (GameObject border in borders)
        {
            border.SetActive(true);
            border.GetComponent<SpriteRenderer>().color = ownerColor[ownerID];
        }

        foreach (Tile neighbor in neighbors)
        {
            //border disapper when two territories are adjacent
            if (owner.territory.Contains(neighbor))
            {
                int index = pos.x % 2 == 0 ?
                    TileManager.instance.neighborIndexEvenRow[neighbor.pos - pos] :
                    TileManager.instance.neighborIndexOddRow[neighbor.pos - pos];

                borders[index].SetActive(false);

                index = neighbor.pos.x % 2 == 0 ?
                    TileManager.instance.neighborIndexEvenRow[pos - neighbor.pos] :
                    TileManager.instance.neighborIndexOddRow[pos - neighbor.pos];

                neighbor.borders[index].SetActive(false);
            }
        }
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

    #region Two Updates

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
        //check if already territory if in extra view 
        if (PlayerController.instance.territory.Contains(this) ||
            PlayerController.instance.extraViewTiles[pos.x, pos.y] > 0) return;

        //check if bound by territory 
        foreach (Tile neighbor in neighbors)
        {
            if (PlayerController.instance.territory.Contains(neighbor))
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
