using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;

public class Referee : NetworkBehaviour // The referee is a networkobject; most of its functions are carried out *only on server*.
{
    private ulong activePlayer; // server = 1, client = 2. 0 is reserved as null value to be consistent with the Player.playerId and HexCell.commander fields.
                                // CAREFUL: when sending an RPC to a specific player, don't forget to subtract 1 in order to convert to actual clientId! (0 for server, 1 for client)
    private int round = 0;
    public List<Card> cardList { get; private set; } = new List<Card>(); // in order of play (newest last)
    public static Referee Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(StartGame()); // Have to do it this way so we can wait until all necessary networkobjects have spawned
    }

    private IEnumerator StartGame()
    {
        // Wait until all NetworkObjects that are necessary to start the game have spawned
        while(!CardManager.Instance.IsSpawned) // Add more NetworkObjects here as required
            yield return null;

        // Start the game for the players
        PlayerStartGameRpc();
        activePlayer = 1; // Always make server starting player
        PlayerBeginTurnRpc(RpcTarget.Single(activePlayer - 1, RpcTargetUse.Temp)); // THIS IS OK! (NOT AWAIT)
        PlayerBeginViewRpc(RpcTarget.Single((3 - activePlayer) - 1, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.ClientsAndHost)] // Make *everyone* draw their cards at start of game
    private void PlayerStartGameRpc()
    {
        Player.Instance.DrawCards();
        // Other stuff that both players need to do at start of game goes here
    }

    [Rpc(SendTo.SpecifiedInParams)] // Call BeginTurn on the player with ID specified in rpcParams
    private void PlayerBeginTurnRpc(RpcParams rpcParams)
    {
        Player.Instance.BeginTurn();
    }

    [Rpc(SendTo.SpecifiedInParams)] // Same thing but for BeginView
    private void PlayerBeginViewRpc(RpcParams rpcParams)
    {
        Player.Instance.BeginView();
    }

    [Rpc(SendTo.SpecifiedInParams)] // Same thing but for EndTurn
    private void PlayerEndTurnRpc(RpcParams rpcParams)
    {
        Player.Instance.EndTurn();
    }

    [Rpc(SendTo.Server)] // Turn end stuff is only done on server
    public void EndTurnRpc() // Was async Task, but RPCs can only be void; check if this causes any problems.
    {
        Debug.Log(string.Format("ROUND: {0}", round));

        PlayerEndTurnRpc(RpcTarget.Single(activePlayer - 1, RpcTargetUse.Temp));

        // Temp:
        //activePlayer.gameObject.SetActive(false);

        if (round % 2 == 0 && activePlayer == 1)
            activePlayer = 2;
        else if (round % 2 == 0 && activePlayer == 2)
        {
            //await ExecuteCards();
            ExecuteCards(); // Can't await anymore since RPC cannot be async; see if this causes any trouble
            round++;
        }
        else if (round % 2 == 1 && activePlayer == 1)
        {
            //await ExecuteCards();
            ExecuteCards(); // Can't await anymore since RPC cannot be async; see if this causes any trouble
            round++;
        }
        else if (round % 2 == 1 && activePlayer == 2)
            activePlayer = 1;

        PlayerBeginTurnRpc(RpcTarget.Single(activePlayer - 1, RpcTargetUse.Temp)); // Note: cannot be awaited anymore because it is an RPC...
        PlayerBeginViewRpc(RpcTarget.Single((3 - activePlayer) - 1, RpcTargetUse.Temp));
    }

    public async Task ExecuteCards() // Only called from within a server RPC, hence only executed on server
    {
        // Execute cards from most recent to oldest
        for (int i = cardList.Count - 1; i >= 0; i--)
        {
            if (cardList[i] != null) // check if card still exists
                await cardList[i].ExecuteInstructions();
        }

        // Remove cards after executions to prevent order errors
        cardList.RemoveAll(item => item == null);
        RefreshInitiative();
    }

    [Rpc(SendTo.Server)] // Only the server is allowed to add cards to the initiative queue
    public void AddCardRpc(NetworkBehaviourReference cardReference)
    {
        // Note that removing cards is almoast always done during execution time, and then we dont want to refresh initiative, so thats why the remove function doesnt exist :P
        if (cardReference.TryGet(out Card card))
            cardList.Add(card);
        else
            Debug.LogError(string.Format("Referee couldn't find card with reference {0}", cardReference));
        RefreshInitiative();
    }

    public void RefreshInitiative() // Only called on server
    {
        for (int i = cardList.Count - 1; i >= 0; i--)
            cardList[i].SetInitiativeRpc(i);
    }
}