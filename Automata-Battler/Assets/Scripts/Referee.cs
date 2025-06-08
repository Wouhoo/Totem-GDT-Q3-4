using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine.SceneManagement;

public class Referee : NetworkBehaviour // The referee is a networkobject; most of its functions are carried out *only on server*.
{
    public static Referee Instance { get; private set; }
    private ulong activePlayer; // server = 1, client = 2. 0 is reserved as null value to be consistent with the Player.playerId and HexCell.commander fields.
                                // CAREFUL: when sending an RPC to a specific player, don't forget to subtract 1 in order to convert to actual clientId! (0 for server, 1 for client)
    private int round = 0;
    public List<Card> cardList { get; private set; } = new List<Card>(); // in order of play (newest last)
    private bool p1Ready = false;
    private bool p2Ready = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(StartGame()); // Have to do it this way so we can wait until all necessary networkobjects have spawned
            NetworkManager.Singleton.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
        }
    }

    // Triggered on server when a scene is (re)loaded
    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == "MainMenu") // If the main menu got loaded: someone left the lobby, so shut down network
        {
            Debug.Log("Connection shut down!");
            NetworkManager.Singleton.Shutdown();
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerReadyRpc(ulong playerId) // Called by a Player once they have finished their initializations
    {
        if (playerId == 1)
            p1Ready = true;
        else if (playerId == 2)
            p2Ready = true;
    }

    private IEnumerator StartGame()
    {
        // Wait until all NetworkObjects that are necessary to start the game have spawned and both players have initialized
        while (!CardManager.Instance.IsSpawned || !p1Ready || !p2Ready) // Add more NetworkObjects here as required
            yield return null;

        // Start the game for the players
        PlayerStartGameRpc();
        activePlayer = 1; // Always make server starting player
        PlayerBeginTurnRpc(RpcTarget.Single(activePlayer - 1, RpcTargetUse.Temp)); // THIS IS OK! (NOT AWAIT)
        PlayerBeginViewRpc(RpcTarget.Single((3 - activePlayer) - 1, RpcTargetUse.Temp)); // Problem: the camera controller may not have been initialized yet at this point
        // Theory: the game first starts the transition (with p1's camera targets for both players), then initializes the camera, then finishes transitioning (so still to p1's targets)
        // We need to wait with the transition until the camera is initialized.
        // Idea: make the player's initialize functions fire an event on server once initialization is complete.
        // Make the server's referee listen to this event being fired; once it has fired twice, we know both players have been initialized.
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
        Debug.Log("STARTING TURN");
        Player.Instance.BeginTurn();
    }

    [Rpc(SendTo.SpecifiedInParams)] // Same thing but for BeginView
    private void PlayerBeginViewRpc(RpcParams rpcParams)
    {
        Debug.Log("STARTING VIEW");
        Player.Instance.BeginView();
    }

    [Rpc(SendTo.ClientsAndHost)] // Similar thing but for WatchGame
    // This is called when the execution phase starts, and applies to both players!
    private void PlayerBeginWatchRpc()
    {
        Debug.Log("STARTING WATCH");
        Player.Instance.WatchGame();
    }

    [Rpc(SendTo.ClientsAndHost)] // Same thing but for EndTurn
    private void PlayerEndTurnRpc()
    {
        Player.Instance.DrawCards();
    }

    [Rpc(SendTo.Server)] // Turn end stuff is only done on server
    public void EndTurnRpc() // Was async Task, but RPCs can only be void; check if this causes any problems.
    {
        EndTurn();
    }

    private async void EndTurn()
    {
        Debug.Log(string.Format("ROUND: {0}", round));

        PlayerEndTurnRpc();

        // Temp:
        //activePlayer.gameObject.SetActive(false);

        if (round % 2 == 0 && activePlayer == 1)
        {
            activePlayer = 2;
        }
        else if (round % 2 == 0 && activePlayer == 2)
        {
            //await ExecuteCards();
            PlayerBeginWatchRpc(); // Make players watch the board
            ChangeTurnTextRpc(0);  // Temporarily set the current active player to 0 (meaning "executing")
            await ExecuteCards();  // Can't await anymore since RPC cannot be async; see if this causes any trouble
            round++;
        }
        else if (round % 2 == 1 && activePlayer == 1)
        {
            //await ExecuteCards();
            PlayerBeginWatchRpc(); // Make players watch the board
            ChangeTurnTextRpc(0);  // Temporarily set the current active player to 0 (meaning "executing")
            await ExecuteCards();  // Can't await anymore since RPC cannot be async; see if this causes any trouble
            round++;
        }
        else if (round % 2 == 1 && activePlayer == 2)
        {
            activePlayer = 1;
        }

        ChangeTurnTextRpc(activePlayer);
        PlayerBeginTurnRpc(RpcTarget.Single(activePlayer - 1, RpcTargetUse.Temp)); // Note: cannot be awaited anymore because it is an RPC...
        PlayerBeginViewRpc(RpcTarget.Single((3 - activePlayer) - 1, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.ClientsAndHost)] // Notify all players who the new active player is so they can change the turn indicator
    private void ChangeTurnTextRpc(ulong currPlayerId)
    {
        UIManager.Instance.ChangeTurnIndicator(currPlayerId);
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
        RemoveCardsRpc();
    }

    [Rpc(SendTo.ClientsAndHost)] // Only the server is allowed to add cards to the initiative queue (client and host to fix huge headache issue with card P2 on board selection)
    public void AddCardRpc(NetworkBehaviourReference cardReference)
    {
        // Note that removing cards is almoast always done during execution time, and then we dont want to refresh initiative, so thats why the remove function doesnt exist :P
        if (cardReference.TryGet(out Card card))
            cardList.Add(card);
        else
            Debug.LogError(string.Format("Referee couldn't find card with reference {0}", cardReference));
        RefreshInitiative();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RemoveCardsRpc()
    {
        cardList.RemoveAll(item => item == null);
        RefreshInitiative();
    }

    public void RefreshInitiative() // Only called on server
    {
        for (int i = cardList.Count - 1; i >= 0; i--)
            cardList[i].SetInitiativeRpc(i);
    }

    [Rpc(SendTo.ClientsAndHost)] // Let all players know the game has ended
    public void TriggerGameEndRpc(ulong winningPlayer)
    {
        // Play cool sound effect or smth
        UIManager.Instance.ShowEndScreen(winningPlayer);
        Time.timeScale = 0f;
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        // Restart the game. Currently, the game is restarted for both players if *either* of them presses the Rematch button.
        Time.timeScale = 1f;
        NetworkManager.Singleton.SceneManager.LoadScene("Scenes/GridTesting", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    [Rpc(SendTo.Server)]
    public void BackToMenuServerRpc()
    {
        // Go back to the main menu and shut down the relay connection.
        NetworkManager.Singleton.SceneManager.LoadScene("Scenes/MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        // Because of the SceneManager_OnLoadComplete event, the connection is shut down only once the scene has been loaded for everyone
    }
}