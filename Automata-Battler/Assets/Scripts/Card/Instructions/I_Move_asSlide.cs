using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class I_Move_asSlide : MonoBehaviour
{
    public static async Task Execute(Card card, HexDirection direction, int moveAmount)
    {
        Debug.Log($"Executing Instruction: I_Move_asSlide ({card}, {direction}, {moveAmount})");
        Board board = FindFirstObjectByType<Board>();
        HexCoordinates targetDirection = direction.GetRelativeCoordinates();

        int amountMoved = 0;
        for (int i = 0; i < moveAmount; i++)
        {
            if (!board.CanPlace(card._ownerPlayer, card._position + (amountMoved + 1) * targetDirection))
                break;
            amountMoved++;
        }

        if (amountMoved != 0)
        {
            board.Set_TileOccupant(card._position, null);
            card.Set_Position(card._position + amountMoved * targetDirection);
            board.Set_TileOccupant(card._position, card);
            await Animate_Success(card);
            return;
        }
        else // Failed to move
            await Animate_Failure(card);
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
        await CardAnimator.Lerp_SlideTo(card.transform, HexCoordinates.ToWorldPosition(card._position), 0.5f);
    }

    public static async Task Animate_Failure(Card card)
    {
        await CardAnimator.Lerp_JumpTo(card.transform, HexCoordinates.ToWorldPosition(card._position), 0.5f);
    }
}