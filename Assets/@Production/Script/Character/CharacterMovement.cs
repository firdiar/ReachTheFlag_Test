using Gtion.Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Windows;

public class CharacterMovement : MonoBehaviour, INetworkUpdateSystem
{
    [SerializeField]
    private ClientInput input;
    [SerializeField]
    private Rigidbody2D rb;
    [SerializeField]
    private Transform thisTransform;
    [SerializeField]
    private float moveSpeed = 5f;


    void Start()
    {
        NetworkUpdateLoop.RegisterNetworkUpdate(this, NetworkUpdateStage.Update);
        NetworkUpdateLoop.RegisterNetworkUpdate(this, NetworkUpdateStage.FixedUpdate);
    }

    private void OnDestroy()
    {
        NetworkUpdateLoop.UnregisterNetworkUpdate(this, NetworkUpdateStage.Update);
        NetworkUpdateLoop.UnregisterNetworkUpdate(this, NetworkUpdateStage.FixedUpdate);
    }

    public void NetworkUpdate(NetworkUpdateStage updateStage)
    {
        if (updateStage == NetworkUpdateStage.FixedUpdate)
        {
            //update for logic, sync
            if (!GameplayManager.Instance.IsGameRunning)
            {
                rb.velocity = Vector3.zero;
                return;
            }
            rb.velocity = input.CharaInput.GetMovement() * moveSpeed;
        }
        else //regular update
        {
            thisTransform.up = input.CharaInput.GetLookUp();
        }
    }
}
