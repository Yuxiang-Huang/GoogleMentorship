using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

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

    [PunRPC]
    public void getReady()
    {
        //twice for each player to check playerList created and tiles created
        playerCount++;
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

            GameManager.instance.PV.RPC("getReady", RpcTarget.MasterClient);
        }
    }

    public void checkStart()
    {
        //master client start game once
        if (playerCount == PhotonNetwork.CurrentRoom.PlayerCount * 2)
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

            //every player spawn
            foreach (PlayerController player in allPlayers)
            {
                player.PV.RPC(nameof(player.spawn), player.PV.Owner);
            }

            //players takes turn one at a time
            takeTurn();
        }
    }

    [PunRPC]
    public void takeTurn()
    {
        //each player take turn and then call back to prevent collision
        playerCount++;

        if (playerCount < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            allPlayers[playerCount].PV.RPC("takeTurn_Player", allPlayers[playerCount].PV.Owner);
        }
        else
        {
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
