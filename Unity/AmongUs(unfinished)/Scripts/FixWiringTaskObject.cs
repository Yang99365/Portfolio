using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixWiringTaskObject : MonoBehaviour
{
    [SerializeField]
    private Sprite _UseButtonSprite;

    private SpriteRenderer _SpriteRenderer;

    void Start()
    {
        _SpriteRenderer = GetComponent<SpriteRenderer>();
        _SpriteRenderer.material = Instantiate(_SpriteRenderer.material);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var character = collision.GetComponent<InGameCharacterMover>();
        if(character != null && character.isOwned)
        {
            _SpriteRenderer.material.SetFloat("_Highlighted", 1f);
            InGameUIManager.instance.SetUseButton(_UseButtonSprite, OnClickUse);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var character = collision.GetComponent<InGameCharacterMover>();
        if(character != null && character.isOwned)
        {
            _SpriteRenderer.material.SetFloat("_Highlighted", 0f);
            InGameUIManager.instance.UnSetUseButton();
        }
    }

    public void OnClickUse()
    {
        InGameUIManager.instance.FixWiringTaskUI.Open();
    }
}
