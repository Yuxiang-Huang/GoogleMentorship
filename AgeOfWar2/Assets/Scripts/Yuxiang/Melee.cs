using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Melee : Troop
{
    public override bool attack()
    {
        SortedDictionary<float, Tile> targets = new SortedDictionary<float, Tile>();

        //check all surrounding tiles
        foreach (Tile tile in tile.neighbors)
        {
            //if can see this tile and there is enemy unit on it
            if (!tile.dark.activeSelf && tile.unit != null && tile.unit.ownerID != ownerID)
            {
                tile.unit.PV.RPC(nameof(takeDamage), RpcTarget.AllBuffered, damage);
            }
        }

        return false;
    }
}
