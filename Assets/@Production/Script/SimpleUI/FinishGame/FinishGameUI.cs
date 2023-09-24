using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public enum Decision : byte
{
    None,
    NextGame,
    LeaveGame
}

public enum EndGameState : byte
{
    Draw,
    Win,
    Lose
}

public class FinishGameUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI endGameText;
    [SerializeField]
    List<ItemDecision> itemDecisitonList = new List<ItemDecision>();

    public void UpdateView(bool updateMine = false) 
    {
        ulong myClientId = NetworkManager.Singleton.LocalClientId;
        int playerC = GameplayManager.Instance.RequiredPlayerCount;
        for (int i = 0; i < itemDecisitonList.Count; i++)
        {
            if (i >= playerC)
            {
                itemDecisitonList[i].gameObject.SetActive(false);
            }
            else
            {
                itemDecisitonList[i].gameObject.SetActive(true);
                var targetClientId = (ulong)i;
                if (myClientId == targetClientId)
                {
                    if (updateMine)
                    {
                        itemDecisitonList[i].Initialize(targetClientId, SelectDecistion);
                    }
                }
                else
                {
                    itemDecisitonList[i].InitializeOther(targetClientId, GameplayManager.Instance.EndGameDecision[i]);
                }
            }
        }
    }

    public void Initialized(ulong winner , EndGameState endGameState)
    {
        if (NetworkManager.Singleton.LocalClientId == winner)
        {
            endGameText.text = "You WIN!";
        }
        else if (endGameState == EndGameState.Draw)
        {
            endGameText.text = "Game is Draw, GG";
        }
        else
        {
            endGameText.text = "You Lose :'";
        }

        UpdateView(true);
    }

    private void SelectDecistion(bool isNextGame)
    {
        Decision decision = isNextGame ? Decision.NextGame : Decision.LeaveGame;
        GameplayManager.Instance.SetEndgameDecistionServerRPC(decision);
    }
}
