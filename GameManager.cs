using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.Text;
using Random = UnityEngine.Random;
using EnumTypes;
using System;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Singleton
    public static GameManager instance = null;
    void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }
    #endregion

    #region Event
    public delegate void GameCompletedHandler();
    public event GameCompletedHandler onGameCompleted;

    public delegate void WordCheckedHandler(string letter);
    public event WordCheckedHandler onWordChecked;

    public delegate void LetterEnterHandler(string letter);
    public event LetterEnterHandler onLetterEnter;

    public delegate void LetterDeleteHandler();
    public event LetterDeleteHandler onLetterDelete;

    public delegate void LetterCompletedHandler();
    public event LetterCompletedHandler onLetterCompleted;
    #endregion

    #region Enum
    public GameMode gameMode;
    public GameState currentGameState;
    public TurnState myTurnState;
    public TurnState currentTurn;
    #endregion

    #region Scripts
    [SerializeField]
    private PlayerManager _playerManager;
    [SerializeField]
    private WordChecker _wordChecker;
    [SerializeField]
    private WordGenerator _wordGenerator;
    public UIManager _uiManager;
    [SerializeField]
    private PhotonView view;
    #endregion

    StringBuilder input = new StringBuilder();

    public int[] statusGuess = new int[5];

    public int count;
    public string myAnswer;
    public string opponentAnswer;

    void Start()
    {
        _wordChecker = GetComponent<WordChecker>();
        _wordGenerator = GetComponent<WordGenerator>();
        _playerManager = PlayerManager.instance;
        view = GetComponent<PhotonView>();

        if (_playerManager.playerData.nickname == "")
        {
            string nickname = "Player" + Random.Range(0, 1000).ToString("0000");
            _playerManager.playerData.nickname = nickname;
        }
        else
            PhotonNetwork.NickName = _playerManager.playerData.nickname;
    }
    void Initiallize()
    {
        currentGameState = GameState.StandBy;
        count = 0;
        input.Clear();
        myAnswer = "";
        opponentAnswer = "";
    }
    public void SingleGameStart()
    {
        Initiallize();
        gameMode = GameMode.SinglePlayer;
        StartCoroutine(GenerateRandomWordUntilValid()); 
    }
    IEnumerator GenerateRandomWordUntilValid()
    {
        while (true)
        {
            yield return _wordGenerator.GenerateRandomWord((randomWord) =>
            {
                StartCoroutine(_wordChecker.Check(randomWord, (isValid) =>
                {
                    if (isValid)
                    {
                        opponentAnswer = randomWord;
                        StopAllCoroutines();
                    }
                    else //사전 API에 없는 단어일 경우
                        StartCoroutine(GenerateRandomWordUntilValid()); 
                }));
            });

            // 다음 반복을 위해 잠시 대기
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void MultiGameStart()
    {
        Initiallize();
        gameMode = GameMode.MultiPlayer;
        StartCoroutine(CheckStartCondition()); // 서로 단어 지정
    }
    public void SetMyWord(string word, Action<bool> callback)
    {
        StartCoroutine(_wordChecker.Check(word, (b) => {
            if (!b)
                onWordChecked?.Invoke(word);
            else
            {
                myAnswer = word.ToUpper();
                callback?.Invoke(b);
                view.RPC("ReceiveWordFromOpponent", RpcTarget.Others, word);
            }
        }));
    }
    [PunRPC]
    private void ReceiveWordFromOpponent(string word)
    {
        opponentAnswer = word.ToUpper();
    }

    private IEnumerator CheckStartCondition()
    {
        while (true)
        {
            if (myAnswer != "" && opponentAnswer != "")
            {
                ChooseTurn();
                currentGameState = GameState.Start;
                yield break;
            }
            yield return new WaitForSeconds(1.5f);
        }
    }
    public void ChooseTurn()
    {
        currentTurn = TurnState.Player1;

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[i])
            {
                int turnIndex = i;
                myTurnState = (TurnState)turnIndex;
            }
        }
    }

    public void EndTurn()
    {
        switch (currentTurn)
        {
            case TurnState.Player1:
                //view.RPC("SyncTurn", RpcTarget.All, (int)TurnState.Player2);
                SyncTurn((int)TurnState.Player2);
                break;
            case TurnState.Player2:
                //view.RPC("SyncTurn", RpcTarget.All, (int)TurnState.Player1);
                SyncTurn((int)TurnState.Player1);
                break;
        }
    }
    [PunRPC]
    void SyncTurn(int turnState)
    {
        currentTurn = (TurnState)turnState;
    }

    public bool IsMyTurn()
    {
        return currentTurn == myTurnState;
    }

    public void EnterLetter(string letter)
    {
        if (!IsMyTurn()) return;

        if (gameMode.Equals(GameMode.MultiPlayer))
        {
            // 멀티플레이 모드에서의 동작
            switch (letter)
            {
                case "Enter":
                    view.RPC("CompleteLetter", RpcTarget.All);
                    break;
                case "Delete":
                    view.RPC("DeleteLetter", RpcTarget.All);
                    break;
                default:
                    view.RPC("AddLetter", RpcTarget.All, letter);
                    break;
            }
        }
        else
        {
            // 싱글 플레이 모드에서의 동작
            switch (letter)
            {
                case "Enter":
                    CompleteLetter();
                    break;
                case "Delete":
                    DeleteLetter();
                    break;
                default:
                    AddLetter(letter);
                    break;
            }
        }
    }

    [PunRPC]
    void CompleteLetter()
    {
        if (input.Length < Globals.COL_LEN) return;
        StartCoroutine(_wordChecker.Check(input.ToString(), (b) => UpdateGameState(b)));
    }
    public void UpdateGameState(bool b)
    {
        if (!b)
        {
            onWordChecked?.Invoke(input.ToString());
            return;
        }

        // 입력한 단어를 체크
        bool isCorrect = CheckLetter();

        if(IsMyTurn()) count++;

        if (isCorrect) // 성공
        {
            if (gameMode.Equals(GameMode.MultiPlayer))
                view.RPC("GameFinish", RpcTarget.All, currentTurn);
            else
                GoodJob();
        }
        else if (count >= Globals.ROW_LEN)
            TooBad();
        else
        {
            input.Clear();
            _uiManager.CompleteLetter();
            if(gameMode == GameMode.MultiPlayer) EndTurn();
        }
    }
    [PunRPC]
    void DeleteLetter()
    {
        int lastIndex = input.Length - 1;
        if (lastIndex >= 0) input.Remove(lastIndex, 1);

        _uiManager.DeleteLetter();
    }
    [PunRPC]
    protected void AddLetter(string letter)
    {
        if (input.Length > 4)
            return;

        input.Append(letter);
        _uiManager.AddLetter(letter);
    }
    bool CheckLetter()
    {
        Feedback();
        return (IsMyTurn() && opponentAnswer.Equals(input.ToString()));
    }
    void Feedback()
    {
        int tempIdx = 0;

        string answer = "";
        if (IsMyTurn())
            answer = opponentAnswer;
        else
            answer = myAnswer;

        for (int i = 0; i < Globals.COL_LEN; i++)
        {
            if (answer.IndexOf(input[i]) > -1)
            {
                if (input[i] == answer[i])
                    tempIdx = 2; 
                else
                {
                    int index = input.ToString().IndexOf(input[i]);
                    if (index == i)
                        tempIdx = 3;
                    else
                    {
                        int lastIndex = answer.LastIndexOf(input[i]);
                        if (index == lastIndex)
                            tempIdx = 1;
                        else
                            tempIdx = 3;
                    }
                }
            }
            else
                tempIdx = 1;

            statusGuess[i] = tempIdx;
        }
        _uiManager.Feedback(statusGuess, input.ToString());
    }
    [PunRPC]
    public void GameFinish(TurnState winTurn)
    {
        currentGameState = GameState.End;
        if (myTurnState == winTurn)
            GoodJob();
        else
            TooBad();
    }
    public void GoodJob() // 성공
    {
        //onGameCompleted?.Invoke();
        if (gameMode == GameMode.SinglePlayer)
        {
            _playerManager.playerData.singlePlay.playCount++;
            _playerManager.playerData.singlePlay.winCount++;
            _playerManager.playerData.singlePlay.guessCounts[count - 1]++;
        }
        else
        {
            _playerManager.playerData.multiPlay.playCount++;
            _playerManager.playerData.multiPlay.winCount++;
        }
        string mode = gameMode == GameMode.SinglePlayer ? "SINGLE" : "MULTI";
        _playerManager.SaveHistory(mode, true, count - 1, DateTime.Now.ToString("yyyyMMdd"));
        _playerManager.Save();
        // 결과 UI 띄우기
        _uiManager.GameWin();
    }
    public void TooBad() // 실패(횟수초과)
    {
        _playerManager.playerData.singlePlay.playCount++;
        string mode = gameMode == GameMode.SinglePlayer ? "SINGLE" : "MULTI";

        _playerManager.SaveHistory(mode, false, -1, DateTime.Now.ToString("yyyyMMdd"));
        _playerManager.Save();

        // 결과 UI 띄우기
        _uiManager.GameLose();
    }

    public void TryAgain()
    {
        if (gameMode == GameMode.SinglePlayer)
            SingleGameStart();
        else
            MultiGameStart();

        PhotonNetwork.LoadLevel(1);
    }

    public void Exit()
    {
        Destroy(this.gameObject);
        PhotonNetwork.LoadLevel(0);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.NickName);
        _uiManager.PlayerLeftRoom(otherPlayer.NickName);
        Invoke("Exit", 2f);
        PhotonNetwork.LeaveRoom();
    }
}
