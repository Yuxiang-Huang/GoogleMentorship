using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] GameObject highlightTile;

    public void highlight(bool status)
    {
        highlightTile.SetActive(status);
    }
}
