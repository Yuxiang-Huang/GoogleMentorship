using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int pos;

    public List<Tile> neighbors;

    [SerializeField] GameObject highlightTile;

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }

    public override string ToString()
    {
        return pos.ToString();
    }
}
