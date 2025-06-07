using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchMaker
{
    private static MatchMaker _instance;
    public static MatchMaker Instance => _instance ??= new MatchMaker();
    private int playerCount = 6;

    public HashSet<HashSet<Player>> FindMatchingTeams(Player player, HashSet<Player> lobby)
    {
        Debug.Log($"[MatchMaking] Kombinációk keresése a játékos: {player.NickName} számára.");
        if (!player.CustomProperties.TryGetValue("ELO", out object ELO)) return null;

        float elo = Convert.ToSingle(ELO);
        lobby.Add(player);

        var validCombinations = new HashSet<HashSet<Player>>();
        var combinations = GenerateCombinations(new List<Player>(lobby), playerCount);

        foreach (var combination in combinations)
        {
            float minELO = float.MaxValue;
            float maxELO = float.MinValue;

            foreach (var _player in combination)
            {
                if (_player.CustomProperties.TryGetValue("ELO", out object eloObj) &&
                    float.TryParse(eloObj.ToString(), out float otherPlayersELO))
                {
                    minELO = Mathf.Min(minELO, otherPlayersELO);
                    maxELO = Mathf.Max(maxELO, otherPlayersELO);
                }
                else
                {
                    return null;
                }
            }

            if (maxELO - minELO <= 70f)
            {
                validCombinations.Add(new HashSet<Player>(combination));
            }
        }

        return validCombinations;
    }

    private List<List<Player>> GenerateCombinations(List<Player> players, int teamSize)
    {
        var result = new List<List<Player>>();
        Generate(players, 0, new List<Player>(), teamSize, result);
        return result;
    }

    private void Generate(List<Player> players, int start, List<Player> current, int remaining, List<List<Player>> result)
    {
        if (remaining == 0)
        {
            result.Add(new List<Player>(current));
            return;
        }

        for (int i = start; i <= players.Count - remaining; i++)
        {
            current.Add(players[i]);
            Generate(players, i + 1, current, remaining - 1, result);
            current.RemoveAt(current.Count - 1);
        }
    }
}
