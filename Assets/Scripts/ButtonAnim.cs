using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAnim : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    private void Start()
    {
        currSprite = GetComponent<Image>().sprite;
    }

    public Sprite newSprite;
    public Sprite currSprite;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        GetComponent<Image>().sprite = newSprite;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        GetComponent<Image>().sprite = currSprite;
    }
}
