using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Stats : MonoBehaviour
{
	#region Notifications
	public static string WillChangeNotification (StatTypes type)
	{
		if (!_willChangeNotifications.ContainsKey(type))
			_willChangeNotifications.Add(type, string.Format("Stats.{0}WillChange", type.ToString()));
		return _willChangeNotifications[type];
	}
	
	public static string DidChangeNotification (StatTypes type)
	{
		if (!_didChangeNotifications.ContainsKey(type))
			_didChangeNotifications.Add(type, string.Format("Stats.{0}DidChange", type.ToString()));
		return _didChangeNotifications[type];
	}
	
	static Dictionary<StatTypes, string> _willChangeNotifications = new Dictionary<StatTypes, string>();
	static Dictionary<StatTypes, string> _didChangeNotifications = new Dictionary<StatTypes, string>();
	#endregion
	
	#region Fields / Properties
	public int this[StatTypes s]
	{
		get { return _data[(int)s]; }
		set { SetValue(s, value, true); }
	}
	int[] _data = new int[ (int)StatTypes.Count ];
	#endregion
	
	#region Public
	public void SetValue (StatTypes type, int value, bool allowExceptions)
	{
		int oldValue = this[type];
		if (oldValue == value)
			return;
		
		if (allowExceptions)
		{
			// 여기서 규칙에 대한 예외를 허용합니다.
			ValueChangeException exc = new ValueChangeException( oldValue, value );
			
			// 알림은 통계 유형별로 고유합니다.
			this.PostNotification(WillChangeNotification(type), exc);
			
			// 값을 수정한 것이 있나요?
			value = Mathf.FloorToInt(exc.GetModifiedValue());
			
			// 변경 사항이 무효화되었나요?
			if (exc.toggle == false || value == oldValue)
				return;
		}
		
		_data[(int)type] = value;
		this.PostNotification(DidChangeNotification(type), oldValue);
	}
	#endregion
}