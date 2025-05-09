using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update4
    const int playerCount = 3;
    public TMP_Text errorMessage;
    public TMP_Text roomName;
    public TMP_Text timerText;
    public float remainingTime=30;
    public bool isRoomFull = false;
    public bool isTimeFinish = true;
    public TMP_Text[] userNames;
    Player player;
    private bool isInLobby;
    PhotonView _photonView;
    Coroutine countdownCoroutine;
    private bool isPlayerLeftRoom; 
    
    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        bool isSuccessfull = PhotonNetwork.ConnectUsingSettings();
        isInLobby = false;
        if (!isSuccessfull)
        {
            MenuManager.Instance.OpenMenu("ErrorMenu");
        }
        timerText.gameObject.SetActive(false);
        isPlayerLeftRoom = false;
    }

    public override void OnConnectedToMaster()
    {
        bool isConnected = PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        isInLobby = true;
        MenuManager.Instance.OpenMenu("Default");
    }

    public void CheckRooms()
    {
        if (!isInLobby) return;
        JoinRoom();
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = playerCount };
        PhotonNetwork.CreateRoom(null, roomOptions); // null -> automatikus név
    }

    //public override void OnCreatedRoom()
    //{
    //    Debug.Log("Már létre van hozva a szoba!!!");
    //}

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("ErrorMenu");
        errorMessage.text = "RoomJoined Failed" + message;
    }

    public void JoinRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady == false || PhotonNetwork.Server != ServerConnection.MasterServer || !isInLobby)
        {
            return;
        }

        PhotonNetwork.JoinRandomRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom()
    {
        
        MenuManager.Instance.OpenMenu("Default");
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        PhotonNetwork.NickName = "Player" + UnityEngine.Random.Range(0, 1000).ToString("0000");
        int index = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (index < userNames.Length)
            {
                userNames[index].text = player.NickName;
                _photonView.RPC("SyncPlayerName", RpcTarget.All, player.NickName, index);
                index++;
            }
        }

    }

    [PunRPC]
    void SyncPlayerName(string nickName, int index)
    {
        if (index >= 0 && index < userNames.Length)
        {
            userNames[index].text = nickName;
        }
    }
    
    [PunRPC]
    void SyncPlayerLeftRoom(string nickName, int index)
    {
        Debug.Log(index);
        if (index >= 0 && index < userNames.Length)
        {
            if (userNames[index].text == nickName)
            {
                userNames[index].text = "";
            }
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("ErrorMenu");
        errorMessage.text = "RoomJoined Failed" + message;
        CreateRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //PlayTheGame();
        newPlayer.NickName = "Player" + UnityEngine.Random.Range(0, 1000).ToString("0000");
        Debug.Log("player entered the room");
        for (int i = 0; i < userNames.Length; i++)
        {
            if (userNames[i].text == "")
            {
                userNames[i].text = newPlayer.NickName;
                return;
            }
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == playerCount)
        {
            _photonView.RPC("StartCountdown", RpcTarget.All);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
        StopCoroutine(StartCountdown(30f));
        isPlayerLeftRoom=true;
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("Default");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        remainingTime = 30f;
        if (player == otherPlayer)
        {
            _photonView.RPC("SyncPlayerLeftRoom", RpcTarget.All, otherPlayer.NickName);
        }
        StopCountdown();
    }

    [PunRPC]
    public void StartCountdown()
    {
        if (!isRoomFull) // csak ha még nem indult el
        {
            countdownCoroutine = StartCoroutine(StartCountdown(5f)); // 30 másodperces visszaszámlálás
        }
    }

    IEnumerator StartCountdown(float time)
    {
        if (isPlayerLeftRoom) yield break;
        timerText.gameObject.SetActive(true);
        isRoomFull = true;
        isTimeFinish = true;
        remainingTime = time;

        while (remainingTime > 0)
        {
            timerText.text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f); // vár 1 másodpercet
            remainingTime -= 1f;
        }
        if (!isPlayerLeftRoom)
        {
            isTimeFinish = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            isPlayerLeftRoom = true;
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
            timerText.gameObject.SetActive(false); // opcionális UI cleanup
            Debug.Log("Visszaszámlálás megszakítva.");
        }
    }


    public void PlayTheGame()
    {
        if (PhotonNetwork.CurrentRoom is not null && PhotonNetwork.CurrentRoom.PlayerCount == playerCount)
        {
            JoinRoom();
            _photonView.RPC(
                "StartCountdown"
                , RpcTarget.All);
        }
    }
}
