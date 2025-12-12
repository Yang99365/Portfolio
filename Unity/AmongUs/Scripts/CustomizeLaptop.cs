using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeLaptop : MonoBehaviour
{
    [SerializeField]
    private Sprite useButtonSprite;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var inst = Instantiate(spriteRenderer.material);
        spriteRenderer.material = inst;
    }

    private void OnTriggerEnter2D(Collider2D collsion) {
        
        var character = collsion.GetComponent<CharacterMover>();
        if(character != null && character.isOwned)
        {
            spriteRenderer.material.SetFloat("_Highlighted", 1f);
            LobbyUIManager.instance.SetUseButton(useButtonSprite, OnClickUse);
        }
        
    }
    private void OnTriggerExit2D(Collider2D collsion) {
        var character = collsion.GetComponent<CharacterMover>();
        if(character != null && character.isOwned)
        {
            spriteRenderer.material.SetFloat("_Highlighted", 0f);
            LobbyUIManager.instance.UnsetUseButton();
        }
    }

    public void OnClickUse()
    {
        LobbyUIManager.instance.CustomizeUI.Open();
    }
}
