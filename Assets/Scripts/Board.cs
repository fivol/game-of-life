using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


public class Board : MonoBehaviour
{
    public GameObject tilePrefab;
    public int SideTilesCount;
    private Tile[][] _tiles;
    private List<bool[][]> _history = new List<bool[][]>();
    public int activationsLimit;
    public delegate void ClickCallback();

    private ClickCallback _callback;

    private void _createBoard(float startX, float startY, float sideSize)
    {
        var gap = 0.02f;
        var tileSize = (sideSize - (SideTilesCount - 1) * gap) / SideTilesCount;
        var side = (int)Math.Round(sideSize / tileSize);

        _tiles = new Tile[side][];
        tilePrefab.transform.localScale = new Vector2(tileSize, tileSize);
        for (var i = 0; i < side; i++)
        {
            _tiles[i] = new Tile[side];
            for (var j = 0; j < side; j++)
            {
                var pos = new Vector3(
                    tileSize / 2 + startX + i * tileSize + gap * i,
                    tileSize / 2 + startY + j * tileSize + gap * j,
                    0);
                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity);
                tileObj.transform.parent = transform;
                tileObj.name = $"Tile({i}, {j})";
                Tile obj = tileObj.GetComponent<Tile>();
                obj.Initialize(i, j);
                _tiles[i][j] = obj;
            }
        }
    }

    [CanBeNull]
    Tile _getCloseTile(int x, int y, int shiftX, int shiftY)
    {
        var newX = x + shiftX;
        var newY = y + shiftY;
        if (newX < 0 || newY < 0 || newX >= SideTilesCount || newY >= SideTilesCount)
        {
            return null;
        }

        return _tiles[newX][newY];
    }

    int _countCloseTiles(int x, int y)
    {
        var count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                var tile = _getCloseTile(x, y, i, j);
                if (tile is not null && tile.IsActive())
                {
                    count++;
                }
            }
        }

        return count;
    }

    bool _iterateLife()
    {
        var updates = new List<Tile>();
        for (int i = 0; i < _tiles.Length; i++)
        {
            for (int j = 0; j < _tiles[i].Length; j++)
            {
                var tile = _tiles[i][j];
                var neighbours = _countCloseTiles(tile.X, tile.Y);
                if ((neighbours < 2 || neighbours > 3) && tile.IsActive())
                {
                    updates.Add(tile);
                }
                else if (neighbours == 3 && !tile.IsActive())
                {
                    updates.Add(tile);
                }
            }
        }

        foreach (var upd in updates)
        {
            upd.Toggle();
        }
        return updates.Count > 0;
    }

    public bool[][] GetState()
    {
        var state = new bool[SideTilesCount][];
        for (int i = 0; i < SideTilesCount; i++)
        {
            state[i] = new bool[SideTilesCount];
            for (int j = 0; j < SideTilesCount; j++)
            {
                state[i][j] = _tiles[i][j].IsActive();
            }
        }

        return state;
    }
    
    public void Initialize(float x, float y, float sideSize, int limit, ClickCallback callback=null)
    {
        _createBoard(x, y, sideSize);
        activationsLimit = limit;
        _callback = callback;
    }
    
    void _handleClick()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider is not null)
        {
            Debug.Log("Hit Object: " + hit.collider.gameObject.name);

            var tile = hit.collider.gameObject.GetComponent<Tile>();
            if (tile is not null && !tile.IsActive() && activationsLimit > 0)
            {
                activationsLimit--;
                tile.Activate();
                _callback?.Invoke();
            }
        }
    }

    public bool NextIteration()
    {
        _history.Add(GetState());
        return _iterateLife();
    }

    public void PrevIteration()
    {
        if (_history.Count == 0)
        {
            return;
        }
        var state = _history[_history.Count - 1];
        _history.RemoveAt(_history.Count - 1);
        SetState(state);
    }

    public void SetState(bool[][] state)
    {
        for (int i = 0; i < state.Length; i++)
        {
            for (int j = 0; j < state[i].Length; j++)
            {
                _tiles[i][j].SetActive(state[i][j]);
            }
        }
    }

    public int CountTiles()
    {
        var count = 0;
        for (int i = 0; i < SideTilesCount; i++)
        {
            for (int j = 0; j < SideTilesCount; j++)
            {
                count += _tiles[i][j].IsActive() ? 1 : 0;
            }
        }

        return count;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _handleClick();
        }
    }
}