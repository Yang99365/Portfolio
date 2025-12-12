using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

public class GameSystem : NetworkBehaviour
{
    public static GameSystem instance;

    private List<InGameCharacterMover> players = new List<InGameCharacterMover>();

    [SerializeField]
    private Transform spawnTransform;

    [SerializeField]
    private float spawnDistance;

    [SyncVar]
    public float killCooldown;

    [SyncVar]
    public int killRange;

    [SyncVar]
    public int skipVotePlayerCount;

    [SyncVar]
    public float remainTime;

    [SerializeField]
    private Light2D shadowLight;
    [SerializeField]
    private Light2D lightMapLight;
    [SerializeField]
    private Light2D globalLight;

    public void AddPlayer(InGameCharacterMover player)
    {
        if(!players.Contains(player))
        {
            players.Add(player);
        }
    }

    private IEnumerator GameReady()
    {
        var manager = NetworkManager.singleton as AmongUsRoomManager;
        killCooldown = manager.gameRuleData.killCooldown;
        killRange = (int)manager.gameRuleData.killRange;
        while (manager.roomSlots.Count != players.Count)
        {
            yield return null;
        }
        for (int i = 0; i < manager.imposterCount; i++)
        {
            var player = players[Random.Range(0, players.Count)];
            if (player.playerType != EPlayerType.Imposter)
            {
                player.playerType = EPlayerType.Imposter;
            }
            else
            {
                i--;
            }
        }

        AlllocatePlayerToAroundTable(players.ToArray());

        yield return new WaitForSeconds(1f);
        RpcStartGame();

        foreach (var player in players)
        {
            player.SetKillCooldown();
        }
    }

    private void AlllocatePlayerToAroundTable(InGameCharacterMover[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            float radian = (2f * Mathf.PI) / players.Length;
            radian *= i;
            players[i].RpcTeleport(spawnTransform.position + (new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0) * spawnDistance));
        }
    }

    [ClientRpc]
    private void RpcStartGame()
    {
        StartCoroutine(StartGameCoroutine());
    }
    private IEnumerator StartGameCoroutine()
    {
        yield return StartCoroutine(InGameUIManager.instance.InGameIntroUI.ShowIntroSequence());

        InGameCharacterMover myCharacter = null;
        foreach(var player in players) // 이 클래스에 등록된 캐릭터들 중 자신의 캐릭터를 찾은 뒤, 각 플레이어에게 SetNicknameColor 함수 호출
        {
            if(player.isOwned)
            {
                myCharacter = player;
                break;
            }
        }

        foreach(var player in players)
        {
            player.SetNicknameColor(myCharacter.playerType);
        }
        yield return new WaitForSeconds(3f);
        InGameUIManager.instance.InGameIntroUI.Close();
    }
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(isServer)
        {
            StartCoroutine(GameReady());
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public List<InGameCharacterMover> GetPlayerList()
    {
        return players;
    }

    public void ChangeLightMode(EPlayerType type)
    {
        if(type == EPlayerType.Ghost)
        {
            lightMapLight.lightType = Light2D.LightType.Global;
            shadowLight.intensity = 0f;
            globalLight.intensity = 1f;
        }
        else
        {
            lightMapLight.lightType = Light2D.LightType.Point;
            shadowLight.intensity = 0.5f;
            globalLight.intensity = 0.5f;
        }
    }

    public void StartReportMeeting(EPlayerColor deadbodyColor)
    {
        RpcSendReportSign(deadbodyColor);
        StartCoroutine(MeetingProcess_Coroutine());
    }

    private IEnumerator StartMeeting_Coroutine()
    {
        yield return new WaitForSeconds(3f);
        InGameUIManager.instance.ReportUI.Close();
        InGameUIManager.instance.MeetingUI.Open();
        InGameUIManager.instance.MeetingUI.ChangeMeetingState(EMeetingState.Meeting);
    }

    private IEnumerator MeetingProcess_Coroutine()
    {
        // 회의가 시작된 직후엔 회의시간동안엔 투표못하게 isVote를 true로 설정
        var players = FindObjectsOfType<InGameCharacterMover>();
        foreach(var player in players)
        {
            player.isVote = true;
        }

        yield return new WaitForSeconds(3f);

        var manager = NetworkManager.singleton as AmongUsRoomManager;
        remainTime = manager.gameRuleData.meetingsTime;
        while(true)
        {
            remainTime -= Time.deltaTime;
            yield return null;
            if(remainTime <= 0)
            {
                break;
            }
        }

        // 투표시간이 되면 투표를 시작하기 위해 isVote를 false로 설정
        skipVotePlayerCount = 0;
        foreach(var player in players)
        {
            if((player.playerType & EPlayerType.Ghost) != EPlayerType.Ghost)
            {
                player.isVote = false;
            }
            player.vote = 0;
        }
        
        RpcStartVoteTime();
        remainTime = manager.gameRuleData.voteTime;
        while(true)
        {
            remainTime -= Time.deltaTime;
            yield return null;
            if(remainTime <= 0)
            {
                break;
            }
        }

        foreach(var player in players)
        {
            // 투표를 하지 않은 플레이어는 스킵투표로 처리
            if(!player.isVote && (player.playerType & EPlayerType.Ghost) != EPlayerType.Ghost)
            {
                player.isVote = true;
                skipVotePlayerCount += 1;
                RpcSignSkipVote(player.playerColor);
            }
            
        }
        RpcEndVoteTime();

        yield return new WaitForSeconds(3f);

        StartCoroutine(CalculateVoteResult_Corutine(players));

    }

    private class CharacterVoteComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            InGameCharacterMover xPlayer = (InGameCharacterMover)x;
            InGameCharacterMover yPlayer = (InGameCharacterMover)y;
            return xPlayer.vote <= yPlayer.vote ? 1 : -1;
        }
    }
    private IEnumerator CalculateVoteResult_Corutine(InGameCharacterMover[] players)
    {
        System.Array.Sort(players, new CharacterVoteComparer());

        int remainImposter = 0;
        foreach(var player in players)
        {
            if((player.playerType & EPlayerType.Imposter_Alive) == EPlayerType.Imposter_Alive)
            {
                remainImposter++;
            }
        }
        if(skipVotePlayerCount >= players[0].vote) // 스킵한 유저 수가 가장많이 득표한 유저와 같거나 많다면 투표결과 없음
        {
            RpcOpenEjectionUI(false, EPlayerColor.Black, false, remainImposter);
        }
        else if(players[0].vote == players[1].vote)
        {
            RpcOpenEjectionUI(false, EPlayerColor.Black, false, remainImposter);
        }
        else
        {
            bool isImposter = (players[0].playerType & EPlayerType.Imposter) == EPlayerType.Imposter;
            RpcOpenEjectionUI(true, players[0].playerColor, isImposter, isImposter ? remainImposter-1 : remainImposter);

            players[0].Dead(true);
        }

        var deadbodies = FindObjectsOfType<Deadbody>();
        for(int i=0; i < deadbodies.Length; i++)
        {
            Destroy(deadbodies[i].gameObject);
        }

        AlllocatePlayerToAroundTable(players);

        yield return new WaitForSeconds(10f);

        RpcCloseEjectionUI();
    }

    [ClientRpc]
    public void RpcOpenEjectionUI(bool isEjection, EPlayerColor ejectionPlayerColor, bool isImposter, int remainImposterCount)
    {
        InGameUIManager.instance.EjectionUI.Open(isEjection, ejectionPlayerColor, isImposter, remainImposterCount);
        InGameUIManager.instance.MeetingUI.Close();
    }

    [ClientRpc]
    public void RpcCloseEjectionUI()
    {
        InGameUIManager.instance.EjectionUI.Close();
        AmongUsRoomPlayer.MyRoomPlayer.myCharacter.IsMovable = true;
    }

    [ClientRpc]
    public void RpcStartVoteTime()
    {
        InGameUIManager.instance.MeetingUI.ChangeMeetingState(EMeetingState.Vote);
    }
    [ClientRpc]
    public void RpcEndVoteTime()
    {
        InGameUIManager.instance.MeetingUI.CompleteVote();
    }
    [ClientRpc]
    private void RpcSendReportSign(EPlayerColor deadbodyColor)
    {
        InGameUIManager.instance.ReportUI.Open(deadbodyColor);

        StartCoroutine(StartMeeting_Coroutine());
    }

    [ClientRpc]
    public void RpcSignVoteEject(EPlayerColor votercolor, EPlayerColor ejectColor)
    {
        InGameUIManager.instance.MeetingUI.UpdateVote(votercolor, ejectColor);
    }
    
    [ClientRpc]
    public void RpcSignSkipVote(EPlayerColor skipVotePlayerColor)
    {
        InGameUIManager.instance.MeetingUI.UpdateSkipVotePlayer(skipVotePlayerColor);
    }
}
