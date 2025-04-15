using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using System;

public class Referee : MonoBehaviour
{
    private List<Card> cardList = new List<Card>(); // in order of play (newest last)

    public void ExecuteCards()
    {
        // Execute cards from most recent to oldest
        for (int i = cardList.Count - 1; i >= 0; i--)
        {
            cardList[i].ExecuteInstructions();
        }
    }
}