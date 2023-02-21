using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Building : MonoBehaviourPunCallbacks, IUnit
{
    public PhotonView PV;

    [SerializeField] int health;
    public int fullHealth;
    [SerializeField]  int damage;
    [SerializeField] TextMeshProUGUI healthText; 

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

    public void takeDamage(int incomingDamage)
    {
        health -= incomingDamage;

        healthText.text = health + " / " + fullHealth;
    }
}
