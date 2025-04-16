using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update4
    const int playerCount = 2;
    public TMP_Text errorMessage;
    //public TMP_Text roomName
    public TMP_Text timerText;
    public float remainingTime;
    public bool isRoomFull = false;
    public bool isTimeFinish = true;
    public TMP_Text[] userNames;
    Player player;
    private bool isInLobby;
    void Start()
    {
        bool isSuccessfull = PhotonNetwork.ConnectUsingSettings();
        isInLobby = false;
        if (!isSuccessfull)
        {
            Debug.Log("Startnál elbukott");
            MenuManager.Instance.OpenMenu("ErrorMenu");
        }
        timerText.gameObject.SetActive(false);
    }
    private void FixedUpdate()
    {
        Timer();
    }
    public override void OnConnectedToMaster()
    {
        bool isConnected = PhotonNetwork.JoinLobby();
        if (!isConnected)
        {
            Debug.Log("OnConnectedToMaster()-nél elbukott");
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("we joint to the master");//hiba
    }
    public override void OnJoinedLobby()
    {
        isInLobby = true;
        MenuManager.Instance.OpenMenu("Default");
        CheckRooms();
        Debug.Log("We join to the OnJoinLobby");
    }
    public void CheckRooms()
    {
        if (!isInLobby) return;
        if (PhotonNetwork.CountOfPlayersOnMaster != 0 && ((PhotonNetwork.CountOfRooms / PhotonNetwork.CountOfPlayersOnMaster) == 0))
        {
            CreateRoom();
        }
    }
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = playerCount };
        PhotonNetwork.CreateRoom(null, roomOptions); // null -> automatikus név
        Debug.Log("Létrehoztunk egy szovát.");
        //MenuManager.Instance.OpenMenu("Loading");
        Debug.Log("Creating a new room...");
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Már létre van hozva a szoba!!!");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("ErrorMenu");
        errorMessage.text = "RoomJoined Failed" + message;
    }
    public void JoinRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.Server != ServerConnection.MasterServer)
        {
            Debug.LogError("Cannot join a random room. Not connected to Master Server.");
            return;
        }

        PhotonNetwork.JoinRandomRoom();
        MenuManager.Instance.OpenMenu("Loading");
        Debug.Log("Attempting to join a random room...");
    }
    public override void OnJoinedRoom()
    {
        PlayTheGame();
        Debug.Log("csatlakoztunk a szobához!");
        MenuManager.Instance.OpenMenu("Default");
       // roomName.text = PhotonNetwork.CurrentRoom.Name;
        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
        int index = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (index < userNames.Length)
            {
                userNames[index].text = player.NickName;
                index++;
            }
            Debug.Log(userNames[index]);
        }
        //CreateRoom();
        // joined a room successfully, both JoinRandomRoom or CreateRoom lead here on success
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("ErrorMenu");
        errorMessage.text = "RoomJoined Failed" + message;
        CreateRoom();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayTheGame();
        newPlayer.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
        Debug.Log("player entered the room");
        for (int i = 0; i < userNames.Length; i++)
        {
            if (userNames[i].text == "")
            {
                userNames[i].text = newPlayer.NickName;
                return;
            }
        }
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }
    public override void OnLeftRoom()
    {
        Debug.Log("We left the room");
        MenuManager.Instance.OpenMenu("Default");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            for (int i = 0; i < userNames.Length; i++)
            {
                if (userNames[i].text == otherPlayer.NickName)
                {
                    userNames[i].text = null;
                }
            }
        }
    }
    public void Timer()
    {
        if (isRoomFull)
        {
            if (remainingTime >= 0)
            {
                remainingTime -= Time.deltaTime;
                int seconds = Mathf.FloorToInt(remainingTime);
                timerText.text = string.Format("{0}", seconds);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                remainingTime = 0;
                isTimeFinish = false;
            }
        }
    }
    public void PlayTheGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == playerCount)
        {
           // SartGame.gameObject.SetActive(true);
            timerText.gameObject.SetActive(true);
            isRoomFull = true;
        }
    }
}
