using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DisplayeDatas : MonoBehaviour
{
    public TMP_Text[] Names;
    public TMP_Text[] Kills;
    public TMP_Text[] Assists;
    public TMP_Text[] Deaths;
    public Button BackButton;
    Dictionary<string, MatchResult> values = new();
    private void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Debug.Log("Cursor állapot: " + UnityEngine.Cursor.lockState + ", látható: " + UnityEngine.Cursor.visible);
        values = MatchController.gameStats;
        int index = 0;
        foreach (var value in values)
        {
            Names[index].text = value.Key;
            Kills[index].text = value.Value.Kill.ToString();
            Assists[index].text = value.Value.Assist.ToString();
            Deaths[index].text = value.Value.Deaths.ToString();
            index++;
        }
       
    }

    public async void SendData()
    { 
        await ApiHandler.instance.GlobalStatisticsAsync();
        await ApiHandler.instance.HandleMatchAPIAsync();
    }
}
