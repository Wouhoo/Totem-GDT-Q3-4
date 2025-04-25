using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Referee : MonoBehaviour
{
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;
    private Player activePlayer;
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

    void Start()
    {
        activePlayer = player1;
        activePlayer.DrawCards();
        activePlayer.ResetMana();
        activePlayer.BeginTurn(); // THIS IS OK! (NOT AWAIT)
    }

    public async Task EndTurn()
    {
        Debug.Log(round);
        activePlayer.EndTurn();
        await activePlayer.WatchGame();
        activePlayer.gameObject.SetActive(false);

        if (round % 2 == 0 && activePlayer == player1)
            activePlayer = player2;
        else if (round % 2 == 0 && activePlayer == player2)
        {
            await ExecuteCards();
            round++;
        }
        else if (round % 2 == 1 && activePlayer == player1)
        {
            await ExecuteCards();
            round++;
        }
        else if (round % 2 == 1 && activePlayer == player2)
            activePlayer = player1;

        activePlayer.gameObject.SetActive(true);

        activePlayer.DrawCards();
        await activePlayer.BeginTurn();
    }

    public async Task ExecuteCards()
    {
        // Execute cards from most recent to oldest
        for (int i = cardList.Count - 1; i >= 0; i--)
        {
            await cardList[i].ExecuteInstructions();
        }
    }

    public void AddCard(Card card)
    {
        cardList.Add(card);
        RefreshInitiative();
    }

    public void RemoveCard(Card card)
    {
        cardList.Remove(card);
        RefreshInitiative();
    }

    public void RefreshInitiative()
    {
        for (int i = cardList.Count - 1; i >= 0; i--)
        {
            cardList[i].Set_Initiative(i);
        }
    }

    // Special actions

    public void Action_ChangeCardOwner(Card card)
    {

    }
    public void Action_RotateCardInstructions(int byAmount)
    {

    }
    public void Action_FreezeCard()
    {

    }
}