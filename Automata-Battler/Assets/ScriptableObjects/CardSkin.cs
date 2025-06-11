using UnityEngine;

[CreateAssetMenu(fileName = "CardSkin", menuName = "Scriptable Objects/CardSkin")]
public class CardSkin : ScriptableObject
{
    [SerializeField] Color fontColor;
    [SerializeField] Texture2D basePanel;
    [SerializeField] Texture2D healthPanel;
    [SerializeField] Texture2D attackPanel;
    [SerializeField] Texture2D instructionsPanel;
    [SerializeField] Texture2D initiativePanel;
    [SerializeField] Texture2D rotateRPanel;
    [SerializeField] Texture2D rotateLPanel;
}
