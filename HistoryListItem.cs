using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Classes;

public class HistoryListItem : MonoBehaviour
{
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text modeText;
    [SerializeField] TMP_Text winOrloseText;

    public void SetUp(History history)
    {
        timeText.text = history.time;
        modeText.text = history.mode;
        winOrloseText.text = history.isWin ? "<color=blue>Win</color>" : "<color=red>Lose</color>";
    }
}
