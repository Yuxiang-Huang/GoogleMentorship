using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;
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
    public HashSet<Building> allBuildings = new HashSet<Building>();
    public HashSet<Tile> territory = new HashSet<Tile>();

    public Building myCastle;

    [Header("Spawn")]
    public bool[,] canSpawn;
    public Vector2[,] canSpawnDirection;
    public string toSpawn;
    public int goldNeedToSpawn;
    [SerializeField] List<SpawnInfo> spawnList = new List<SpawnInfo>();

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

        GameManager.instance.createPlayerList();

        if (!PV.IsMine) return;

        instance = this;
    }

    #region ID

    [PunRPC]
    public void startGame(int newID)
    {
        //assign id
        id = newID;
        PV.RPC(nameof(startGame_all), RpcTarget.AllViaServer, newID);

        mode = "start";

        //assign starting territory
        Tile[,] tiles = TileManager.instance.tiles;

        int startTerritory = 3;

        if (id == 1)
        {
            for (int i = 0; i < startTerritory; i++)
            {
                for (int j = 0; j < startTerritory; j++)
                {
                    tiles[i, j].updateStatus(id, null);
                }
            }
        }

        else if (id == 0)
        {
            for (int i = 0; i < startTerritory; i++)
            {
                for (int j = 0; j < startTerritory; j++)
                {
                    tiles[tiles.GetLength(0) - 1 - i, tiles.GetLength(0) - 1 - j].updateStatus(id, null);
                }
            }
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

        //testing purpose
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spawn();
            takeTurn_Player(); 
        }

        //spawn castle
        if (mode == "start")
        {
            //highlight territory tiles
            Tile newHighlighted = TileManager.instance.getTile();

            if (highlighted != newHighlighted)
            {
                if (highlighted != null)
                    highlighted.highlight(false);

                highlighted = newHighlighted;

                if (highlighted != null)
                {
                    //can only spawn on territory tiles and terrain is land
                    if (territory.Contains(highlighted) && highlighted.terrain == "land")
                    {
                        highlighted.highlight(true);
                    }
                    else
                    {
                        highlighted = null;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0) && highlighted != null)
            {
                Tile[,] tiles = TileManager.instance.tiles;

                //spawn castle
                Vector2Int startingTile = highlighted.pos;

                myCastle = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Building/Castle"),
                    TileManager.instance.getWorldPosition(tiles[startingTile.x, startingTile.y]), Quaternion.identity).
                    GetComponent<Building>();

                myCastle.gameObject.GetPhotonView().RPC("Init", RpcTarget.All, id, startingTile.x, startingTile.y);

                //only update canSpawn if my castle
                if (PV.IsMine)
                {
                    canSpawn = new bool[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];
                    canSpawnDirection = new Vector2[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];
                    myCastle.GetComponent<Building>().updateCanSpawn();
                    allBuildings.Add(myCastle);
                }

                mode = "move";
            }
        }
        //move
        else if (mode == "move")
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
                    //if a tile is highlighted, a unit is on the tile, it's my unit, and it's a movable unit
                    if (highlighted != null && highlighted.GetComponent<Tile>().unit != null &&
                        highlighted.GetComponent<Tile>().unit.ownerID == id &&
                        highlighted.GetComponent<Tile>().unit.gameObject.CompareTag("Troop"))
                    {
                        //select unit on the tile
                        playerSelected = highlighted.GetComponent<Tile>().unit.gameObject.GetComponent<Troop>();
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
        }
        //spawn
        else if (mode == "spawn")
        {
            //highlight spawnable tiles
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
                    GameManager.instance.updateGoldText();

                    spawnList.Add(new SpawnInfo(highlighted, toSpawn, 1));
                }
                mode = "move";
            }
        }
    }

    [PunRPC]
    public void spawn()
    {
        for (int i = spawnList.Count - 1; i >= 0; i --)
        {
            SpawnInfo info = spawnList[i];

            info.turn--;

            if (info.turn == 0)
            {
                //spawn unit and initiate
                GameObject newUnit = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", info.unitName),
                info.spawnTile.gameObject.transform.position, Quaternion.identity);

                if (newUnit.CompareTag("Troop"))
                {
                    newUnit.GetComponent<Troop>().PV.RPC("Init", RpcTarget.AllViaServer,
                        id, info.spawnTile.pos.x, info.spawnTile.pos.y,
                        canSpawnDirection[info.spawnTile.pos.x, info.spawnTile.pos.y]);
                }

                spawnList.Remove(info);
            }

            //building code here 
        }
    }

    [PunRPC]
    public void takeTurn_Player()
    {
        gold += territory.Count;

        GameManager.instance.updateGoldText();

        foreach (Troop troop in allTroops)
        {
            troop.takeTurn();
        }

        GameManager.instance.PV.RPC("takeTurn", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void deadTroop()
    {
        foreach (Troop troop in allTroops)
        {
            troop.PV.RPC(nameof(troop.checkAlive), RpcTarget.All);
        }
    }
}