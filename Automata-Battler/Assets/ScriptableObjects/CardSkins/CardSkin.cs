using UnityEngine;

[CreateAssetMenu(fileName = "CardSkin", menuName = "Scriptable Objects/CardSkin")]
public class CardSkin : ScriptableObject
{
    //yeah yeah should be read-only gtfo
    public Color fontColor;
    public Sprite basePanel;
    public Sprite healthPanel;
    public Sprite attackPanel;
    public Sprite instructionsPanel;
    public Sprite initiativePanel;
    public Sprite rotateRPanel;
    public Sprite rotateLPanel;

    public CardArt.CardArtVariant artVariant;
}
