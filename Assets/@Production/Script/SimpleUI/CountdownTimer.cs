using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;
    private DateTime endTime;
    private bool isCountingDown = false;

    // Update is called once per frame
    void Update()
    {
        if (isCountingDown && GameplayManager.Instance.IsGameRunning)
        {
            UpdateTimer();
        }
    }

    public void StartTimer(DateTime endTime)
    {
        this.endTime = endTime;

        isCountingDown = true;
    }

    private void UpdateTimer()
    {
        TimeSpan remainingTime = endTime - DateTime.UtcNow;

        // If the timer has ended
        if (remainingTime.TotalSeconds <= 0)
        {
            isCountingDown = false;
            timerText.text = "00:00";
            TimerFinished();
            return;
        }

        // Update the text on the screen
        timerText.text = string.Format("{0:D2}:{1:D2}", remainingTime.Minutes, remainingTime.Seconds);
    }

    private void TimerFinished()
    {
        Debug.Log("Time Up");
    }
}
