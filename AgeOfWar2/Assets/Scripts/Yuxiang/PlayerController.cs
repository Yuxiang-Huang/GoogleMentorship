using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.IO;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    public static PlayerController instance;

    public int id;

    Tile highlighted;

    Troop playerSelected;

    public string mode;

    public List<Troop> allTroops = new List<Troop>();
    public HashSet<Building> allBuildings = new HashSet<Building>();
    public HashSet<Tile> territory = new HashSet<Tile>();

    public Building myCastle;

    [Header("Spawn")]
    public bool[,] canSpawn;
    public Vector2[,] canSpawnDirection;
    public string toSpawn;
    public GameObject toSpawnImage;
    public int goldNeedToSpawn;
    [SerializeField] List<SpawnInfo> spawnList = new List<SpawnInfo>();
    [SerializeField] HashSet<Vector2> spawnLocations = new HashSet<Vector2>();

    [Header("Gold")]
    public int gold;

    [Header("Turn")]
    [SerializeField] int spawnNum;
    [SerializeField] int troopNum;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        //master client in charge making grid
        if (PhotonNetwork.IsMasterClient && PV.IsMine)
        {
            TileManager.instance.makeGrid(19, 6);
        }

        //keep track of all players
        //GameManager.instance.playerList.Add(PV.OwnerActorNr, this);
        //GameManager.instance.createPlayerList();

        if (!PV.IsMine) return;
        instance = this;
    }

    #region ID

    [PunRPC]
    public void startGame(int newID)
    {
        //assign id
        id = newID;
        //PV.RPC(nameof(startGame_all), RpcTarget.AllViaServer, newID);

        mode = "start";

        //assign starting territory
        Tile[,] tiles = TileManager.instance.tiles;

        int xOffset = 8;

        int yOffset = 2;

        if (id == 0)
        {
            Tile root = tiles[xOffset, yOffset + 1];

            root.setDark(true);

            foreach (Tile neighbor in root.neighbors)
            {
                neighbor.setDark(false);
            }

            foreach (Tile neighbor in root.neighbors2)
            {
                neighbor.setDark(false);
            }
        }

        else if (id == 1)
        {
            Tile root = tiles[tiles.GetLength(0) - 1 - xOffset, tiles.GetLength(1) - 1 - yOffset];

            root.setDark(false);

            foreach (Tile neighbor in root.neighbors)
            {
                neighbor.setDark(false);
            }

            foreach (Tile neighbor in root.neighbors2)
            {
                neighbor.setDark(false);
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
        }

        Tile newHighlighted = null;

        //tile at mousePosition
        if (TileManager.instance.tiles != null)
        {
            newHighlighted = TileManager.instance.getTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        //spawn castle
        if (mode == "start")
        {
            //highlight territory tiles
            if (highlighted != newHighlighted)
            {
                if (highlighted != null)
                    highlighted.highlight(false);

                highlighted = newHighlighted;

                if (highlighted != null)
                {
                    if (territory.Contains(highlighted))
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
                    TileManager.instance.getWorldPosition(highlighted), Quaternion.identity).
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
                        playerSelected.gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
                    }
                }
                //findPath
                else
                {
                    if (highlighted != null)
                    {
                        playerSelected.findPath(highlighted.GetComponent<Tile>());
                    }

                    //deselect
                    playerSelected.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    playerSelected = null;
                }
            }
        }
        //spawn
        else if (mode == "spawn")
        {
            //highlight spawnable tiles
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
                //can't spawn in a tile where there is already a unit going to be spawned
                if (highlighted != null && !spawnLocations.Contains(highlighted.pos))
                {
                    //deduct gold
                    gold -= goldNeedToSpawn;
                    GameManager.instance.updateGoldText();

                    //spawn an image
                    GameObject spawnImage = Instantiate(toSpawnImage,
                    highlighted.gameObject.transform.position, Quaternion.identity);

                    //add to spawn list
                    spawnList.Add(new SpawnInfo(highlighted, toSpawn, 1, spawnImage));

                    spawnLocations.Add(highlighted.pos);
                }
                else
                {
                    //only change mode when didn't spawn correctly
                    mode = "move";

                    //clear selection
                    SpawnManager.instance.lastImage.GetComponent<Image>().color = Color.white;
                    SpawnManager.instance.lastImage = null;
                }
            }
        }
    }

    #region Turn

    [PunRPC]
    public void spawn()
    {
        gold += territory.Count;

        GameManager.instance.updateGoldText();

        for (int i = spawnList.Count - 1; i >= 0; i --)
        {
            SpawnInfo info = spawnList[i];

            info.turn--;

            //time to spawn
            if (info.turn == 0)
            {
                //spawn unit and initiate
                GameObject newUnit = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", info.unitName),
                info.spawnTile.gameObject.transform.position, Quaternion.identity);

                if (newUnit.CompareTag("Troop"))
                {
                    newUnit.GetComponent<Troop>().PV.RPC("Init", RpcTarget.All,
                        id, info.spawnTile.pos.x, info.spawnTile.pos.y,
                        canSpawnDirection[info.spawnTile.pos.x, info.spawnTile.pos.y]);

                    allTroops.Add(newUnit.GetComponent<Troop>());
                }

                Destroy(info.spawnImage);

                spawnList.Remove(info);

                spawnLocations.Remove(info.spawnTile.pos);
            }

            //building code here 
        }

        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("Spawned", true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    [PunRPC]
    public void troopMove()
    {
        foreach (Troop troop in allTroops)
        {
            troop.move();
        }

        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("Moved", true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    [PunRPC]
    public void troopAttack()
    {
        foreach (Troop troop in allTroops)
        {
            troop.attack();
        }

        Hashtable playerProperties = new Hashtable();
        playerProperties.Add("Attacked", true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    [PunRPC]
    public void checkTroopDeath()
    {
        foreach (Troop troop in allTroops)
        {
            troop.PV.RPC(nameof(troop.checkDeath), RpcTarget.All);
        }

        for (int i = allTroops.Count - 1; i >= 0; i --)
        {
            if (allTroops[i].health <= 0)
            {
                allTroops.Remove(allTroops[i]);
            }
        }
    }

    #endregion
}