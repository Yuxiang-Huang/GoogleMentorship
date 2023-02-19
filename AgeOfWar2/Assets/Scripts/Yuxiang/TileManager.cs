using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [SerializeField] GameObject castle;
    [SerializeField] GameObject landTilePrefab;
    [SerializeField] GameObject waterTilePrefab;

    public static TileManager instance;

    public const float cellSize = 1;

    public Tile [,] tiles;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    //create the map
    public void makeGrid(int rows, int cols)
    {
        tiles = new Tile[rows, cols];

        GameObject parent = new GameObject("Map");

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                //instantiate
                if (Random.Range(0, 2) == 0)
                {
                    tiles[i, j] = Instantiate(waterTilePrefab, new Vector3(i * cellSize, j * cellSize, 0),
    Quaternion.identity).GetComponent<Tile>();
                    tiles[i, j].terrain = "water";
                }

                else
                {
                    tiles[i, j] = Instantiate(landTilePrefab, new Vector3(i * cellSize, j * cellSize, 0),
    Quaternion.identity).GetComponent<Tile>();
                    tiles[i, j].terrain = "land";
                }

                tiles[i, j].transform.SetParent(parent.transform);

                //set tile stats
                tiles[i, j].GetComponent<Tile>().pos = new Vector2Int(i, j);
            }
        }

        //set neighbors
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                List<Tile> neighbors = tiles[row, col].GetComponent<Tile>().neighbors;

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            int curRow = row + i;
                            int curCol = col + j;

                            if (curRow >= 0 && curRow < tiles.GetLength(0) && 
                                curCol >= 0 && curCol < tiles.GetLength(1))
                            {
                                neighbors.Add(tiles[curRow, curCol].GetComponent<Tile>());
                            }
                        }
                    }
                } 
            }
        }

        GameObject myCastle = Instantiate(castle, new Vector3(0, 0, 0), Quaternion.identity);

        tiles[0, 0].GetComponent<Tile>().unit = myCastle;
    }

    //get the tile the cursor is on
    public Tile getTile()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int x = (int)((pos.x + cellSize / 2.0) / cellSize);
        int y = (int)((pos.y + cellSize / 2.0) / cellSize);

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y].GetComponent<Tile>();
        }
        return null;
    }

    public Vector2 getWorldPosition(Tile tile)
    {
        return new Vector2(tile.pos.x * cellSize + cellSize / 2, tile.pos.y * cellSize + cellSize / 2);
    }
}
