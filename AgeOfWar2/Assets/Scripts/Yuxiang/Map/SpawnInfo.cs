using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInfo
{
    public Tile spawnTile;

    public string unitName;

    public int turn;

    public GameObject spawnImage;

    public SpawnInfo(Tile spawnTile, string unitName, int turn, GameObject spawnImage)
    {
        this.spawnTile = spawnTile;
        this.unitName = unitName;
        this.turn = turn;
        this.spawnImage = spawnImage;
    }
}
