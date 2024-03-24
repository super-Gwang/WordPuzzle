using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structs;
using System.Text;
using System;
using Classes;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance = null;
    private const int MaxHistories = 10;

    [SerializeField]
    public PlayerData playerData;
    [SerializeField]
    public List<History> histories = new List<History>();

    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        playerData = new PlayerData();
        Load();
    }

    void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetString("Nickname", playerData.nickname);
        PlayerPrefs.SetString("History", Merge_History());

        PlayerPrefs.SetInt("PlayCount_s", playerData.singlePlay.playCount);
        PlayerPrefs.SetInt("WinCount_s", playerData.singlePlay.winCount);
        PlayerPrefs.SetString("GuessCount_s", Merge_GuessCount(playerData.singlePlay.guessCounts));

        PlayerPrefs.SetInt("PlayCount_m", playerData.multiPlay.playCount);
        PlayerPrefs.SetInt("WinCount_m", playerData.multiPlay.winCount);
        PlayerPrefs.SetString("GuessCount_m", Merge_GuessCount(playerData.multiPlay.guessCounts));

        PlayerPrefs.Save();
    }

    void Load()
    {
        //if (!PlayerPrefs.HasKey("Nickname") || PlayerPrefs.GetString("Nickname") == null) return;
        playerData.nickname = PlayerPrefs.GetString("Nickname");
        //histories = LoadMatchRecords(PlayerPrefs.GetString("History"));
        playerData.histories = LoadMatchRecords(PlayerPrefs.GetString("History"));

        playerData.singlePlay.playCount = PlayerPrefs.GetInt("PlayCount_s");
        playerData.singlePlay.winCount = PlayerPrefs.GetInt("WinCount_s");
        playerData.singlePlay.guessCounts = Split_GuessCount(PlayerPrefs.GetString("GuessCount_s"));

        playerData.multiPlay.playCount = PlayerPrefs.GetInt("PlayCount_m");
        playerData.multiPlay.winCount = PlayerPrefs.GetInt("WinCount_m");
        playerData.multiPlay.guessCounts = Split_GuessCount(PlayerPrefs.GetString("GuessCount_m"));

    }

    public void ChangeNickName(string nickname)
    {
        playerData.nickname = nickname;
    }

    int[] Split_GuessCount(string _guessCount)
    {
        int[] result = new int[6];
        string[] pairs = _guessCount.Split(',');
        foreach(string pair in pairs)
        {
            string[] keyValue = pair.Split(':');
            if (keyValue.Length == 1) continue;

            int key = int.Parse(keyValue[0]);
            int value = int.Parse(keyValue[1]);

            result[key] = value;
        }
        return result;
    }

    string Merge_GuessCount(int[] _guessCount)
    {
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < _guessCount.Length; i++)
        {
            if (_guessCount[i] == 0) continue;
            result.Append(i).Append(":").Append(_guessCount[i]).Append(",");
        }
        return result.ToString();
    }

    public List<History> LoadMatchRecords(string histories)
    {
        // 저장된 문자열 불러오기
        string[] recordStrings = histories.Split(';');

        List<History> records = new List<History>();
        foreach (string recordString in recordStrings)
        {
            string[] parts = recordString.Split('|');
            if (parts.Length == 4)
            {
                bool winOrLose = bool.Parse(parts[1]);
                string mode = parts[0];
                int guessCount = int.Parse(parts[2]);
                string matchTime = parts[3];
                records.Add(new History(mode, winOrLose, guessCount, matchTime));
            }
        }
        return records;
    }
    public void SaveHistory(string _mode, bool _isWin, int _guessCount, string _time)
    {
        History history = new History(_mode, _isWin, _guessCount, _time);
        playerData.histories.Add(history);

        if (playerData.histories.Count > MaxHistories)
        {
            playerData.histories.RemoveAt(0);
        }
    }

    string Merge_History()
    {
        StringBuilder result = new StringBuilder();
        foreach (History history in playerData.histories)
        {
            string recordString = $"{history.mode}|{history.isWin}|{history.guessCount}|{history.time};";
            result.Append(recordString);
        }

        return result.ToString();
    }
}
