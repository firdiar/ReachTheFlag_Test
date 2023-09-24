using BrunoMikoski.AnimationSequencer;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gtion.UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI levelText;
        [SerializeField]
        CountdownTimer countdownTimer;
        public CountdownTimer Timer => countdownTimer;
        [SerializeField]
        AnimationSequencerController startGameAnim;
        public void Initialize()
        {
            levelText.text = $"Level : {GameplayManager.Instance.GameLevel}";
            countdownTimer.StartTimer(GameplayManager.Instance.EndSessionTime.Value.ToDateTime());

            gameObject.SetActive(true);
            startGameAnim.Play();
        }

        public void BeginAnimation() 
        {
            
        }
    }
}
