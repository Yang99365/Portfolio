using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class StatPanel : MonoBehaviour 
{
	public Panel panel;
	public Sprite allyBackground;
	public Sprite enemyBackground;
	public Image background;
	public Image avatar;
	public Text nameLabel;
	public Text hpLabel;
	public Text mpLabel;
	public Text lvLabel;
	public void Display (GameObject obj)
	{
		Alliance alliance = obj.GetComponent<Alliance>();
		// 아군은 파란배경, 적은 붉은배경
		background.sprite = alliance.type == Alliances.Enemy ? enemyBackground : allyBackground;
		// 아바타.스프라이트 = null; 이 데이터를 제공하는 구성 요소가 필요합니다.
		nameLabel.text = obj.name;
		Stats stats = obj.GetComponent<Stats>();
		if (stats)
		{
			hpLabel.text = string.Format( "HP {0} / {1}", stats[StatTypes.HP], stats[StatTypes.MHP] );
			mpLabel.text = string.Format( "MP {0} / {1}", stats[StatTypes.MP], stats[StatTypes.MMP] );
			lvLabel.text = string.Format( "LV. {0}", stats[StatTypes.LVL]);
		}
	}
}