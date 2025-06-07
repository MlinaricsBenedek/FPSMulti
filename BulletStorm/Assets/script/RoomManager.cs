using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
        Scene scene = SceneManager.GetActiveScene();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 2)
        {
            StartCoroutine(WaitUntilPlayerWillBeInTheRoom());
            StartCoroutine(Timer.Instance.GameTimer());
        }
    }

    IEnumerator WaitUntilPlayerWillBeInTheRoom()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);
        if (PhotonNetwork.LocalPlayer.TagObject == null)
        {
            GameObject playerManager = PhotonNetwork.Instantiate("PlayerManager", Vector3.zero, Quaternion.identity);
            PhotonNetwork.LocalPlayer.TagObject = playerManager; 
        }
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "GunPV"), spawnPoint.position, spawnPoint.rotation);
    }
}
