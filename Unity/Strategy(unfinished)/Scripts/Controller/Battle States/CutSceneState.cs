using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CutSceneState : BattleState 
{
	ConversationController conversationController;
	ConversationData data;
	protected override void Awake ()
	{
		base.Awake ();
		conversationController = owner.GetComponentInChildren<ConversationController>();
	}
	public override void Enter ()
	{
		base.Enter ();
		// 플레이중인 레벨에 따라 대화 데이터를 로드하도록 개선해야함.

		if (IsBattleOver())
		{
			if (DidPlayerWin())
				data = Resources.Load<ConversationData>("Conversations/OutroSceneWin");
			else
				data = Resources.Load<ConversationData>("Conversations/OutroSceneLose");
		}
		else
		{
			data = Resources.Load<ConversationData>("Conversations/IntroScene");
		}

		conversationController.Show(data);
	}

	public override void Exit ()
	{
		base.Exit ();
		if (data)
			Resources.UnloadAsset(data);
	}

	protected override void AddListeners ()
	{
		base.AddListeners ();
		ConversationController.completeEvent += OnCompleteConversation;
	}
	protected override void RemoveListeners ()
	{
		base.RemoveListeners ();
		ConversationController.completeEvent -= OnCompleteConversation;
	}
	protected override void OnFire (object sender, InfoEventArgs<int> e)
	{
		base.OnFire (sender, e);
		conversationController.Next();
	}
	void OnCompleteConversation (object sender, System.EventArgs e)
	{
		if (IsBattleOver())
			owner.ChangeState<EndBattleState>();
		else
			owner.ChangeState<SelectUnitState>();
	}
}