using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour
{
    public Tile tile;
    Tile lastTarget;

    public List<Tile> path;

    [SerializeField] GameObject arrow;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject highlightTile;

    private void Start()
    {
        tile = TileManager.instance.getTile();
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
                if (!visited[curTile.pos.x, curTile.pos.y])
                {
                    visited[curTile.pos.x, curTile.pos.y] = true;

                    List<Tile> dup = new List<Tile>(cur);
                    dup.Add(curTile);

                    if (curTile == target)
                    {
                        reach = true;
                        dup.RemoveAt(0);
                        path = dup;
                    }

                    allPath.Enqueue(dup);
                }
            }
        }

        //display arrow
        if (path.Count != 0)
        {
            if (arrow != null)
            {
                Destroy(arrow);
            }

            arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

            float angle = Mathf.Atan2(path[0].pos.y - tile.pos.y, path[0].pos.x - tile.pos.x);

            arrow.transform.Rotate(Vector3.forward, angle * 180 / Mathf.PI);
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
                tile.unit = null;
                tile = path[0];
                path[0].unit = this.gameObject;
                Vector2Int tilePos = path[0].pos;
                transform.position = new Vector3(tilePos.x, tilePos.y, transform.position.z);
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
}
