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

    [Header("Turn")]
    [SerializeField] GameObject turnBtn;
    [SerializeField] Coroutine timeCoroutine;

    [Header("Age")]
    [SerializeField] List<string> ageNameList;
    [SerializeField] GameObject ageAdvanceBtn;
    [SerializeField] TextMeshProUGUI ageText;
    [SerializeField] TextMeshProUGUI goldNeedToAdvanceText;

    void Awake()
    {
        instance = this;
        PV = GetComponent<PhotonView>();

        turnBtn.SetActive(false);
    }

    public void startGame(int id)
    {
        IntroText.SetActive(false);
        Shop.SetActive(true);

        playerUI = playerUIList[id];
        playerUI.gameObject.SetActive(true);
    }

    public void startTurn()
    {
        turnBtn.SetActive(true);
        timeCoroutine = StartCoroutine(nameof(timer));
    }

    IEnumerator timer()
    {
        int time = 10 * (PlayerController.instance.age + 1);

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

    public void updateGoldText()
    {
        playerUI.goldText.text = "Gold: " + PlayerController.instance.gold;
    }

    [PunRPC]
    public void updateTimeText(string message)
    {
        playerUI.timeText.text = message;
    }

    #region Age System

    public void ageAdvance()
    {
        //if enough gold
        if (PlayerController.instance.gold >= PlayerController.instance.goldNeedToAdvance)
        {
            PlayerController.instance.gold -= PlayerController.instance.goldNeedToAdvance;

            //modify age
            PlayerController.instance.age++;
            ageText.text = ageNameList[PlayerController.instance.age - 1];
            PlayerController.instance.goldNeedToAdvance *= 2;
            goldNeedToAdvanceText.text = "Advance: " + PlayerController.instance.goldNeedToAdvance + " gold";
            playerUI.goldText.text = "Gold: " + PlayerController.instance.gold;

            //age limit
            if (PlayerController.instance.age >= 5)
            {
                ageAdvanceBtn.SetActive(false);
            }

            //update building health
            PlayerController.instance.updateExistingUnits();
        }
    }

    #endregion
}
