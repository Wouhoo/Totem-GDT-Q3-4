using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class CardFunctionCall
{

    public enum InstructionType
    {
        Move,
        Jump,
        Slide,
        Attack,
        Arrow,
        Shoot,
        Die
    }

    public InstructionType functionType;
    public HexDirection direction;
    public int damageAmount;
}