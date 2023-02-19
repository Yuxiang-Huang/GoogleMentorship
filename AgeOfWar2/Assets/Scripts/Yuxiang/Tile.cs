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

    [SerializeField] List<GameObject> ownerColor;

    GameObject lastColor;

    private void Awake()
    {
        //all highlight off
        foreach (GameObject highlight in ownerColor)
        {
            highlight.SetActive(false);
        }

        ownerColor[0].SetActive(true);
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
            this.owner = player;
            owner.territory.Add(this);
        }
        this.unit = newUnit;

        //highlight
        ownerColor[0].SetActive(false);

        lastColor = ownerColor[owner.id];

        lastColor.SetActive(true);

        foreach (Tile tile in neighbors)
        {
            tile.removeDark();
        }
    }

    public void removeDark()
    {
        ownerColor[0].SetActive(false);
    }
}
