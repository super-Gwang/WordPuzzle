using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Security.Cryptography;
using System.Text;
using ExitGames.Client.Photon;
using System.IO;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [SerializeField] MenuManager menu;
    [SerializeField] GameManager gm;

    [SerializeField] PhotonView view;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text sessionKey;
    [SerializeField] TMP_InputField nickNameText;
    [SerializeField] TMP_InputField roomSessionKey;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListPref;
    [SerializeField] GameObject playerListPref;
    [SerializeField] Button startGameButton;

    void Awake()
    {
        instance = this;
        menu = MenuManager.instance;
        gm = FindObjectOfType<GameManager>();
        //view = gm.GetComponent<PhotonView>();
        view = GetComponent<PhotonView>();
    }
    void Start()
    {
        Debug.Log("Connecting to Master");
        if(PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
        if (PhotonNetwork.IsConnected)
            OnConnectedToMaster();
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        menu.OpenMenu("mode");
        nickNameText.onEndEdit.AddListener(ValueChanged);
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnJoinedLobby()
    {
        menu.OpenMenu("title");
        Debug.Log("Joined Lobby");
        //PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000");
    }
    
    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInputField.text))
            return;

        ExitGames.Client.Photon.Hashtable hashtables = new ExitGames.Client.Photon.Hashtable();
        hashtables.Add("RoomName", roomNameInputField.text);
        hashtables.Add("SessionKey", GenerateSessionKey(3));

        string[] propertiesListedInLobby = new string[2];
        propertiesListedInLobby[0] = "RoomName";
        propertiesListedInLobby[1] = "SessionKey";

        PhotonNetwork.CreateRoom(
            roomNameInputField.text,
            new RoomOptions
            {
                MaxPlayers = 2,
                IsVisible = true,
                IsOpen = true,
                CustomRoomProperties = hashtables,
                CustomRoomPropertiesForLobby = propertiesListedInLobby
            });

        menu.OpenMenu("loading");
    }
    
    public override void OnJoinedRoom()
    {
        menu.OpenMenu("room");
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        sessionKey.text = PhotonNetwork.CurrentRoom.CustomProperties["SessionKey"].ToString();
        
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        Player[] player = PhotonNetwork.PlayerList;
        for (int i = 0; i < player.Length; i++)
        {
            Instantiate(playerListPref, playerListContent).GetComponent<PlayerListItem>().SetUp(player[i]);
        }
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }
    public override void OnJoinRoomFailed(short errorCode, string message)
    {
        if (errorCode == 3)
        {
            Debug.Log("룸이 이미 가득 찼습니다. 다른 룸을 찾거나 나중에 다시 시도해주세요.");
        }
        else
        {
            Debug.Log("룸에 입장하는 동안 오류가 발생했습니다: " + errorCode + ", " + message);
        }
    }

    //public override void OnMasterClientSwitched(Player newMasterClient)
    //{
    //    startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    //}
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        menu.OpenMenu("error");
    }
    public void StartGame(string mode)
    {
        switch (mode)
        {
            case "single":
                gm.SingleGameStart();
                break;
            case "multi":
                view.RPC("GameStart", RpcTarget.All);
                break;
        }
        PhotonNetwork.LoadLevel(1);
    }
    [PunRPC]
    private void GameStart()
    {
        gm.MultiGameStart();
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        menu.OpenMenu("loading");
    }
    public override void OnLeftRoom()
    {
        menu.OpenMenu("title");
    }
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListPref,roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }
    public void FindRoom()
    {
        string sessionKey = roomSessionKey.text;
        if (!string.IsNullOrEmpty(sessionKey))
        {
            foreach (Transform child in roomListContent)
            {
                RoomInfo room = child.GetComponent<RoomListItem>().info;
                if (room.CustomProperties.ContainsKey("SessionKey") && (string)room.CustomProperties["SessionKey"] == sessionKey)
                {
                    PhotonNetwork.JoinRoom(room.Name);
                    menu.OpenMenu("loading");
                    return;
                }
                else
                {
                    Debug.Log(room.CustomProperties["SessionKey"]);
                }
            }
            Debug.Log("일치하는 방을 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log("세션 키를 입력하세요.");
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListPref, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    public static string GenerateSessionKey(int length)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenData = new byte[length];
            rng.GetBytes(tokenData);

            StringBuilder result = new StringBuilder(length * 2);
            for (int i = 0; i < tokenData.Length; i++)
            {
                result.Append(tokenData[i].ToString("X2"));
            }
            return result.ToString();
        }
    }

    void ValueChanged(string text)
    {
        PhotonNetwork.NickName = text;
    }
}
