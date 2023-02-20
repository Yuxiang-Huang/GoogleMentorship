using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    public static GameManager instance;

    public SortedDictionary<int, PlayerController> playerList = new SortedDictionary<int, PlayerController>();

    public List<PlayerController> allPlayers;

    [SerializeField] TextMeshProUGUI goldText;

    private void Awake()
    {
        instance = this;

        PV = GetComponent<PhotonView>();

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    public void checkStart()
    {
        //assign ID
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
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    allPlayers[i].PV.RPC("startGame", allPlayers[i].PV.Owner, i);
                }
            }
        }
    }

    public void updateGoldText()
    {
        goldText.text = "Gold: " + PlayerController.instance.gold;
    }
}
