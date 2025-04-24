using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class Player : MonoBehaviour
{
    [SerializeField] private static bool activePlayer;
    private Referee referee;
    private PlayerStateManager playerStateManager;
    [SerializeField] private Deck deck;

    void Awake()
    {
        playerStateManager = GetComponent<PlayerStateManager>();

        referee = FindFirstObjectByType<Referee>();
    }

    void Start()
    {
        // playerStateManager._isPlayerTurn = false;
        // playerStateManager.ToState(PlayerState.ViewingHand);
    }

    //
    // Hand
    //

    public List<Card> _hand { get; private set; } = new List<Card>();

    public void DrawCards()
    {
        if (_hand.Count == 5)
            return;
        int neededCards = 5 - _hand.Count;
        for (int i = 0; i < neededCards; i++)
            _hand.Add(deck.DrawCard());

        for (int i = 0; i < 5; i++)
        {
            Card card = _hand[i];
            card.transform.position = deck.slots[i].position;
        }
    }

    //
    // Mana System
    //

    public int _mana { get; private set; } = 3;

    public void ResetMana()
    {
        _mana = 3;
    }

    //
    // Play Card
    //

    public bool PlayCard(Card card, HexCell tile)
    {
        if (!_hand.Contains(card)) // card not in hand
            return false;
        if (tile.Get_Card() != null) // tile not free
            return false;
        if (card._cost > _mana) // not enough mana
            return false;

        // Else we now play our card
        card.PlaceCard(tile.coordinates);
        _hand.Remove(card);
        _mana -= card._cost;
        referee.AddCard(card);
        return true;
    }

    //
    // Forced to watch by referee
    //

    public void WatchGame()
    {
        playerStateManager.ToState(PlayerState.WatchingGame);
    }

    public void BeginTurn()
    {
        ResetMana();
        playerStateManager._isPlayerTurn = true;
        playerStateManager.ToState(PlayerState.ViewingHand);
    }

    public void EndTurn()
    {
        playerStateManager._isPlayerTurn = false;
    }
}
