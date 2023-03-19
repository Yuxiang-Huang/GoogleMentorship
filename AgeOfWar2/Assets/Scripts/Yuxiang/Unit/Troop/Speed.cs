using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;
using UnityEngine;

public class Speed : Troop
{
    public override void attack()
    {
        List<Tile> targets = new List<Tile>();

        //attack all enemies around whose health is more than 0
        foreach (Tile curTile in tile.neighbors)
        {
            //if can see this tile and there is enemy unit on it
            if (!curTile.dark.activeSelf && curTile.unit != null && curTile.unit.ownerID != ownerID)
            {
                if (curTile.unit.health > 0)
                {
                    curTile.unit.PV.RPC(nameof(takeDamage), RpcTarget.AllViaServer, damage);
                }
            }
        }
    }

    public override void move()
    {
        base.move();
        base.move();
    }

    public override void displayArrow()
    {
        //destroy arrow
        if (arrow != null)
        {
            Destroy(arrow);
        }

        //show arrow if there is two tile to go
        if (path.Count > 1)
        {
            arrow = Instantiate(UIManager.instance.arrowPrefab, transform.position, Quaternion.identity);

            arrow.transform.localScale = new Vector3(2.5f, 1, 1);

            Vector2 arrowDirection = TileManager.instance.getWorldPosition(path[1]) - TileManager.instance.getWorldPosition(tile);

            float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x);

            arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
        }
        //show arrow if there is only one tile to go
        else if (path.Count > 0)
        {
            arrow = Instantiate(UIManager.instance.arrowPrefab, transform.position, Quaternion.identity);

            Vector2 arrowDirection = TileManager.instance.getWorldPosition(path[0]) - TileManager.instance.getWorldPosition(tile);

            float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x);

            arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
        }
    }
}