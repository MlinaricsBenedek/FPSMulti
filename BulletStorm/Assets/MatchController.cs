using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController Instance;

    public Dictionary<string,PlayerStats> gameStats= new ();

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

    public void SaveDatas()
    { 
        gameStats.Clear();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            player.CustomProperties.TryGetValue("Kills",out object Kills);
            player.CustomProperties.TryGetValue("Deaths", out object Deaths);
            player.CustomProperties.TryGetValue("Assits", out object Assits);

            gameStats[player.NickName] = new PlayerStats()
            {
               Assists = (int)Assits,
               Kills = (int)Kills,
               Deaths = (int)Deaths
            };
                
        }
    }
}
