using System.Threading.Tasks;
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
    public PlayerCameraState _currentCameraState { get; private set; }
    public PlayerRequestState _currentRequestState { get; private set; }


    void Awake()
    {
        cameraController = GetComponent<CameraController>();
        selectionManager = GetComponent<SelectionManager>();
    }

    public async Task ToState(PlayerState toState, PlayerCameraState toCamera, PlayerRequestState toRequest)
    {
        Debug.Log("Start Transition");
        _currentState = PlayerState.Transitioning;
        _currentRequestState = PlayerRequestState.None;
        selectionManager.UpdateSelectables(toState, toCamera, toRequest);
        await cameraController.MoveCamera(toCamera); // This takes multiple frames to conclude // To Wouter: maybe multiple instances of this since both use the same camera? (is it easier to give each player their own camera? Dunno?)
        _currentState = toState;
        _currentCameraState = toCamera;
        _currentRequestState = toRequest;
        Debug.Log($"Change state to {_currentState}, {_currentCameraState}, {_currentRequestState}");
    }

    public bool IsHoverAllowed()
    {
        return _currentState != PlayerState.Transitioning || _currentState == PlayerState.WatchingGame;
    }
}
