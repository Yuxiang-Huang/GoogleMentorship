using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public PhotonView PV;

    public Canvas healthbarCanvas;

    public GameObject arrowPrefab;

    [Header("Settting")]
    [SerializeField] int initialTime;
    [SerializeField] int timeInc;

    [Header("Start Game")]
    [SerializeField] GameObject IntroText;
    [SerializeField] GameObject Shop;
    [SerializeField] GameObject AgeUI;

    [Header("Turn")]
    [SerializeField] GameObject turnBtn;
    [SerializeField] GameObject cancelTurnBtn;
    [SerializeField] Coroutine timeCoroutine;
    [SerializeField] int curTimeUsed;
    [SerializeField] Coroutine cancelTimeCoroutine;

    [Header("InfoTab - Unit")]
    [SerializeField] GameObject infoTab;
    [SerializeField] TextMeshProUGUI unitNameText;
    [SerializeField] TextMeshProUGUI unitHealthText;
    [SerializeField] TextMeshProUGUI unitDamageText;
    [SerializeField] TextMeshProUGUI unitSellText;
    public GameObject sellBtn;

    [Header("InfoTab - Player")]
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI goldText;

    [SerializeField] GameObject playerList;
    [SerializeField] List<TextMeshProUGUI> playerNameList;

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
        Shop.SetActive(false);
        turnBtn.SetActive(false);
        infoTab.SetActive(false);
        AgeUI.SetActive(false);
        cancelTurnBtn.SetActive(false);
        IntroText.SetActive(true);
        timeText.gameObject.SetActive(false);
        playerList.SetActive(false);
    }

    #region Start Game

    public void startGame()
    {
        //time option setting
        initialTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["initialTime"];
        timeInc = (int)PhotonNetwork.CurrentRoom.CustomProperties["timeInc"];

        //set UI active
        IntroText.SetActive(false);
        Shop.SetActive(true);
        AgeUI.SetActive(true);
        timeText.gameObject.SetActive(true);

        //Player list
        playerList.SetActive(true);
        for (int i = 0; i < GameManager.instance.allPlayersOriginal.Count; i++)
        {
            playerNameList[i].text = GameManager.instance.allPlayersOriginal[i].PV.Owner.NickName;
            playerNameList[i].gameObject.transform.parent.gameObject.SetActive(true);
        }

        goldNeedToAdvanceText.text = "Advance: " + PlayerController.instance.goldNeedToAdvance + " gold";
    }

    #endregion

    #region Turn

    public void startTurn()
    {
        turnBtn.SetActive(true);

        //reset timer
        curTimeUsed = initialTime + timeInc * PlayerController.instance.age;
        timeCoroutine = StartCoroutine(nameof(timer));
    }

    IEnumerator timer()
    {
        for (int i = curTimeUsed; i > 0; i--)
        {
            timeText.text = "Time Left:\n" + i + " seconds";

            curTimeUsed = i;

            yield return new WaitForSeconds(1f);
        }

        curTimeUsed = 0;

        GameManager.instance.endTurn();
    }

    IEnumerator cancelTimer()
    {
        //keep track of time left during end turn for canceling
        for (int i = curTimeUsed; i > 0; i--)
        {
            curTimeUsed = i;

            yield return new WaitForSeconds(1f);
        }

        curTimeUsed = 0;

        cancelTurnBtn.SetActive(false);
    }

    public void endTurn()
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
        //keep track time after end turn
        timeCoroutine = StartCoroutine(nameof(cancelTimer));

        timeText.text = "Waiting for opponents...";

        turnBtn.SetActive(false);

        //only if have time left
        if (curTimeUsed > 0)
            cancelTurnBtn.SetActive(true);
    }

    public void cancelEndTurn()
    {
        turnBtn.SetActive(true);
        cancelTurnBtn.SetActive(false);

        //stop the timer that keep track after end turn and start another timer
        StopCoroutine(nameof(cancelTimer));
        timeCoroutine = StartCoroutine(nameof(timer));
    }

    [PunRPC]
    public void turnPhase()
    {
        StopCoroutine(nameof(cancelTimer));
        turnBtn.SetActive(false);
        cancelTurnBtn.SetActive(false);
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

            //modify gold
            PlayerController.instance.goldNeedToAdvance *= GameManager.instance.ageCostFactor;
            goldNeedToAdvanceText.text = "Advance: " + PlayerController.instance.goldNeedToAdvance + " gold";

            //age limit
            if (PlayerController.instance.age >= 5)
            {
                ageAdvanceBtn.SetActive(false);
            }

            //update building health
            PlayerController.instance.updateExistingUnits();

            //update spawn buttons
            foreach (SpawnButton spawnBtn in SpawnManager.instance.spawnBtnList)
            {
                spawnBtn.ageAdvanceUpdate();
            }
        }
    }

    #endregion

    [PunRPC]
    public void updateGoldText()
    {
        goldText.text = "Gold: " + PlayerController.instance.gold;
    }

    [PunRPC]
    public void updateTimeText(string message)
    {
        timeText.text = message;
    }
}
