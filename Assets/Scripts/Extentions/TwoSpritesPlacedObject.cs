using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoSpritesPlacedObject : MonoBehaviour
{
    public bool isSecond = false;
    public Sprite firstSprite = null;
    public Sprite secondSprite = null;
    SpriteRenderer spriteRenderer = null;
    
    public void ChangeSprite()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        // Debug.Log(spriteRenderer);

        if (isSecond)
        {
            spriteRenderer.sprite = firstSprite;
            isSecond = false;
        }
        else 
        {
            spriteRenderer.sprite = secondSprite;
            isSecond = true;
        }
    }   
}
