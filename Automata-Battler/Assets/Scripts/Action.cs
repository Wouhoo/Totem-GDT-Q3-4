using UnityEngine;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using System.Threading.Tasks;

public interface IAction
{
    public PlayerCameraState Get_ActionCamera();
    public PlayerRequestState Get_ActionInput();
    public async Task Act(ISelectable selectable) { } // THIS IS OK!
}

