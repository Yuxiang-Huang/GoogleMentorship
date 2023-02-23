using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnButton : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField] string path;

    [SerializeField] int goldNeedToSpawn;

    [SerializeField] GameObject spawnImage;

    public void spawn()
    {
        SpawnManager.instance.spawn(image, path, goldNeedToSpawn, spawnImage);
    }
}
