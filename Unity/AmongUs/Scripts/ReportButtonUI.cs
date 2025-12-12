using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReportButtonUI : MonoBehaviour
{
    [SerializeField]
    private Button reportButton;
    
    public void SetInteractable(bool interactable)
    {
        reportButton.interactable = interactable;
    }

    public void OnClickButton()
    {
        var character = AmongUsRoomPlayer.MyRoomPlayer.myCharacter as InGameCharacterMover;
        character.Report();
    }
}
