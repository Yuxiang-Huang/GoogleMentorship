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

    [SerializeField] int playerCount;

    public SortedDictionary<int, PlayerController> playerList = new SortedDictionary<int, PlayerController>();

    public List<PlayerController> allPlayers;

    [SerializeField] TextMeshProUGUI goldText;

    [SerializeField] int playerEndedTurn;

    [SerializeField] GameObject turnBtn;

    private void Awake()
    {
        instance = this;
        PV = GetComponent<PhotonView>();

        //not able to access after game begins
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    #region Begin Game

    //call when each player is ready
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!changedProps.ContainsKey("Ready")) return;

        checkStart();
    }

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

    #region Turns

    [PunRPC]
    public void checkTurn()
    {
        playerEndedTurn++;

        //everyone is ready
        if (playerEndedTurn == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            //reset
            playerEndedTurn = 0;

            playerCount = -1;

            //all players spawn
            foreach (PlayerController player in allPlayers)
            {
                player.PV.RPC(nameof(player.spawn), player.PV.Owner);
            }

            takeTurn();
        }
    }

    [PunRPC]
    public void takeTurn()
    {
        int numOfPlayer = PhotonNetwork.CurrentRoom.PlayerCount;

        playerCount++;

        //each player take turn and then call back to prevent collision
        if (playerCount >= numOfPlayer && playerCount < numOfPlayer * 2)
        {
            allPlayers[playerCount % numOfPlayer].PV.RPC("troopMove", allPlayers[playerCount % numOfPlayer].PV.Owner);
        }
        //all players attack
        else if (playerCount == numOfPlayer * 2)
        {
            for (int i = 0; i < allPlayers.Count; i++)
            {
                PlayerController player = allPlayers[i];

                player.PV.RPC(nameof(player.troopAttack), player.PV.Owner);
            }
        }
        else if (playerCount == numOfPlayer * 3)
        {
            //check dead troop
            foreach (PlayerController player in allPlayers)
            {
                player.PV.RPC(nameof(player.checkTroopDeath), player.PV.Owner);
            }

            //different player start every turn
            allPlayers.Add(allPlayers[0]);
            allPlayers.RemoveAt(0);

            PV.RPC(nameof(startTurn), RpcTarget.AllViaServer);
        }
    }

    [PunRPC]
    public void startTurn()
    {
        turnBtn.SetActive(true);
    }

    public void endTurn()
    {
        turnBtn.SetActive(false);

        //ask master client to count player
        PV.RPC("checkTurn", RpcTarget.MasterClient);
    }

    public void updateGoldText()
    {
        goldText.text = "Gold: " + PlayerController.instance.gold;
    }

    #endregion
}
