using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CardManager : NetworkBehaviour
{
    public List<GameObject> cards = new List<GameObject>();

    private Referee referee;

    void Awake()
    {
        referee = FindFirstObjectByType<Referee>();
    }

    [Rpc(SendTo.Server)] // Execute this only on server; return a NetworkObjectReference which is sent to the correct Player (server or client)
    // NOTE: In the long-term, when both players are able to build their own decks, the client's deck will probably be saved on their device.
    // This means the client needs to be able to send which card they want to draw with this RPC.
    // The easiest way to accomplish this would probably be to have a "database" of all cards (with no stats modified)
    // and refer to them by index in this database.
    // For now though, we'll continue drawing random cards for testing purposes until the multiplayer stuff is done.
    public void DrawCardRpc(ulong playerId)
    {
        int index = UnityEngine.Random.Range(0, cards.Count);
        Debug.Log(string.Format("CARD INDEX: {0}", index));
        GameObject cardObject = Instantiate(cards[index], transform); // To Wouter: other transform? <- Not necessary?
        cardObject.GetComponent<NetworkObject>().Spawn(true);         // Also spawn the card across the network
        Debug.Log(cardObject);
        AbstractCard card = cardObject.GetComponent<AbstractCard>();  // AbstractCard extends NetworkBehaviour, so it can actually be used as an argument/return of an RPC
                                                                      // as a NetworkBehaviourReference; no changes required!
        card.Set_Owner(playerId); // To Wouter: change to players int
        if (playerId == 2 && card is Card card1)
            card1.Rotate(3); // align with player view (temp sorta?)

        // Return card to correct caller
        ReturnCardRpc(card, RpcTarget.Single(playerId-1, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)] // Runs only on a specific player depending on rpcParams
    private void ReturnCardRpc(NetworkBehaviourReference card, RpcParams rpcParams)
    {
        Debug.Log("RETURNING CARD TO HAND");
        Player.Instance.AddCardToHand(card);
    }
}
