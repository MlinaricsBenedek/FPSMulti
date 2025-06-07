using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    public GameObject[] spawnPoints;

    private void Awake()
    {
        instance = this;
    }

    public Transform GetSpawnPoint()
    {
        if (Teams.BlueTeams.Contains(PhotonNetwork.LocalPlayer))
        {
            return spawnPoints[Random.Range(0, 2)].transform;
        }
        else
        {
            return spawnPoints[Random.Range(3, 5)].transform;
        }
    }
}
