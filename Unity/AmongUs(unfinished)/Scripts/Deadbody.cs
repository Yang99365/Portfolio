using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Deadbody : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;

    private EPlayerColor deadbodyColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    [ClientRpc]
    public void RpcSetColor(EPlayerColor color)
    {
        deadbodyColor = color;
        spriteRenderer.material.SetColor("_PlayerColor", PlayerColor.GetColor(color));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<InGameCharacterMover>();
        if(player != null && player.isOwned && (player.playerType & EPlayerType.Ghost)!= EPlayerType.Ghost)
        {
            InGameUIManager.instance.ReportButtonUI.SetInteractable(true);
            var myCharacter = AmongUsRoomPlayer.MyRoomPlayer.myCharacter as InGameCharacterMover;
            myCharacter.foundDeadbodyColor = deadbodyColor;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<InGameCharacterMover>();
        if(player != null && player.isOwned && (player.playerType & EPlayerType.Ghost)!= EPlayerType.Ghost)
        {
            InGameUIManager.instance.ReportButtonUI.SetInteractable(false);
        }
    }
}
    
