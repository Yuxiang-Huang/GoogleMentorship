using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MainBase : Building
{
    [PunRPC]
    public void updateTerritory()
    {
        foreach (Tile neigbhor in tile.neighbors)
        {
            neigbhor.updateStatus(ownerID, null);
        }
    }

    [PunRPC]
    public override void checkDeath()
    {
        if (health <= 0)
        {
            tile.unit = null;

            foreach (Tile neighbor in tile.neighbors)
            {
                neighbor.updateCanSpawn();
            }

            Destroy(healthbar.gameObject);
            Destroy(this.gameObject);

            PlayerController.instance.end();
        }
    }
}
