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

    // Referee now needs to keep track of health for both players to decide when to play winning/losing themes
    private int p1CommanderHealth;
    private int p2CommanderHealth;
    private int winningThreshold = 2; // A player is considered "winning" if their commander's health is larger than the opponent's by at least this number.

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        BGMPlayer.Instance.PlayBGMTheme(BGMPlayer.BGMTheme.Tutorial);
        Time.timeScale = 0.0f; // Delay game start until both players are ready
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
    public void PlayerReadyRpc(ulong playerId) // Called by the UIManager once the player has pressed "Done" on the initial tutorial pop-up
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
        p1CommanderHealth = Player.Instance._health; // Initialize health to max health
        p2CommanderHealth = Player.Instance._health;
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
        Time.timeScale = 1.0f; // Actually start the game
        UIManager.Instance.HideWaitingForOpponentScreen();
        BGMPlayer.Instance.PlayBGMTheme(BGMPlayer.BGMTheme.Battle);
        Player.Instance.DrawCards();
        // Other stuff that both players need to do at start of game goes here
    }

    [Rpc(SendTo.SpecifiedInParams)] // Call BeginTurn on the player with ID specified in rpcParams
    private void PlayerBeginTurnRpc(RpcParams rpcParams)
    {
        Debug.Log("STARTING TURN");
        UIManager.Instance.FlashYourTurnScreen();
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
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.DrawCard);
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
            ChangeTurnIndicatorRpc(0);  // Temporarily set the current active player to 0 (meaning "executing")
            await ExecuteCards();  // Can't await anymore since RPC cannot be async; see if this causes any trouble
            round++;
        }
        else if (round % 2 == 1 && activePlayer == 1)
        {
            //await ExecuteCards();
            PlayerBeginWatchRpc(); // Make players watch the board
            ChangeTurnIndicatorRpc(0);  // Temporarily set the current active player to 0 (meaning "executing")
            await ExecuteCards();  // Can't await anymore since RPC cannot be async; see if this causes any trouble
            round++;
        }
        else if (round % 2 == 1 && activePlayer == 2)
        {
            activePlayer = 1;
        }

        ChangeTurnIndicatorRpc(activePlayer);
        PlayerBeginTurnRpc(RpcTarget.Single(activePlayer - 1, RpcTargetUse.Temp)); // Note: cannot be awaited anymore because it is an RPC...
        PlayerBeginViewRpc(RpcTarget.Single((3 - activePlayer) - 1, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.ClientsAndHost)] // Notify all players who the new active player is so they can change the turn indicator
    private void ChangeTurnIndicatorRpc(ulong currPlayerId)
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.TurnChange); // Also play a sound effect for turn change on both client and host
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

    [Rpc(SendTo.Server)]
    public void UpdateCommanderHealthServerRpc(ulong playerId, int health) // Update server's tracking of player health values
    {
        //Debug.Log(string.Format("NEW PLAYER {0} HEALTH: {1}", playerId, health));
        if(playerId == 1)
            p1CommanderHealth = health;
        else if(playerId == 2)
            p2CommanderHealth = health;

        // Decide whether to play winning or losing theme
        // Alternatively, this check could be performed at the start of the next turn so we don't constantly switch themes when both commanders are being damaged
        // W: @Lars/@Kerem let me know if I should move the check there instead
        ulong winningPlayer = 0;
        if (p1CommanderHealth - p2CommanderHealth >= winningThreshold)
            winningPlayer = 1;
        else if (p2CommanderHealth - p1CommanderHealth >= winningThreshold)
            winningPlayer = 2;

        UpdateCommanderHealthClientRpc(playerId, health, winningPlayer);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateCommanderHealthClientRpc(ulong playerId, int health, ulong winningPlayer) // Play damage sound and update health text on both client and server
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.Damage);
        UIManager.Instance.UpdateCommanderHealthText(playerId, health);
        PlayWinOrLoseTheme(winningPlayer);
    }

    private void PlayWinOrLoseTheme(ulong winningPlayer)
    {
        // @Lars/@Kerem maybe add some fade-ins and fade-outs here idk
        if (winningPlayer == 0)                             // If players are evenly matched (again): go back to normal battle theme
            BGMPlayer.Instance.PlayBGMTheme(BGMPlayer.BGMTheme.Battle);
        else if (winningPlayer == Player.Instance.playerId) // If the local player id is the same as winning player
            BGMPlayer.Instance.PlayBGMTheme(BGMPlayer.BGMTheme.Winning);
        else                                                // If the local player is not winning, they are losing
            BGMPlayer.Instance.PlayBGMTheme(BGMPlayer.BGMTheme.Losing);
    }

    [Rpc(SendTo.ClientsAndHost)] // Let all players know the game has ended
    public void TriggerGameEndRpc(ulong winningPlayer)
    {
        BGMPlayer.Instance.StopPlaying(); // @Lars/@Kerem maybe add a fade-out here idk
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