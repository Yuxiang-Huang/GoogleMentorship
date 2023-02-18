using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    GameObject[,] tiles;

    const int cellSize = 1;

    [SerializeField] GameObject landTilePrefab;
    [SerializeField] GameObject waterTilePrefab;

    GameObject highlighted;

    // Start is called before the first frame update
    void Start()
    {
        makeGrid(10, 10);
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
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = getMousePos();

        GameObject newHilighted = getTile(mousePos);

        if (highlighted != newHilighted)
        {
            if (highlighted != null)
                highlighted.GetComponent<Tile>().highlight(false);

            highlighted = newHilighted;

            if (newHilighted != null) 
                highlighted.GetComponent<Tile>().highlight(true);
        }
    }

    GameObject getTile(Vector3 pos)
    {
        int x = (int)(pos.x + cellSize / 2.0) / cellSize;
        int y = (int)(pos.y + cellSize / 2.0) / cellSize;

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y];
        }
        return null;
    }

    Vector3 getMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

}
