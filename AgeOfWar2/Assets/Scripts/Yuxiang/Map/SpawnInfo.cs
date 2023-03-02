using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInfo
{
    public Tile spawnTile;

    public string unitName;

    public GameObject spawnImage;

    public SpawnInfo(Tile spawnTile, string unitName, GameObject spawnImage)
    {
        this.spawnTile = spawnTile;
        this.unitName = unitName;
        this.spawnImage = spawnImage;
    }
}
