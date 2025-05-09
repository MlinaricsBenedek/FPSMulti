using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerManager : MonoBehaviour
{
    PhotonView Pv;
    GameObject controller;
    public int Kills=0;
    public int Death=0;
    public int Assist = 0;
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

        Death++;
        Hashtable hash = new Hashtable();
        hash.Add("Death", Death);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void GetKill() 
    {
        Pv.RPC(nameof(RPC_GetKilled),Pv.Owner);
    }

    [PunRPC]
    void RPC_GetKilled()
    {
        Kills++;
        Hashtable hash= new Hashtable();
        hash.Add("Kills", Kills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

    }

    public void GetAssist()
    {
        Pv.RPC(nameof(RPC_GetAssist), Pv.Owner);
    }

    [PunRPC]
    public void RPC_GetAssist()
    {
        Debug.Log("meghívtuk az assistot");   
        Assist++;
        Debug.Log(Assist);
        Hashtable hash = new Hashtable();
        hash.Add("Assist", Assist);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public static PlayerManager Find(Player player)
    { 
        return FindObjectsOfType<PlayerManager>().SingleOrDefault(x=>x.Pv.Owner ==player);
    
    }
}
