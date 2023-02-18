using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour, ITroop
{
    Tile tile;

    Tile lastTarget;

    public Queue<Tile> path;

    private void Start()
    {
        tile = TileManager.instance.getTile(transform.position);
    }

    public void findPath(Tile target)
    {
        if (lastTarget == target) return; //same path

        lastTarget = target;

        Queue<Queue<Tile>> allPath = new Queue<Queue<Tile>>();

        Queue<Tile> root = new Queue<Tile>();
        root.Enqueue(tile);

        allPath.Enqueue(root);


        bool[,] visited = new bool[TileManager.instance.tiles.GetLength(0),
                                   TileManager.instance.tiles.GetLength(1)];

        bool reach = false;

        int len = 0;
        while (allPath.Count != 0 && !reach)
        {
            Debug.Log("Len: " + len);

            Queue<Tile> cur = allPath.Dequeue();
            Tile lastTile = cur.Peek();

            if (len > 0)
            {
                Debug.Log(len + ": " + lastTile.ToString());
            }

            foreach (Tile curTile in lastTile.neighbors)
            {
                if (!visited[curTile.pos.x, curTile.pos.y])
                {
                    visited[curTile.pos.x, curTile.pos.y] = true;

                    Queue<Tile> dup = new Queue<Tile>(cur);
                    dup.Enqueue(curTile);

                    if (curTile == target)
                    {
                        reach = true;
                        path = dup;
                        Debug.Log(path.ToString());
                    }

                    Debug.Log("curTile: " + curTile);
                    Debug.Log(dup.Dequeue().ToString());

                    allPath.Enqueue(dup);
                }
            }

            len++;

            if (len > 100)
            {
                break;
            }

            Debug.Log(allPath.Count);
        }


    }

    public void move()
    {
        if (path.Count != 0)
        {
            Vector2Int tilePos = path.Dequeue().pos;
            transform.position = new Vector3(tilePos.x, tilePos.y, transform.position.z);
        }
    }
}
