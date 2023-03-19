using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;
using UnityEngine;

public class Speed : Troop
{
    public override void attack()
    {
        SortedDictionary<float, Tile> targets = new SortedDictionary<float, Tile>();

        //check all surrounding tiles
        foreach (Tile curTile in tile.neighbors)
        {
            //if can see this tile and there is enemy unit on it
            if (!curTile.dark.activeSelf && curTile.unit != null && curTile.unit.ownerID != ownerID)
            {
                //attack order depending on dot product
                targets.TryAdd(Vector2.Dot(direction,
                    TileManager.instance.getWorldPosition(curTile) - TileManager.instance.getWorldPosition(tile)),
                    curTile);
            }
        }

        //attack all enemies around whose health is more than 0
        while (targets.Count > 0)
        {
            if (targets.Values.Last().unit.health > 0)
            {
                targets.Values.Last().unit.PV.RPC(nameof(takeDamage), RpcTarget.AllViaServer, damage);
            }
            targets.Remove(targets.Keys.Last());
        }
    }

    public override void move()
    {
        base.move();
        base.move();
    }
}
