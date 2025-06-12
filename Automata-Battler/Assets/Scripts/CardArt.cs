using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardArt : MonoBehaviour
{
    //THIS SUCKS, DICTIONARIES AREN'T SERIALIZABLE
    [SerializeField] private Sprite CyanArt;
    [SerializeField] private Sprite OrangeArt;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSkin(CardArtVariant variant)
    {
        if (variant == CardArtVariant.Cyan)
        {
            spriteRenderer.sprite = CyanArt;
        }
        else
        {
            spriteRenderer.sprite = OrangeArt;
        }
    }

    public enum CardArtVariant
    {
        Cyan,
        Orange
    }
}
