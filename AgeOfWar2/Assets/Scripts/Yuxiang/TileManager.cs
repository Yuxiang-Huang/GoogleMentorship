using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using System.Text;

public class TileManager : MonoBehaviourPunCallbacks
{
    public static TileManager instance;

    public PhotonView PV;

    public Tile[,] tiles;

    //building blocks
    public const float tileSize = 1;

    [SerializeField] GameObject landTilePrefab;
    [SerializeField] GameObject waterTilePrefab;

    #region Perlin Noise

    // Perlin Noise Bits
    public float fade(float x)
    {
        return x * x * x * (x * (6 * x - 15) + 10);
        // x^3 ( x (6x - 15) + 10) = x^3 ( 6x^2 - 15x + 10) = 6x^5 - 15x^4 + 10x^3
    }
    public float linInterp(float a, float b, float c)
    {
        return b + a * (c - b);
        // b + ac - ab = ac - b(a-1)
    }
    public float gradient(int hash, float x, float y, float z)
    {
        int h = hash & 15;  // 4 bits
        float u = h < 8 ? x : y;   // u is x if h < 8; and y otherwise
        float v = h < 4 ? y : h == 12 || h == 14 ? x : z;  // if h is less than 4, v is y. if h is 12 or 14, v is x. Else, v is z;

        // if the lowest bit of h is 0, u is positive, else u is negative
        // if the second bit digit of h is 0, v is positive. Else v is negative;
        if ((h & 1) == 1) { u *= -1; }
        if ((h & 2) == 1) { v *= -1; }

        return u + v;
    }

    public int[] randomVals = { 151,160,137,91,90,15,
           131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
           190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
          88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
           77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
           102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
           135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
           5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
           223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
           129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
           251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
           49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
           138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
       };

    public int[] usedVals = new int[512];

    public float pNoise(float x, float y, float z)
    {

        // First we find a unit cube that encompasses the point
        int cubeX = (int)x & 255; // This reduces the number of the cube to 8 bits
        int cubeY = (int)y & 255; // This reduces the number of the cube to 8 bits
        int cubeZ = (int)z & 255; // This reduces the number of the cube to 8 bits

        float pointX = x - cubeX; // The point on that cube
        float pointY = y - cubeY;
        float pointZ = z - cubeZ;

        // Creating a fade for each point
        float a = fade(pointX);
        float b = fade(pointY);
        float c = fade(pointZ);

        // Creating additional points for vectors
        int A = usedVals[cubeX] + cubeY,
            A0 = usedVals[A] + cubeZ,
            A1 = usedVals[A + 1] + cubeZ,
            B = usedVals[cubeX + 1] + cubeY,
            B0 = usedVals[B] + cubeZ,
            B1 = usedVals[B + 1] + cubeZ;

        // Take these points, and form the vectors, and interpolate
        return linInterp(c,
                 linInterp(b,
                       linInterp(a,
                             gradient(usedVals[A0], pointX, pointY, pointZ),
                             gradient(usedVals[B0], pointX - 1, pointY, pointZ)
                               ),
                       linInterp(a,
                             gradient(usedVals[A1], pointX, pointY - 1, pointZ),
                             gradient(usedVals[B1], pointX - 1, pointY - 1, pointZ)
                               )
                 ),
                 linInterp(b,
                       linInterp(a,
                             gradient(usedVals[A0 + 1], pointX, pointY, pointZ - 1),
                             gradient(usedVals[B0 + 1], pointX - 1, pointY, pointZ - 1)
                               ),
                       linInterp(a,
                              gradient(usedVals[A1 + 1], pointX, pointY - 1, pointZ - 1),
                              gradient(usedVals[B1 + 1], pointX - 1, pointY - 1, pointZ - 1))));

    }

    #endregion

    void Awake()
    {
        instance = this;

        PV = GetComponent<PhotonView>();

        makeGrid(20, 6);
    }

    public void makeGrid(float rows, float cols)
    {
        GameObject parent = new GameObject("Map");

        // Random array of  512 values that are [0, 255]
        for (int i = 0; i < 256; i++)
        {
            usedVals[i] = randomVals[i];
            usedVals[256 + i] = randomVals[i];
        }
        float ranElem = Random.Range(0, 200);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                float noiseNum = pNoise(17 * col / cols + ranElem, 17 * row / rows + ranElem, 0) + 0.1f;
                if (noiseNum >= 0) { mapGen(row, col, landTilePrefab); }
                else { mapGen(row, col, waterTilePrefab); }
            }
        }

        ////assign type of tiles using bit
        //StringBuilder instruction = new StringBuilder();

        //for (int i = 0; i < rows; i++)
        //{
        //    for (int j = 0; j < cols; j++)
        //    {
        //        instruction.Append(Random.Range(0, 4));
        //    }
        //}

        //PV.RPC(nameof(makeGrid_RPC), RpcTarget.AllViaServer, rows, cols, instruction.ToString());
    }

    public void mapGen(int row, int col, GameObject tile)
    {
        float xPos = row * 0.5f;
        float yPos = col * Mathf.Sqrt(3f) + (row % 2 * Mathf.Sqrt(3f) / 2);
        Instantiate(tile, new Vector3(xPos, yPos, 0), Quaternion.identity);
    }

    [PunRPC]
    public void makeGrid_RPC(int rows, int cols, string instruction)
    {
        tiles = new Tile[rows, cols];

        GameObject parent = new GameObject("Map");

        int count = 0;

        //generate the grid using instruction
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                //instantiate
                if (instruction[count] == '0')
                {
                    tiles[i, j] = Instantiate(waterTilePrefab, new Vector3(i * tileSize, j * tileSize, 0),
    Quaternion.identity).GetComponent<Tile>();
                    tiles[i, j].terrain = "water";
                }
                else
                {
                    tiles[i, j] = Instantiate(landTilePrefab, new Vector3(i * tileSize, j * tileSize, 0),
    Quaternion.identity).GetComponent<Tile>();
                    tiles[i, j].terrain = "land";
                }

                tiles[i, j].transform.SetParent(parent.transform);

                //set tile stats
                tiles[i, j].GetComponent<Tile>().pos = new Vector2Int(i, j);

                count++;
            }
        }

        Vector2Int corner = new Vector2Int(1, 1);

        //make sure corner is land
        Destroy(tiles[corner.x, corner.y].gameObject);
        tiles[corner.x, corner.y] = Instantiate(landTilePrefab, new Vector3(corner.x * tileSize, corner.y * tileSize, 0),
    Quaternion.identity).GetComponent<Tile>();
        tiles[corner.x, corner.y].terrain = "land";
        tiles[corner.x, corner.y].GetComponent<Tile>().pos = corner;
        tiles[corner.x, corner.y].transform.SetParent(parent.transform);

        corner = new Vector2Int(rows - 2, cols - 2);

        //make sure corner is land
        Destroy(tiles[corner.x, corner.y].gameObject);
        tiles[corner.x, corner.y] = Instantiate(landTilePrefab, new Vector3(corner.x * tileSize, corner.y * tileSize, 0),
    Quaternion.identity).GetComponent<Tile>();
        tiles[corner.x, corner.y].terrain = "land";
        tiles[corner.x, corner.y].GetComponent<Tile>().pos = corner;
        tiles[corner.x, corner.y].transform.SetParent(parent.transform);

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

        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["Ready"] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    //get the tile the cursor is on
    public Tile getTile()
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int x = (int)((pos.x + tileSize / 2.0) / tileSize);
        int y = (int)((pos.y + tileSize / 2.0) / tileSize);

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y].GetComponent<Tile>();
        }
        return null;
    }

    //get the tile depending on row col
    public Tile getTile(Vector2 pos)
    {
        int x = (int)((pos.x + tileSize / 2.0) / tileSize);
        int y = (int)((pos.y + tileSize / 2.0) / tileSize);

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y].GetComponent<Tile>();
        }

        return null;
    }

    //get world position from row col
    public Vector2 getWorldPosition(Tile tile)
    {
        return new Vector2(tile.pos.x * tileSize + tileSize / 2, tile.pos.y * tileSize + tileSize / 2);
    }
}
