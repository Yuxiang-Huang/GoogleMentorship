using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInfo
{
    public Tile spawnTile;

    public string unitName;

    public int turn;

    public SpawnInfo(Tile spawnTile, string unitName, int turn)
    {
        this.spawnTile = spawnTile;
        this.unitName = unitName;
        this.turn = turn;
    }
}
