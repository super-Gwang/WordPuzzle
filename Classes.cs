using System;
using System.Collections.Generic;
using Structs;
using UnityEngine;

namespace Classes
{
    [Serializable]
    public class PlayerData
    {
        public string nickname;
        public PlayStat singlePlay;
        public PlayStat multiPlay;
        public List<History> histories;
        public PlayerData()
        {
            singlePlay = new PlayStat(0, 0);
            multiPlay = new PlayStat(0, 0);
            histories = new List<History>();
        }
    }

    [Serializable]
    public class History
    {
        public string mode;
        public bool isWin;
        public int guessCount;
        public string time;

        public History(string _mode, bool _isWin, int _guessCount, string _time)
        {
            mode = _mode;
            isWin = _isWin;
            guessCount = _guessCount;
            time = _time;
        }
    }
}
