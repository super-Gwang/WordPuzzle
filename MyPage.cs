using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.MUIP;
using Classes;

public class MyPage : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager;

    [SerializeField] TMP_Text nicknameText;
    [SerializeField] GameObject newNicknamePopup;
    [SerializeField] TMP_InputField newNicknameText;
    [SerializeField] TMP_Text totalText;
    [SerializeField] TMP_Text wonCountText;
    [SerializeField] TMP_Text wonRateText;
    [SerializeField] ProgressBar[] progressBars;
    [SerializeField] HistoryListItem[] historyListItem;

    PlayerData playerData;
    private void OnEnable()
    {
        if(playerManager == null) playerManager = PlayerManager.instance;
        playerData = playerManager.playerData;
        Init();
    }
    void Init()
    {
        nicknameText.text = playerData.nickname;
        totalText.text = (playerData.singlePlay.playCount + playerData.multiPlay.playCount).ToString();
        wonCountText.text = (playerData.singlePlay.winCount + playerData.multiPlay.winCount).ToString();
        float rate = ((float)(playerData.singlePlay.winCount + playerData.multiPlay.winCount) / (float)(playerData.singlePlay.playCount + playerData.multiPlay.playCount)) * 100;
        wonRateText.text = rate == float.NaN ? "0" : Mathf.RoundToInt(rate) + "%";
        for(int i = 0; i < progressBars.Length; i++)
        {
            progressBars[i].maxValue = playerData.singlePlay.playCount + playerData.multiPlay.playCount;
            progressBars[i].ChangeValue(playerData.singlePlay.guessCounts[i]);
        }
        for(int i = 0; i < playerManager.playerData.histories.Count; i++)
        {
            historyListItem[i].SetUp(playerManager.playerData.histories[i]);
        }
    }

    public void ChangeNickname()
    {
        nicknameText.text = newNicknameText.text;
        playerManager.ChangeNickName(newNicknameText.text);
    }

    public void AddHistory()
    {
        int index = playerManager.histories.Count - 1;
        historyListItem[index].SetUp(playerManager.histories[index]);
    }
}
