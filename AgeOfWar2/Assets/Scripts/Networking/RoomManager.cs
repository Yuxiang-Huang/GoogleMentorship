using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [SerializeField] TextMeshProUGUI mapSettingText;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        //Debug.Log(scene.buildIndex);

        if (scene.buildIndex == 1)// game scene
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player/PlayerManager"), Vector3.zero, Quaternion.identity);
        }
    }

    //change map setting
    public void changeMapSetting()
    {
        bool hasWater = !(bool)PhotonNetwork.CurrentRoom.CustomProperties["Water"];

        if (hasWater)
        {
            mapSettingText.text = "Water: On";
        }
        else
        {
            mapSettingText.text = "Water: Off";
        }

        Hashtable hash = new Hashtable();
        hash.Add("Water", hasWater);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }
}
