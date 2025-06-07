using UnityEngine;

public class RotationArrow : MonoBehaviour, ISelectable
{
    [SerializeField] public bool clockwise;
    public bool Q_isActive = false;

    public void OnHoverEnter()
    {
        // glow
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
            OutlineController.SetOutline(true);
    }

    public void OnHoverExit()
    {
        // stop glowing
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
            OutlineController.SetOutline(false);
    }
}
