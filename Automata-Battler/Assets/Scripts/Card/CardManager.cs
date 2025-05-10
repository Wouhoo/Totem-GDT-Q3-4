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
    public void DrawCardRpc(ulong playerId)   //(Player player) // Temp test: always set P1 as owner
    {
        Player player = FindFirstObjectByType<Player>();
        int index = UnityEngine.Random.Range(0, cards.Count);
        GameObject cardObject = Instantiate(cards[index], transform); // To Wouter: other transform? <- Not necessary?
        cardObject.GetComponent<NetworkObject>().Spawn(true);         // Also spawn the card across the network
        AbstractCard card = cardObject.GetComponent<AbstractCard>();  // AbstractCard extends NetworkBehaviour, so it can actually be used as an argument/return of an RPC
                                                                      // as a NetworkBehaviourReference; no changes required!
        card.Set_Owner(player); // To Wouter: change to players int
        if (player != referee._player1 && card is Card card1)
            card1.Rotate(3); // align with player view (temp sorta?)

        // Return card to correct caller
        ReturnCardRpc(card, RpcTarget.Single(playerId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)] // Runs only on a specific player depending on rpcParams
    private void ReturnCardRpc(NetworkBehaviourReference card, RpcParams rpcParams)
    {
        Player.Instance.AddCardToHand(card);
    }
}
