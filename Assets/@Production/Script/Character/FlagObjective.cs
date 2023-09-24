using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class FlagObjective : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (NetworkManager.Singleton.IsServer && collision.tag == "Player")
        {
            NetworkObject netObj = collision.GetComponent<NetworkObject>();
            GameplayManager.Instance.FinishGame(netObj.OwnerClientId);
            Debug.Log("GameFinished! Winer : "+ netObj.OwnerClientId);
        }
    }
}
