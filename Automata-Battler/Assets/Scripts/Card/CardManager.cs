using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR;

public class CardManager : NetworkBehaviour
{
    public static CardManager Instance { get; private set; } // Singleton so the DrawCardRpc doesn't bug out
    // Note: I *think* this still allows P1 and P2 to have different sets of cards (decks),
    // but even if not there's ways to get around this.

    public List<GameObject> cards = new List<GameObject>();

    //skins for the cards, i.e. cyan or orange
    public CardSkin p1Skin;
    public CardSkin p2Skin;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Debug.Log(string.Format("CARDMANAGER CARDS: {0}", cards.Count));
    }

    // Execute this only on server; return a NetworkObjectReference which is sent to the correct Player (server or client)
    // NOTE: In the long-term, when both players are able to build their own decks, the client's deck will probably be saved on their device.
    // This means the client needs to be able to send which card they want to draw with this RPC.
    // The easiest way to accomplish this would probably be to have a "database" of all cards (with no stats modified) and refer to them by index in this database.
    // For now though, we'll continue drawing random cards for testing purposes until the multiplayer stuff is done.
    [Rpc(SendTo.Server)]
    public void DrawCardRpc(ulong playerId, int handSlot)
    {
        Debug.Log(string.Format("PLAYER CALLING: {0}, CARDMANAGER CARDS: {1}", playerId, cards.Count));

        // Draw & spawn random card
        int index = UnityEngine.Random.Range(0, cards.Count);
        Debug.Log(string.Format("CARD INDEX: {0}", index));
        GameObject cardObject = Instantiate(cards[index], new Vector3(0, 100, 0), Quaternion.identity, transform);
        cardObject.GetComponent<NetworkObject>().Spawn(true);         // Also spawn the card across the network
        Debug.Log(cardObject);

        // Set ownership of card (also sets card material)
        AbstractCard card = cardObject.GetComponent<AbstractCard>();  // AbstractCard extends NetworkBehaviour, so it can actually be used as an argument/return of an RPC
                                                                      // as a NetworkBehaviourReference; no changes required!
        card.Set_Owner_Rpc(playerId);
        if (playerId == 2 && card is Card card1) // Flip instructions for player 2
            card1.RotateInstructionsRpc(3);

        // Return card to correct caller
        ReturnCardRpc(card, handSlot, RpcTarget.Single(playerId - 1, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)] // Runs only on a specific player depending on rpcParams
    private void ReturnCardRpc(NetworkBehaviourReference card, int handSlot, RpcParams rpcParams)
    {
        Player.Instance.AddCardToHand(card, handSlot);
    }
}
