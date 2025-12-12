using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReportUI : MonoBehaviour
{
    [SerializeField]
    private Image deadbodyImage;
    [SerializeField]
    private Material material;

    public void Open(EPlayerColor deadbodyColor)
    {
        AmongUsRoomPlayer.MyRoomPlayer.myCharacter.IsMovable = false;

        Material inst = Instantiate(material);
        deadbodyImage.material = inst;

        gameObject.SetActive(true);

        deadbodyImage.material.SetColor("_PlayerColor", PlayerColor.GetColor(deadbodyColor));
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
