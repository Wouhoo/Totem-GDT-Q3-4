using UnityEngine;
using UnityEngine.Animations;

public class Button : MonoBehaviour, ISelectable
{
    [SerializeField] public ButtonType buttonType;

    public void OnHoverEnter() { }

    public void OnHoverExit() { }
}