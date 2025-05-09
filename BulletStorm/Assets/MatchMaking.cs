using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Rendering;

public class MatchMaking : MonoBehaviour
{
    public class PlayerStats
    {
        public int Kills;
        public int Assists;
        public bool Won;
        public bool WasUnderdog;
        public bool SlowMatch;
        public int Deaths;
    }
    
    private float killsMultiplier = 0.15f;
    private float assistMultiplier = 0.075f;
    private float deathMultiplayer = -0.10f;
    private float winStreakBonus = 1.05f;
    private float Lamdba = 0.2f;
    int kills = 0;
    int avarageKill = 0;
    DateTime lastMatch;
    float gameTime;
    float avarageGameTime;
    int wins = 4;
    //NewRating=OldRating+K⋅[W+λ⋅PerfScore+μ⋅(FormScore⋅RecencyFactor⋅ConsistencyFactor)+δ⋅MatchQualityBonus+ω⋅StreakBonus]
    //K: 
    public double CalculateNewElo(int currentElo, int opponentElo, PlayerStats stats,PlayerStats[] playerStats, float K = 32f)
    {
        double plus = WeightFactor(currentElo)*((stats.Won?1:0)+Lamdba*PlayerPerformance(stats)+MU(PlayerActivite(lastMatch))*
           (ConsistenciFactor()* PlayerActivite(lastMatch)* MatchesScore(playerStats))+MatchQuality(kills,avarageKill) +
           Omega()*WinStreak(wins));

        float expectedScore = 1f / (1f + Mathf.Pow(10f, (opponentElo - currentElo) / 400f));

        float actualScore = stats.Won ? 1 : 0;

        float ELO = currentElo + K * (actualScore - expectedScore);

        double newELO = 0.7*ELO+0.3* plus;
        return newELO;

    }

    private double MU(float recency)
    {
        return Mathf.Clamp01(0.05f + 0.2f * recency);
    }

    private float Omega()
    {
        return Mathf.Clamp(0.05f*wins,0,0.15f);
    }
    private  float PlayerPerformance(PlayerStats stats)
    {
        float score = 0f;

        score += stats.Kills * killsMultiplier;
        score += stats.Assists * assistMultiplier;    
        score += stats.Deaths * deathMultiplayer;
        score = Mathf.Clamp(score, 0f, 1f);
        return score;
    }

    private float MatchesScore(PlayerStats[] playerStats)
    {
        float score = 0f;
        foreach (var stat in playerStats)
        { 
            score+=PlayerPerformance(stat);
        }
        return score/playerStats.Length;
    }

    private float ConsistenciFactor()
    {
        float oldSigma=1f;
        int matches=3;
        PlayerStats stats=new PlayerStats();
        return 1 / (1 + Dispersion(oldSigma,matches,stats));
    }

    private float Dispersion(float sigma,int matches,PlayerStats stats)
    {
        matches++;
        sigma += PlayerPerformance(stats);
        return sigma;
    }

    private float WinStreak(int wins)
    {
        if (wins > 2)
        {
            return Mathf.Min(winStreakBonus * wins, 5);
        }
        else
        {
            return 0;
        }
    }

    private double MatchQuality(int kill,int avarageKill)
    {
        float ratio = (float)kill / avarageKill;
        float bonus = 0.05f * (1 - ratio);
        return Mathf.Clamp(ratio,-0.05f,0.05f);     
    }

    private float PlayerActivite(DateTime lastMatch)
    {
        TimeSpan elapsedDate =  DateTime.Now-lastMatch;
        int days = elapsedDate.Days;
        double insecurity = Mathf.Exp((float)days * (-0.5f));
        return (float)insecurity;
    }

    private float WeightFactor(int oldELO)
    {
        int maxElo = 5000;
        int maxWeight = 40;
        float x = 1-( (float)oldELO / maxElo);

        float K = Mathf.Clamp( x * maxWeight,5,maxWeight);
        return K;
    }
}
