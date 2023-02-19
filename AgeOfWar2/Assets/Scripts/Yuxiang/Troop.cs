using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public PlayerController owner;

    public Tile tile;
    Tile lastTarget;

    public List<Tile> path;

    [SerializeField] GameObject arrow;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject highlightTile;

    public void Init(PlayerController player, Tile startingTile)
    {
        owner = player;
        tile = startingTile;
        player.allTroops.Add(this);
    }

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }

    public void attack()
    {

    }

    public void findPath(Tile target)
    {
        if (lastTarget == target || target == tile) return; //same path or same tile

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

                    List<Tile> dup = new List<Tile>(cur);
                    dup.Add(curTile);

                    float curDist = dist(target, curTile);

                    if (curDist < 0.01)
                    {
                        Debug.Log("reach");
                        reach = true;
                        path = dup;
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

                float angle = Mathf.Atan2(path[0].pos.y - tile.pos.y, path[0].pos.x - tile.pos.x);

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

        if (path.Count != 0)
        {
            //move to next tile on list if no unit is there
            if (path[0].unit == null)
            {
                //last tile
                tile.unit = null;

                //update tile
                tile = path[0];
                tile.updateStatus(owner, this.gameObject);

                //position
                transform.position = new Vector3(tile.pos.x, tile.pos.y, transform.position.z);

                path.RemoveAt(0);
            }

            //display arrow
            if (path.Count != 0)
            {
                arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

                float angle = Mathf.Atan2(path[0].pos.y - tile.pos.y, path[0].pos.x - tile.pos.x);

                arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
            }
        }
    }

    float dist(Tile t1, Tile t2)
    {
        Vector2 p1 = TileManager.instance.getWorldPosition(t1);
        Vector2 p2 = TileManager.instance.getWorldPosition(t2);
        return Mathf.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y));
    }
}
