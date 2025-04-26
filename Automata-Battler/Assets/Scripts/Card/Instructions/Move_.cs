using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
/*
public class Move_ : IInstruction
{
    private HexDirection direction;
    private Card card;
    private Board board;

    public Move_(Card card, Board board, HexDirection direction)
    {
        this.card = card;
        this.board = board;
        this.direction = direction;
    }

    public async Task Execute(Card card)
    {
        HexCoordinates target = card._position + direction.GetRelativeCoordinates();

        if (board.CanPlace(target)) // ask if move is possible
        {
            board.Set_TileOccupant(card._position, null);
            card.Set_Position(target);
            board.Set_TileOccupant(card._position, this);
            await cardAnimator.Move_asJump(card._position);
            return;
        }

        // Failed to move
        await card.cardAnimator.Move_asJump_FAIL(target);
        return;
    }

    public void Rotate(int byAmount)
    {

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

    public string GetVisual()
    {
        return instructionVisual[direction];
    }

    private async Task Animate(HexCoordinates toPos)
    {
        float elapsed = 0f;
        Vector3 start = card.transform.position;
        Vector3 relativeEnd = HexCoordinates.ToWorldPosition(toPos) - start;

        while (elapsed < duration)
        {
            float t = Easing_Smooth(elapsed / duration);
            transform.position = start + relativeEnd * t + midpointMax * 4 * t * (1 - t);
            elapsed += Time.deltaTime;
            await Task.Yield(); // Crucial to yield control per frame
        }

        transform.position = destination;
    }

    private float Easing_Smooth(float t)
    {
        // Start: 0
        return 3 * t * t - 2 * t * t * t;
        // End: 1
    }
}
*/
