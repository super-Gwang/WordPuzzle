using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    void Awake()
    {
        if(instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += onSceneLoaded;
    }
    public void OnDisable()
    {
        base.OnDisable();
    }
    void onSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        //if (scene.buildIndex == 1)
        //{
        //    PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "GameManager"), Vector3.zero, Quaternion.identity);
        //}
    }
}
