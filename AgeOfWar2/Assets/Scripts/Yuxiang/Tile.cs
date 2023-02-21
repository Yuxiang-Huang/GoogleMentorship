using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Info")]
    public Vector2Int pos;

    public List<Tile> neighbors;

    public IUnit unit;

    public string terrain;

    public int ownerID = -1;

    [Header("Highlight")]
    [SerializeField] GameObject highlightTile;

    public GameObject dark;

    GameObject lastColor;

    [SerializeField] List<GameObject> ownerColor;

    private void Awake()
    {
        //no one own this land in the beginning
        foreach (GameObject highlight in ownerColor)
        {
            highlight.SetActive(false);
        }

        //covered in the beginning
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

    public void updateStatus(int newOwnerID, IUnit newUnit)
    {
        if (ownerID != newOwnerID)
        {
            //remove from other player's land
            if (ownerID != -1)
            {
                GameManager.instance.allPlayers[ownerID].territory.Remove(this);

                //update dark if mine
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

            Debug.Log(ownerID);
            Debug.Log(GameManager.instance.allPlayers);

            GameManager.instance.allPlayers[ownerID].territory.Add(this);
        }
        this.unit = newUnit;

        //highlight if land
        if (terrain == "land")
        {
            //replace the color if different
            if (lastColor != ownerColor[ownerID])
            {
                if (lastColor != null)
                {
                    lastColor.SetActive(false);
                }

                lastColor = ownerColor[ownerID];

                lastColor.SetActive(true);
            }
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
}
