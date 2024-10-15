using UnityEditor.UI;
using UnityEngine;


class Tile: MonoBehaviour
{
    public void Initialize(int x, int y)
    {
        X = x;
        Y = y;
        _renderer = gameObject.GetComponent<SpriteRenderer>();
        if (Random.Range(0, 10) == 0)
        {
            Activate();
        }
    }

    private SpriteRenderer _renderer;
    public int X, Y;
    private bool _isActive;

    public bool IsActive()
    {
        return _isActive;
    }

    public void Activate()
    {
        _isActive = true;
        _renderer.color = Color.black;
    }

    public void Deactivate()
    {
        _isActive = false;
        _renderer.color = Color.white;
    }

    public void Toggle()
    {
        SetActive(!IsActive());
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }
}
