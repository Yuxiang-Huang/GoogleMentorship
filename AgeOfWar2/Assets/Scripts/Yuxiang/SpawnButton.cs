using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnButton : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField] string path;

    public void spawn()
    {
        SpawnManager.instance.spawn(image, path);
    }
}
