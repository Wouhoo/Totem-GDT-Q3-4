using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform boardViewTarget; // DO NOT SET FROM THE INSPECTOR! (SerializeField is only as a check if they are correct)
    [SerializeField] private Transform handViewTarget;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float transitionDuration = 1.0f;

    public void InitializeCamera(ulong playerId)
    {
        // Get correct camera targets based on Player ID
        if (playerId == 1)
        {
            boardViewTarget = GameObject.Find("Board Camera Target 1").GetComponent<Transform>(); // There might a better way to do this idk
            handViewTarget = GameObject.Find("Hand Camera Target 1").GetComponent<Transform>();
        }
        else
        {
            boardViewTarget = GameObject.Find("Board Camera Target 2").GetComponent<Transform>();
            handViewTarget = GameObject.Find("Hand Camera Target 2").GetComponent<Transform>();
        }
        cameraTransform.position = handViewTarget.position; // Move camera to correct position at game start
        cameraTransform.rotation = handViewTarget.rotation;
    }

    public async Task MoveCamera(PlayerCameraState toState)
    {
        if (toState == PlayerCameraState.ViewingHand)
            await TransitionTo(handViewTarget);
        else if (toState == PlayerCameraState.ViewingBoard)
            await TransitionTo(boardViewTarget);
    }

    private async Task TransitionTo(Transform target)
    {
        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float t = 0f;

        while (t < 1f)
        {
            cameraTransform.position = Vector3.Lerp(startPos, endPos, t);
            cameraTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
            t += Time.deltaTime / transitionDuration;
            await Task.Yield();
        }

        cameraTransform.position = endPos;
        cameraTransform.rotation = endRot;
    }
}