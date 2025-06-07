using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon.StructWrapping;
public class ScoreBoardItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text userName;
    [SerializeField] TMP_Text KillsText;
    [SerializeField] TMP_Text DeathsTexts;
    [SerializeField] TMP_Text AssitsTexts;
    Player Player;
    
    public void Initialized(Player player)
    {
        userName.text = player.NickName;
        Player= player;
        UpdateState();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer == Player)
        {
            if (changedProps.ContainsKey("Kills"))
            {
                UpdateState();
            }
            if (changedProps.ContainsKey("Death"))
            {
                UpdateState();
            }
            if (changedProps.ContainsKey("Assist"))
            {
                UpdateState();
            }
        }
    }

    void UpdateState()
    {
        if (Player.CustomProperties.TryGetValue("Kills",out object Kills))
        {
            KillsText.text = Kills.ToString();
        }
        if (Player.CustomProperties.TryGetValue("Death", out object Death))
        {
            DeathsTexts.text = Death.ToString();
        }
        if (Player.CustomProperties.TryGetValue("Assist", out object Assist))
        {
            Debug.Log(Assist);
            AssitsTexts.text = Assist.ToString();
        }
    }
}
