using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    public static GameManager instance;

    public List<PlayerController> allPlayers = new List<PlayerController>();
    
    private void Awake()
    {
        instance = this;

        PV = GetComponent<PhotonView>();

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    public void checkStart()
    {
        if (allPlayers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            for (int i = 0; i < allPlayers.Count; i++)
            {
                allPlayers[i].PV.RPC("updateID", allPlayers[i].PV.Owner, i + 1);
            }
        }
    }
}
