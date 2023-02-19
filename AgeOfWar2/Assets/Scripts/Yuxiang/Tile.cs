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

    [SerializeField] List<GameObject> ownerHighlight;

    GameObject lastHighlight;

    private void Awake()
    {
        //all highlight off
        foreach (GameObject highlight in ownerHighlight)
        {
            highlight.SetActive(false);
        }

        ownerHighlight[0].SetActive(true);
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
        this.owner = player;
        owner.territory.Add(this);
        this.unit = newUnit;

        //highlight
        ownerHighlight[0].SetActive(false);

        lastHighlight = ownerHighlight[owner.id];

        lastHighlight.SetActive(true);

        foreach (Tile tile in neighbors)
        {
            tile.removeDark();
        }
    }

    public void removeDark()
    {
        ownerHighlight[0].SetActive(false);
    }
}
