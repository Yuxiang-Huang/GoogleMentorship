using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnButton : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField] string path;

    [SerializeField] string type;

    [SerializeField] int goldNeedToSpawn;

    [SerializeField] GameObject spawnImage;

    public void spawn()
    {
        //not during taking turn phase
        if (PlayerController.instance.mode != "")
            SpawnManager.instance.spawn(image, path, goldNeedToSpawn, spawnImage, type);
    }
}
