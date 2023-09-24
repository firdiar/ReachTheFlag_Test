using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDecision : MonoBehaviour
{
    [SerializeField]
    Image selectionColor;
    [SerializeField]
    TextMeshProUGUI playerText;
    [SerializeField]
    Button nextBtn;
    [SerializeField]
    TextMeshProUGUI nextText;
    [SerializeField]
    Button leaveBtn;

    Action<bool> onChooseDecision;

    private void Start()
    {
        nextBtn.onClick.AddListener(() => onChooseDecision?.Invoke(true));
        leaveBtn.onClick.AddListener(() => onChooseDecision?.Invoke(false));

        nextBtn.onClick.AddListener(() => SetActiveBtn(false));
        leaveBtn.onClick.AddListener(() => SetActiveBtn(false));

        nextBtn.onClick.AddListener(() => selectionColor.color = Color.green);
        leaveBtn.onClick.AddListener(() => selectionColor.color = Color.red);
    }
    public void InitializeOther(ulong clientId, Decision decision)
    {
        playerText.text = $"Player {clientId + 1}";

        if (decision == Decision.None)
        {
            selectionColor.color = Color.white;
        }
        else if (decision == Decision.NextGame)
        {
            selectionColor.color = Color.green;
        }
        else
        {
            selectionColor.color = Color.red;
        }

        SetActiveBtn(false);
    }

    public void Initialize(ulong clientId , Action<bool> onChooseDecision)
    {
        playerText.text = $"Player {clientId + 1}";
        selectionColor.color = Color.white;
        this.onChooseDecision = onChooseDecision;

        bool allowNextLevel = EndGameState.Win == GameplayManager.Instance.EndGameState;
        nextText.text = allowNextLevel ? "Next Level" : "Retry";

        SetActiveBtn(true);
    }

    private void SetActiveBtn(bool isActive)
    {
        nextBtn.gameObject.SetActive(isActive);
        leaveBtn.gameObject.SetActive(isActive);
    }
}
