using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        TileManager.instance.makeGrid(10, 10);
    }
}
