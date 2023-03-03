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

    Vector3 offset = new Vector3(0, 0.5f, 0);

    [SerializeField] int ageFactor = 4;

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
        fullHealth *= (int)Mathf.Pow(ageFactor, age);

        //health
        health = fullHealth;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;
        healthbar.gameObject.transform.SetParent(UIManager.instance.healthbarCanvas.gameObject.transform);
        healthbar.gameObject.transform.position = transform.position + offset;

        healthbar.gameObject.SetActive(false);
    }

    //can spawn troop on land tiles around building
    public void updateCanSpawn()
    {
        foreach (Tile neighbor in tile.neighbors)
        {
            if (neighbor.terrain == "land")
            {
                PlayerController.instance.canSpawn[neighbor.pos.x, neighbor.pos.y] = true;

                PlayerController.instance.spawnDirection[neighbor.pos.x, neighbor.pos.y] =
                    TileManager.instance.getWorldPosition(neighbor) - TileManager.instance.getWorldPosition(tile);
            }
        }
    }

    [PunRPC]
    public void updateHealth()
    {
        //health double when age increase
        fullHealth *= ageFactor;
        health *= ageFactor;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;
    }

    public void setHealthBar(bool status)
    {
        healthbar.gameObject.SetActive(status);
    }

    [PunRPC]
    public void takeDamage(int incomingDamage)
    {
        health -= incomingDamage;
        healthbar.value = health;
    }

    [PunRPC]
    public void checkDeath()
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
        }
    }
}
