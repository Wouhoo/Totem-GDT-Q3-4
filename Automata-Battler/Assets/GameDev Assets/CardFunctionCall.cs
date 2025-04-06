using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class CardFunctionCall
{
    public enum InstructionType
    {
        Move_Slide,
        Move_Jump,
        Attack_Slide,
        Attack_Jump
    }

    public InstructionType functionType;
    public Vector3Int position;
    public bool globalPositioning;
    public int damageAmount;
}