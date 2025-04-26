using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class I_Attack_asJump : MonoBehaviour
{
    public static async Task Execute(Card card, HexDirection direction, int moveAmount, int damageAmount)
    {
        Debug.Log($"Executing Instruction: I_Attack_asJump ({card}, {direction}, {moveAmount}, {damageAmount})");
        Board board = FindFirstObjectByType<Board>();

        HexCoordinates target = card._position + moveAmount * direction.GetRelativeCoordinates();

        if (board.CanAttack(target)) // ask if attack is possible
        {
            await Animate_Success(card, target);
            await I_TakeDamage.Execute(board.TileOccupant(target), damageAmount);
            return;
        }
        else // Failed to attack
            await Animate_Failure(card);
    }

    private static readonly Dictionary<HexDirection, string> instructionVisual = new()
    {
        {HexDirection.N,  "⇑" },
        {HexDirection.NE, "⇗" },
        {HexDirection.SE, "⇘" },
        {HexDirection.S,  "⇓" },
        {HexDirection.SW, "⇙" },
        {HexDirection.NW, "⇖" }
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

    public static async Task Animate_Success(Card card, HexCoordinates toPos)
    {
        await CardAnimator.Lerp_JumpTo(card.transform, HexCoordinates.ToWorldPosition(toPos), 0.25f);
        await CardAnimator.Lerp_JumpTo(card.transform, HexCoordinates.ToWorldPosition(card._position), 0.25f);
    }

    public static async Task Animate_Failure(Card card)
    {
        await CardAnimator.Lerp_JumpTo(card.transform, HexCoordinates.ToWorldPosition(card._position), 0.5f);
    }
}