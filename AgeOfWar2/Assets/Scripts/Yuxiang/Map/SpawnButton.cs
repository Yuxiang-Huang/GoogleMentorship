using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnButton : MonoBehaviour
{
    [SerializeField] Image image;

    public string path;

    [SerializeField] string type;

    [SerializeField] int goldNeedToSpawn;

    [SerializeField] GameObject spawnImage;

    [SerializeField] TextMeshProUGUI costText;

    [SerializeField] GameObject unit;

    public List<Sprite> unitImages;

    void Awake()
    {
        ageAdvanceUpdate();
    }

    public void spawn()
    {
        //not during taking turn phase
        if (!PlayerController.instance.turnEnded)
        {
            SpawnManager.instance.spawn(image, path,
                goldNeedToSpawn * (int) Mathf.Pow(GameManager.instance.ageCostFactor, PlayerController.instance.age),
                spawnImage, type, unit.GetComponent<IUnit>());
            UIManager.instance.updateInfoTabSpawn(unit.GetComponent<IUnit>());
        }
        else
        {
            UIManager.instance.hideInfoTab();
        }
    }

    public void ageAdvanceUpdate()
    {
        //exception of main base
        if (type == "MainBase") return;

        costText.text = goldNeedToSpawn
        * (int) Mathf.Pow(GameManager.instance.ageCostFactor, PlayerController.instance.age)
        +" gold";

        GetComponent<Image>().sprite = unitImages[PlayerController.instance.age];
        spawnImage.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = unitImages[PlayerController.instance.age];
    }
}
