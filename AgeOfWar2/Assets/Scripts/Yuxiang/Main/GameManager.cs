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

    [SerializeField] int playerMoved;

    public int ageIncomeOffset;
    public int ageCostFactor;

    private void Awake()
    {
        instance = this;
        PV = GetComponent<PhotonView>();

        bool offlineMode = true;

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
        playerMoved = 0;

        PlayerController.instance.turnEnded = false;

        UIManager.instance.startTurn();

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
        //stop action of player
        PlayerController.instance.stop();

        UIManager.instance.endTurn();

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

    #endregion

    #region TakeTurn

    public void checkEndTurn()
    {
        //everyone is ready
        var players = PhotonNetwork.PlayerList;
        if (players.All(p => p.CustomProperties.ContainsKey("EndTurn") && (bool)p.CustomProperties["EndTurn"]))
        {
            UIManager.instance.PV.RPC(nameof(UIManager.instance.updateTimeText), RpcTarget.All, "Take Turns...");

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

    #endregion
}
