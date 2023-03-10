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
    }

    public void makeGrid()
    {
        //store type of tiles using bit
        StringBuilder instruction = new StringBuilder();

        // Random array of  512 values that are [0, 255]
        for (int i = 0; i < 256; i++)
        {
            usedVals[i] = randomVals[i];
            usedVals[256 + i] = randomVals[i];
        }
        float ranElem = Random.Range(0, 200);

        int rows = -1;
        int cols = -1;

        //decide map
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["Water"])
        {
            rows = 25;
            cols = 8;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    ////border by water
                    //if (row <= 4 || row >= rows - 5)
                    //{
                    //    instruction.Append(1);
                    //}
                    //else if (row % 2 == 0 && col == 1)
                    //{
                    //    instruction.Append(1);
                    //}
                    //else if (row % 2 == 1 && col <= 1)
                    //{
                    //    instruction.Append(1);
                    //}
                    //else if (row % 2 == 0 && col == cols - 1)
                    //{
                    //    instruction.Append(1);
                    //}
                    //else if (row % 2 == 1 && col >= cols - 2)
                    //{
                    //    instruction.Append(1);
                    //}
                    //else
                    //{
                        float noiseNum = pNoise(17f * col / cols + ranElem, 17f * row / rows + ranElem, 0) + 0.1f;

                        //assign type
                        if (noiseNum >= 0)
                        {
                            instruction.Append(0);
                        }
                        else
                        {
                            instruction.Append(1);
                        }
                    //}
                }
            }
        }
        else
        {
            rows = 19;
            cols = 6;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    instruction.Append(0);
                }
            }
        }

        PV.RPC(nameof(makeGrid_RPC), RpcTarget.AllViaServer, rows, cols, instruction.ToString());
    }

    [PunRPC]
    public void makeGrid_RPC(int rows, int cols, string instruction)
    {
        //setting camera
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["Water"])
        {
            Camera.main.orthographicSize = 8.5f;
            Camera.main.transform.position = new Vector3(5, 7f, -10);
        }
        else
        {
            Camera.main.orthographicSize = 6.5f;
            Camera.main.transform.position = new Vector3(4, 5.25f, -10);
        }

        //make map
        tiles = new Tile[rows, cols];

        GameObject parent = new GameObject("Map");

        int count = 0;

        //generate the grid using instruction
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                //skip bottom row
                if (!(i % 2 == 0 && j == 0))
                {
                    float xPos = i * 0.5f * tileSize;
                    float yPos = j * Mathf.Sqrt(3f) * tileSize + (i % 2 * Mathf.Sqrt(3f) / 2 * tileSize);

                    Vector3 pos = new Vector3(xPos, yPos, 0);

                    //instantiate
                    if (instruction[count] == '0')
                    {
                        tiles[i, j] = Instantiate(landTilePrefab, pos, Quaternion.identity).GetComponent<Tile>();
                        tiles[i, j].terrain = "land";
                    }
                    else
                    {
                        tiles[i, j] = Instantiate(waterTilePrefab, pos, Quaternion.identity).GetComponent<Tile>();
                        tiles[i, j].terrain = "water";
                    }

                    //set tile stats
                    tiles[i, j].transform.SetParent(parent.transform);

                    tiles[i, j].GetComponent<Tile>().pos = new Vector2Int(i, j);
                }

                count++;
            }
        }

        //set neighbors
        for (int row = 0; row < tiles.GetLength(0); row++)
        {
            for (int col = 0; col < tiles.GetLength(1); col++)
            {
                //skip bottom row
                if (!(row % 2 == 0 && col == 0))
                {
                    List<Tile> neighbors = tiles[row, col].GetComponent<Tile>().neighbors;

                    //left and right
                    if (row >= 2)
                    {
                        neighbors.Add(tiles[row - 2, col]);
                    }
                    if (row < tiles.GetLength(0) - 2)
                    {
                        neighbors.Add(tiles[row + 2, col]);
                    }

                    if (row % 2 == 0)
                    {
                        //there is a row before it
                        if (row > 0)
                        {
                            neighbors.Add(tiles[row - 1, col]);

                            //even row decrease col
                            if (col >= 1)
                            {
                                neighbors.Add(tiles[row - 1, col - 1]);
                            }
                        }
                        //there is a row after it
                        if (row < tiles.GetLength(0) - 1)
                        {
                            neighbors.Add(tiles[row + 1, col]);

                            //even row decrease col
                            if (col >= 1)
                            {
                                neighbors.Add(tiles[row + 1, col - 1]);
                            }
                        }
                    }
                    else
                    {
                        //there is a row before it
                        if (row > 0)
                        {
                            neighbors.Add(tiles[row - 1, col]);

                            //odd row increase col
                            if (col < tiles.GetLength(1) - 1)
                            {
                                neighbors.Add(tiles[row - 1, col + 1]);
                            }
                        }
                        //there is a row after it
                        if (row < tiles.GetLength(0) - 1)
                        {
                            neighbors.Add(tiles[row + 1, col]);

                            //odd row increase col
                            if (col < tiles.GetLength(1) - 1)
                            {
                                neighbors.Add(tiles[row + 1, col + 1]);
                            }
                        }
                    }

                    //remove null due to skip bottom row
                    for (int i = neighbors.Count - 1; i >= 0; i--)
                    {
                        if (neighbors[i] == null)
                        {
                            neighbors.RemoveAt(i);
                        }
                    }
                }
            }
        }

        //set neighbors2
        for (int row = 0; row < tiles.GetLength(0); row++)
        {
            for (int col = 0; col < tiles.GetLength(1); col++)
            {
                //skip bottom row
                if (!(row % 2 == 0 && col == 0))
                {
                    tiles[row, col].neighbors2 = findNeighbors2(tiles[row, col]);
                }
            }
        }

        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        hash["Ready"] = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    //all tiles two tiles away
    public List<Tile> findNeighbors2(Tile tile)
    {
        List<Tile> ans = new List<Tile>();

        foreach (Tile neighbor in tile.neighbors)
        {
            ans.Add(neighbor);

            foreach (Tile neighbor2 in neighbor.neighbors)
            {
                if (!ans.Contains(neighbor2))
                {
                    ans.Add(neighbor2);
                }
            }
        }

        //remove inside tiles
        foreach (Tile neighbor in tile.neighbors)
        {
            ans.Remove(neighbor);
        }

        ans.Remove(tile);

        return ans;
    }

    //get the tile depending on world position
    public Tile getTile(Vector2 pos)
    {
        //simple division to find rough x and y
        int roundX = (int) (pos.x / 0.5f / tileSize);

        int roundY = (int) (pos.y / Mathf.Sqrt(3f) / tileSize);

        if (roundX < 0 || roundX >= tiles.GetLength(0) || roundY < 0 || roundY >= tiles.GetLength(1))
        {
            return null;
        }

        //compensate for the row skipped
        if (roundY == 0)
        {
            roundY++;
        }

        //compare with all neighbors
        Tile oneTile = tiles[roundX, roundY];

        Tile bestTile = oneTile;

        float minDist = dist(pos, getWorldPosition(oneTile));

        foreach (Tile neighbor in oneTile.neighbors)
        {
            float mayDist = dist(pos, getWorldPosition(neighbor));
            if (mayDist < minDist)
            {
                minDist = mayDist;
                bestTile = neighbor;
            }
        }
        foreach (Tile neighbor in oneTile.neighbors2)
        {
            float mayDist = dist(pos, getWorldPosition(neighbor));
            if (mayDist < minDist)
            {
                minDist = mayDist;
                bestTile = neighbor;
            }
        }
        return bestTile;
    }

    //get world position from row col
    public Vector2 getWorldPosition(Tile tile)
    {
        return new Vector2(tile.pos.x * 0.5f, tile.pos.y * Mathf.Sqrt(3f) + (tile.pos.x % 2 * Mathf.Sqrt(3f) / 2));
    }

    //find distance between two vector2
    float dist(Vector2 v1, Vector2 v2)
    {
        return Mathf.Sqrt((v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y));
    }
}
