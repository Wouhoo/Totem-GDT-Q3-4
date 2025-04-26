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
    private Player player;

    public PlayerState _currentState { get; private set; }


    void Awake()
    {
        player = GetComponent<Player>();
        cameraController = GetComponent<CameraController>();
        selectionManager = GetComponent<SelectionManager>();
    }

    public async Task ToState(PlayerState toState)
    {
        _currentState = PlayerState.Transitioning;
        selectionManager.UpdateSelectables(toState);
        await cameraController.MoveCamera(toState); // This takes multiple frames to conclude
        _currentState = toState;
        Debug.Log($"{player} change state to {_currentState}");
    }

    public bool IsHoverAllowed()
    {
        return _currentState == PlayerState.ViewingBoard || _currentState == PlayerState.ViewingHand || _currentState == PlayerState.PlacingCard;
    }
}
