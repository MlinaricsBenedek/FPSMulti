using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Newtonsoft.Json.Bson;
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
using WebSocketSharp;

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
    HashSet<Player> lobby = new HashSet<Player>();
    const int tolarence = 70;
    const int playerCount = 6;
    float playerElo = 0f;
    string currentRoomName;
    bool shouldCreateRoomAfterLeaving = false;

    bool isJoiningRoom;
    List<Player> teams = new(); 

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
        
        PhotonNetwork.AutomaticallySyncScene = true;
    }
   
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.LocalPlayer.NickName = UserInformations.Name;
       // Debug.Log("Csatlakozás a lobbyhoz");
        isInLobby = true;

        if (shouldCreateRoomAfterLeaving && !string.IsNullOrEmpty(currentRoomName))
        {
            shouldCreateRoomAfterLeaving = false;
           // Debug.Log("megprobáljuk létrehozni a szobát!");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = playerCount;
            bool valt = PhotonNetwork.JoinOrCreateRoom(currentRoomName, roomOptions, TypedLobby.Default);
            //if (valt)
            //{
            //    Debug.Log("Szoba sikeresen létrejött");
            //}
            return;
        }

        if (isJoiningRoom && !string.IsNullOrEmpty(currentRoomName))
        {
            //Debug.Log("megprobálunk hozzá csatlakozni");
            bool val1 = PhotonNetwork.JoinRoom(currentRoomName);
            //    if (val1)
            //    {
            //        Debug.Log("csatlakozás sikeres volt");
            //    }
            //}
            //Debug.Log(_photonView.ViewID);
            MenuManager.Instance.OpenMenu("Default");
        }
    }

    public void CreateRoom()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.Name == "lobby")
        {
          //  Debug.Log("beléptünk a create room függvénybe");
            isInLobby = true;
            shouldCreateRoomAfterLeaving = true;
            PhotonNetwork.LeaveRoom();
        }
       
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {

        if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey("currentRoomName"))
        { 
            currentRoomName = changedProps["currentRoomName"] as string;
            isJoiningRoom = true;
            if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.InRoom && PhotonNetwork.NetworkClientState != ClientState.Leaving)
            {
                PhotonNetwork.LeaveRoom();
            }
        }
    }

    public void AddPlayer()
    {
        bool eloExist = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("ELO", out object ELO);
        float elo = (float)ELO;
        var combinations = MatchMaker.Instance.FindMatchingTeams(PhotonNetwork.LocalPlayer, lobby);
        float minELO = float.MaxValue;
        float maxELO = 0;
        foreach ( var combination in combinations ) 
        {
            if (minELO - elo > -tolarence && maxELO - elo < tolarence)
            {
                TeamController.CreateTeams(combination, out var blueTeam, out var redTeam);
                CreateMatch(combination);

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
        if (!lobby.Contains(requestingPlayer))
        {
            lobby.Add(requestingPlayer);
        }
        AddPlayer();
    }

    public void CreateMatch(HashSet<Player> players) 
    {
        currentRoomName = $"Match_{Guid.NewGuid().ToString().Substring(0, 5)}";
        //Debug.Log($"[Photon] Szoba létrehozása: {currentRoomName}");
        foreach (var player in players)
        {
            if (player == PhotonNetwork.LocalPlayer)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props["currentRoomName"] = currentRoomName;
                player.SetCustomProperties(props);
            }
            else
            {
                photonView.RPC("RPC_SetRoom", player, currentRoomName);
            }
        }

        foreach (var player in players)
        {
            if (player == PhotonNetwork.LocalPlayer)
            {
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                props["currentRoomName"] = currentRoomName;
                player.SetCustomProperties(props);
            }
            else
            {
                photonView.RPC("RPC_SetRoom", player,currentRoomName);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        { 
            CreateRoom();
        }
        foreach (var player in players)
        {
            teams.Add(player);
        }
    }


    public void HandleLobbyRoom(float elo)
    {
        RoomOptions roomOptions = new RoomOptions();
        TypedLobby typedLobby = new TypedLobby("lobby",LobbyType.Default);
        PhotonNetwork.JoinOrCreateRoom("lobby",roomOptions,typedLobby);
    }

    [PunRPC]
    void RPC_SetRoom(string roomName)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["currentRoomName"] = roomName;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    [PunRPC]
    void RPC_SetTeams(string team)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["team"] = team;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public async void GetPlayerElo()
    {
        playerElo = await ApiHandler.instance.GetUserStatisticsAsync(TokenController.Token);
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
            MenuManager.Instance.OpenMenu("PlayerLobby");
            int index = 0;
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
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


    void HandleTeams()
    {
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (Teams.BlueTeams.Contains(player))
            {
                photonView.RPC("RPC_SetTeams", player,"blueTeam");
            }
            if (Teams.RedTeams.Contains(player))
            {
                photonView.RPC("RPC_SetTeams", player, "redTeam");
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
        for (int i = 0; i < userNames.Length; i++)
        {
            if (userNames[i].text == "")
            {
                userNames[i].text = newPlayer.NickName;
                return;
            }
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == playerCount && PhotonNetwork.CurrentRoom.Name != "lobby")
        {
            _photonView.RPC("RPC_StartTimer", RpcTarget.All);
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
     //   Debug.Log("belépítünk az onleftroom függgvénybe ");

        var valt =PhotonNetwork.JoinLobby();
        //if (valt)
        //{
        //    Debug.Log("csatlakozás a lobbyhoz");
        //}
        if (isInLobby)
        {
            isJoiningRoom = true;
            isInLobby = false;
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
    public void RPC_StartTimer()
    {
        if (!isRoomFull) 
        {
            countdownCoroutine = StartCoroutine(Timer(15f)); 
        }
    }

   
    IEnumerator Timer(float time)
    {
        if (isPlayerLeftRoom) yield break;
        timerText.gameObject.SetActive(true);
        isRoomFull = true;
        isTimeFinish = true;
        remainingTime = time;
        HandleTeams();
       // Debug.Log(PhotonNetwork.CurrentRoom.Players.Count+PhotonNetwork.CurrentRoom.Name);
        while (remainingTime > 0)
        {
            timerText.text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f); 
            remainingTime -= 1f;
        }
        if (!isPlayerLeftRoom)
        {
            isTimeFinish = false;
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
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
