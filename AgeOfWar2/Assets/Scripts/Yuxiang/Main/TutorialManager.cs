using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] Canvas tutorialCanvas;
    [SerializeField] List<TextMeshProUGUI> instructions;
    [SerializeField] int index;

    private void Start()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Tutorial") &&
            (bool)PhotonNetwork.CurrentRoom.CustomProperties["Tutorial"]){
            tutorialCanvas.gameObject.SetActive(true);
        }
        else
        {
            tutorialCanvas.gameObject.SetActive(false);
        }
    }

    public void changeInstruction(int dir)
    {
        instructions[index].gameObject.SetActive(false);
        index += dir;
        instructions[index].gameObject.SetActive(true);
    }

    public void endTutorial()
    {
        Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.LoadLevel(0);
    }
}
