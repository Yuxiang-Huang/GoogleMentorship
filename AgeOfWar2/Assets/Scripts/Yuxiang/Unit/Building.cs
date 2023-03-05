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

    public int ownerID { get; set; }

    public Tile tile;

    [SerializeField] int ageFactor = 2;

    public int sellGold { get; set; }

    public SpriteRenderer imageRenderer;

    [Header("Health")]
    public Slider healthbar;
    public int health { get; set; }
    public int fullHealth;
    Vector3 offset = new Vector3(0, 0.5f, 0);

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void Init(int playerID, int startingtTileX, int startingtTileY, int age, int sellGold)
    {
        ownerID = playerID;
        tile = TileManager.instance.tiles[startingtTileX, startingtTileY];
        tile.updateStatus(ownerID, this);

        this.sellGold = sellGold;

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

    #region UI

    public void fillInfoTab(TextMeshProUGUI nameText, TextMeshProUGUI healthText,
        TextMeshProUGUI damageText, TextMeshProUGUI sellText)
    {
        string unitName = ToString();
        nameText.text = unitName.Substring(0, unitName.IndexOf("("));
        healthText.text = "Health: " + health + " / " + fullHealth;
        damageText.text = "Damage: n/a";
        sellText.text = "Sell: " + sellGold + " Gold";
    }

    public void setImage(Color color)
    {
        imageRenderer.color = color;
    }

    #endregion

    #region Damage

    [PunRPC]
    public void updateHealth()
    {
        //health double when age increase
        fullHealth *= ageFactor;
        health *= ageFactor;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;

        //update sell gold
        sellGold += sellGold / (PlayerController.instance.age - 1);
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

    public void sell()
    {
        PlayerController.instance.gold += sellGold;

        PlayerController.instance.allBuildings.Remove(this);

        tile.unit = null;

        foreach (Tile neighbor in tile.neighbors)
        {
            neighbor.updateCanSpawn();
        }

        Destroy(healthbar);

        Destroy(gameObject);
    }

    #endregion
}
