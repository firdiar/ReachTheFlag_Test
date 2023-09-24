using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLocalCharacter : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer spriteRenderer;
    [SerializeField]
    Color localColor;

    public void InitLocalCharacter() 
    {
        spriteRenderer.color = localColor;
    }
}
