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

    public HashSet<Troop> allTroops = new HashSet<Troop>();
    public HashSet<Tile> territory = new HashSet<Tile>();

    [SerializeField] GameObject castle;
    public GameObject myCastle;

    [Header("Spawn")]
    public bool[,] canSpawn;
    public string toSpawn;
    public int goldNeedToSpawn;

    [Header("Gold")]
    public int gold;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        //master client in charge making grid
        if (PhotonNetwork.IsMasterClient && PV.IsMine)
        {
            TileManager.instance.makeGrid(10, 10);
        }

        //keep track of all players
        GameManager.instance.playerList.Add(PV.OwnerActorNr, this);

        GameManager.instance.checkStart();

        if (!PV.IsMine) return;

        instance = this;
    }

    #region ID

    [PunRPC]
    public IEnumerator startGame(int newID)
    {
        //assign id
        id = newID;
        PV.RPC(nameof(startGame_all), RpcTarget.AllBuffered, newID);

        //make sure map is spawned
        Tile[,] tiles = null;
        do
        {
            tiles = TileManager.instance.tiles;
            yield return new WaitForSeconds(1f);
        } while (tiles == null);

        //spawn castle
        Vector2Int startingTile = new Vector2Int(0, 0);

        if (id == 0)
        {
            startingTile = new Vector2Int(1, 1);
        }

        else if (id == 1)
        {
            startingTile = new Vector2Int(tiles.GetLength(0) - 2, tiles.GetLength(1) - 2);
        }

        myCastle = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Building/Castle"),
            TileManager.instance.getWorldPosition(tiles[startingTile.x, startingTile.y]), Quaternion.identity);

        myCastle.GetPhotonView().RPC("Init", RpcTarget.AllBuffered, id, startingTile.x, startingTile.y);

        //only update canSpawn if my castle
        if (PV.IsMine)
        {
            canSpawn = new bool[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];
            myCastle.GetComponent<Building>().updateCanSpawn();
        }
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
            //highlight any tile
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
                    //if a tile is highlighted, a unit is not on the tile, and it's a movable unit
                    if (highlighted != null && highlighted.GetComponent<Tile>().unit != null &&
                        highlighted.GetComponent<Tile>().unit.CompareTag("Troop"))
                    {
                        //select unit on the tile
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
                    }

                    playerSelected.highlight(false);
                    playerSelected = null;
                }
            }

            //testing purpose
            if (Input.GetKeyDown(KeyCode.Space))
            {
                nextTurn();
            }
        }
        //spawn
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
                    //deduct gold
                    gold -= goldNeedToSpawn;

                    //spawn unit and initiate
                    GameObject newUnit = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", toSpawn),
                    highlighted.gameObject.transform.position, Quaternion.identity);

                    highlighted.updateStatus(id, newUnit);

                    if (newUnit.CompareTag("Troop"))
                    {
                        newUnit.GetComponent<Troop>().PV.RPC("Init", RpcTarget.AllBuffered,
                            id, highlighted.pos.x, highlighted.pos.y);
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

        GameManager.instance.updateGoldText();

        foreach (Troop troop in allTroops)
        {
            troop.takeTurn();
        }
    }
}