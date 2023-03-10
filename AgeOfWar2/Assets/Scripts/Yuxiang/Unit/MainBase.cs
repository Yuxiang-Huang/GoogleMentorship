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

    ////don't remove yet
    //public override void sell()
    //{
    //    PlayerController.instance.gold += sellGold;
    //    UIManager.instance.updateGoldText();

    //    PV.RPC(nameof(kill), RpcTarget.All);
    //}

    [PunRPC]
    public override void checkDeath()
    {
        if (health <= 0)
        {
            //prevent infinite loop
            //if (ownerID == PlayerController.instance.id)
            //    PlayerController.instance.allBuildings.Remove(this);

            tile.unit = null;

            foreach (Tile neighbor in tile.neighbors)
            {
                neighbor.updateCanSpawn();
            }

            Destroy(healthbar.gameObject);
            Destroy(this.gameObject);

            //end game for owner
            if (PlayerController.instance.id == ownerID)
                PlayerController.instance.end();
        }
    }
}
