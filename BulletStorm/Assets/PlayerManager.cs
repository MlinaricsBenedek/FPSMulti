using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    PhotonView Pv;
    GameObject controller;
    public int Kills;
    public int Assist;
    Scene scene;
    
    private void Awake()
    {
        Pv = GetComponent<PhotonView>();
        scene = SceneManager.GetActiveScene();
    }
    
    void Start()
    {
        if (Pv.IsMine)
        {
            CreateController();
        }
        Kills = 0;
        Assist = 0;
    }
  
    void CreateController()
    {
        if (scene.buildIndex == 1)
        {
            Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs",
          "SK_Military_Survivalist"),
          spawnPoint.position, spawnPoint.rotation, 0, new object[] { Pv.ViewID }
          );
        }
          
    }

    public void Die()
    {
        Debug.Log("Destroy the player");
        PhotonNetwork.Destroy(controller);
        CreateController();
    }

}
