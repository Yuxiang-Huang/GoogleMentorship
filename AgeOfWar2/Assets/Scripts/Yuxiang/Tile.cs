using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int row, col;

    public List<Tile> neighbors;

    [SerializeField] GameObject highlightTile;

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }
}
