using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class EjectionUI : MonoBehaviour
{
    [SerializeField]
    private Text ejectionResultText;
    [SerializeField]
    private Image ejectionPlayer;
    [SerializeField]
    private RectTransform letf;
    [SerializeField]
    private RectTransform right;
    // Start is called before the first frame update
    void Start()
    {
        ejectionPlayer.material = Instantiate(ejectionPlayer.material);
    }

    public void Open(bool isEjection, EPlayerColor ejectionPlayerColor, bool isImposter, int remainImposterCoint)
    {
        string text = "";
        InGameCharacterMover ejectPlayer = null;
        if (isEjection)
        {
            InGameCharacterMover[] players = FindObjectsOfType<InGameCharacterMover>();
            foreach (var player in players)
            {
                if (player.playerColor == ejectionPlayerColor)
                {
                    ejectPlayer = player;
                    break;
                }
            }
            text = string.Format("{0}은 임포스터{1}\n임포스터가{2}명 남았습니다.",
                ejectPlayer.nickname, isImposter ? "입니다" : "가 아니었습니다.", remainImposterCoint);
        }
        else
        {
            text = string.Format("아무도 퇴출되지 않았습니다.\n임포스터가{0}명 남았습니다.", remainImposterCoint);
        }

        gameObject.SetActive(true);

        StartCoroutine(ShowEjectionResult_Corutine(ejectPlayer, text));
    }

    private IEnumerator ShowEjectionResult_Corutine(InGameCharacterMover ejectPlayerMover, string text)
    {
        // 글자가 한번에 나오는게 아니라 한글자씩 나오게 하기 위해
        ejectionResultText.text = "";

        string forwardText = "";
        string backText = "";

        if(ejectPlayerMover != null)
        {
            // 퇴출된 플레이어의 색상을 변경 후 이동
            ejectionPlayer.material.SetColor("_PlayerColor", PlayerColor.GetColor(ejectPlayerMover.playerColor));

            float timer = 0f;
            while(timer <= 1f)
            {
                yield return null;
                timer += Time.deltaTime *0.5f;
                
                ejectionPlayer.rectTransform.anchoredPosition = Vector2.Lerp(letf.anchoredPosition, right.anchoredPosition, timer);
                ejectionPlayer.rectTransform.rotation = Quaternion.Euler(ejectionPlayer.rectTransform.rotation.eulerAngles + new Vector3(0f, 0f, -360f * Time.deltaTime));
                
            }
        }
        // 글자가 한번에 나오는게 아니라 한글자씩 나오게 하기 위해
        backText = text;
        while(backText.Length != 0)
        {
            forwardText += backText[0];
            backText = backText.Remove(0,1);
            ejectionResultText.text = string.Format("<color=#FFFFFF>{0}</color><color=#000000>{1}</color>", forwardText, backText);
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
