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

    //
    // Card Requests
    //

    public bool Placement_Request(Card card, Vector3Int pos)
    {
        if (!board._tiles.Contains(pos))
            return false;           // position does not exist
        if (tileToCardMap[pos] != null)
            return false;           // position occupied

        // else we place card
        card.Set_Position(pos);
        tileToCardMap[pos] = card;
        cardList.Add(card);
        return true;    // success!
    }

    public bool Move_Jump_Request(Card card, Vector3Int toPos)
    {
        if (!board._tiles.Contains(toPos))
            return false;           // position does not exist
        if (tileToCardMap[toPos] != null)
            return false;           // position occupied

        // else we move card
        tileToCardMap[card.Get_Position()] = null;
        card.Set_Position(toPos);
        tileToCardMap[toPos] = card;
        return true;    // success!
    }

    public bool Attack_Jump_Request(Card card, Vector3Int toPos, int byAmount)
    {
        if (!board._tiles.Contains(toPos))
            return false;           // position does not exist
        if (tileToCardMap[toPos] == null)
            return false;           // position unoccupied

        // else we damage target card
        tileToCardMap[toPos].RecieveDamage(byAmount);
        return true;    // success!
    }

    public bool Die_Request(Card card)
    {
        if (cardList.Contains(card))
        {
            cardList.Remove(card);
            tileToCardMap[card.Get_Position()] = null;
        }
        Destroy(card);
        return true;
    }



}
