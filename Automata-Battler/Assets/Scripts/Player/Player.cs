using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using Unity.Mathematics;
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
    private CardManager cardManager;
    public Material cardMaterial;

    void Awake()
    {
        playerStateManager = GetComponent<PlayerStateManager>();
        referee = FindFirstObjectByType<Referee>();
        cardManager = FindFirstObjectByType<CardManager>();
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
            _hand.Add(DrawCard());

        for (int i = 0; i < 5; i++)
        {
            Card card = _hand[i];
            card.transform.position = deck.slots[i].position;
        }
    }

    public Card DrawCard()
    {
        int index = UnityEngine.Random.Range(0, cardManager.playableCards.Count);
        GameObject cardObject = Instantiate(cardManager.playableCards[index], cardManager.transform);
        Card card = cardObject.GetComponent<Card>();
        card.Set_Owner(this);
        if (this != referee._player1)
            card.Rotate(3); // align with player view
        return card;
    }

    //
    // Mana System
    //

    public int _mana { get; private set; } = 3;

    public void ResetMana()
    {
        _mana = 3;
    }

    // Damage system

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
    // Play Card
    //

    public async Task<bool> PlayCard(Card card, HexCell tile)
    {
        if (!_hand.Contains(card)) // card not in hand
            return false;
        if (tile.Get_Card() != null) // tile not free
            return false;
        if (card._cost > _mana) // not enough mana
            return false;

        // Else we now play our card
        await card.PlaceCard(tile.coordinates);
        _hand.Remove(card);
        _mana -= card._cost;
        referee.AddCard(card);
        return true;
    }

    //
    // Forced to watch by referee
    //

    public bool _isPlayerTurn { get; private set; } = false;

    public async Task WatchGame()
    {
        await playerStateManager.ToState(PlayerState.WatchingGame);
    }

    public async Task BeginTurn()
    {
        ResetMana();
        // Temp?:
        await playerStateManager.ToState(PlayerState.ViewingHand);
        _isPlayerTurn = true;
    }

    public void EndTurn()
    {
        DrawCards();
        _isPlayerTurn = false;
    }
}
