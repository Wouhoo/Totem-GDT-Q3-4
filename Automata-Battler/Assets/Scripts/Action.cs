using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAction
{
    public bool Q_CanBeginAction();
    public PlayerCameraState Get_ActionCamera();
    public PlayerRequestState Get_ActionInput();
    public async Task<PlayerCameraState> Act(ISelectable selectable) // THIS IS OK! (Returns resulting target player camera state)
    { return PlayerCameraState.ViewingHand; } // (Default behaviour)
}
