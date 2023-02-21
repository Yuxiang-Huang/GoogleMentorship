using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public PhotonView PV;

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

    public void checkStart()
    {
        //everyone joined
        if (playerList.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            //sorted list
            foreach (KeyValuePair<int, PlayerController> kvp in playerList)
            { 
                allPlayers.Add(kvp.Value);
            }

            //start game once
            if (PhotonNetwork.IsMasterClient)
            {
                //ask all player to start game
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    allPlayers[i].PV.RPC("startGame", allPlayers[i].PV.Owner, i);
                }
            }
        }
    }

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

            //ask every player to next turn
            foreach (PlayerController cur in allPlayers)
            {
                cur.PV.RPC(nameof(cur.nextTurn), cur.PV.Owner);
            }

            //different player start every turn
            allPlayers.Add(allPlayers[0]);
            allPlayers.RemoveAt(0);

            PV.RPC(nameof(startTurn), RpcTarget.AllBuffered);
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
