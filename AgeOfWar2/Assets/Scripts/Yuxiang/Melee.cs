using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class Melee : Troop
{
    public override void attack()
    {
        SortedDictionary<float, Tile> targets = new SortedDictionary<float, Tile>();

        //check all surrounding tiles
        foreach (Tile curTile in tile.neighbors)
        {
            //if can see this tile and there is enemy unit on it
            if (!curTile.dark.activeSelf && curTile.unit != null) //&& curTile.unit.ownerID != ownerID)
            {
                targets.TryAdd(Vector2.Dot(direction,
                    TileManager.instance.getWorldPosition(curTile) - TileManager.instance.getWorldPosition(tile)),
                    curTile);
            }
        }

        //attack order depending on dot product
        if (targets.Count != 0)
        {
            targets.Values.Last().unit.PV.RPC(nameof(takeDamage), RpcTarget.AllViaServer, damage);
        }
    }
}
