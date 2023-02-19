using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Tile tile;

    // Start is called before the first frame update
    public void Init(Tile startingTile, bool[,] canSpawn)
    {
        tile = startingTile;

        foreach (Tile neighbor in tile.neighbors)
        {
            canSpawn[neighbor.pos.x, neighbor.pos.y] = true;
        }
    }
}
