using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Referee : MonoBehaviour
{
    [SerializeField] private Player player1;
    public Player _player1 => player1;
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
        player1.DrawCards();
        player2.DrawCards();
        activePlayer = player1;
        player2.gameObject.SetActive(false);
        activePlayer.BeginTurn(); // THIS IS OK! (NOT AWAIT)
        // and otherPlayer.BeginView(); (if they were active...)
    }

    public async Task EndTurn()
    {
        Debug.Log(round);

        activePlayer.EndTurn();

        // Temp:
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

        // TEMP: 
        activePlayer.gameObject.SetActive(true);

        await activePlayer.BeginTurn();
        // and otherPlayer.BeginView();
    }

    public async Task ExecuteCards()
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

    public void AddCard(Card card)
    {
        // Note that removing cards is almoast always done during execution time, and then we dont want to refresh initiative, so thats why the remove function doesnt exist :P
        cardList.Add(card);
        RefreshInitiative();
    }

    public void RefreshInitiative()
    {
        for (int i = cardList.Count - 1; i >= 0; i--)
            cardList[i].Set_Initiative(i);
    }
}