using System.Collections;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.Cms;
using UnityEngine;
using UnityEngine.UI;

public class KillButtonUI : MonoBehaviour
{

    [SerializeField]
    private Button killButton;
    [SerializeField]
    private Text cooldownText;
    
    private InGameCharacterMover targetPlayer;
    
    public void Show(InGameCharacterMover player)
    {
        gameObject.SetActive(true);
        targetPlayer = player;
    }
    

    // Update is called once per frame
    void Update()
    {
        if(targetPlayer != null)
        {
           if(!targetPlayer.isKillable)
           {
                cooldownText.text = targetPlayer.KillCooldown > 0 ? ((int)targetPlayer.KillCooldown).ToString() : "";
                killButton.interactable = false;
           }
           else
           {
                cooldownText.text = "";
                killButton.interactable = true;
           }
        }
    }
    public void OnClickKillButton()
    {
        targetPlayer.Kill();
    }
}
