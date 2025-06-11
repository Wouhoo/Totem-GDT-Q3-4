using UnityEngine;
using Unity.Netcode;
using UnityEngine.UIElements;

public class AbstractCard : NetworkBehaviour, ISelectable
{
    public int _inHandIndex;

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

    public ulong _ownerPlayer { get; private set; }
    public void Set_Owner(ulong playerId)
    {
        _ownerPlayer = playerId;
        SetMaterialRpc(playerId);
    }

    [Rpc(SendTo.ClientsAndHost)] // Has to be sent to both players, otherwise the other player's cards will have missing textures
    private void SetMaterialRpc(ulong playerId)
    {
        if (playerId == 1)
        {
            //gameObject.GetComponent<MeshRenderer>().material = CardManager.Instance.p1Material;
            gameObject.GetComponent<CardRenderer>().SetSkin(CardManager.Instance.p1Skin);
        }
        else if (playerId == 2) // Keeping the if in case playerId is somehow ever 0 (null)
        {
            //gameObject.GetComponent<MeshRenderer>().material = CardManager.Instance.p2Material;
            gameObject.GetComponent<CardRenderer>().SetSkin(CardManager.Instance.p2Skin);
            //gameObject.GetComponent<Card>().Rotate(3);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void Set_WorldPosition_Rpc(Vector3 pos)
    {
        transform.position = pos;
    }

    [Rpc(SendTo.ClientsAndHost)] // Why does this work like this??!?!?!?!?!?
    public void DrawCard_Placement_Rpc(Vector3 newPos, Quaternion newRot)
    {
        CardAnimator.Card_FlyIn_Rpc(transform, newPos, newRot, 0.12f);
    }

    [SerializeField] private int cost = 1;
    public int _cost => cost;
}
