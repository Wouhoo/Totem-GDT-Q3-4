using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class I_Die : MonoBehaviour
{
    public static async Task Execute(Card card)
    {
        Debug.Log($"Executing Instruction: I_Die ({card})");
        Board board = FindFirstObjectByType<Board>();
        Referee referee = FindFirstObjectByType<Referee>();

        await Animate(card);
        // Remove from board
        board.Set_TileOccupant(card._position, null);
        referee.RemoveCard(card);
        // destroy card game object
        // Destroy(card.gameObject);
    }

    public static string GetVisual()
    {
        return "X";
    }

    private static async Task Animate(Card card)
    {
        await CardAnimator.Lerp_SlideTo(card.transform, new Vector3(0, 100, 0), 0.5f);
    }
}