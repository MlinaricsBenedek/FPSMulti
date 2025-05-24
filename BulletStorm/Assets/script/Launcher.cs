using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviourPunCallbacks
{
    public TMP_Text errorMessage;

    public TMP_Text timerText;
    public float remainingTime = 30;
    public bool isRoomFull = false;
    public bool isTimeFinish = true;
    public TMP_Text[] userNames;

    private bool isInLobby;
    PhotonView _photonView;
    Coroutine countdownCoroutine;
    private bool isPlayerLeftRoom;
    List<Player> lobby = new List<Player>();
    const int tolarence = 70;
    const int playerCount = 6;
    float playerElo = 0f;
    string currentRoomName;
    string prevRoomName;
    bool isJoiningRoom;
    bool basicClient;
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
        isJoiningRoom=false;
        basicClient = false;
    }
   
    public override void OnConnectedToMaster()
    {
        
        bool isConnected = PhotonNetwork.JoinLobby();
        if (isJoiningRoom)
        {
            CreateRoom(currentRoomName);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        isInLobby = true;
        
        MenuManager.Instance.OpenMenu("Default");
    }

    public void CreateRoom(string roomName)
    {
        if (PhotonNetwork.CurrentRoom.Name == "Lobby" && PhotonNetwork.InRoom)
        {
            prevRoomName = "Lobby";
            PhotonNetwork.LeaveRoom();
        }
        if (basicClient == true)
        {
            PhotonNetwork.JoinRoom(currentRoomName);
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = playerCount;
        
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions,TypedLobby.Default); 
    }

    public void AddPlayer()
    {
        Debug.Log($"[Lobby] {PhotonNetwork.LocalPlayer.NickName} belépett a lobbyba. Jelenlegi lobby létszám: {lobby.Count}");
        bool eloExist = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("ELO", out object ELO);
        if (!eloExist)
        {
            Debug.Log("There are no elo");
            return;
        }
        float elo = (float)ELO;
       

        var combinations = MatchMaker.Instance.FindMatchingTeams(PhotonNetwork.LocalPlayer, lobby);
        float minELO = float.MaxValue;
        float maxELO = 0;
        foreach ( var combination in combinations ) 
        {
            Debug.Log(combinations.Count);
            if (minELO - elo > -tolarence && maxELO - elo < tolarence)
            {
                Debug.Log("megfelelõ találat:" );
                List<Player> redTeam;
                List<Player> blueTeam;
                List<Player> teams = TeamController.CreateTeams(combination, out blueTeam, out redTeam);
                for (int i = 0; i < teams.Count; i++)
                {
                    if (i < 3)
                    {
                        redTeam.Add(teams[i]);
                    }
                    else
                    {
                        blueTeam.Add(teams[i]);
                    }
                }
                CreateMatch(blueTeam, redTeam);

                foreach (var playerInTheMatch in combination)
                {
                    lobby.Remove(playerInTheMatch);
                }
            }        
        }
    }

    [PunRPC]
    void RPC_JoinLobby(PhotonMessageInfo info)
    {
        Player requestingPlayer = info.Sender;
        lobby.Add(requestingPlayer);

        AddPlayer();
    }

    public void CreateMatch(List<Player> blueTeam,List<Player> redTeam)
    {
        currentRoomName = $"Match_{Guid.NewGuid().ToString().Substring(0, 5)}";
        Debug.Log($"[Photon] Szoba létrehozása: {currentRoomName}");
        foreach (var player in redTeam)
        {
            if (player == PhotonNetwork.LocalPlayer)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props["team"] = "redTeam";
                player.SetCustomProperties(props);
            }
            else
            {
                photonView.RPC("RPC_SetTeams", player, "red");
            }
        }

        foreach (var player in blueTeam)
        {
            if (player == PhotonNetwork.LocalPlayer)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props["team"] = "blueTeam";
                props["currentRoomName"] = currentRoomName;
                player.SetCustomProperties(props);
            }
            else
            {
                photonView.RPC("RPC_SetTeams", player, "blue");
            }
        }
        if (PhotonNetwork.CurrentRoom.Name =="Lobby"&&PhotonNetwork.InRoom)
        {
            prevRoomName = "Lobby";
            PhotonNetwork.LeaveRoom(); 
        }
      

        foreach (var player in redTeam.Concat(blueTeam))
        {
            if (player != PhotonNetwork.LocalPlayer)
            {
                _photonView.RPC("GetRoomByName", player, currentRoomName);
            }
        }
    }

    public void HandleLobbyRoom(float elo)
    {
        RoomOptions roomOptions = new RoomOptions();
        TypedLobby typedLobby = new TypedLobby("Lobby",LobbyType.Default);
        PhotonNetwork.JoinOrCreateRoom("lobby",roomOptions,typedLobby);
    }


    [PunRPC]
    void GetRoomByName(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    [PunRPC]
    void RPC_SetTeams(string team)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "team", team } });
    }

    public async void GetPlayerElo()
    {
        
        playerElo = await ApiHandler.instance.GetUserStatisticsAsync(TokenController.Token);
        if (playerElo == 0)
        {
            Debug.Log("There no elo");
        }
        Debug.Log("elo" + playerElo);
        ExitGames.Client.Photon.Hashtable ELO = new ExitGames.Client.Photon.Hashtable();
        ELO["ELO"] = playerElo;
        PhotonNetwork.LocalPlayer.SetCustomProperties(ELO);
        HandleLobbyRoom(playerElo);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.Instance.OpenMenu("ErrorMenu");
        errorMessage.text = "RoomJoined Failed" + message;
    }
    
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.Name == "lobby")
        {
            _photonView.RPC("RPC_JoinLobby", RpcTarget.MasterClient);
        }
        else
        {

            MenuManager.Instance.OpenMenu("Default");
            PhotonNetwork.NickName = UserInformations.Name;
            int index = 0;
            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (index < userNames.Length)
                {
                    userNames[index].text = player.NickName;
                    _photonView.RPC("RPC_SyncPlayerName", RpcTarget.All, player.NickName, index);
                    index++;
                }
            }
        }

    }

    [PunRPC]
    void RPC_SyncPlayerName(string nickName, int index)
    {
        if (index >= 0 && index < userNames.Length)
        {
            userNames[index].text = nickName;
        }
    }
    
    [PunRPC]
    void RPC_SyncPlayerLeftRoom(string nickName, int index)
    {
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
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    { 
        newPlayer.NickName =  UserInformations.Name;
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
            _photonView.RPC("StartTimer", RpcTarget.All);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
        StopCoroutine(Timer(5f));
        isPlayerLeftRoom=true;
    }

    public override void OnLeftRoom()
    {
        if (prevRoomName == "lobby")
        {
            isJoiningRoom = true;
        }
        else
        {
            MenuManager.Instance.OpenMenu("Default");
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        remainingTime = 5f;
        if (PhotonNetwork.LocalPlayer == otherPlayer)
        {
            _photonView.RPC("RPC_SyncPlayerLeftRoom", RpcTarget.All, otherPlayer.NickName);
        }
        StopTimer();
    }

    [PunRPC]
    public void StartTimer()
    {
        if (!isRoomFull) 
        {
            countdownCoroutine = StartCoroutine(Timer(5f)); 
        }
    }

    IEnumerator Timer(float time)
    {
        if (isPlayerLeftRoom) yield break;
        timerText.gameObject.SetActive(true);
        isRoomFull = true;
        isTimeFinish = true;
        remainingTime = time;

        while (remainingTime > 0)
        {
            timerText.text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f); 
            remainingTime -= 1f;
        }
        if (!isPlayerLeftRoom)
        {
            isTimeFinish = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void StopTimer()
    {
        if (countdownCoroutine != null)
        {
            isPlayerLeftRoom = true;
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
            timerText.gameObject.SetActive(false); 
        }
    }
}
