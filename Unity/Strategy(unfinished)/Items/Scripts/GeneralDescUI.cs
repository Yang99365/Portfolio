using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GeneralDescUI : MonoBehaviour
{
    //[SerializeField] private GeneralBase general;
    
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI AttackText;
    [SerializeField] private TextMeshProUGUI DefenseText;
    [SerializeField] private TextMeshProUGUI SpeedText;

    [SerializeField] private TextMeshProUGUI SkillText;
    [SerializeField] private TextMeshProUGUI SkillText2;
    [SerializeField] private TextMeshProUGUI PassiveSkillText;
    [SerializeField] private Image EquipmentImage1;
    [SerializeField] private Image EquipmentImage2;

    public void Awake() 
    {
        ResetDescription();
    }

    public void ResetDescription()
    {
        portrait.gameObject.SetActive(false);
        nameText.text = "";
        AttackText.text = "";
        DefenseText.text = "";
        SpeedText.text = "";
        SkillText.text = "";
        SkillText2.text = "";
        PassiveSkillText.text = "";
        EquipmentImage1.gameObject.SetActive(false);
        EquipmentImage2.gameObject.SetActive(false);
    }
    public void SetDescription(Sprite portrait, string name, int attack, int defense, int speed, string skill=null , string skill2=null, string passiveSkill=null, EquipItem equipment1 =null, EquipItem equipment2=null)
    {
        this.portrait.gameObject.SetActive(true);
        this.portrait.sprite = portrait;
        nameText.text = name;
        AttackText.text = "Attack = "+ attack.ToString();
        DefenseText.text = "Defence = "+ defense.ToString();
        SpeedText.text = "Speed = "+ speed.ToString();
        SkillText.text = skill; // 임시
        SkillText2.text = skill2;
        PassiveSkillText.text = passiveSkill;
        this.EquipmentImage1.gameObject.SetActive(true);
        this.EquipmentImage2.gameObject.SetActive(true);
        if (equipment1 != null)
        {
            EquipmentImage1.sprite = equipment1.ItemImage;
            EquipmentImage1.gameObject.SetActive(true);
        }
        else
        {
            EquipmentImage1.gameObject.SetActive(false); // 장비 이미지가 없으면 비활성화
        }

        // 장비 2의 이미지를 설정하고, 이미지가 null이 아니면 활성화합니다.
        if (equipment2 != null)
        {
            EquipmentImage2.sprite = equipment2.ItemImage;
            EquipmentImage2.gameObject.SetActive(true);
        }
        else
        {
            EquipmentImage2.gameObject.SetActive(false); // 장비 이미지가 없으면 비활성화
        }

    }
    
    
}
