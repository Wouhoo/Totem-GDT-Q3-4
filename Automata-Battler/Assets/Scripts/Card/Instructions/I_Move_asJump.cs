using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class I_Move_asJump : MonoBehaviour
{
    public static async Task Execute(Card card, HexDirection direction, int moveAmount)
    {
        Debug.Log($"Executing Instruction: I_Move_asJump ({card}, {direction}, {moveAmount})");
        Board board = FindFirstObjectByType<Board>();
        HexCoordinates target = card._position + moveAmount * direction.GetRelativeCoordinates();

        if (board.CanPlace(card._ownerPlayer, target)) // ask if move is possible
        {
            board.Set_TileOccupant(card._position, null);
            card.Set_Position(target);
            board.Set_TileOccupant(card._position, card);
            await Animate_Success(card);
        }
        else // Failed to move
            await Animate_Failure(card);
    }

    public void Rotate(int byAmount)
    {
        // Todo
    }

    private static readonly Dictionary<HexDirection, string> instructionVisual = new()
    {
        {HexDirection.N,  "↑" },
        {HexDirection.NE, "↗" },
        {HexDirection.SE, "↘" },
        {HexDirection.S,  "↓" },
        {HexDirection.SW, "↙" },
        {HexDirection.NW, "↖" }
    };

    public static string GetVisual(HexDirection direction, int moveAmount)
    {
        string result = instructionVisual[direction];
        if (moveAmount > 1)
        {
            for (int i = 2; i <= moveAmount; i++)
                result += "*";
        }
        return result;
    }

    public static async Task Animate_Success(Card card)
    {
        await CardAnimator.Lerp_JumpTo(card.transform, HexCoordinates.ToWorldPosition(card._position), 0.5f);
    }

    public static async Task Animate_Failure(Card card)
    {
        await CardAnimator.Lerp_JumpTo(card.transform, HexCoordinates.ToWorldPosition(card._position), 0.5f);
    }
}