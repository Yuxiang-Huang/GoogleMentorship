using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEditor;

public class Building : MonoBehaviourPunCallbacks, IUnit
{
    public PhotonView PV { get; set; }

    public int health { get; set; }

    public int fullHealth;
    [SerializeField]  int damage;

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
        tile = TileManager.instance.tiles[startingtTileX, startingtTileY];
        tile.updateStatus(ownerID, this);
    }

    //can spawn troop around building
    public void updateCanSpawn()
    {
        foreach (Tile neighbor in tile.neighbors)
        {
            GameManager.instance.allPlayers[ownerID].canSpawn[neighbor.pos.x, neighbor.pos.y] = true;

            GameManager.instance.allPlayers[ownerID].canSpawnDirection[neighbor.pos.x, neighbor.pos.y] =
                TileManager.instance.getWorldPosition(neighbor) - TileManager.instance.getWorldPosition(tile);
        }
    }

    [PunRPC]
    public void takeDamage(int incomingDamage)
    {
        health -= incomingDamage;
    }
}