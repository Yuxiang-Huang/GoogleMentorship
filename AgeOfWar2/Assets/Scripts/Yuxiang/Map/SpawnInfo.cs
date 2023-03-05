using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnInfo
{
    public Tile spawnTile;

    public string unitName;

    public GameObject spawnImage;

    public int sellGold;

    public IUnit unit;

    public SpawnInfo(Tile spawnTile, string unitName, IUnit unit, GameObject spawnImage, int sellGold)
    {
        this.spawnTile = spawnTile;
        this.unitName = unitName;
        this.spawnImage = spawnImage;
        this.sellGold = sellGold;
        this.unit = unit;
    }

    public void fillInfoTab(TextMeshProUGUI nameText, TextMeshProUGUI healthText,
    TextMeshProUGUI damageText, TextMeshProUGUI sellText)
    {
        unit.fillInfoTab(nameText, healthText, damageText, sellText);
    }
}
