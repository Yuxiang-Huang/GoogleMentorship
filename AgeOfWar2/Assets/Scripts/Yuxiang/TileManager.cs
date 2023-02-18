using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;

    const int cellSize = 1;

    public GameObject[,] tiles;

    [SerializeField] GameObject landTilePrefab;
    [SerializeField] GameObject waterTilePrefab;

    [SerializeField] GameObject clubMan;

    Tile highlighted;

    ITroop player;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        makeGrid(10, 10);
        GameObject playerObject = Instantiate(clubMan, new Vector3(0, 0, 0), Quaternion.identity);
        player = playerObject.GetComponent<Melee>();
    }

    //create the map
    void makeGrid(int rows, int cols)
    {
        tiles = new GameObject[rows, cols];

        GameObject parent = new GameObject("Map");

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                //instantiate
                if ((i + j) % 2 == 1)
                {
                    tiles[i, j] = Instantiate(waterTilePrefab, new Vector3(i * cellSize, j * cellSize, 0),
    Quaternion.identity);
                }

                else
                {
                    tiles[i, j] = Instantiate(landTilePrefab, new Vector3(i * cellSize, j * cellSize, 0),
    Quaternion.identity);
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
                        int curRow = row + i;
                        int curCol = col + j;

                        if (curRow != 0 || curCol != 0)
                        {
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

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = getMousePos();

        Tile newHilighted = getTile(mousePos);

        if (highlighted != newHilighted)
        {
            if (highlighted != null)
                highlighted.highlight(false);

            highlighted = newHilighted;

            if (newHilighted != null) 
                highlighted.highlight(true);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (highlighted != null)
            {
                player.findPath(highlighted.GetComponent<Tile>());
                StartCoroutine(nameof(moveTest));
            }
        }
    }

    IEnumerator moveTest()
    {
        yield return new WaitForSeconds(0.3f);

        player.move();
    }


    public Tile getTile(Vector3 pos)
    {
        int x = (int)(pos.x + cellSize / 2.0) / cellSize;
        int y = (int)(pos.y + cellSize / 2.0) / cellSize;

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y].GetComponent<Tile>();
        }
        return null;
    }

    Vector3 getMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
