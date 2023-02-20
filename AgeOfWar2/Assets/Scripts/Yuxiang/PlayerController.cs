using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Photon.Pun;
using System.IO;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    public static PlayerController instance;

    public int id;

    Tile highlighted;

    Troop playerSelected;

    public string mode;

    public List<Troop> allTroops = new List<Troop>();
    public List<Tile> territory = new List<Tile>();

    [SerializeField] GameObject castle;
    public GameObject myCastle;

    [Header("Spawn")]
    public bool[,] canSpawn;
    public string toSpawn;
    public int goldNeedToSpawn;

    [Header("Gold")]
    [SerializeField] int gold;

    private void Awake()
    {
        //master client in charge of all players
        if (PhotonNetwork.IsMasterClient)
        {
            //make grid once
            if (PV.IsMine)
            {
                TileManager.instance.makeGrid(10, 10);
            }
            GameManager.instance.allPlayers.Add(this);
            GameManager.instance.checkStart();
        }

        if (!PV.IsMine) return;

        instance = this;
    }

    #region ID

    [PunRPC]
    public void startGame(int newID)
    {
        id = newID;

        //spawn castle
        Vector2Int pos = new Vector2Int(0, 0);

        Tile[,] tiles = TileManager.instance.tiles;

        //spawn castle
        if (id == 1)
        {
            pos = new Vector2Int(1, 1);
        }

        else if (id == 2)
        {
            pos = new Vector2Int(tiles.GetLength(0) - 2, tiles.GetLength(1) - 2);
        }

        canSpawn = new bool[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];

        myCastle = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Building/Castle"),
            TileManager.instance.getWorldPosition(tiles[pos.x, pos.y]), Quaternion.identity);
        myCastle.GetComponent<Building>().Init(TileManager.instance.tiles[pos.x, pos.y], canSpawn);
        TileManager.instance.tiles[pos.x, pos.y].updateStatus(this, myCastle);

        PV.RPC(nameof(startGame_all), RpcTarget.AllBuffered, newID);
    }

    [PunRPC]
    void startGame_all(int newID)
    {
        id = newID;
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        //move
        if (mode == "move")
        {
            //highlight
            Tile newHighlighted = TileManager.instance.getTile();

            if (highlighted != newHighlighted)
            {
                if (highlighted != null)
                    highlighted.highlight(false);

                highlighted = newHighlighted;

                if (newHighlighted != null)
                    highlighted.highlight(true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                //select player
                if (playerSelected == null)
                {
                    if (highlighted != null && highlighted.GetComponent<Tile>().unit != null)
                    {
                        playerSelected = highlighted.GetComponent<Tile>().unit.GetComponent<Troop>();
                        playerSelected.highlight(true);
                    }
                }
                //findPath
                else
                {
                    if (highlighted != null)
                    {
                        playerSelected.findPath(highlighted.GetComponent<Tile>());
                        playerSelected.highlight(false);
                        playerSelected = null;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                gold += territory.Count;

                foreach (Troop troop in allTroops)
                {
                    troop.move();
                }
            }
        }
        else if (mode == "spawn")
        {
            //highlight
            Tile newHighlighted = TileManager.instance.getTile();

            if (highlighted != newHighlighted)
            {
                if (highlighted != null) 
                    highlighted.highlight(false);

                highlighted = newHighlighted;

                if (highlighted != null)
                {
                    //can only spawn on spawnable tiles and no unit and terrain is not water
                    if (canSpawn[highlighted.pos.x, highlighted.pos.y] && highlighted.unit == null
                        && highlighted.terrain != "water")
                    {
                        highlighted.highlight(true);
                    }
                    else
                    {
                        highlighted = null;
                    }
                }
            }

            //click to spawn
            if (Input.GetMouseButtonDown(0))
            {
                if (highlighted != null)
                {
                    gold -= goldNeedToSpawn;

                    //spawn unit and relation tile and unit
                    GameObject newUnit = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", toSpawn),
                    highlighted.gameObject.transform.position, Quaternion.identity);

                    highlighted.updateStatus(this, newUnit);

                    if (newUnit.GetComponent<Troop>() != null)
                    {
                        newUnit.GetComponent<Troop>().Init(this, highlighted);
                    }

                    //building code here 
                }

                mode = "move";
            }
        }
    }

    public void nextTurn()
    {
        gold += territory.Count;

        foreach (Troop troop in allTroops)
        {
            troop.move();
        }
    }
}