using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MapViewHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerClickHandler
{
    private Image _image;
    private Texture2D _colorMap;

    public event Action<Region,int> OnRegionClicked;

    public void MapHighliterAwake()
    {
        _image = GetComponent<Image>();
        _colorMap = _image.material.GetTexture("_ColorMap") as Texture2D;
        if (_colorMap == null)
        {
            Debug.LogError("ColorMap is not set or not a Texture2D.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateHighlight(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.material.SetColor("_TargetColor", Color.clear);
        
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        UpdateHighlight(eventData);
    }

    
    void UpdateHighlight(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_image.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
        {
            float px = Mathf.InverseLerp(-_image.rectTransform.rect.width * 0.5f, _image.rectTransform.rect.width * 0.5f, localCursor.x);
            float py = Mathf.InverseLerp(-_image.rectTransform.rect.height * 0.5f, _image.rectTransform.rect.height * 0.5f, localCursor.y);

            _image.material.SetVector("_MousePosition", new Vector2(px, py));
            _image.material.SetFloat("_HighlightIntensity", 0.5f); // 하이라이트 강도 조정
            _image.material.SetColor("_HighlightColor", new Color(1, 1, 1, 1)); // 하이라이트 색상 설정
        }
        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_image.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
        {
            float px = Mathf.InverseLerp(-_image.rectTransform.rect.width * 0.5f, _image.rectTransform.rect.width * 0.5f, localCursor.x);
            float py = Mathf.InverseLerp(-_image.rectTransform.rect.height * 0.5f, _image.rectTransform.rect.height * 0.5f, localCursor.y);

            Color clickedColor = _colorMap.GetPixelBilinear(px, py);
            Region clickedRegion = Managers.Game.FindRegionByColor(clickedColor);


            if (clickedRegion != null)
            {
                OnRegionClicked?.Invoke(clickedRegion, Managers.Game.selectedFactionId);
            }
            else
            {
                Debug.LogError("No region found for the clicked color.");
                OnRegionClicked?.Invoke(null, Managers.Game.selectedFactionId);
            }
            Managers.UI.CloseAllPopupUI();
        }
    }

    public void OffRegionPopup()
    {
        OnRegionClicked?.Invoke(null, Managers.Game.selectedFactionId);
    }
}