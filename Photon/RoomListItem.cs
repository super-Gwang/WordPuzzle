using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class RoomListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;
    public TMP_Text PlayersCountText;
    public RoomInfo info;
    public string sessionKey;
    public void SetUp(RoomInfo _info)
    {
        info = _info;
        text.text = (string)_info.CustomProperties["RoomName"];
        sessionKey = (string)_info.CustomProperties["SessionKey"];
        //PlayersCountText.text = $"( {PhotonNetwork.CountOfPlayersInRooms} / {_info.MaxPlayers} )"; 
    }

    public void onClick()
    {
        Launcher.instance.JoinRoom(info);
    }
}
