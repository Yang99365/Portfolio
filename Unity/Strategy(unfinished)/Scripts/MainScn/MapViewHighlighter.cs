using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class MapViewHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    private Image _image;
    private Texture2D _colorMap;

    private void Awake()
    {
        _image = GetComponent<Image>();

        // Get the Material from the Image
        Material mat = _image.material;

        // Get the ColorMap texture
        _colorMap = mat.GetTexture("_ColorMap") as Texture2D;
        if (_colorMap == null)
        {
            Debug.LogError("ColorMap is not set or not a Texture2D.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Do something when the pointer enters the Image
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset _TargetColor when the pointer exits the Image
        _image.material.SetColor("_TargetColor", Color.black - Color.white);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_image.rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
        {
            // Calculate the UV coordinate
            float px = Mathf.InverseLerp(-_image.rectTransform.rect.width * 0.5f, _image.rectTransform.rect.width * 0.5f, localCursor.x);
            float py = Mathf.InverseLerp(-_image.rectTransform.rect.height * 0.5f, _image.rectTransform.rect.height * 0.5f, localCursor.y);

            Color regionColor = _colorMap.GetPixelBilinear(px, py);

            if (regionColor.a >= 0.01f)
            {
                _image.material.SetVector("_MousePosition", new Vector2(px, py));
                _image.material.SetColor("_TargetColor", regionColor);
            }
            else
            {
                _image.material.SetColor("_TargetColor", Color.black - Color.white);
            }
        }
    }
}
