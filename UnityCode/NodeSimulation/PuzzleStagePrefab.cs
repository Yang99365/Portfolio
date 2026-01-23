using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PuzzleStagePrefab : MonoBehaviour
{
    [SerializeField]
    private Image puzzleImage;
    [SerializeField]
    private TextMeshProUGUI puzzleStageNameText;
    [SerializeField]
    private TextMeshProUGUI clearTiemText;
    [SerializeField]
    private GameObject clearInfo;
    [SerializeField]
    private bool hasReward = false;
    [SerializeField]
    private TextMeshProUGUI rewardNodeTextObj;
    [SerializeField]
    private GameObject rewardObj;
    [SerializeField]
    private bool hasNeedNode = false;
    [SerializeField] 
    private string needNode;
    [SerializeField]
    private TextMeshProUGUI needNodeTextObj;
    [SerializeField]
    private GameObject lockPanel;
    [SerializeField]
    private Button puzzleButton;

    private StageData stageData;

    [SerializeField]
    private PuzzleInteraction puzzleInteraction;

    private void Start()
    {
        SetInfo();

        puzzleInteraction.OnPuzzleValidation += PuzzleSolved;
        PlayerNodeInventory.OnNodeUnlocked += OnNodeUnlocked;
    }
    public void SetInfo()
    {
        puzzleStageNameText.text = puzzleInteraction.puzzleName;

        bool isClear;
        stageData = GameSaveManager.Instance.FindPuzzleDataState(puzzleInteraction.puzzleName);

        if (stageData == null)
        {
            isClear = false;
        }
        else
        {
            isClear = stageData.Clear;
            clearTiemText.text = stageData.ClearTime.ToString("F2") + " sec";
        }

        if (isClear)
        {
            clearInfo.SetActive(true);
            clearTiemText.gameObject.SetActive(true);
        }
        else
        {
            clearInfo.SetActive(false);
            clearTiemText.gameObject.SetActive(false);
        }
        if (hasReward)
        {
            rewardNodeTextObj.text = puzzleInteraction.puzzleName;
            rewardObj.SetActive(true);
        }
        else
        {
            rewardObj.SetActive(false);
        }
        if (hasNeedNode)
        {
            needNodeTextObj.text = needNode;

            Type needNodeType = Type.GetType(needNode);

            if (needNodeType != null && PlayerNodeInventory.IsNodeAvailable(needNodeType))
            {
                lockPanel.SetActive(false);
            }
            else
            {
                lockPanel.SetActive(true);
            }
        }
        else
        {
            lockPanel.SetActive(false);
        }

        puzzleButton.onClick.AddListener(() =>
        {
                puzzleInteraction.Interact();
        });
    }

    private void PuzzleSolved(bool isSolved)
    {
        if (isSolved)
        {
            SetInfo();
            Debug.Log("Puzzle Solved: " + puzzleInteraction.puzzleName);
        }
    }
    private void OnNodeUnlocked(Type unlockedNodeType)
    {
        if (hasNeedNode)
        {
            Type needNodeType = Type.GetType(needNode);

            if (needNodeType == unlockedNodeType)
            {
                SetInfo();
            }
        }
    }
    
    private void OnDestroy()
    {
        puzzleInteraction.OnPuzzleValidation -= PuzzleSolved;
        PlayerNodeInventory.OnNodeUnlocked -= OnNodeUnlocked;
    }
}
