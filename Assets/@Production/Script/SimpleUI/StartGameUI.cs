using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Gtion.UI
{
    public class StartGameUI : MonoBehaviour
    {
        [Header("Single Player")]
        [SerializeField]
        Button singglePlayer;

        [Header("Multi Player")]
        [SerializeField]
        Button mulPlayerHost;
        [SerializeField]
        Button mulPlayerClient;

        [Header("Manager")]
        [SerializeField]
        GameplayManager gameplayManager;

        private void Start()
        {
            singglePlayer.onClick.AddListener(Deactive);
            mulPlayerHost.onClick.AddListener(Deactive);
            mulPlayerClient.onClick.AddListener(Deactive);

            singglePlayer.onClick.AddListener(() =>
            {
                GameplayManager.Instance.InitGame(1);
            });

            mulPlayerHost.onClick.AddListener(() =>
            {
                GameplayManager.Instance.InitGame(2);
            });

            mulPlayerClient.onClick.AddListener(() =>
            {
                GameplayManager.Instance.JoinGame();
            });
        }

        private void Deactive()
        {
            gameObject.SetActive(false);
        }
    }
}