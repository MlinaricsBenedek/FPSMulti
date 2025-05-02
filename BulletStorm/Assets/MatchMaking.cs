using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchMaking : MonoBehaviour
{
    public class PlayerStats
    {
        public int Kills;
        public int Assists;
        public bool Won;
        public bool WasUnderdog;
        public bool SlowMatch;
    }

    public static int CalculateNewElo(int currentElo, int opponentElo, PlayerStats stats, float K = 32f)
    {

        float expectedScore = 1f / (1f + Mathf.Pow(10f, (opponentElo - currentElo) / 400f));

        float actualScore = CalculateMatchScore(stats);

        float newElo = currentElo + K * (actualScore - expectedScore);

        return Mathf.RoundToInt(newElo);
    }

    private static float CalculateMatchScore(PlayerStats stats)
    {
        float score = 0f;

        if (stats.Won)
        {
            score += 0.6f;
        }
        else
        {
            score -= 0.6f;
        }
        if (stats.Kills >= 1)
        {
            score += stats.Won ? 0.15f : -0.15f;
        }

        if (stats.Kills >= 2)
        {
            score += stats.Won ? 0.2f : -0.2f;
        }

        if (stats.Kills >= 3)
        {
            score += stats.Won ? 0.05f : -0.05f; 
        }
        if (stats.Assists >= 1)
        {
            score += stats.Won ? 0.05f : -0.05f;
        }

        if (stats.Assists >= 3)
        {
            score += stats.Won ? 0.025f : -0.025f; 
        }

        if (stats.WasUnderdog)
        {
            score += stats.Won ? 0.10f : -0.10f;
        }

        if (stats.SlowMatch)
        {
            score += stats.Won ? 0.05f : -0.05f;
        }
        score = Mathf.Clamp(score, 0f, 1f);

        return score;
    }
}
