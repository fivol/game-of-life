using TMPro;
using UnityEngine;

public class TextAnim : MonoBehaviour
{

    public float targetFontSize;
    public float loopTime = 1.0f;
    
    private float _currPhase = 0;
    private int _upDirection = 1;

    private float _fontSize;

    private void Start()
    {
        _fontSize = GetComponent<TMP_Text>().fontSize;
    }

    void Update()
    {
        if (_upDirection == 1 && _currPhase > 1)
        {
            _upDirection = -1;
        } else if (_upDirection == -1 && _currPhase < 0)
        {
            _upDirection = 1;
        }
        _currPhase += Time.deltaTime / loopTime * 2 * _upDirection;


        GetComponent<TMP_Text>().fontSize = _fontSize + (targetFontSize - _fontSize) * _currPhase;
    }
}