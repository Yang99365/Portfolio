using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeetingPlayerPanel : MonoBehaviour
{
    [SerializeField]
    private Image characterImg;
    [SerializeField]
    private Text nickNameText;
    [SerializeField]
    private GameObject deadPlayerBlock;
    [SerializeField]
    private GameObject reportSign;
    [SerializeField]
    private GameObject voteButtons;

    [HideInInspector]
    public InGameCharacterMover targetPlayer;

    [SerializeField]
    private GameObject voteSign;
    [SerializeField]
    private GameObject voterPrefab;
    [SerializeField]
    private Transform voterParentTransform;

    public void UpdatePanel(EPlayerColor voterColor)
    {
        var voter = Instantiate(voterPrefab,voterParentTransform).GetComponent<Image>();
        voter.material = Instantiate(voter.material);
        voter.material.SetColor("_PlayerColor", PlayerColor.GetColor(voterColor));
    }

    public void UpdateVoteSign(bool isVoted)
    {
        voteSign.SetActive(isVoted);
    }
    
    public void SetPlayer(InGameCharacterMover target)
    {
        Material inst = Instantiate(characterImg.material);
        characterImg.material = inst;

        targetPlayer = target;
        characterImg.material.SetColor("_PlayerColor", PlayerColor.GetColor(targetPlayer.playerColor));
        nickNameText.text = targetPlayer.nickname;

        //임포스터일때 다른 임포스터들 이름 빨간색으로 표시
        var myCharacter = AmongUsRoomPlayer.MyRoomPlayer.myCharacter as InGameCharacterMover;
        if(((myCharacter.playerType & EPlayerType.Imposter) == EPlayerType.Imposter)
        && ((targetPlayer.playerType & EPlayerType.Imposter) == EPlayerType.Imposter))
        {
            nickNameText.color = Color.red;
        }

        // 패널의 대상인 플레이어가 죽은 상태라면 블록을 활성화
        bool isDead = (targetPlayer.playerType & EPlayerType.Ghost) == EPlayerType.Ghost;
        deadPlayerBlock.SetActive(isDead);
        GetComponent<Button>().interactable = !isDead; // 죽은 플레이어는 클릭 불가능
        reportSign.SetActive(targetPlayer.isReporter); // 리포트한 플레이어라면 리포트 표시 활성화
    }

    public void OnClickPlayerPanel()
    {
        var myCharacter = AmongUsRoomPlayer.MyRoomPlayer.myCharacter as InGameCharacterMover;
        if(myCharacter.isVote) return;

        if((myCharacter.playerType&EPlayerType.Ghost) != EPlayerType.Ghost) // 내 캐릭터가 죽은 상태가 아니라면
        {
            InGameUIManager.instance.MeetingUI.SelectPlayerPanel(); // 모든 패널 선택해제
            voteButtons.SetActive(true); // 클릭한 패널의 투표 버튼 활성화
        }
    }
    public void Select()
    {
        var myCharacter = AmongUsRoomPlayer.MyRoomPlayer.myCharacter as InGameCharacterMover;
        myCharacter.CmdVoteEjectPlayer(targetPlayer.playerColor);
        Unselect();
    }
    public void Unselect()
    {
        voteButtons.SetActive(false);
    }

    public void OpenResult()
    {
        voterParentTransform.gameObject.SetActive(true);
    }
}
