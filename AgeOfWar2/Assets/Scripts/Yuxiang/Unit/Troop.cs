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

    public int sellGold;

    public SpriteRenderer imageRenderer;

    [SerializeField] List<GameObject> unitImages;

    [Header("Health")]
    public Slider healthbar;
    public int health { get; set; }
    public int fullHealth;
    public int damage;
    public Vector2 direction;
    protected Vector3 offset = new Vector3(0, 0.5f, 0);

    [Header("Movement")]
    public Tile tile;
    protected Tile lastTarget;
    protected List<Tile> path = new List<Tile>();
    protected GameObject arrow;

    public bool moved;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public virtual void Init(int playerID, int startingtTileX, int startingtTileY, Vector2 startDirection,
        string path, int age, int sellGold)
    {
        //setting tile, ID, direction, sell gold
        ownerID = playerID;

        tile = TileManager.instance.tiles[startingtTileX, startingtTileY];
        tile.updateStatus(ownerID, this);

        direction = startDirection;

        this.sellGold = sellGold;

        //modify images
        foreach (GameObject cur in unitImages)
        {
            cur.SetActive(false);
        }
        unitImages[age].SetActive(true);
        imageRenderer = unitImages[age].GetComponent<SpriteRenderer>();
        //imageRenderer.color = UIManager.instance.playerColors[playerID];

        //modify health and damage according to age
        fullHealth *= (int) Mathf.Pow(GameManager.instance.ageUnitFactor, age);
        damage *= (int) Mathf.Pow(GameManager.instance.ageUnitFactor, age);

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

    public virtual void findPath(Tile target)
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

                arrow = Instantiate(UIManager.instance.arrowPrefab, transform.position, Quaternion.identity);

                Vector2 arrowDirection = TileManager.instance.getWorldPosition(path[0]) - TileManager.instance.getWorldPosition(tile);

                float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x);

                arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
            }
        }
    }

    public virtual void move()
    {
        //moved in this turn already
        if (moved) return;

        moved = true;

        //destroy arrow
        if (arrow != null)
        {
            Destroy(arrow);
        }

        //if has next tile to go
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
                arrow = Instantiate(UIManager.instance.arrowPrefab, transform.position, Quaternion.identity);

                Vector2 arrowDirection = TileManager.instance.getWorldPosition(path[0]) - TileManager.instance.getWorldPosition(tile);

                float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x);

                arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
            }
        }
    }

    [PunRPC]
    public virtual void moveUpdate_RPC(int nextTileX, int nextTileY)
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
    public virtual void checkDeath()
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
        fullHealth *= GameManager.instance.ageUnitFactor;
        health *= GameManager.instance.ageUnitFactor;
        healthbar.maxValue = fullHealth;
        healthbar.value = health;

        damage *= GameManager.instance.ageUnitFactor;

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
        healthText.text = "Full Health: " + fullHealth * (int)Mathf.Pow(GameManager.instance.ageUnitFactor, age);
        damageText.text = "Damage: " + damage * (int)Mathf.Pow(GameManager.instance.ageUnitFactor, age);
        sellText.text = "Despawn";
    }

    public void sell()
    {
        PlayerController.instance.gold += sellGold;
        UIManager.instance.updateGoldText();

        PlayerController.instance.allTroops.Remove(this);

        PV.RPC(nameof(kill), RpcTarget.All);

        PlayerController.instance.mode = "select";
    }

    [PunRPC]
    public virtual void kill()
    {
        health = 0;
        checkDeath();
    }

    #endregion

    //find distance between two tiles
    public float dist(Tile t1, Tile t2)
    {
        Vector2 p1 = TileManager.instance.getWorldPosition(t1);
        Vector2 p2 = TileManager.instance.getWorldPosition(t2);
        return Mathf.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y));
    }
}
