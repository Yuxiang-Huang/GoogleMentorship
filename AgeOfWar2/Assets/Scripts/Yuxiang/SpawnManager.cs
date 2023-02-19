using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject clubMan;

    public static SpawnManager instance;

    public PlayerController player;

    public void Awake()
    {
        instance = this;
    }

    public void spawn()
    {
        if (player.mode == "spawn")
        {
            player.mode = "move";
        }
        else
        {
            player.mode = "spawn";
            player.toSpawn = clubMan;
            player.goldNeedToSpawn = 2;
        }
    }
}
