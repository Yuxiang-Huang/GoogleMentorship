using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Building : MonoBehaviourPunCallbacks, IUnit
{
    public PhotonView PV { get; set; }

    public int health { get; set; }

    public Slider healthbar;

    public int fullHealth;

    public int ownerID { get; set; }

    public Tile tile;

    Vector3 offset = new Vector3(0, -0.5f, 0);

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void Init(int playerID, int startingtTileX, int startingtTileY, int age)
    {
        ownerID = playerID;
        tile = TileManager.instance.tiles[startingtTileX, startingtTileY];
        tile.updateStatus(ownerID, this);

        //modify according to age
        fullHealth *= (int)Mathf.Pow(2, age);

        //health
        health = fullHealth;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;
        healthbar.gameObject.transform.SetParent(GameManager.instance.healthbarCanvas.gameObject.transform);
        healthbar.gameObject.transform.position = transform.position + offset;

        healthbar.gameObject.SetActive(!tile.dark.activeSelf);
    }

    //can spawn troop on land tiles around building
    public void updateCanSpawn()
    {
        foreach (Tile neighbor in tile.neighbors)
        {
            if (neighbor.terrain == "land")
            {
                GameManager.instance.allPlayers[ownerID].canSpawn[neighbor.pos.x, neighbor.pos.y] = true;

                GameManager.instance.allPlayers[ownerID].spawnDirection[neighbor.pos.x, neighbor.pos.y] =
                    TileManager.instance.getWorldPosition(neighbor) - TileManager.instance.getWorldPosition(tile);
            }
        }
    }

    [PunRPC]
    public void updateVisibility()
    {
        healthbar.gameObject.SetActive(!tile.dark.activeSelf);
    }

    [PunRPC]
    public void updateHealth()
    {
        //health double when age increase
        fullHealth *= 2;
        health *= 2;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;
    }

    [PunRPC]
    public void takeDamage(int incomingDamage)
    {
        health -= incomingDamage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    public void checkDeath()
    {
        if (health <= 0)
        {
            tile.unit = null;
            Destroy(healthbar.gameObject);
            Destroy(this.gameObject);
        }
    }
}
