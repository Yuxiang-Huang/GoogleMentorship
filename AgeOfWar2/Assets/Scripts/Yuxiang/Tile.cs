using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int pos;

    public List<Tile> neighbors;

    public GameObject unit;

    public string terrain;

    public int owner;

    [SerializeField] GameObject highlightTile;

    [SerializeField] List<GameObject> ownerHighlight;

    GameObject lastHighlight;

    private void Awake()
    {
        //start with dark highlight
        foreach (GameObject highlight in ownerHighlight)
        {
            highlight.SetActive(false);
        }

        lastHighlight = ownerHighlight[0];
        lastHighlight.SetActive(true);
    }

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }

    public override string ToString()
    {
        return pos.ToString();
    }

    public void updateHighlight()
    {
        lastHighlight.SetActive(false);

        lastHighlight = ownerHighlight[owner];

        lastHighlight.SetActive(true);
    }
}
