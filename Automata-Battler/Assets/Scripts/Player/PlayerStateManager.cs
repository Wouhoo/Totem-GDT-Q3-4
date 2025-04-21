using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(CameraController))]
[RequireComponent(typeof(SelectionManager))]
[DisallowMultipleComponent]
public class PlayerStateManager : MonoBehaviour
{
    private CameraController cameraController;
    private SelectionManager selectionManager;

    public PlayerState _currentState { get; private set; }

    void Awake()
    {
        cameraController = GetComponent<CameraController>();
        selectionManager = GetComponent<SelectionManager>();
    }

    public void ToState(PlayerState toState)
    {
        selectionManager.UpdateSelectables(toState);
        cameraController.MoveCamera(toState); // This takes multiple frames to conclude
    }

    public void SetState(PlayerState state)
    {
        // Should only be called by the camera controller after a sucsessful transition has been made
        _currentState = state;
    }

    public bool IsInteractionAllowed()
    {
        return _currentState == PlayerState.ViewingBoard || _currentState == PlayerState.ViewingHand;
    }
}
