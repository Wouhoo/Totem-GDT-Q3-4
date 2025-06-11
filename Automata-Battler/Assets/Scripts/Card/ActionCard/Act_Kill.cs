using System.Threading.Tasks;
using UnityEngine;
/*
public class Act_Kill : AbstractCard, IAction
{
    public bool Q_CanBeginAction()
    {
        if (Player.Instance._mana >= _cost)
            return true;
        else return false;
    }

    public async Task<PlayerCameraState> Act(ISelectable selectable)
    {
        if (selectable is Card card && card._inPlay)
        {
            Player.Instance.UseMana(_cost);
            await I_Die.Execute(card);
            //_ownerPlayer._hand.Remove(this); // To Wouter: int thingy
            Player.Instance.RemoveCardFromHand(this);
            return PlayerCameraState.ViewingBoard;
            Destroy(this.gameObject); // uhhhhhhhhhhh....?
        }
        // else we failed
        return PlayerCameraState.ViewingHand;

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
*/
