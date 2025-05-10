using UnityEngine;
using Unity.Netcode;

public class AbstractCard : NetworkBehaviour, ISelectable
{
    public void OnHoverEnter()
    {
        // glow
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
        {
            OutlineController.SetOutline(true);
        }
    }

    public void OnHoverExit()
    {
        // stop glowing
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
        {
            OutlineController.SetOutline(false);
        }
    }

    public Player _ownerPlayer { get; private set; } // To Wouter: int thing
    public void Set_Owner(Player player)
    {
        _ownerPlayer = player;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = player.cardMaterial;
    }

    [SerializeField] private int cost = 1;
    public int _cost => cost;
}
