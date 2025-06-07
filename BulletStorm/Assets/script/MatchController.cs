using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController Instance;

    public static Dictionary<string, MatchResult> gameStats = new();
    List<Player> redTeam = new List<Player>();
    List<Player> blueTeam = new List<Player>();
    int redTeamsKill = 0;
    int blueTeamsKill = 0;

    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CountTeamsKill(Player player, int playerKills)
    {
        if (player.CustomProperties.TryGetValue("team", out object team))
        {
            string teamName = (string)team;
            if (teamName.Equals("redTeam"))
            {
                redTeam.Add(player);
                redTeamsKill += playerKills;
            }
            else
            {
                blueTeam.Add(player);
                blueTeamsKill += playerKills;
            }
        }

    }

    public void HandleWins()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        bool WON = false;
        if (redTeamsKill > blueTeamsKill)
        {
            WON = true;
            foreach (var teamate in redTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = WON });
            foreach (var teamate in blueTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = !WON });
        }
        else
        {
            foreach (var teamate in redTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = WON });
            foreach (var teamate in blueTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = !WON });
        }
    }

    public void SaveDatas()
    {
        gameStats.Clear();
        if (PhotonNetwork.InRoom)
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                player.CustomProperties.TryGetValue("Kills", out object Kills);
                player.CustomProperties.TryGetValue("Death", out object Death);
                player.CustomProperties.TryGetValue("Assist", out object Assist);
                player.CustomProperties.TryGetValue("ELO", out object ELO);
               
                gameStats[player.NickName] = new MatchResult()
                {   
                    Assist = Assist is int a ? a : 0,
                    Kill = Kills is int k ? k : 0,
                    Deaths = Death is int d ? d : 0,
                    OldELO = ELO is float e ? e : (ELO is double dbl ? (float)dbl : 0f),
                    Won = false,
                  
                };
                CountTeamsKill(player, gameStats[player.NickName].Kill);
            }
            HandleWins();
            if (PhotonNetwork.InRoom)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    player.CustomProperties.TryGetValue("WON", out object WON);
                    gameStats[player.NickName].Won = WON is bool a ? a : false;
                }
            }
        }
    }
}
