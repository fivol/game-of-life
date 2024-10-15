using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Random = UnityEngine.Random;

public class LifeGameGrid : MonoBehaviour
{
    public Color cellColor;
    public int gridSizeX = 20;
    public int gridSizeY = 20;
    public float cellSize = 20f;
    public float fadeTime = 0.4f;
    public GameObject cellPrefab;
    public Transform canvasTransform;
    private bool[,] _cells;
    private float[,] _fadeValues;
    private Image[,] _cellImages;
    private Coroutine _coroutine;

    void Start()
    {
        _cells = new bool[gridSizeX, gridSizeY];
        _fadeValues = new float[gridSizeX, gridSizeY];
        _cellImages = new Image[gridSizeX, gridSizeY];

        CreateGrid();
        _coroutine = StartCoroutine(GameLoop());
    }

    private void OnEnable()
    {
        _coroutine = StartCoroutine(GameLoop());
    }

    private void OnDisable()
    {
        StopCoroutine(_coroutine);
    }

    void CreateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                GameObject cell = Instantiate(cellPrefab, canvasTransform);
                cell.transform.parent = gameObject.transform;
                RectTransform rectTransform = cell.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(x * cellSize - (gridSizeX * cellSize) / 2,
                    y * cellSize - (gridSizeY * cellSize) / 2);
                rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                _cellImages[x, y] = cell.GetComponent<Image>();
                _cellImages[x, y].color = new Color(0, 0, 0, 0);
            }
        }
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            Randomize3X3();
            yield return StartCoroutine(FadeIn());

            for (int i = 0; i < 10; i++)
            {
                if (!ApplyLifeRules())
                {
                    continue;
                }

                UpdateGridVisuals();
                yield return new WaitForSeconds(0.15f);
            }

            yield return StartCoroutine(FadeOut());
        }
    }

    void Randomize3X3()
    {
        int startX = Random.Range(0, gridSizeX - 3);
        int startY = Random.Range(0, gridSizeY - 3);

        for (int x = startX; x < startX + 4; x++)
        {
            for (int y = startY; y < startY + 4; y++)
            {
                if (Random.Range(0, 2) == 0)
                {
                    _cells[x, y] = true;
                }
            }
        }
    }

    IEnumerator FadeIn()
    {
        float time = 0;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float t = 1 - time / fadeTime;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (_cells[x, y])
                    {
                        _fadeValues[x, y] = 1 - t;
                    }
                }
            }

            UpdateGridVisuals();
            yield return null;
        }

        UpdateGridVisuals();
    }

    IEnumerator FadeOut()
    {
        float time = 0;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            float t = 1 - time / fadeTime;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (_cells[x, y])
                    {
                        _fadeValues[x, y] = t;
                    }
                }
            }

            UpdateGridVisuals();
            yield return null;
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                _cells[x, y] = false;
                _fadeValues[x, y] = 0;
            }
        }

        UpdateGridVisuals();
    }

    bool ApplyLifeRules()
    {
        bool changed = false;
        bool[,] newCells = new bool[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                int aliveNeighbors = CountAliveNeighbors(x, y);
                if (!_cells[x, y] && aliveNeighbors == 3)
                {
                    changed = true;
                    newCells[x, y] = true;
                    _fadeValues[x, y] = 1;

                }
                else if (_cells[x, y] && (aliveNeighbors < 2 || aliveNeighbors > 3))
                {
                    newCells[x, y] = false;
                    changed = true;
                    _fadeValues[x, y] = 0;
                }
                else
                {
                    newCells[x, y] = _cells[x, y];
                }
            }
        }

        _cells = newCells;
        return changed;
    }

    int CountAliveNeighbors(int x, int y)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;
                int nx = x + i;
                int ny = y + j;
                if (nx >= 0 && nx < gridSizeX && ny >= 0 && ny < gridSizeY && _cells[nx, ny])
                {
                    count++;
                }
            }
        }

        return count;
    }

    void UpdateGridVisuals()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (_fadeValues[x, y] > 0)
                {
                    cellColor.a = _fadeValues[x, y];
                    _cellImages[x, y].color = cellColor;
                }
                else
                {
                    _cellImages[x, y].color = new Color(0, 0, 0, 0);
                }
            }
        }
    }
}