using System.Collections.Generic;
using System.Collections;
using System.Data.Common;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; } // In multiplayer, this is now a singleton

    [SerializeField] private static bool activePlayer;
    private Referee referee;
    private PlayerStateManager playerStateManager;
    private CameraController cameraController;
    //[SerializeField] CardManager cardManager; // No longer necessary since CardManager is now a singleton
    //public Material cardMaterial; // Now tracked by CardManager

    public ulong playerId; // server = 1, client = 2. Use 0 for null values (e.g. if a HexCell belongs to neither player, its commander value is 0)
                           // CAREFUL: when sending an RPC to a specific player (using RpcTarget.Single), you need to put in the *actual* clientId (0 for server, 1 for client),
                           // so don't forget to subtract 1.
    private int handSize = 5;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerStateManager = GetComponent<PlayerStateManager>();
        cameraController = GetComponent<CameraController>();
        referee = FindFirstObjectByType<Referee>();
        //cardManager = FindFirstObjectByType<CardManager>();

        // To Wouter: Assign intergers here (not start, since the ref needs them at start!)
        playerId = NetworkManager.Singleton.LocalClientId + 1; // The +1 is important so we can use 0 as a "null value"
        Debug.Log(string.Format("PLAYER ID: {0}", playerId));

        // Initialize player-specific things 
        cameraController.InitializeCamera(playerId);
        // Add other player-specific inits here (e.g. using if(playerId == 1) { ... } else { ... })
    }

    //
    // Hand
    //

    public List<AbstractCard> _hand { get; private set; } = new List<AbstractCard>();

    public void DrawCards()
    {
        Debug.Log("DRAWING CARDS");
        for (int i = _hand.Count; i < handSize; i++)
        {
            // W: There used to be a problem that the card manager may not have spawned yet at this point.
            // This *should* be fixed by the check now built into Referee, but I'm keeping this error catch just in case
            if (!CardManager.Instance.IsSpawned)
            {
                Debug.LogError("CARDMANAGER HASN'T SPAWNED YET!!!");
            }
            CardManager.Instance.DrawCardRpc(playerId, i); 
        }
        // Moving cards to hand is now done by the CardManager
    }

    public void AddCardToHand(NetworkBehaviourReference cardReference) // Has to be a separate function so it can be called from CardManager
    {
        //print("ADDING CARD TO HAND");
        if (cardReference.TryGet(out AbstractCard card)) // Get AbstractCard from NetworkBehaviourReference (TryGet returns False if the card cannot be found; this should never happen)
        {
            //print("FOUND CARD");
            _hand.Add(card);
        }
    }

    //
    // Mana & Damage System
    //

    public int _mana = 3;

    public bool AttemptManaUse(int amount)
    {
        if (amount <= _mana)
        {
            _mana -= amount;
            return true;
        }
        else
            return false;
    }

    public int _health { get; private set; } = 10;

    public void TakeDamage(int amount)
    {
        _health = math.max(0, _health - amount);
        if (_health == 0)
            Die();
    }

    private void Die()
    {
        // TODO
    }

    //
    // State Changes For The Ref:
    //

    public async Task BeginTurn()
    {
        _mana = 3;
        await playerStateManager.ToState(PlayerState.Playing, PlayerCameraState.ViewingHand, PlayerRequestState.None);
    }

    public async Task BeginView()
    {
        _mana = 3;
        await playerStateManager.ToState(PlayerState.Viewing, PlayerCameraState.ViewingHand, PlayerRequestState.None);
    }

    public async Task WatchGame()
    {
        await playerStateManager.ToState(PlayerState.WatchingGame, PlayerCameraState.ViewingBoard, PlayerRequestState.None);
    }

    public void EndTurn()
    {
        DrawCards();
    }
}
