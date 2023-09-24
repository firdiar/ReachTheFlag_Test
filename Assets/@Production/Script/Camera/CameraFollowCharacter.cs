using Gtion.Plugin.DI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowCharacter : MonoBehaviour
{
    public Transform target; // The character or object the camera is following
    public Rigidbody2D targetRb; // The character or object the camera is following
    public float smoothSpeed = 0.125f; // The smoothness of the camera movement
    public Vector3 offset; // The offset from the character to the camera
    public float lookaheadFactor = 3.0f; // How much the camera should look ahead based on player's movement direction

    private Vector3 velocity = Vector3.zero; // Used for smooth damp function

    [GInject]
    MyLocalCharacter localCharacter;

    private void Start()
    {
        GDi.Request(this, OnDependencyReady);
    }

    private void OnDependencyReady() 
    {
        target = localCharacter.transform;
        targetRb = localCharacter.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (target == null) return;//ignore

        //because this camera is focused on character, and character move on FixedUpdate, this have to be in fixed Update
        UpdatePosition(Time.fixedDeltaTime);
    }


    void UpdatePosition(float deltaTime) 
    {
        Vector3 lookAheadPos = targetRb.velocity * lookaheadFactor;
        Vector3 desiredPosition = target.position + offset + lookAheadPos;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed, 100, deltaTime);
        transform.position = smoothedPosition;
    }
}
