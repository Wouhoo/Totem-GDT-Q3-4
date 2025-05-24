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
            case CardInstructionType.Slide:
                await I_Move_asSlide.Execute(card, this.direction, 99);
                break;
            case CardInstructionType.Attack:
                await I_Attack_asJump.Execute(card, this.direction, 1, card._damage);
                break;
            case CardInstructionType.Lob:
                await I_Attack_asJump.Execute(card, this.direction, 2, card._damage);
                break;
            case CardInstructionType.Shoot:
                await I_Attack_asSlide.Execute(card, this.direction, 99, card._damage);
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
            case CardInstructionType.Slide:
                return I_Move_asSlide.GetVisual(this.direction, 1);
            case CardInstructionType.Attack:
                return I_Attack_asJump.GetVisual(this.direction, 1);
            case CardInstructionType.Lob:
                return I_Attack_asJump.GetVisual(this.direction, 2);
            case CardInstructionType.Shoot:
                return I_Attack_asSlide.GetVisual(this.direction, 1);
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


