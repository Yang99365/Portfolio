using UnityEngine;
using System.Collections;
public class MatchException : BaseException 
{
	public readonly GeneralUnit attacker;
	public readonly GeneralUnit target;
	public MatchException (GeneralUnit attacker, GeneralUnit target) : base (false)
	{
		this.attacker = attacker;
		this.target = target;
	}
}