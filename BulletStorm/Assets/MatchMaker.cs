using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Rendering;

public class MatchMaker 
{
    private static MatchMaker _instance;
    public static MatchMaker Instance => _instance ??= new MatchMaker();
    private int playerCount = 6;

    public List<List<Player>> FindMatchingTeams(Player player,List<Player> lobby)
    {
        Debug.Log($"[MatchMaking] Kombinációk keresése a játékos: {player.NickName}  számára.");
        bool eloExist = player.CustomProperties.TryGetValue("ELO", out object ELO);

        if (!eloExist) return null;
        float elo = (float)ELO;

        lobby.Add(player);
        var validCombinations = new List<List<Player>>();
        var combinations = Combinastions(lobby, playerCount);
        Debug.Log($"[MatchMaking] Összes kombináció: {combinations.Count}");
        foreach (var combination in combinations)
        {
            float minELO = float.MaxValue;
            float maxELO = 0;
            foreach (var _player in combination)
            {
                if (_player.CustomProperties.TryGetValue("ELO", out object eloObj) && float.TryParse(eloObj.ToString(), out float _otherPlayerselo))
                {
                    float otherPlayersELO = (float)_otherPlayerselo;
                    if (otherPlayersELO < minELO) minELO = otherPlayersELO;
                    if (otherPlayersELO > maxELO) maxELO = otherPlayersELO;
                }
                else
                {
                    return null;
                }
            }
            if (maxELO - minELO <= 70f) 
            {
                validCombinations.Add(combination);
            }
        }
        return validCombinations;
    }

    private static List<List<Player>> Combinastions(List<Player> lobby, int requiredTeamMembers)
    {
        List<List<Player>> playerInTheMatch = new();
        RecursiveAddPlayer(lobby, new List<Player>(), 0, requiredTeamMembers, playerInTheMatch);
        return playerInTheMatch;
    }

    static void RecursiveAddPlayer(List<Player> lobby, List<Player> combination, int start, int playersInTheGame, List<List<Player>> combinations)
    {
        if (playersInTheGame == 0)
        {
            combinations.Add(new List<Player>(combination));
            return;
        }

        for (int i = start; i <= lobby.Count - playersInTheGame; i++)
        {
            combination.Add(lobby[i]);
            RecursiveAddPlayer(lobby, combination, i + 1, playersInTheGame - 1, combinations);
            combination.RemoveAt(combination.Count - 1);
        }
    }

}
