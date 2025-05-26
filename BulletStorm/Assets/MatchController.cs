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

    public Dictionary<string, MatchResult> gameStats = new();
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
        ExitGames.Client.Photon.Hashtable WON = new ExitGames.Client.Photon.Hashtable();
        bool redWon = false;
        if (redTeamsKill > blueTeamsKill)
        {
            redWon = true;
            foreach (var teamate in redTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = redWon });
            foreach (var teamate in blueTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = !redWon });
        }
        else
        {
            foreach (var teamate in redTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = redWon });
            foreach (var teamate in blueTeam)
                teamate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { ["WON"] = !redWon });
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
                player.CustomProperties.TryGetValue("Deaths", out object Deaths);
                player.CustomProperties.TryGetValue("Assits", out object Assits);
                player.CustomProperties.TryGetValue("ELO", out object ELO);

                gameStats[player.NickName] = new MatchResult()
                {
                    Assist = (int)Assits,
                    Kill = (int)Kills,
                    Deaths = (int)Deaths,
                    OldELO = (float)ELO,
                };
                CountTeamsKill(player, gameStats[player.NickName].Kill);
            }
            HandleWins();
            if (PhotonNetwork.InRoom)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    player.CustomProperties.TryGetValue("WON", out object WON);
                    gameStats[player.NickName].Won = (bool)WON;
                }
            }
        }
    }
}
