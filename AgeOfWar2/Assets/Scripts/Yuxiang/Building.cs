using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UI.GridLayoutGroup;

public class Building : MonoBehaviourPunCallbacks, IUnit
{
    public PhotonView PV;

    public int ownerID { get; set; }

    public Tile tile;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void Init(int playerID, int startingtTileX, int startingtTileY)
    {
        ownerID = playerID;
        tile = TileManager.instance.getTile(new Vector2(startingtTileX, startingtTileY));
        tile.updateStatus(ownerID, this);
    }

    //can spawn troop around building
    public void updateCanSpawn()
    {
        foreach (Tile neighbor in tile.neighbors)
        {
            GameManager.instance.allPlayers[ownerID].canSpawn[neighbor.pos.x, neighbor.pos.y] = true;
        }
    }
}
