using System.Collections.Generic;
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
    [SerializeField] private Deck deck;
    private CardManager cardManager;
    public Material cardMaterial;

    private ulong playerId;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerStateManager = GetComponent<PlayerStateManager>();
        referee = FindFirstObjectByType<Referee>();
        cardManager = FindFirstObjectByType<CardManager>();

        // To Wouter: Assign intergers here (not start, since the ref needs them at start!)
        playerId = NetworkManager.Singleton.LocalClientId;
    }

    //
    // Hand
    //

    public List<AbstractCard> _hand { get; private set; } = new List<AbstractCard>();

    public void DrawCards()
    {
        int neededCards = 5 - _hand.Count;
        for (int i = 0; i < neededCards; i++)
            cardManager.DrawCardRpc(playerId);  // To Wouter: pass the player int instead of "this" or smth

        for (int i = 0; i < 5; i++)
        {
            AbstractCard card = _hand[i];
            card.transform.position = deck.slots[i].position;
        }
    }

    public void AddCardToHand(NetworkBehaviourReference cardReference) // Has to be a separate function so it can be called from CardManager
    {
        if(cardReference.TryGet(out AbstractCard card)) // Get AbstractCard from NetworkBehaviourReference
                                                        // (TryGet returns False if the card cannot be found; this should never happen)
            _hand.Add(card);
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
