using Gtion.Plugin.DI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Gtion.Networking
{
    public struct PlayerInput : INetworkSerializeByMemcpy
    { 
        public short Movement { get; set; }
        public short Rotation { get; set; }

        public short Encode(Vector2 direction)
        {
            if (direction == Vector2.zero) return 1001;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            return (short)(angle * (short.MaxValue / 360f));
        }

        public Vector2 Decode(short encoded)
        {
            if (encoded == 1001) return Vector2.zero;

            float angle = encoded * 360f / short.MaxValue;
            float radian = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public void SetMovement(Vector2 direction)
        {
            Movement = Encode(direction);
        }

        public Vector2 GetMovement()
        {
            return Decode(Movement);
        }

        public void SetLookUp(Vector2 lootAtDirection)
        {
            Rotation = Encode(lootAtDirection);
        }

        public Vector2 GetLookUp()
        {
            return Decode(Rotation);
        }
    }

    public class ClientInput : NetworkBehaviour
    {
        //Owner Authority
        NetworkVariable<PlayerInput> input = new NetworkVariable<PlayerInput>(writePerm: NetworkVariableWritePermission.Owner);

        //Every period of time, we resync the position
        NetworkVariable<Vector2> position = new NetworkVariable<Vector2>(writePerm: NetworkVariableWritePermission.Server);

        [SerializeField]
        MyLocalCharacter myLocalCharacter;

        public PlayerInput CharaInput => input.Value;
        Camera mainCamera;

        float syncPositionInterval = 1000;

        private void Start()
        {
            mainCamera = Camera.main;
            position.OnValueChanged += SyncPosition;
        }

        private void SyncPosition(Vector2 previousValue, Vector2 newValue)
        {
            transform.position = newValue;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsOwner)
            {
                GDi.Register(myLocalCharacter);
            }

            GameplayManager.Instance.RegisterPlayerObject(OwnerClientId, transform);
        }

        private void Update()
        {
            if (IsServer)
            {
                syncPositionInterval += Time.deltaTime;
                if (syncPositionInterval > 10)
                {
                    syncPositionInterval = 0;
                    position.Value = (Vector2)transform.position; //my code is actually deterministic, so this just addition sync in case un-expected packet loss happening
                }
            }


            if (!IsOwner) return;

            PlayerInput playerInput = new PlayerInput();

            var movement = new Vector2(Input.GetAxisRaw("Horizontal") , Input.GetAxisRaw("Vertical"));
            playerInput.SetMovement(movement);

            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
            Vector2 lookDirection = new Vector2(
                mousePosition.x - transform.position.x,
                mousePosition.y - transform.position.y
            );
            playerInput.SetLookUp(lookDirection);
            input.Value = playerInput;
        }


    }
}