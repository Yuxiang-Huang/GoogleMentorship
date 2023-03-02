using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using System.IO;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public PhotonView PV;

    public SortedDictionary<int, PlayerController> playerList = new SortedDictionary<int, PlayerController>();

    public List<PlayerController> allPlayersOriginal;
    public List<PlayerController> allPlayers;

    [SerializeField] GameObject turnBtn;

    [SerializeField] GameObject ageAdvanceBtn;

    [SerializeField] int playerMoved;


    [SerializeField] TextMeshProUGUI goldText;

    [SerializeField] Coroutine timeCoroutine;

    [SerializeField] TextMeshProUGUI timerText;

    public Canvas healthbarCanvas;

    [Header("Age")]    
    [SerializeField] TextMeshProUGUI ageText;
    [SerializeField] TextMeshProUGUI goldNeedToAdvanceText;

    private void Awake()
    {
        instance = this;
        PV = GetComponent<PhotonView>();

        bool offlineMode = false;

        //not able to access after game begins
        if (!offlineMode)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }
        else
        {
            PhotonNetwork.OfflineMode = true;
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomProperties = new Hashtable { {"Water", false} };
            PhotonNetwork.CreateRoom("offline", roomOptions);

            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player/PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    //call when each player is ready
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        //start
        if (changedProps.ContainsKey("Ready")) checkStart();

        #region turns

        //start turn
        else if (changedProps.ContainsKey("EndTurn")) checkEndTurn();

        else if (changedProps.ContainsKey("Spawned")) checkSpawn();

        else if (changedProps.ContainsKey("Moved")) checkMove();

        else if (changedProps.ContainsKey("Attacked")) checkAttack();

        #endregion
    }

    #region Begin Game

    public void createPlayerList()
    {
        //everyone joined
        if (playerList.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            //sorted list depending on actor number
            foreach (KeyValuePair<int, PlayerController> kvp in playerList)
            {
                allPlayersOriginal.Add(kvp.Value);
            }

            allPlayers = new List<PlayerController>(allPlayersOriginal);
        }
    }

    public void checkStart()
    {
        //master client start game once when everyone is ready
        var players = PhotonNetwork.PlayerList;
        if (players.All(p => p.CustomProperties.ContainsKey("Ready") && (bool)p.CustomProperties["Ready"]))
        {
            //ask all player to start game
            for (int i = 0; i < allPlayers.Count; i++)
            {
                allPlayers[i].PV.RPC("startGame", allPlayers[i].PV.Owner, i);
            }
        }
    }

    #endregion

    #region Start Turn

    [PunRPC]
    public void startTurn()
    {
        turnBtn.SetActive(true);

        playerMoved = 0;

        PlayerController.instance.mode = "select";

        //timer
        timeCoroutine = StartCoroutine(nameof(timer));

        //reset all vars
        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("EndTurn", false);
        playerProperties.Add("Spawned", false);
        playerProperties.Add("Attacked", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        //reset movement;
        foreach (Troop troop in PlayerController.instance.allTroops)
        {
            troop.moved = false;
        }
    }

    public void endTurn()
    {
        PlayerController.instance.mode = "";

        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }

        timerText.text = "Waiting for opponents...";

        turnBtn.SetActive(false);

        if (PhotonNetwork.OfflineMode)
        {
            allPlayers[0].spawn();
        }
        else
        {
            //ask master client to count player
            Hashtable playerProperties = new Hashtable();
            playerProperties.Add("EndTurn", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }
    }

    IEnumerator timer()
    {
        int time = 10 * (PlayerController.instance.age + 1);

        for (int i = 0; i < time; i++)
        {
            timerText.text = "Time Left: " + (time - i) + " seconds";

            yield return new WaitForSeconds(1f);
        }

        endTurn();
    }

    #endregion

    #region TakeTurn

    public void checkEndTurn()
    {
        //everyone is ready
        var players = PhotonNetwork.PlayerList;
        if (players.All(p => p.CustomProperties.ContainsKey("EndTurn") && (bool)p.CustomProperties["EndTurn"]))
        {
            PV.RPC(nameof(updateTimeText), RpcTarget.All, "Take Turns...");

            //all players spawn
            foreach (PlayerController player in allPlayers)
            {
                player.PV.RPC("spawn", player.PV.Owner);
            }
        }
    }

    public void checkSpawn()
    {
        //everyone is ready
        var players = PhotonNetwork.PlayerList;
        if (players.All(p => p.CustomProperties.ContainsKey("Spawned") && (bool)p.CustomProperties["Spawned"]))
        {
            //all players spawn
            allPlayers[playerMoved].PV.RPC("troopMove", allPlayers[playerMoved].PV.Owner);
        }
    }

    public void checkMove()
    {
        playerMoved++;

        //all player moved
        if (playerMoved == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            //different player start every turn
            allPlayers.Add(allPlayers[0]);
            allPlayers.RemoveAt(0);

            //all players attack
            foreach (PlayerController player in allPlayers)
            {
                player.PV.RPC(nameof(player.troopAttack), player.PV.Owner);
            }
        }
        else
        {
            //next player move
            allPlayers[playerMoved].PV.RPC("troopMove", allPlayers[playerMoved].PV.Owner);
        }
    }

    public void checkAttack()
    {
        //everyone is ready
        var players = PhotonNetwork.PlayerList;
        if (players.All(p => p.CustomProperties.ContainsKey("Attacked") && (bool)p.CustomProperties["Attacked"]))
        {
            //all players check dead troop
            foreach (PlayerController player in allPlayers)
            {
                player.PV.RPC(nameof(player.checkDeath), player.PV.Owner);
            }

            PV.RPC(nameof(startTurn), RpcTarget.AllViaServer);
        }
    }

    public void updateGoldText()
    {
        goldText.text = "Gold: " + PlayerController.instance.gold;
    }

    [PunRPC]
    void updateTimeText(string message)
    {
        timerText.text = message;
    }

    #endregion

    #region Age System

    public void ageAdvance()
    {
        //if enough gold
        if (PlayerController.instance.gold >= PlayerController.instance.goldNeedToAdvance)
        {
            PlayerController.instance.gold -= PlayerController.instance.goldNeedToAdvance;

            //modify age
            PlayerController.instance.age++;
            ageText.text = "Upgrade Age: " + PlayerController.instance.age;
            PlayerController.instance.goldNeedToAdvance *= 2;
            goldNeedToAdvanceText.text = PlayerController.instance.goldNeedToAdvance + " gold";
            goldText.text = "Gold: " + PlayerController.instance.gold;

            //age limit
            if (PlayerController.instance.age >= 5)
            {
                ageAdvanceBtn.SetActive(false);
            }

            //update building health
            PlayerController.instance.updateBuldingHealth();
        }
    }

    #endregion
}
