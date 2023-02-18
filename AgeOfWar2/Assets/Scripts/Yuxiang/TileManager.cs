using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    GameObject[,] tiles;

    const int cellSize = 1;

    [SerializeField] GameObject landTilePrefab;
    [SerializeField] GameObject waterTilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        makeGrid(10, 10);
    }

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
        
    }
}
