using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static UnityEditor.PlayerSettings;

public class Building : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    public PlayerController owner;

    public Tile tile;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void Init(int playerID, int startingtTileX, int startingtTileY)
    {
        owner = GameManager.instance.allPlayers[playerID];
        tile = TileManager.instance.getTile(new Vector2(startingtTileX, startingtTileY));

        foreach (Tile neighbor in tile.neighbors)
        {
            owner.canSpawn[neighbor.pos.x, neighbor.pos.y] = true;
        }

        tile.updateStatus(owner, this.gameObject);
    }
}
