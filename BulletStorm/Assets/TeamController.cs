using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TeamController 
{
    public static List<Player> CreateTeams(List<Player> players,out List<Player> blueTeam,out List<Player> redTeams)
    {
        List<Player> teams = new();
        redTeams = new List<Player>();
        blueTeam = new List<Player>();
        if (players.Count > 6)
        {
            return null;
        }
        players.Sort((a, b) =>
        {
            double eloA = a.CustomProperties.TryGetValue("elo", out object eloAObj) ? Convert.ToDouble(eloAObj) : 0.0;
            double eloB = b.CustomProperties.TryGetValue("elo", out object eloBObj) ? Convert.ToDouble(eloBObj) : 0.0;
            return eloA.CompareTo(eloB);
        });
        int lowEloPlayers = 0;
        int highEloPlayers= players.Count - 1;
        bool color = true;
        while (lowEloPlayers < highEloPlayers)
        {
            var low = players[lowEloPlayers];
            var high = players[highEloPlayers];
            if (color)
            {
                redTeams.Add(low);
                blueTeam.Add(high);
            }
            else
            {
                redTeams.Add(high);
                blueTeam.Add(low);
            }
            lowEloPlayers++;
            highEloPlayers--;
            color = !color;
        }
        teams.AddRange(blueTeam);
        teams.AddRange(redTeams);
        return teams;
    }
}
public class PlayerData
{
    public float ELO;
    public string Name; 
}
