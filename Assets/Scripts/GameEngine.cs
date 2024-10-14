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
    public int playerN = 0;
    
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

    private Board[] _boards = new Board[2]; 
    
    public void PlayerMoves()
    {
        currentStage = Stage.PlayerStage;
        preGameUI.SetActive(false);
        gameUI.SetActive(true);
        var board = _createBoard();
        var width = _getWidth();
        var height = width / Screen.width * Screen.height;
        var margin = 0.2f;
        var boardSize = height - 2 * margin;
        var x = width / 2 - boardSize / 2;
        board.Initialize(-x, -boardSize/2, boardSize);
        _boards[playerN] = board;
    }

    public void PlayerDone()
    {
        boardsParent.SetActive(false);
        if (playerN == 0)
        {
            playerN = 1;
            PreGame();
        }
        else
        {
            GameResults();
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
        playerN = 0;
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
        return Camera.main.orthographicSize * 2;
    }

    public void GameAnimation()
    {
        currentStage = Stage.AnimationState;
        gameUI.SetActive(false);
        animationUI.SetActive(true);

        var state1 = _boards[0].GetState();
        var state2 = _boards[1].GetState();
        Destroy(_boards[0]);
        Destroy(_boards[1]);
        
        var width = _getWidth();
        var distance = 0.4f;
        var height = width / Screen.width * Screen.height;
        var margin = 0.2f;
        var boardSize = Math.Min((width - margin * 2 - distance) / 2, height - margin * 2);
        var startX = -boardSize - distance / 2;
        
        _boards[0] = _createBoard();
        _boards[0].Initialize(startX, -boardSize / 2, boardSize);
        _boards[0].SetState(state1);
        
        _boards[1] = _createBoard();
        _boards[1].Initialize(distance / 2, -boardSize / 2, boardSize);
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
        for(int i = 0; i < 50; i++)
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
    
    public void GameResults()
    {
        currentStage = Stage.PlayResultsStage;
        gameUI.SetActive(false);
        animationUI.SetActive(false);
        resultsUI.SetActive(true);

        var score1 = _boards[0].CountTiles();
        var score2 = _boards[1].CountTiles();
        
        var changed1 = _boards[0].changedTilesCount;
        var changed2 = _boards[1].changedTilesCount;
        
        player1Desc.text = $"{changed1} cells changed\n{score1} cells survived";
        player2Desc.text = $"{changed2} cells changed\n{score2} cells survived";

        var winColor = new Color32(0x00, 0x9E, 0x39, 0xFF);
        var loseColor = new Color32(0xE6, 0x25, 0x42, 0xFF);
        if (score1 > score2)
        {
            player1win.text = "win";
            player1win.color = winColor;
            player2win.text = "lose";
            player2win.color = loseColor;
        }
        else
        {
            player1win.text = "lose";
            player1win.color = loseColor;
            player2win.text = "win";
            player2win.color = winColor;
        }
    }

    public void PreGame()
    {
        currentStage = Stage.PreGameStage;
        menuUI.SetActive(false);
        preGameUI.GetComponentsInChildren<TMP_Text>()[0].text = "player " + (playerN + 1).ToString();
        preGameUI.SetActive(true);
    }

    public void StartGame()
    {
        PreGame();
    }

    void Start()
    {
        menuUI.SetActive(true);
        resultsUI.SetActive(false);
        gameUI.SetActive(false);
        preGameUI.SetActive(false);
        pauseUI.SetActive(false);
    }
    
}