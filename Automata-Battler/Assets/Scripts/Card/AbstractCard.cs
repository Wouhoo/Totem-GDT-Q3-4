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

    public ulong _ownerPlayer { get; private set; } // To Wouter: int thing
    public void Set_Owner(ulong playerId)
    {
        _ownerPlayer = playerId;
        //MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        //meshRenderer.material = player.cardMaterial;
        GetMaterialRpc(RpcTarget.Single(playerId-1, RpcTargetUse.Temp)); // Get material from correct player
    }

    [Rpc(SendTo.SpecifiedInParams)] // TEST: Get player material from the correct player
    private void GetMaterialRpc(RpcParams rpcParams)
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = Player.Instance.cardMaterial;
    }

    [SerializeField] private int cost = 1;
    public int _cost => cost;
}
