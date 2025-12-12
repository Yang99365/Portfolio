using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 이 대리자는 EventHandler와 유사합니다.
/// 첫 번째 매개변수는 발신자입니다.
/// 두 번째 매개변수는 전달할 인수/정보입니다.
/// </summary>
using Handler = System.Action<System.Object, System.Object>;
/// <summary>
/// SenderTable은 객체(알림 보낸 사람)에서 매핑됩니다.
/// 핸들러 메소드 목록
/// * 참고 - SenderTable에 보낸 사람이 지정되지 않은 경우,
///NotificationCenter 자체가 발신자 키로 사용됩니다.
/// </summary>
using SenderTable = System.Collections.Generic.Dictionary<System.Object, System.Collections.Generic.List<System.Action<System.Object, System.Object>>>;
public class NotificationCenter
{
	#region Properties
	/// <summary>
	/// 사전 "key"(문자열)는 관찰할notificationName 속성을 나타냅니다.
	/// 사전 "값"(SenderTable)은 보낸 사람과 관찰자 하위 테이블 간에 매핑됩니다.
	/// </summary>
	private Dictionary<string, SenderTable> _table = new Dictionary<string, SenderTable>();
	private HashSet<List<Handler>> _invoking = new HashSet<List<Handler>>();
	#endregion
	
	#region Singleton Pattern
	public readonly static NotificationCenter instance = new NotificationCenter();
	private NotificationCenter() {}
	#endregion
	
	#region Public
	public void AddObserver (Handler handler, string notificationName)
	{
		AddObserver(handler, notificationName, null);
	}
	
	public void AddObserver (Handler handler, string notificationName, System.Object sender)
	{
		if (handler == null)
		{
			Debug.LogError("Can't add a null event handler for notification, " + notificationName);
			return;
		}
		
		if (string.IsNullOrEmpty(notificationName))
		{
			Debug.LogError("Can't observe an unnamed notification");
			return;
		}
		
		if (!_table.ContainsKey(notificationName))
			_table.Add(notificationName, new SenderTable());
		
		SenderTable subTable = _table[notificationName];
		
		System.Object key = (sender != null) ? sender : this;
		
		if (!subTable.ContainsKey(key))
			subTable.Add(key, new List<Handler>());
		
		List<Handler> list = subTable[key];
		if (!list.Contains(handler))
		{
			if (_invoking.Contains(list))
				subTable[key] = list = new List<Handler>(list);
			
			list.Add( handler );
		}
	}
	
	public void RemoveObserver (Handler handler, string notificationName)
	{
		RemoveObserver(handler, notificationName, null);
	}
	
	public void RemoveObserver (Handler handler, string notificationName, System.Object sender)
	{
		if (handler == null)
		{
			Debug.LogError("Can't remove a null event handler for notification, " + notificationName);
			return;
		}
		
		if (string.IsNullOrEmpty(notificationName))
		{
			Debug.LogError("A notification name is required to stop observation");
			return;
		}
		
		// No need to take action if we dont monitor this notification
		if (!_table.ContainsKey(notificationName))
			return;
		
		SenderTable subTable = _table[notificationName];
		System.Object key = (sender != null) ? sender : this;
		
		if (!subTable.ContainsKey(key))
			return;
		
		List<Handler> list = subTable[key];
		int index = list.IndexOf(handler);
		if (index != -1)
		{
			if (_invoking.Contains(list))
				subTable[key] = list = new List<Handler>(list);
			list.RemoveAt(index);
		}
	}
	
	public void Clean ()
	{
		string[] notKeys = new string[_table.Keys.Count];
		_table.Keys.CopyTo(notKeys, 0);
		
		for (int i = notKeys.Length - 1; i >= 0; --i)
		{
			string notificationName = notKeys[i];
			SenderTable senderTable = _table[notificationName];
			
			object[] senKeys = new object[ senderTable.Keys.Count ];
			senderTable.Keys.CopyTo(senKeys, 0);
			
			for (int j = senKeys.Length - 1; j >= 0; --j)
			{
				object sender = senKeys[j];
				List<Handler> handlers = senderTable[sender];
				if (handlers.Count == 0)
					senderTable.Remove(sender);
			}
			
			if (senderTable.Count == 0)
				_table.Remove(notificationName);
		}
	}
	
	public void PostNotification (string notificationName)
	{
		PostNotification(notificationName, null);
	}
	
	public void PostNotification (string notificationName, System.Object sender)
	{
		PostNotification(notificationName, sender, null);
	}
	
	public void PostNotification (string notificationName, System.Object sender, System.Object e)
	{
		if (string.IsNullOrEmpty(notificationName))
		{
			Debug.LogError("A notification name is required");
			return;
		}
		
		// 이 알림을 모니터링하지 않으면 조치를 취할 필요가 없습니다.
		if (!_table.ContainsKey(notificationName))
			return;
		
		// 관찰할 발신자를 지정한 구독자에게 게시
		SenderTable subTable = _table[notificationName];
		if (sender != null && subTable.ContainsKey(sender))
		{
			List<Handler> handlers = subTable[sender];
			_invoking.Add(handlers);
			for (int i = 0; i < handlers.Count; ++i)
				handlers[i]( sender, e );
			_invoking.Remove(handlers);
		}
		
		// 관찰할 발신자를 지정하지 않은 구독자에게 게시
		if (subTable.ContainsKey(this))
		{
			List<Handler> handlers = subTable[this];
			_invoking.Add(handlers);
			for (int i = 0; i < handlers.Count; ++i)
				handlers[i]( sender, e );
			_invoking.Remove(handlers);
		}
	}
	#endregion
}