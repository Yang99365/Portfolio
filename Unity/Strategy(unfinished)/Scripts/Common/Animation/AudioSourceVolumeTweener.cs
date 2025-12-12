using UnityEngine;
using System.Collections;
public class AudioSourceVolumeTweener : Tweener 
{
	public AudioSource source 
	{
		get 
		{
			if (_source == null)
				_source = GetComponent<AudioSource>();
			return _source;
		}
		set
		{
			_source = value;
		}
	}
	protected AudioSource _source;
	protected override void OnUpdate () 
	{
		base.OnUpdate ();
		source.volume = currentValue;
	}
} // 이 클래스를 상속받아서 사용하면 AudioSource의 볼륨을 트윈으로 조절할 수 있다.
