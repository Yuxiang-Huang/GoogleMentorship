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
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public PhotonView PV;

    public static PlayerController instance;

    public int id;

    Tile highlighted;

    public IUnit unitSelected;
    public SpawnInfo spawnInfoSelected;

    public string mode;

    public bool turnEnded;

    public List<Troop> allTroops = new List<Troop>();
    public List<Building> allBuildings = new List<Building>();
    public HashSet<Tile> territory = new HashSet<Tile>();

    public HashSet<Tile> toUpdateVisibility = new HashSet<Tile>();

    [Header("Spawn")]
    public bool[,] canSpawn;
    public Vector2[,] spawnDirection;

    public string toSpawnType;
    public string toSpawnPath;
    public GameObject toSpawnImage;
    public IUnit toSpawnUnit;
    public int goldNeedToSpawn;
    public Image curSpawnImage;
    public Dictionary<Vector2, SpawnInfo> spawnList = new Dictionary<Vector2, SpawnInfo>();

    [Header("Gold")]
    public int gold;
    public int age;
    public int goldNeedToAdvance;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();

        //keep track of all players
        GameManager.instance.playerList.Add(PV.OwnerActorNr, this);
        GameManager.instance.createPlayerList();

        //master client in charge making grid
        if (PhotonNetwork.IsMasterClient && PV.IsMine)
        {
            TileManager.instance.makeGrid();
        }

        if (!PV.IsMine) return;
        instance = this;
    }

    #region ID

    [PunRPC]
    public void startGame(int newID, Vector2Int spawnLocation)
    {
        //assign id
        id = newID;

        PV.RPC(nameof(startGame_all), RpcTarget.AllViaServer, newID);

        mode = "start";

        //assign starting territory
        Tile[,] tiles = TileManager.instance.tiles;

        Tile root = tiles[spawnLocation.x, spawnLocation.y];

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

        Tile newHighlighted = null;

        //tile at mousePosition
        if (TileManager.instance.tiles != null)
        {
            newHighlighted = TileManager.instance.getTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        //spawn castle in the start
        if (mode == "start")
        {
            //highlight revealed land tiles
            if (highlighted != newHighlighted)
            {
                if (highlighted != null)
                    highlighted.highlight(false);

                highlighted = newHighlighted;

                if (highlighted != null)
                {
                    if (!highlighted.dark.activeSelf && highlighted.terrain == "land")
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

                MainBase myBase = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Building/MainBase"),
                    TileManager.instance.getWorldPosition(highlighted), Quaternion.identity).
                    GetComponent<MainBase>();

                myBase.gameObject.GetPhotonView().RPC("Init", RpcTarget.All, id, startingTile.x, startingTile.y,
                     "Building/MainBase", age, -1);

                //update canSpawn
                canSpawn = new bool[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];
                spawnDirection = new Vector2[TileManager.instance.tiles.GetLength(0), TileManager.instance.tiles.GetLength(1)];
                myBase.GetComponent<Building>().updateCanSpawn();
                allBuildings.Add(myBase);

                myBase.PV.RPC(nameof(myBase.updateTerritory), RpcTarget.All);

                UIManager.instance.startGame();

                GameManager.instance.endTurn();
            }
        }
        //select
        else if (mode == "select")
        {
            //highlight any revealed
            if (highlighted != newHighlighted)
            {
                if (highlighted != null)
                {
                    highlighted.highlight(false);

                    if (highlighted.unit != null)
                        highlighted.unit.setHealthBar(false);
                }

                highlighted = newHighlighted;

                if (highlighted != null && !highlighted.dark.activeSelf)
                {
                    highlighted.highlight(true);
                }
                else
                {
                    highlighted = null;
                }
            }

            //show healthbar if there is a unit here
            if (highlighted != null && highlighted.unit != null)
            {
                highlighted.unit.setHealthBar(true);
            }

            //select unit when mouse pressed
            if (Input.GetMouseButtonDown(0))
            {
                //deselect if something is selected
                if (unitSelected != null)
                {
                    unitSelected.setImage(Color.white);

                    UIManager.instance.hideInfoTab();

                    unitSelected = null;
                }

                //if a tile is highlighted
                if (highlighted != null)
                {
                    //if a unit is on the tile and it's my unit
                    if (highlighted.GetComponent<Tile>().unit != null &&
                        highlighted.GetComponent<Tile>().unit.ownerID == id)
                    {
                        //don't show health bar
                        highlighted.unit.setHealthBar(false);

                        //update info tab
                        UIManager.instance.updateInfoTab(highlighted.unit);

                        //select unit
                        unitSelected = highlighted.GetComponent<Tile>().unit.gameObject.GetComponent<IUnit>();

                        //change color to show selection
                        unitSelected.setImage(Color.grey);

                        //if movable and turn not ended
                        if ((highlighted.GetComponent<Tile>().unit.gameObject.CompareTag("Troop"))
                            && !turnEnded)
                        {
                            mode = "move";
                        }
                    }
                    //if I am going to spawn a unit here
                    else if (spawnList.ContainsKey(highlighted.pos))
                    {
                        spawnInfoSelected = spawnList[highlighted.pos];
                        UIManager.instance.updateInfoTab(spawnInfoSelected);
                    }
                }
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
                {
                    highlighted.highlight(true);
                }
                else
                {
                    newHighlighted = null;
                }                    
            }

            if (Input.GetMouseButtonDown(0))
            {
                //findPath
                if (highlighted != null)
                {
                    highlighted.highlight(false);
                    unitSelected.gameObject.GetComponent<Troop>().findPath(highlighted.GetComponent<Tile>());
                }

                //deselect
                unitSelected.setImage(Color.white);
                unitSelected = null;
                UIManager.instance.hideInfoTab();

                highlighted = null;

                mode = "select";
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

                //if tile is not null and no unit is here and the tile is still my territory
                //and no units is going to be spawn here
                if (highlighted != null && highlighted.unit == null
                    && territory.Contains(highlighted) && !spawnList.ContainsKey(highlighted.pos))
                {
                    //for troops
                    if (toSpawnType == "Troop")
                    {
                        //can only spawn on spawnable tiles 
                        if (canSpawn[highlighted.pos.x, highlighted.pos.y])
                        {
                            highlighted.highlight(true);
                        }
                        else
                        {
                            highlighted = null;
                        }
                    }
                    //for buildings
                    else if (toSpawnType == "Building")
                    {
                        highlighted.highlight(true);
                    }
                }
                else
                {
                    highlighted = null;
                }
            }

            //click to spawn
            if (Input.GetMouseButtonDown(0))
            {
                //there is a highlighted tile and enough gold
                if (highlighted != null && gold >= goldNeedToSpawn)
                {
                    //deduct gold
                    gold -= goldNeedToSpawn;
                    UIManager.instance.updateGoldText();

                    //spawn an image
                    GameObject spawnImage = Instantiate(toSpawnImage,
                    highlighted.gameObject.transform.position, Quaternion.identity);

                    //add to spawn list
                    spawnList.Add(highlighted.pos, new SpawnInfo(highlighted, toSpawnPath, toSpawnUnit,
                        spawnImage, age, goldNeedToSpawn, goldNeedToSpawn / 2));

                    //reset to prevent double spawn
                    highlighted.highlight(false);
                    highlighted = null;
                }
                else
                {
                    //only change mode when didn't spawn correctly
                    mode = "select";

                    //clear selection
                    SpawnManager.instance.lastImage.GetComponent<Image>().color = Color.white;
                    SpawnManager.instance.lastImage = null;

                    //info tab
                    UIManager.instance.hideInfoTab();
                    UIManager.instance.sellBtn.SetActive(true);
                }
            }
        }
    }

    #region Turn

    public void stop()
    {
        if (mode == "spawn")
        {
            curSpawnImage.color = Color.white;
            UIManager.instance.sellBtn.SetActive(true);
        }

        else if (mode == "move")
        {
            unitSelected.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            unitSelected = null;
        }

        mode = "select";
        turnEnded = true;
    }

    [PunRPC]
    public void spawn()
    {
        //income from territory and all buildings
        gold += (territory.Count + allBuildings.Count - 1) * (age + GameManager.instance.ageIncomeOffset);

        UIManager.instance.updateGoldText();

        foreach (SpawnInfo info in spawnList.Values)
        {
            //spawn unit and initiate
            GameObject newUnit = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", info.unitName),
            info.spawnTile.gameObject.transform.position, Quaternion.identity);

            if (newUnit.CompareTag("Troop"))
            {
                newUnit.GetComponent<Troop>().PV.RPC("Init", RpcTarget.All,
                    id, info.spawnTile.pos.x, info.spawnTile.pos.y,
                    spawnDirection[info.spawnTile.pos.x, info.spawnTile.pos.y],
                    info.unitName, info.age, info.sellGold);

                allTroops.Add(newUnit.GetComponent<Troop>());
            }
            else if (newUnit.CompareTag("Building"))
            {
                newUnit.GetComponent<Building>().PV.RPC("Init", RpcTarget.All,
                    id, info.spawnTile.pos.x, info.spawnTile.pos.y,
                    info.unitName, age, info.sellGold);
                newUnit.GetComponent<Building>().updateCanSpawn();

                allBuildings.Add(newUnit.GetComponent<Building>());
            }

            Destroy(info.spawnImage);
        }

        //clear list
        spawnList = new Dictionary<Vector2, SpawnInfo>();

        if (PhotonNetwork.OfflineMode)
        {
            troopMove();
        }
        else
        {
            Hashtable playerProperties = new Hashtable();
            playerProperties.Add("Spawned", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties); ;
        }
    }

    [PunRPC]
    public void troopMove()
    {
        foreach (Troop troop in allTroops)
        {
            troop.move();
        }

        if (PhotonNetwork.OfflineMode)
        {
            troopAttack();
        }
        else
        {
            Hashtable playerProperties = new Hashtable();
            playerProperties.Add("Moved", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }
    }

    [PunRPC]
    public void troopAttack()
    {
        foreach (Troop troop in allTroops)
        {
            troop.attack();
        }

        if (PhotonNetwork.OfflineMode)
        {
            endCheck();
        }
        else
        {
            Hashtable playerProperties = new Hashtable();
            playerProperties.Add("Attacked", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties); ;
        }
    }

    [PunRPC]
    public void endCheck()
    {
        //troops
        foreach (Troop troop in allTroops)
        {
            troop.PV.RPC(nameof(troop.checkDeath), RpcTarget.All);
        }

        for (int i = allTroops.Count - 1; i >= 0; i--)
        {
            if (allTroops[i].health <= 0)
            {
                allTroops.Remove(allTroops[i]);
            }
        }

        //buildings
        foreach (Building building in allBuildings)
        {
            building.PV.RPC(nameof(building.checkDeath), RpcTarget.All);
        }

        for (int i = allBuildings.Count - 1; i >= 0; i--)
        {
            if (allBuildings[i].health <= 0)
            {
                allBuildings.Remove(allBuildings[i]);
            }
        }

        //visibility
        foreach (Tile neighbor in toUpdateVisibility)
        {
            neighbor.updateVisibility();
        }

        toUpdateVisibility = new HashSet<Tile>();

        if (PhotonNetwork.OfflineMode)
        {
            GameManager.instance.startTurn();
        }
    }

    #endregion

    #region Age

    //called when age increase
    public void updateExistingUnits()
    {
        foreach (Building building in allBuildings)
        {
            building.PV.RPC(nameof(building.ageUpdateInfo), RpcTarget.All, age);
        }

        //foreach (Troop troop in allTroops)
        //{
        //    troop.PV.RPC(nameof(troop.ageUpdateInfo), RpcTarget.All, age);
        //}
    }

    #endregion

    #region UI

    public void fillInfoTab(List<TextMeshProUGUI> playerInfo)
    {
        //name
        playerInfo[0].text = PV.Owner.NickName;

        //Color
        playerInfo[1].text = UIManager.instance.colorNameList[id];

        //Age
        playerInfo[2].text = UIManager.instance.ageNameList[age];

        //Gold
        playerInfo[3].text = "Gold: " + gold;

        //Troop
        playerInfo[4].text = "Troop: " + allTroops.Count;

        //Buliding
        playerInfo[5].text = "Building: " + allBuildings.Count;

        //Territory
        playerInfo[6].text = "Territory: " + territory.Count;
    }

    #endregion
}