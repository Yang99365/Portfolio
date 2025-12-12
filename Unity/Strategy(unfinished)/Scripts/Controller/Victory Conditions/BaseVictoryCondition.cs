using UnityEngine;
using System.Collections;
public abstract class BaseVictoryCondition : MonoBehaviour
{
	#region Fields & Properties
	public Alliances Victor
	{
		get { return victor; } 
		protected set { victor = value; }
	}
	Alliances victor = Alliances.None;
	
	protected BattleController bc;
	#endregion
	
	#region MonoBehaviour
	protected virtual void Awake ()
	{
		bc = GetComponent<BattleController>();
	}
	
	protected virtual void OnEnable ()
	{
		this.AddObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
	}
	
	protected virtual void OnDisable ()
	{
		this.RemoveObserver(OnHPDidChangeNotification, Stats.DidChangeNotification(StatTypes.HP));
	}
	#endregion
	
	#region Notification Handlers
	protected virtual void OnHPDidChangeNotification (object sender, object args)
	{
		CheckForGameOver();
	}
	#endregion
	
	#region Protected
	protected virtual void CheckForGameOver ()
	{
		if (PartyDefeated(Alliances.Hero))
			Victor = Alliances.Enemy;
	}
	
	protected virtual bool PartyDefeated (Alliances type) // 소속type을 체크해서 그 소속의 유닛을 확인해 패배체크
	{
		for (int i = 0; i < bc.units.Count; ++i)
		{
			Alliance a = bc.units[i].GetComponent<Alliance>();
			if (a == null)
				continue;
			
			if (a.type == type && !IsDefeated(bc.units[i]))
				return false;
		}
		return true;
	}
	
	protected virtual bool IsDefeated (GeneralUnit unit)
	{
		Health health = unit.GetComponent<Health>();
		if (health)
			return health.MinHP == health.HP;
		
		Stats stats = unit.GetComponent<Stats>();
		return stats[StatTypes.HP] == 0;
	}
	#endregion
}