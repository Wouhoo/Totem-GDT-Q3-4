using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Rendering.Universal;
using System.Threading.Tasks;

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

    public async Task Execute(Card card)
    {
        switch (this.instructionType)
        {
            case CardInstructionType.Move:
                await I_Move_asJump.Execute(card, this.direction, 1);
                break;
            case CardInstructionType.Jump:
                await I_Move_asJump.Execute(card, this.direction, 2);
                break;
            case CardInstructionType.Attack:
                await I_Attack_asJump.Execute(card, this.direction, 1, card._damage);
                break;
            case CardInstructionType.Lob:
                await I_Attack_asJump.Execute(card, this.direction, 2, card._damage);
                break;
            case CardInstructionType.Die:
                await I_Die.Execute(card);
                break;
        }
    }

    public string GetVisual()
    {
        switch (this.instructionType)
        {
            case CardInstructionType.Move:
                return I_Move_asJump.GetVisual(this.direction, 1);
            case CardInstructionType.Jump:
                return I_Move_asJump.GetVisual(this.direction, 2);
            case CardInstructionType.Attack:
                return I_Attack_asJump.GetVisual(this.direction, 1);
            case CardInstructionType.Lob:
                return I_Attack_asJump.GetVisual(this.direction, 2);
            case CardInstructionType.Die:
                return I_Die.GetVisual();
        }
        Debug.Log("Visual not implemented.");
        return "";
    }

    public string GetVisual_Client()
    {
        HexDirection clientDirection = this.direction.Rotate(3);
        switch (this.instructionType)
        {
            case CardInstructionType.Move:
                return I_Move_asJump.GetVisual(clientDirection, 1);
            case CardInstructionType.Jump:
                return I_Move_asJump.GetVisual(clientDirection, 2);
            case CardInstructionType.Attack:
                return I_Attack_asJump.GetVisual(clientDirection, 1);
            case CardInstructionType.Lob:
                return I_Attack_asJump.GetVisual(clientDirection, 2);
            case CardInstructionType.Die:
                return I_Die.GetVisual();
        }
        Debug.Log("Visual not implemented.");
        return "";
    }

    public void Rotate(int byAmount)
    {
        this.direction = this.direction.Rotate(byAmount);
    }
}


