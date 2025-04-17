using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Rendering.Universal;
using Mono.Cecil.Cil;

[System.Serializable]
public struct CardInstruction
{
    public CardInstruction(CardInstructionType instructionType, HexDirection direction)
    {
        this.instructionType = instructionType;
        this.direction = direction;
    }

    public CardInstructionType instructionType;
    public HexDirection direction;
}

public static class CardInstructionExtensions
{
    public static string Visualization(this CardInstruction cardInstruction)
    {
        return instructionVisual[cardInstruction];
    }

    private static readonly Dictionary<CardInstruction, string> instructionVisual = new()
    {
        {new CardInstruction(CardInstructionType.Move, HexDirection.N),  "↑" },
        {new CardInstruction(CardInstructionType.Move, HexDirection.NE), "↗" },
        {new CardInstruction(CardInstructionType.Move, HexDirection.SE), "↘" },
        {new CardInstruction(CardInstructionType.Move, HexDirection.S),  "↓" },
        {new CardInstruction(CardInstructionType.Move, HexDirection.SW), "↙" },
        {new CardInstruction(CardInstructionType.Move, HexDirection.NW), "↖" },

        {new CardInstruction(CardInstructionType.Jump, HexDirection.N),  "↑*" },
        {new CardInstruction(CardInstructionType.Jump, HexDirection.NE), "↗*" },
        {new CardInstruction(CardInstructionType.Jump, HexDirection.SE), "↘*" },
        {new CardInstruction(CardInstructionType.Jump, HexDirection.S),  "↓*" },
        {new CardInstruction(CardInstructionType.Jump, HexDirection.SW), "↙*" },
        {new CardInstruction(CardInstructionType.Jump, HexDirection.NW), "↖*" },

        {new CardInstruction(CardInstructionType.Attack, HexDirection.N),  "⇑" },
        {new CardInstruction(CardInstructionType.Attack, HexDirection.NE), "⇗" },
        {new CardInstruction(CardInstructionType.Attack, HexDirection.SE), "⇘" },
        {new CardInstruction(CardInstructionType.Attack, HexDirection.S),  "⇓" },
        {new CardInstruction(CardInstructionType.Attack, HexDirection.SW), "⇙" },
        {new CardInstruction(CardInstructionType.Attack, HexDirection.NW), "⇖" },

        {new CardInstruction(CardInstructionType.Arrow, HexDirection.N),  "⇑*" },
        {new CardInstruction(CardInstructionType.Arrow, HexDirection.NE), "⇗*" },
        {new CardInstruction(CardInstructionType.Arrow, HexDirection.SE), "⇘*" },
        {new CardInstruction(CardInstructionType.Arrow, HexDirection.S),  "⇓*" },
        {new CardInstruction(CardInstructionType.Arrow, HexDirection.SW), "⇙*" },
        {new CardInstruction(CardInstructionType.Arrow, HexDirection.NW), "⇖*" }
    };
}

