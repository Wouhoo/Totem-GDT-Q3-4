using UnityEngine;
using Unity.Netcode;

public class HexCell : NetworkBehaviour, ISelectable
{
    [Header("Hex Coordinates")]
    public HexCoordinates coordinates; //grid coordinates of this cell

    public Color color = Color.white; //current color of the cell, testing purposes

    [Header("Cell properties")]
    private SpriteRenderer placeholderSprite;
    [SerializeField] public BoardCellMesh mesh;
    [SerializeField] public GameObject highlightMesh;
    [SerializeField] public ulong commander;  // Note: default ("null") value will be 0

    //turn off the sprite in play mode, as the mesh will be generated
    void Start()
    {
        placeholderSprite = GetComponentInChildren<SpriteRenderer>();
        placeholderSprite.enabled = false;

        highlightMesh.SetActive(false);
    }

    //'Snap' the coordinate of a cell to it's hex coordinate
    // Only run this in the Editor (not in builds or play mode)
    void OnValidate()
    {
        // using localPosition because parented under Board
        // Otherwise, use position.
        transform.localPosition = HexCoordinates.ToWorldPosition(coordinates); ;
    }


    //Occupant stuff
    // NOTE BY WOUTER: We want the tile's occupant to be synced at all times.
    // This can be done either through RPCs over and back, or by making it a NetworkVariable (preferred)
    // Either way, we cannot sync the card reference itself (since it is a GameObject), so any communication has to send a NetworkBehaviourReference.
    // Internally, this is an index that the client and server can both use to find the Card instance in the shared list of all NetworkBehaviour instances
    // (Card extends AbstractCard, which extends NetworkBehaviour)

    // NETWORKVARIABLE VERSION: sync card reference as a NetworkBehaviourReference NetworkVariable.
    // For whatever reason, this is not allowed to be null, *even though Netcode for GameObjects explicitly says that this is allowed since package update 2.0.0*.
    // This causes a variety of errors, including the client not even loading into the scene.
    // A workaround would be to spawn an invisble "null card" and using a reference to that as the null value, but this feels kinda hacky.
    // We'll do it with RPCs, but I'm gonna keep this version in comments so you can see what the alternative is.

    //private NetworkVariable<NetworkBehaviourReference> _cardReference;

    // RPC VERSION: storing a card as a Card and syncing using RPCs which convert back and forth to NetworkBehaviourReference
    [SerializeField] Card _card = null;

    public Card GetCard()
    {
        // NETWORKVARIABLE VERSION
        /*
        // Translate NetworkBehaviourReference to Card instance if the card exists
        if (_cardReference.Value.TryGet(out Card card))
            return card;
        // Throw error otherwise
        Debug.LogWarning(string.Format("Could not find occupant of cell {0}", coordinates));
        return null;
        */

        // RPC VERSION
        return _card;
    }

    // Setting card can only be done by the server!
    // This update will then be propagated to the client through RPCs.
    public void SetCard(Card card)
    {
        bool removeOccupant = false;
        if(card == null)
            removeOccupant = true; // If card == null we want to remove the card.
                                   // Note: because NetworkBehaviourReference cannot be null, we still have to send the card reference in this case (though it is unused)
        NetworkBehaviourReference cardReference = new NetworkBehaviourReference(card); // Translate Card instance to NetworkBehaviourReference
        SetCardServerRpc(cardReference, removeOccupant);
    }

    [Rpc(SendTo.Server)] // The actual setting of the card is done on Server (so server can validate & shit)
    public void SetCardServerRpc(NetworkBehaviourReference cardReference, bool removeOccupant = false)
    {
        // NETWORKVARIABLE VERSION
        //_cardReference.Value = cardReference;

        // RPC VERSION
        if (removeOccupant || cardReference.TryGet(out Card card)) // Translate NetworkBehaviourReference to Card instance if we don't want to remove
        {
            // DO SERVER-SIDE VALIDATION HERE
            SetCardClientRpc(cardReference, removeOccupant); // Once server has validated the placement, tell all players what the new card is
                                             // This is required only in the RPC version; it happens automatically in the NETWORKVARIABLE version (that's what NetworkVariables are for)
        }
        else
        {
            Debug.LogError(string.Format("Could not find occupant of cell {0}", coordinates)); // We don't want to remove the occupant, but card does not exist; throw error
        }
    }

    [Rpc(SendTo.ClientsAndHost)] // RPC VERSION ONLY
    public void SetCardClientRpc(NetworkBehaviourReference cardReference, bool removeOccupant = false)
    {
        if (removeOccupant)
            _card = null;
        else if (cardReference.TryGet(out Card card))
            _card = card;
        else
            Debug.LogError("Could not find card!");
    }

    public void DamageCommander(int damageAmount) // W: damaging commander now goes through cell instead of directly from board to player,
                                                  // since I don't want Board to be a NetworkObject (hence it cannot have RPCs)
    {
        DamageCommanderRpc(damageAmount, RpcTarget.Single(commander-1, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)] // Sent only to the correct player (so Player.Instance will be the player commanding this tile)
    private void DamageCommanderRpc(int damageAmount, RpcParams rpcParams)
    {
        Player.Instance.TakeDamage(damageAmount);
    }

    public void OnSelect() { }
    public void OnHoverEnter()
    {
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
        {
            OutlineController.SetOutline(true);
        }

    }
    public void OnHoverExit()
    {
        var OutlineController = GetComponent<OutlineController>();
        if (OutlineController != null)
        {
            OutlineController.SetOutline(false);
        }
    }

}
