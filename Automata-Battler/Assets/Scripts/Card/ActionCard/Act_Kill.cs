using System.Threading.Tasks;
using UnityEngine;

public class Act_Kill : AbstractCard, IAction
{
    public async Task Act(ISelectable selectable)
    {
        if (selectable is Card card)
        {
            if (!card._inPlay)
                return;

            if (_ownerPlayer.AttemptManaUse(_cost))
            {
                await I_Die.Execute(card);
                _ownerPlayer._hand.Remove(this); // To Wouter: int thingy
            }
        }
        // else we failed
    }

    public PlayerCameraState Get_ActionCamera()
    {
        return PlayerCameraState.ViewingBoard;
    }

    public PlayerRequestState Get_ActionInput()
    {
        return PlayerRequestState.Cards_InPlay;
    }
}
