using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

public class Troop : MonoBehaviourPunCallbacks, IUnit
{
    public PhotonView PV { get; set; }

    public int ownerID { get; set; }

    public int health { get; set; }

    [Header("Health")]
    public int fullHealth;
    public int damage;
    public Vector2 direction;
    public Slider healthbar;

    [Header("Movement")]
    public Tile tile;

    Tile lastTarget;

    public List<Tile> path;

    [SerializeField] GameObject arrow;
    [SerializeField] GameObject arrowPrefab;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        health = fullHealth;
    }

    [PunRPC]
    public void Init(int playerID, int startingtTileX, int startingtTileY, Vector2 startDirection)
    {
        ownerID = playerID;
        tile = TileManager.instance.tiles[startingtTileX, startingtTileY];
        tile.updateStatus(ownerID, this);

        direction = startDirection;

        healthbar.maxValue = fullHealth;
    }

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
                PV.RPC(nameof(moveUpdate_RPC), RpcTarget.All, path[0].pos.x, path[0].pos.y);

                path.RemoveAt(0);
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
        //last tile
        tile.unit = null;

        //update tile
        tile = TileManager.instance.tiles[nextTileX, nextTileY];
        tile.updateStatus(ownerID, this);

        //update position
        transform.position = TileManager.instance.getWorldPosition(tile);
    }

    [PunRPC]
    public void takeDamage(int incomingDamage)
    {
        health -= incomingDamage;

        //health bar
        healthbar.value = health;
    }

    [PunRPC]
    public void checkDeath()
    {
        if (health <= 0)
        {
            tile.unit = null;
            Destroy(arrow);
            Destroy(this.gameObject);
        }
    }

    //find distance between two tiles
    float dist(Tile t1, Tile t2)
    {
        Vector2 p1 = TileManager.instance.getWorldPosition(t1);
        Vector2 p2 = TileManager.instance.getWorldPosition(t2);
        return Mathf.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y));
    }
}