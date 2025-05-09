using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayeDatas : MonoBehaviour
{
    public TMP_Text[] Names;
    public TMP_Text[] Kills;
    public TMP_Text[] Assists;
    public TMP_Text[] Deaths;

    private void Start()
    {
        Dictionary<string,PlayerStats> values= MatchController.Instance.gameStats;
        int index = 0;
        foreach (var value in values)
        {
            Names[index].text = value.Key;
            Kills[index].text = value.Value.Kills.ToString();
            Assists[index].text = value.Value.Assists.ToString();
            Deaths[index].text = value.Value.Deaths.ToString();
            index++;
        }
    }
}
