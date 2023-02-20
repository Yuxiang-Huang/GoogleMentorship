using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using System.Text;

public class TileManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    [SerializeField] GameObject landTilePrefab;
    [SerializeField] GameObject waterTilePrefab;

    public static TileManager instance;

    public const float cellSize = 1;

    public Tile [,] tiles;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        PV = GetComponent<PhotonView>();
    }

    public void makeGrid(int rows, int cols)
    {
        //assign type of tiles
        StringBuilder instruction = new StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                instruction.Append(Random.Range(0, 4));
            }
        }

        PV.RPC(nameof(makeGrid_RPC), RpcTarget.AllBuffered, rows, cols, instruction.ToString());
    }

    [PunRPC]
    public void makeGrid_RPC(int rows, int cols, string instruction)
    {
        tiles = new Tile[rows, cols];

        GameObject parent = new GameObject("Map");

        int count = 0;

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                //instantiate
                if (instruction[count] == '0')
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

                count++;
            }
        }

        //make sure corner is land
        Destroy(tiles[0, 0].gameObject);
        tiles[0, 0] = Instantiate(landTilePrefab, new Vector3(0 * cellSize, 0 * cellSize, 0),
    Quaternion.identity).GetComponent<Tile>();
        tiles[0, 0].terrain = "land";
        tiles[0, 0].transform.SetParent(parent.transform);

        //set neighbors
        for (int row = 0; row < tiles.GetLength(0); row++)
        {
            for (int col = 0; col < tiles.GetLength(1); col++)
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

    public Tile getTile(Vector2 pos)
    {
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
