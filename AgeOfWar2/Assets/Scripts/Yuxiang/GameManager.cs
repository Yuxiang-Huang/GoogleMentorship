using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public PhotonView PV;

    public SortedDictionary<int, PlayerController> playerList = new SortedDictionary<int, PlayerController>();

    public List<PlayerController> allPlayers;

    [SerializeField] GameObject turnBtn;

    [SerializeField] int playerMoved;


    [SerializeField] TextMeshProUGUI goldText;

    [SerializeField] Coroutine timeCoroutine;

    [SerializeField] TextMeshProUGUI timerText;

    private void Awake()
    {
        instance = this;
        PV = GetComponent<PhotonView>();

        //not able to access after game begins
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
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
                allPlayers.Add(kvp.Value);
            }
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

        //timer
        timeCoroutine = StartCoroutine(nameof(timer));

        //reset all vars
        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("EndTurn", false);
        playerProperties.Add("Spawned", false);
        playerProperties.Add("Attacked", false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public void endTurn()
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }

        timerText.text = "Take Turn...";

        turnBtn.SetActive(false);

        //ask master client to count player
        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("EndTurn", true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    IEnumerator timer()
    {
        for (int i = 0; i < 10; i++)
        {
            timerText.text = "Time Left: " + (10 - i) + " seconds";

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
            //all players spawn
            foreach (PlayerController player in allPlayers)
            {
                player.PV.RPC(nameof(player.spawn), player.PV.Owner);
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
                player.PV.RPC(nameof(player.checkTroopDeath), player.PV.Owner);
            }

            PV.RPC(nameof(startTurn), RpcTarget.AllViaServer);
        }
    }

    public void updateGoldText()
    {
        goldText.text = "Gold: " + PlayerController.instance.gold;
    }

    #endregion
}
