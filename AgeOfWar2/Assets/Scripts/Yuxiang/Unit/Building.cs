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

    public int sellGold;

    public SpriteRenderer imageRenderer;

    [SerializeField] List<GameObject> unitImages;

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
    public void Init(int playerID, int startingtTileX, int startingtTileY,
        string path, int age, int sellGold)
    {
        ownerID = playerID;
        tile = TileManager.instance.tiles[startingtTileX, startingtTileY];
        tile.updateStatus(ownerID, this);

        this.sellGold = sellGold;

        //modify images
        foreach (GameObject cur in unitImages)
        {
            cur.SetActive(false);
        }
        unitImages[age].SetActive(true);
        imageRenderer = unitImages[age].GetComponent<SpriteRenderer>();
        //imageRenderer.color = UIManager.instance.playerColors[playerID];

        //modify health according to age
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

        //main base
        if (sellGold < 0)
        {
            sellText.text = "Quit";
        }
    }

    public void fillInfoTabSpawn(TextMeshProUGUI nameText, TextMeshProUGUI healthText,
        TextMeshProUGUI damageText, TextMeshProUGUI sellText, int age)
    {
        string unitName = ToString();
        nameText.text = unitName.Substring(0, unitName.IndexOf("("));
        healthText.text = "Full Health: " + fullHealth * (int)Mathf.Pow(ageFactor, PlayerController.instance.age);
        damageText.text = "Damage: n/a";
        sellText.text = "Despawn";
    }

    public void setImage(Color color)
    {
        imageRenderer.color = color;
    }

    #endregion

    #region Damage

    [PunRPC]
    public void ageUpdateInfo(int playerAge)
    {
        //health double when age increase
        fullHealth *= ageFactor;
        health *= ageFactor;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;

        //update sell gold
        sellGold *= GameManager.instance.ageCostFactor;
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
        UIManager.instance.updateGoldText();

        PlayerController.instance.allBuildings.Remove(this);

        if (this.gameObject.GetComponent<MainBase>() != null)
        {
            PlayerController.instance.end();
        }

        PV.RPC(nameof(kill), RpcTarget.All);
    }

    [PunRPC]
    public void kill()
    {
        health = 0;
        checkDeath();
    }

    #endregion
}
