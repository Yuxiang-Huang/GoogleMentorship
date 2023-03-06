using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Troop : MonoBehaviourPunCallbacks, IUnit
{
    public PhotonView PV { get; set; }

    public int ownerID { get; set; }

    private int ageFactor;

    public SpriteRenderer imageRenderer;

    public int sellGold;

    [Header("Health")]
    public Slider healthbar;
    public int health { get; set; }
    public int fullHealth;
    public int damage;
    public Vector2 direction;
    Vector3 offset = new Vector3(0, 0.5f, 0);

    [Header("Movement")]
    public Tile tile;
    Tile lastTarget;
    public List<Tile> path;
    [SerializeField] GameObject arrow;
    [SerializeField] GameObject arrowPrefab;

    public bool moved;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        ageFactor = GameManager.instance.ageUnitFactor;
    }

    [PunRPC]
    public void Init(int playerID, int startingtTileX, int startingtTileY, Vector2 startDirection, int age, int sellGold)
    {
        //setting tile, ID, direction, sell gold
        ownerID = playerID;

        tile = TileManager.instance.tiles[startingtTileX, startingtTileY];
        tile.updateStatus(ownerID, this);

        direction = startDirection;

        this.sellGold = sellGold;

        //modify according to age
        fullHealth *= (int) Mathf.Pow(ageFactor, age);
        damage *= (int) Mathf.Pow(ageFactor, age);

        //health
        health = fullHealth;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;
        healthbar.gameObject.transform.SetParent(UIManager.instance.healthbarCanvas.gameObject.transform);
        healthbar.gameObject.transform.position = transform.position + offset;

        healthbar.gameObject.SetActive(false);
    }

    #region Movement

    public virtual void attack() { }

    public void findPath(Tile target)
    {
        if (lastTarget == target) return; //same path

        //same tile reset
        if (target == tile)
        {
            path = new List<Tile>();

            Destroy(arrow);

            lastTarget = null;

            return;
        }

        //otherwise find new path
        lastTarget = target;

        float minDist = dist(target, tile);

        //initiated a queue
        Queue<List<Tile>> allPath = new Queue<List<Tile>>();

        List<Tile> root = new List<Tile>();
        root.Add(tile);

        allPath.Enqueue(root);


        bool[,] visited = new bool[TileManager.instance.tiles.GetLength(0),
                                   TileManager.instance.tiles.GetLength(1)];

        bool reach = false;

        //bfs
        while (allPath.Count != 0 && !reach)
        {
            List<Tile> cur = allPath.Dequeue();
            Tile lastTile = cur[cur.Count - 1];

            foreach (Tile curTile in lastTile.neighbors)
            {
                //not visited and land tile 
                if (!visited[curTile.pos.x, curTile.pos.y] && curTile.terrain == "land")
                {
                    //no team building
                    if (curTile.unit == null || !curTile.unit.gameObject.CompareTag("Building") ||
                        curTile.unit.ownerID != ownerID)
                    { 
                        visited[curTile.pos.x, curTile.pos.y] = true;

                        //check this tile dist
                        List<Tile> dup = new List<Tile>(cur);
                        dup.Add(curTile);

                        float curDist = dist(target, curTile);

                        if (curDist < 0.01)
                        {
                            reach = true;
                            path = dup;
                            minDist = curDist;
                        }
                        else if (curDist < minDist)
                        {
                            minDist = curDist;
                            path = dup;
                        }

                        allPath.Enqueue(dup);
                    }
                }
            }
        }

        //a path is found
        if (path.Count != 0)
        {
            //remove first tile
            path.RemoveAt(0);

            if (path.Count != 0)
            {
                //display arrow
                if (arrow != null)
                {
                    Destroy(arrow);
                }

                arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

                Vector2 arrowDirection = TileManager.instance.getWorldPosition(path[0]) - TileManager.instance.getWorldPosition(tile);

                float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x);

                arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
            }
        }
    }

    public void move()
    {
        //moved in this turn already
        if (moved) return;

        moved = true;

        //destroy arrow
        if (arrow != null)
        {
            Destroy(arrow);
        }

        //move and then attack
        if (path.Count != 0)
        {
            //update direction
            direction = TileManager.instance.getWorldPosition(path[0]) - TileManager.instance.getWorldPosition(tile);

            //move to next tile on list if no unit is there
            if (path[0].unit == null)
            {
                PV.RPC(nameof(removeTileUnit), RpcTarget.All);
                PV.RPC(nameof(moveUpdate_RPC), RpcTarget.All, path[0].pos.x, path[0].pos.y);

                path.RemoveAt(0);
            }
            //ask it to move first
            else if (path[0].unit.gameObject.CompareTag("Troop"))
            {
                //leave space
                PV.RPC(nameof(removeTileUnit), RpcTarget.All);

                path[0].unit.gameObject.GetComponent<Troop>().move();

                //try to move again
                if (path[0].unit == null)
                {
                    PV.RPC(nameof(moveUpdate_RPC), RpcTarget.All, path[0].pos.x, path[0].pos.y);

                    path.RemoveAt(0);
                }
                else
                {
                    //reverse leave space
                    PV.RPC(nameof(updateTileUnit), RpcTarget.All);
                }
            }

            //display arrow
            if (path.Count != 0)
            {
                arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

                Vector2 arrowDirection = TileManager.instance.getWorldPosition(path[0]) - TileManager.instance.getWorldPosition(tile);

                float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x);

                arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
            }
        }
    }

    [PunRPC]
    public void moveUpdate_RPC(int nextTileX, int nextTileY)
    {
        //update tile
        tile = TileManager.instance.tiles[nextTileX, nextTileY];
        tile.updateStatus(ownerID, this);

        //update position
        transform.position = TileManager.instance.getWorldPosition(tile);

        healthbar.gameObject.transform.position = transform.position + offset;
    }

    [PunRPC]
    public void updateTileUnit()
    {
        tile.unit = this;
    }

    [PunRPC]
    public void removeTileUnit()
    {
        tile.unit = null;
    }

    #endregion

    #region Damage

    [PunRPC]
    public void takeDamage(int incomingDamage)
    {
        health -= incomingDamage;
        healthbar.value = health;
    }

    public void setHealthBar(bool status)
    {
        healthbar.gameObject.SetActive(status);
    }

    [PunRPC]
    public void checkDeath()
    {
        if (health <= 0)
        {
            tile.unit = null;
            Destroy(arrow);
            Destroy(healthbar.gameObject);
            Destroy(this.gameObject);
        }
    }

    #endregion

    #region UI

    [PunRPC]
    public void ageUpdateInfo(int playerAge)
    {
        //health double when age increase
        fullHealth *= ageFactor;
        health *= ageFactor;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;

        damage *= ageFactor;

        //update sell gold
        sellGold *= GameManager.instance.ageCostFactor;
    }

    public void setImage(Color color)
    {
        imageRenderer.color = color;
    }

    public void fillInfoTab(TextMeshProUGUI nameText, TextMeshProUGUI healthText,
    TextMeshProUGUI damageText, TextMeshProUGUI sellText)
    {
        string unitName = ToString();
        nameText.text = unitName.Substring(0, unitName.IndexOf("("));
        healthText.text = "Health: " + health + " / " + fullHealth;
        damageText.text = "Damage: " + damage;
        sellText.text = "Sell: " + sellGold + " Gold";
    }

    public void fillInfoTabSpawn(TextMeshProUGUI nameText, TextMeshProUGUI healthText,
        TextMeshProUGUI damageText, TextMeshProUGUI sellText, int age)
    {
        string unitName = ToString();
        nameText.text = unitName.Substring(0, unitName.IndexOf("("));
        healthText.text = "Full Health: " + fullHealth * (int)Mathf.Pow(ageFactor, age);
        damageText.text = "Damage: " + damage * (int)Mathf.Pow(ageFactor, age);
        sellText.text = "Despawn";
    }

    public void sell()
    {
        PlayerController.instance.gold += sellGold;
        UIManager.instance.updateGoldText();

        PlayerController.instance.allTroops.Remove(this);

        tile.unit = null;

        Destroy(arrow);

        Destroy(healthbar);

        Destroy(gameObject);

        PlayerController.instance.mode = "select";
    }

    #endregion

    //find distance between two tiles
    float dist(Tile t1, Tile t2)
    {
        Vector2 p1 = TileManager.instance.getWorldPosition(t1);
        Vector2 p2 = TileManager.instance.getWorldPosition(t2);
        return Mathf.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y));
    }
}
