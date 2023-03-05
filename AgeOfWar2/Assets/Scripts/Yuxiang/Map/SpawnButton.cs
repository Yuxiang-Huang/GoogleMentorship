using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnButton : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField] string path;

    [SerializeField] string type;

    [SerializeField] int goldNeedToSpawn;

    [SerializeField] GameObject spawnImage;

    [SerializeField] TextMeshProUGUI costText;

    void Awake()
    {
        costUpdate();
    }

    public void spawn()
    {
        //not during taking turn phase
        if (!PlayerController.instance.turnEnded)
            SpawnManager.instance.spawn(image, path, goldNeedToSpawn, spawnImage, type);
    }

    public void costUpdate()
    {
        costText.text = PlayerController.instance.age * goldNeedToSpawn + " gold";
    }
}
