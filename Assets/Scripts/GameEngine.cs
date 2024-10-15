using System;
using System.Collections;
using TMPro;
using UnityEngine;


public enum Stage
{
    PlayerStage,
    PreGameStage,
    MenuStage,
    PlayResultsStage,
    AnimationState,
    PauseStage,
}

public class GameEngine : MonoBehaviour
{
    public Stage currentStage;

    public GameObject mainBoard;
    public GameObject boardsParent;

    public GameObject menuUI;
    public GameObject preGameUI;
    public GameObject gameUI;
    public GameObject pauseUI;
    public GameObject resultsUI;
    public GameObject animationUI;

    public TMP_Text player1Desc;
    public TMP_Text player2Desc;
    public TMP_Text player1win;
    public TMP_Text player2win;
    public TMP_Text cellsLeft;
    public int activationsLimit = 5;

    private readonly Board[] _boards = new Board[2];
    private bool[][] _initBoardState;
    private int _playerN = 0;
    
    private int _changedTiles1 = 0;
    private int _changedTiles2 = 0;

    public void PlayerMoves()
    {
        currentStage = Stage.PlayerStage;
        preGameUI.SetActive(false);
        gameUI.SetActive(true);
        boardsParent.SetActive(true);

        _setCellsLeft(activationsLimit);
        var board = _createBoard();
        var width = _getWidth();
        var height = width / Screen.width * Screen.height;
        var margin = 0f;
        var boardSize = height - 2 * margin;
        board.Initialize(-boardSize / 2, -boardSize / 2, boardSize, activationsLimit, OnTileClicked);
        if (_playerN == 1)
        {
            board.SetState(_initBoardState);
        }
        else
        {
            _initBoardState = board.GetState();
        }

        _boards[_playerN] = board;
    }

    private void _setCellsLeft(int n)
    {
        cellsLeft.text = $"{n} cells\nleft";
    }

    public void OnTileClicked()
    {
        _setCellsLeft(_boards[_playerN].activationsLimit);
    }

    public void PlayerDone()
    {
        boardsParent.SetActive(false);
        if (_playerN == 0)
        {
            _playerN = 1;
            PreGame();
        }
        else
        {
            _changedTiles1 = _countDiff(_initBoardState, _boards[0].GetState());
            _changedTiles2 = _countDiff(_initBoardState, _boards[1].GetState());
            GameAnimation();
        }
    }

    public void Continue()
    {
        pauseUI.SetActive(false);
        gameUI.SetActive(true);
        boardsParent.SetActive(true);
    }

    public void Pause()
    {
        currentStage = Stage.PauseStage;
        gameUI.SetActive(false);
        pauseUI.SetActive(true);
        boardsParent.SetActive(false);
    }

    public void Menu()
    {
        currentStage = Stage.MenuStage;
        _destroyBoards();
        _playerN = 0;
        menuUI.SetActive(true);
        pauseUI.SetActive(false);
        resultsUI.SetActive(false);
    }

    private Board _createBoard()
    {
        var board = Instantiate(mainBoard, Vector3.zero, Quaternion.identity).GetComponentInChildren<Board>();
        board.transform.parent = boardsParent.transform;
        return board;
    }

    private float _getWidth()
    {
        return Camera.main.orthographicSize * Camera.main.aspect * 2;
    }

    private void _destroyBoards()
    {
        for (int i = 0; i < 2; i++)
        {
            if (_boards[i] is not null)
            {
                Destroy(_boards[i].gameObject);
                _boards[i] = null;
            }
        }
    }

    public void GameAnimation()
    {
        currentStage = Stage.AnimationState;
        gameUI.SetActive(false);
        preGameUI.SetActive(false);
        animationUI.SetActive(true);
        boardsParent.SetActive(true);

        var state1 = _boards[0].GetState();
        var state2 = _boards[1].GetState();
        _destroyBoards();

        var width = _getWidth();
        var distance = 0.4f;
        var height = width / Screen.width * Screen.height;
        var margin = 0f;
        var boardSize = Math.Min((width - margin * 2 - distance) / 2, height - margin * 2);
        var startX = -boardSize - distance / 2;

        _boards[0] = _createBoard();
        _boards[0].Initialize(startX, -boardSize / 2, boardSize, 0);
        _boards[0].SetState(state1);

        _boards[1] = _createBoard();
        _boards[1].Initialize(distance / 2, -boardSize / 2, boardSize, 0);
        _boards[1].SetState(state2);
    }

    public void AnimationNext()
    {
        _boards[0].NextIteration();
        _boards[1].NextIteration();
    }

    public void AnimationPrev()
    {
        _boards[0].PrevIteration();
        _boards[1].PrevIteration();
    }

    IEnumerator _animationRun()
    {
        for (int i = 0; i < 50; i++)
        {
            var changed = _boards[0].NextIteration();
            changed |= _boards[1].NextIteration();
            yield return new WaitForSeconds(0.1f);
            if (!changed)
            {
                break;
            }
        }

        GameResults();
        yield return null;
    }

    public void AnimationStart()
    {
        StartCoroutine(_animationRun());
    }

    private int _countDiff(bool[][] state1, bool[][] state2)
    {
        int count = 0;
        for (int i = 0; i < state1.Length; i++)
        {
            for (int j = 0; j < state1[i].Length; j++)
            {
                if (state1[i][j] != state2[i][j])
                    count++;
            }
        }

        return count;
    }

    public void GameResults()
    {
        currentStage = Stage.PlayResultsStage;
        gameUI.SetActive(false);
        animationUI.SetActive(false);
        resultsUI.SetActive(true);
        
        var score1 = _boards[0].CountTiles();
        var score2 = _boards[1].CountTiles();
        
        _destroyBoards();

        player1Desc.text = $"{_changedTiles1} cells changed\n{score1} cells survived";
        player2Desc.text = $"{_changedTiles2} cells changed\n{score2} cells survived";

        var winColor = new Color32(0x00, 0x9E, 0x39, 0xFF);
        var loseColor = new Color32(0xE6, 0x25, 0x42, 0xFF);
        player1win.text = "you lose";
        player2win.text = "you lose";
        player1win.color = loseColor;
        player2win.color = loseColor;
        if (score1 > score2)
        {
            player1win.text = "you win";
            player1win.color = winColor;
        }
        else if(score1 < score2)
        {
            player2win.text = "you win";
            player2win.color = winColor;
        }
    }

    public void PreGame()
    {
        currentStage = Stage.PreGameStage;
        menuUI.SetActive(false);
        gameUI.SetActive(false);
        preGameUI.GetComponentsInChildren<TMP_Text>()[0].text = "player " + (_playerN + 1).ToString();
        preGameUI.SetActive(true);
    }

    public void StartGame()
    {
        PreGame();
    }

    void Start()
    {
        animationUI.SetActive(false);
        resultsUI.SetActive(false);
        gameUI.SetActive(false);
        preGameUI.SetActive(false);
        pauseUI.SetActive(false);
        Menu();
    }
}