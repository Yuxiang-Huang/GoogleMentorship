using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject clubMan;

    public static SpawnManager instance;

    public void Awake()
    {
        instance = this;
    }

    public void spawn()
    {
        if (PlayerController.instance.mode == "spawn")
        {
            PlayerController.instance.mode = "move";
        }
        else
        {
            PlayerController.instance.mode = "spawn";
            PlayerController.instance.toSpawn = "Troop/ClubMan";
            PlayerController.instance.goldNeedToSpawn = 2;
        }
    }
}
