using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineObject : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;

    [SerializeField]
    private Color OutlineColor;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var inst = Instantiate(spriteRenderer.material);
        spriteRenderer.material = inst;
        spriteRenderer.material.SetColor("_OutlineColor",OutlineColor);
        
    }

    protected virtual void OnTriggerEnter2D(Collider2D collsion) {
        
        var character = collsion.GetComponent<CharacterMover>();
        if(character != null && character.isOwned)
        {
            spriteRenderer.enabled = true;
        }
        
    }
    protected virtual void OnTriggerExit2D(Collider2D collsion) {
        var character = collsion.GetComponent<CharacterMover>();
        if(character != null && character.isOwned)
        {
            spriteRenderer.enabled = false;
        }
    }
}
