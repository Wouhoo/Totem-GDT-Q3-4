using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform boardViewTarget;
    [SerializeField] private Transform handViewTarget;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float transitionDuration = 1.0f;
    private PlayerStateManager playerStateManager;

    void Awake()
    {
        playerStateManager = GetComponent<PlayerStateManager>();
    }

    public async Task MoveCamera(PlayerState toState)
    {
        if (toState == PlayerState.ViewingHand)
            await TransitionTo(handViewTarget);
        else
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