using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStateManager))]
[DisallowMultipleComponent]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform boardViewTarget;
    [SerializeField] private Transform handViewTarget;
    [SerializeField] private float transitionDuration = 1.0f;
    private PlayerStateManager playerStateManager;

    private Coroutine currentTransition;

    void Awake()
    {
        playerStateManager = GetComponent<PlayerStateManager>();
    }

    public void MoveCamera(PlayerState toState)
    {
        if (currentTransition != null) StopCoroutine(currentTransition);
        if (toState == PlayerState.ViewingHand)
            StartCoroutine(TransitionTo(handViewTarget, toState));
        else
            StartCoroutine(TransitionTo(boardViewTarget, toState));
    }

    private IEnumerator TransitionTo(Transform target, PlayerState targetState)
    {
        playerStateManager.SetState(PlayerState.Transitioning);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        // Return to appropriate state
        playerStateManager.SetState(targetState);

        currentTransition = null;
    }
}