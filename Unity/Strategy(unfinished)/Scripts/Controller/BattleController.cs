using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleController : StateMachine 
{
  public CameraRig cameraRig;
  public Board board;
  public LevelData levelData;
  public Transform tileSelectionIndicator;
  public HitSuccessIndicator hitSuccessIndicator;
  public FacingIndicator facingIndicator;
  public Point pos;

  public AbilityMenuPanelController abilityMenuPanelController;
  public BattleMessageController battleMessageController;

  public StatPanelController statPanelController;

  public IEnumerator round;

  public ComputerPlayer cpu;

	public Turn turn = new Turn();
	public List<GeneralUnit> units = new List<GeneralUnit>();
  
  public Tile currentTile { get { return board.GetTile(pos); }}
  
  void Start ()
  {
    ChangeState<InitBattleState>();
  }
}
