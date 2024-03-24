using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using EnumTypes;
using Michsky.MUIP;

public class UIManager : MonoBehaviour
{
    public GameManager _gameManager;
    public Sprite[] alphabetImgs = new Sprite[26];
    public Image[] keyboard;
    public Sprite emptyImg;
    public GameObject turnPanel;
    public Animator coinAnim;
    public Sprite[] coinImgs = new Sprite[2];
    public Transform singleSlotContainer;
    public Transform multiSlotContainer;
    public Transform multiOtherSlotContainer;
    public List<Transform> mySlots;
    public List<Transform> opponentSlots;
    public int row_Idx;
    public int col_Idx;
    public Color[] colors;
    public GameObject resultUI;
    public GameObject winUI;
    public GameObject loseUI;
    public TMP_Text answerText;
    public GameObject wordInputFieldContent;
    public TMP_InputField wordInputField;
    public NotificationManager notificationPopup;
    public NotificationManager PlayerLeftPopup;
    public GameObject loadingBar;

    private void Start()
    {
        _gameManager = GameManager.instance;
        Init();
    }
    public void Init()
    {
        row_Idx = 0; col_Idx = 0;

        Subscribe_Event();
        SettingSlot();
        ChooseTurn((int)_gameManager.currentTurn);
    }

    void Subscribe_Event()
    {
        _gameManager._uiManager = GetComponent<UIManager>();
        _gameManager.onGameCompleted += GameWin;
        _gameManager.onWordChecked += ShowInvalidWord_Popup;
        _gameManager.onLetterEnter += AddLetter;
        _gameManager.onLetterDelete += DeleteLetter;
        _gameManager.onLetterCompleted += CompleteLetter;
    }

    void SettingSlot()
    {
        if (_gameManager.gameMode.Equals(GameMode.SinglePlayer))
        {
            singleSlotContainer.gameObject.SetActive(true);
            for (int i = 0; i < singleSlotContainer.childCount; i++)
            {
                mySlots.Add(singleSlotContainer.GetChild(i));
            }
        }
        else
        {
            multiSlotContainer.gameObject.SetActive(true);
            multiOtherSlotContainer.gameObject.SetActive(true);

            for (int i = 0; i < multiSlotContainer.childCount; i++)
            {
                mySlots.Add(multiSlotContainer.GetChild(i));
                opponentSlots.Add(multiOtherSlotContainer.GetChild(i));
            }
        }
    }
    public void SetMyWord()
    {
        if (wordInputField.text.Length != Globals.COL_LEN) return;
        
        _gameManager.SetMyWord(wordInputField.text, (b) => 
        {
            if (b)
            {
                wordInputFieldContent.SetActive(false);
                loadingBar.SetActive(true);
                StartCoroutine(ClosePopUp());
            }
            else
                return;
        });

    }
    IEnumerator ClosePopUp()
    {
        yield return new WaitUntil(() => _gameManager.currentGameState == GameState.Start);
        loadingBar.SetActive(false);
    }

    public void ChooseTurn(int index)
    {
        if (_gameManager.gameMode.Equals(GameMode.SinglePlayer)) return;
        wordInputFieldContent.SetActive(true);
    }

    public void EnterLetter(string letter)
    {
        _gameManager.EnterLetter(letter);
    }

    public void AddLetter(string letter)
    {
        if (col_Idx > Globals.COL_LEN)
            return;
        char charLetter = letter[0];
        int imgIndex = charLetter - 'A';

        if (_gameManager.IsMyTurn())
        {
            mySlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetChild(0).GetComponent<Image>().sprite = alphabetImgs[imgIndex];
            mySlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetComponent<Image>().color = colors[0];
            mySlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetComponent<Animator>().Play("Show");
        }
        else
        {
            opponentSlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetChild(0).GetComponent<Image>().sprite = alphabetImgs[imgIndex];
            opponentSlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetComponent<Image>().color = colors[0];
            opponentSlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetComponent<Animator>().Play("Show");
        }    
        col_Idx++;
        col_Idx = Mathf.Clamp(col_Idx, 0, Globals.COL_LEN);
    }

    public void DeleteLetter()
    {
        col_Idx--;
        col_Idx = Mathf.Clamp(col_Idx, 0, Globals.COL_LEN);

        if (_gameManager.IsMyTurn())
        {
            mySlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetChild(0).GetComponent<Image>().sprite = emptyImg;
            mySlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetComponent<Image>().color = Color.white;
        }
        else
        {
            opponentSlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetChild(0).GetComponent<Image>().sprite = emptyImg;
            opponentSlots[row_Idx].GetChild(col_Idx % Globals.COL_LEN).GetComponent<Image>().color = Color.white;
        }
    }

    public void CompleteLetter()
    {
        col_Idx = 0;
        if(_gameManager.gameMode == GameMode.SinglePlayer || _gameManager.currentTurn.Equals(TurnState.Player2)) row_Idx++;
        if (row_Idx >= Globals.ROW_LEN) GameLose();
    }

    public void Feedback(int[] idx, string word)
    {
        if (_gameManager.IsMyTurn())
        {
            for (int i = 0; i < idx.Length; i++)
            {
                mySlots[row_Idx].GetChild(i).GetComponent<Image>().color = colors[idx[i]];
            }
            for (int i = 0; i < Globals.COL_LEN; i++)
            {
                int index = word[i] - 'A';

                if (keyboard[index].color == colors[2]) continue;
                keyboard[index].color = colors[idx[i]];
            }
        }
        else
        {
            for (int i = 0; i < idx.Length; i++)
            {
                opponentSlots[row_Idx].GetChild(i).GetComponent<Image>().color = colors[idx[i]];
            }
        }


    }
    void ShowInvalidWord_Popup(string word)
    {
        notificationPopup.Open(word);
    }

    public void GameWin()
    {
        resultUI.SetActive(true);
        winUI.SetActive(true);
    }
    public void GameLose()
    {
        resultUI.SetActive(true);
        loseUI.SetActive(true);
        answerText.gameObject.SetActive(true);
        answerText.text = "Answer: " + _gameManager.opponentAnswer;
    }

    public void TryAgain()
    {
        _gameManager.TryAgain();
    }

    public void Exit()
    {
        _gameManager.Exit();
    }

    public void PlayerLeftRoom(string nickname)
    {
        PlayerLeftPopup.Open(nickname);
    }
}
