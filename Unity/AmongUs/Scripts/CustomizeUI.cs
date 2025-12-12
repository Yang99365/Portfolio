using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.Examples.Basic;

public class CustomizeUI : MonoBehaviour
{
    [SerializeField]
    private Button colorButton;
    [SerializeField]
    private GameObject colorPanel;
    [SerializeField]
    private Button gameRuleButton;
    [SerializeField]
    private GameObject gameRulePanel;
    [SerializeField]
    private Image characterPreview;
    
    [SerializeField]
    private List<ColorSelectButton> colorSelectButtons;

    // Start is called before the first frame update
    void Start()
    {
        //원본에 영향을 주지 않기 위해 인스턴스 생성
        var inst = Instantiate(characterPreview.material);
        characterPreview.material = inst;
    }
    public void ActiveColorPanel()
    {
        colorButton.image.color = new Color(0f,0f,0f,0.75f);
        gameRuleButton.image.color = new Color(0f,0f,0f,0.25f);

        colorPanel.SetActive(true);
        gameRulePanel.SetActive(false);
    }
    public void ActiveGameRulePanel()
    {
        colorButton.image.color = new Color(0f,0f,0f,0.25f);
        gameRuleButton.image.color = new Color(0f,0f,0f,0.75f);

        colorPanel.SetActive(false);
        gameRulePanel.SetActive(true);
    }

    private void OnEnable()
    {
        UpdateColorButton();
        ActiveColorPanel();

        var roomSlots = (NetworkManager.singleton as AmongUsRoomManager).roomSlots;
        foreach(var player in roomSlots)
        {
            var aPlayer = player as AmongUsRoomPlayer;
            if(aPlayer.isLocalPlayer)
            {
                UpdatePreviewColor(aPlayer.playerColor);
                break;
            }
        }
    }

    public void UpdateColorButton()
    {
        var roomSlots = (NetworkManager.singleton as AmongUsRoomManager).roomSlots;

        for(int i = 0; i < colorSelectButtons.Count; i++)
        {
            colorSelectButtons[i].SetInteractable(true);
        }
        foreach(var player in roomSlots)
        {
            var aPlayer = player as AmongUsRoomPlayer;
            colorSelectButtons[(int)aPlayer.playerColor].SetInteractable(false);
        }
    }

    public void UpdateSelectColorButton(EPlayerColor color)
    {
        colorSelectButtons[(int)color].SetInteractable(false);
    }

    public void UpdateUnselectColorButton(EPlayerColor color)
    {
        colorSelectButtons[(int)color].SetInteractable(true);
    }
    public void UpdatePreviewColor(EPlayerColor color)
    {
        characterPreview.material.SetColor("_PlayerColor",PlayerColor.GetColor(color));
    }

    public void OnClickColorButton(int index)
    {
        if(colorSelectButtons[index].isInteractable)
        {
            AmongUsRoomPlayer.MyRoomPlayer.CmdSetPlayerColor((EPlayerColor)index);
            UpdatePreviewColor((EPlayerColor)index);
        }
    }

    public void Open()
    {
        AmongUsRoomPlayer.MyRoomPlayer.myCharacter.IsMovable=false;
        gameObject.SetActive(true);
    }
    public void Close()
    {
        AmongUsRoomPlayer.MyRoomPlayer.myCharacter.IsMovable=true;
        gameObject.SetActive(false);
    }
}
