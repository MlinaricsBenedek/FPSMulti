using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoard : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform container;
    [SerializeField] GameObject scoreBoardItemPrefab;
    [SerializeField] CanvasGroup CanvasGroup;

    public override void OnJoinedRoom()
    {
        CanvasGroup.alpha = 0;
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            AddScoredBoard(player);
        }
    }

    void AddScoredBoard(Player player)
    { 
        ScoreBoardItem prefab = Instantiate(scoreBoardItemPrefab, container).GetComponent<ScoreBoardItem>();
        prefab.Initialized(player);

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AddScoredBoard(newPlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CanvasGroup.alpha = 1;
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            CanvasGroup.alpha = 0;
        }
    }
}
