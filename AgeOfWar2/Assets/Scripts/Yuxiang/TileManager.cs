using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    GameObject[,] tiles;

    const int cellSize = 1;

    [SerializeField] GameObject tilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        makeGrid(10, 10);
    }

    void makeGrid(int rows, int cols)
    {
        tiles = new GameObject[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                tiles[i, j] = Instantiate(tilePrefab, new Vector3(i * cellSize, j * cellSize, 0),
                    Quaternion.identity);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
