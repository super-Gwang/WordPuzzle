using System;
using System.Linq;

namespace Structs
{
    [Serializable]
    public struct PlayStat
    {
        public int playCount;
        public int winCount;
        public int[] guessCounts;

        public PlayStat(int _playCount, int _winCount)
        {
            playCount = _playCount;
            winCount = _winCount;
            guessCounts = Enumerable.Repeat<int>(0, 6).ToArray<int>();
        }
    }
}
