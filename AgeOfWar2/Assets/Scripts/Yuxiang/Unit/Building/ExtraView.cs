using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;
using UnityEngine;

public class ExtraView : Building
{
    [SerializeField] int damage;

    public override void effect()
    {
        SortedDictionary<float, Tile> targets = new SortedDictionary<float, Tile>();

        //check all surrounding tiles
        foreach (Tile curTile in tile.neighbors)
        {
            //if can see this tile and there is enemy unit on it
            if (!curTile.dark.activeSelf && curTile.unit != null && curTile.unit.ownerID != ownerID)
            {
                //attack order depending on distance to mainbase
                targets.TryAdd(dist(tile, curTile), curTile);
            }
        }

        foreach (Tile curTile in tile.neighbors2)
        {
            //if can see this tile and there is enemy unit on it
            if (!curTile.dark.activeSelf && curTile.unit != null && curTile.unit.ownerID != ownerID)
            {
                //attack order depending on distance to mainbase
                targets.TryAdd(dist(tile, curTile), curTile);
            }
        }

        //don't attack already dead troop
        while (targets.Count > 0 && targets.Values.Last().unit.health <= 0)
        {
            targets.Remove(targets.Keys.Last());
        }

        if (targets.Count > 0)
        {
            targets.Values.Last().unit.PV.RPC(nameof(takeDamage), RpcTarget.AllViaServer, damage);
        }
    }
}
