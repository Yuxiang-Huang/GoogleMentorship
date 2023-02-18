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

        path = new Queue<Tile>();

        path.Enqueue(target);
    }

    public void move()
    {
        Debug.Log(path);

        if (path.Count != 0)
        {
            Vector2Int tilePos = path.Dequeue().pos;
            transform.position = new Vector3(tilePos.x, tilePos.y, transform.position.z);
        }
    }
}
