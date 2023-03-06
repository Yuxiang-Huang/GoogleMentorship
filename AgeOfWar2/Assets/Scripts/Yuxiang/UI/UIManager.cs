using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public PhotonView PV;

    [SerializeField] List<PlayerUI> playerUIList;

    [SerializeField] PlayerUI playerUI;

    public Canvas healthbarCanvas;

    [Header("Start Game")]
    [SerializeField] GameObject IntroText;
    [SerializeField] GameObject Shop;
    [SerializeField] GameObject AgeUI;

    [Header("Turn")]
    [SerializeField] GameObject turnBtn;
    [SerializeField] Coroutine timeCoroutine;

    [Header("InfoTab")]
    [SerializeField] GameObject infoTab;
    [SerializeField] TextMeshProUGUI unitNameText;
    [SerializeField] TextMeshProUGUI unitHealthText;
    [SerializeField] TextMeshProUGUI unitDamageText;
    [SerializeField] TextMeshProUGUI unitSellText;
    public GameObject sellBtn;

    [Header("Age")]
    [SerializeField] List<string> ageNameList;
    [SerializeField] GameObject ageAdvanceBtn;
    [SerializeField] TextMeshProUGUI ageText;
    [SerializeField] TextMeshProUGUI goldNeedToAdvanceText;

    void Awake()
    {
        instance = this;
        PV = GetComponent<PhotonView>();

        //everything set false first
        turnBtn.SetActive(false);
        infoTab.SetActive(false);
        AgeUI.SetActive(false);
    }

    #region Start Game

    //public void startGame(int id)
    //{
    //    IntroText.SetActive(false);
    //    Shop.SetActive(true);
    //    AgeUI.SetActive(true);

    //    playerUI = playerUIList[id];
    //    playerUI.playerName.text = PhotonNetwork.NickName;

    //    goldNeedToAdvanceText.text = "Advance: " + PlayerController.instance.goldNeedToAdvance + " gold";

    //    //PV.RPC(nameof(reveal), RpcTarget.All, id);
    //}

    public void startGame(int id)
    {
        IntroText.SetActive(false);
        Shop.SetActive(true);
        AgeUI.SetActive(true);

        playerUI = playerUIList[0];
        playerUI.nameText.text = PhotonNetwork.NickName;

        goldNeedToAdvanceText.text = "Advance: " + PlayerController.instance.goldNeedToAdvance + " gold";

        playerUIList[0].gameObject.SetActive(true);
    }

    //[PunRPC]
    //public void reveal(int id)
    //{
    //    playerUIList[id].gameObject.SetActive(true);
    //}

    #endregion

    #region Turn

    public void startTurn()
    {
        turnBtn.SetActive(true);
        timeCoroutine = StartCoroutine(nameof(timer));

        //update Player info
        updatePlayerInfo(PlayerController.instance.allTroops.Count,
            PlayerController.instance.allBuildings.Count);
    }

    IEnumerator timer()
    {
        int time = 10 * (PlayerController.instance.age + 2);

        for (int i = 0; i < time; i++)
        {
            playerUI.timeText.text = "Time Left: " + (time - i) + " seconds";

            yield return new WaitForSeconds(1f);
        }

        GameManager.instance.endTurn();
    }

    public void endTurn()
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }

        playerUI.timeText.text = "Waiting for opponents...";

        turnBtn.SetActive(false);
    }

    #endregion

    #region Info Tab

    //for existing units
    public void updateInfoTab(IUnit unit)
    {
        infoTab.SetActive(true);
        unit.fillInfoTab(unitNameText, unitHealthText, unitDamageText, unitSellText);
    }

    //for spawn buttons
    public void updateInfoTabSpawn(IUnit unit)
    {
        infoTab.SetActive(true);
        unit.fillInfoTabSpawn(unitNameText, unitHealthText, unitDamageText, unitSellText, PlayerController.instance.age);
        sellBtn.SetActive(false);
    }

    //for spawn images
    public void updateInfoTab(SpawnInfo spawnInfo)
    {
        infoTab.SetActive(true);
        spawnInfo.unit.fillInfoTabSpawn(unitNameText, unitHealthText, unitDamageText, unitSellText, spawnInfo.age);
    }

    public void hideInfoTab()
    {
        infoTab.SetActive(false);
    }

    public void sell()
    {
        //sell
        if (PlayerController.instance.unitSelected != null)
        {
            PlayerController.instance.unitSelected.sell();
            PlayerController.instance.unitSelected = null;
        }
        //despawn
        else if (PlayerController.instance.spawnInfoSelected != null)
        {
            SpawnInfo cur = PlayerController.instance.spawnInfoSelected;

            //remove from list
            Destroy(cur.spawnImage);
            PlayerController.instance.spawnList.Remove(cur.spawnTile.pos);

            //return gold
            PlayerController.instance.gold += cur.spawnGold;
            updateGoldText();
        }

        infoTab.SetActive(false);
    }

    #endregion

    #region Player Info

    [PunRPC]
    public void updatePlayerInfo(int troopCount, int buildingCount)
    {
        playerUI.troopText.text = "Troop: " + troopCount;
        playerUI.buildingText.text = "Buildings: " + buildingCount;
    }

    #endregion

    #region Age System

    public void ageAdvance()
    {
        //if enough gold
        if (PlayerController.instance.gold >= PlayerController.instance.goldNeedToAdvance)
        {
            infoTab.SetActive(false);

            PlayerController.instance.gold -= PlayerController.instance.goldNeedToAdvance;

            //modify age
            PlayerController.instance.age++;
            ageText.text = ageNameList[PlayerController.instance.age];
            playerUI.ageText.text = ageNameList[PlayerController.instance.age]; //Need to Sync later

            //modify gold
            PlayerController.instance.goldNeedToAdvance *= GameManager.instance.ageCostFactor;
            goldNeedToAdvanceText.text = "Advance: " + PlayerController.instance.goldNeedToAdvance + " gold";
            playerUI.goldText.text = "Gold: " + PlayerController.instance.gold;

            //age limit
            if (PlayerController.instance.age >= 5)
            {
                ageAdvanceBtn.SetActive(false);
            }

            //update building health
            PlayerController.instance.updateExistingUnits();

            //update spawn buttons
            foreach (SpawnButton spawnBtn in SpawnManager.instance.spawnInfoList)
            {
                spawnBtn.ageAdvanceUpdate();
            }
        }
    }

    #endregion

    public void updateGoldText()
    {
        playerUI.goldText.text = "Gold: " + PlayerController.instance.gold;
    }

    [PunRPC]
    public void updateTimeText(string message)
    {
        playerUI.timeText.text = message;
    }
}
