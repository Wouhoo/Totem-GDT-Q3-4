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

    private Referee referee;
    // Decks now need to be on the CardManager since the client's Player doesn't have the authority to move cards spawned by the server
    // Again, there may be better ways to solve this (e.g. using a distributed authority framework rather than server authoritative),
    // but that would require a major architectural rework that we don't have the time for.
    [SerializeField] private Deck p1Deck;
    [SerializeField] private Deck p2Deck;
    // Same story for card materials, easier to do that on CardManager now
    public Material p1Material;
    public Material p2Material;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        referee = FindFirstObjectByType<Referee>();
        Debug.Log(string.Format("CARDMANAGER CARDS: {0}", cards.Count));
    }

    [Rpc(SendTo.Server)] // Execute this only on server; return a NetworkObjectReference which is sent to the correct Player (server or client)
    // NOTE: In the long-term, when both players are able to build their own decks, the client's deck will probably be saved on their device.
    // This means the client needs to be able to send which card they want to draw with this RPC.
    // The easiest way to accomplish this would probably be to have a "database" of all cards (with no stats modified) and refer to them by index in this database.
    // For now though, we'll continue drawing random cards for testing purposes until the multiplayer stuff is done.
    public void DrawCardRpc(ulong playerId, int deckSlot)
    {
        Debug.Log(string.Format("PLAYER CALLING: {0}, CARDMANAGER CARDS: {1}", playerId, cards.Count));

        // Draw & spawn random card
        int index = UnityEngine.Random.Range(0, cards.Count);
        Debug.Log(string.Format("CARD INDEX: {0}", index));
        GameObject cardObject = Instantiate(cards[index], transform);
        cardObject.GetComponent<NetworkObject>().Spawn(true);         // Also spawn the card across the network
        Debug.Log(cardObject);

        // Set ownership of card (also sets card material)
        AbstractCard card = cardObject.GetComponent<AbstractCard>();  // AbstractCard extends NetworkBehaviour, so it can actually be used as an argument/return of an RPC
                                                                      // as a NetworkBehaviourReference; no changes required!
        card.Set_Owner(playerId);
        if (playerId == 2 && card is Card card1) // Flip instructions for player 2
        {
            card1.InvertInstructionsRpc();
        }

        // Move card to correct position & orientation
        //if(card is Card card1) // Check if AbstractCard is also an actual Card
        //{
        //    MoveCardInHand(card1, playerId, deckSlot);
        //}

        // Return card to correct caller
        ReturnCardRpc(card, RpcTarget.Single(playerId - 1, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.Server)]
    public void SortHandRpc(NetworkObjectReference[] hand, ulong playerId)
    {
        // Sort given player's hand
        for (int i = 0; i < hand.Length; i++)
        {
            // Get card NetworkObject
            if (hand[i].TryGet(out NetworkObject card))
            {
                //Debug.Log(string.Format("MOVING CARD {0} TO PLAYER {1} SLOT {2}", card.name, playerId, i));
                // Move card to the correct slot
                if (playerId == 1)
                {
                    card.transform.position = p1Deck.slots[i].position;
                }
                else if (playerId == 2)
                {
                    card.transform.position = p2Deck.slots[i].position;
                    // card.transform.rotation = p2Deck.slots[i].rotation;
                }
            }
            else
                Debug.LogError("Couldn't find card!");
        }
    }

    [Rpc(SendTo.SpecifiedInParams)] // Runs only on a specific player depending on rpcParams
    private void ReturnCardRpc(NetworkBehaviourReference card, RpcParams rpcParams)
    {
        Player.Instance.AddCardToHand(card);
    }
}
