using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviourPunCallbacks
{
    public static Timer Instance;   

    [SerializeField] private TMP_Text timerText;
    private float time = 6000f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public IEnumerator GameTimer()
    {
        float remainingTime = time;

        while (remainingTime > 0)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }
        MatchController.Instance.SaveDatas();
        SceneManager.LoadScene("GameOwer");
    }
}
