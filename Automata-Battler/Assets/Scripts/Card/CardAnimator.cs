using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor.ProjectWindowCallback;

[RequireComponent(typeof(Card))]
[DisallowMultipleComponent]
public class CardAnimator : MonoBehaviour
{

    [SerializeField] private float _duration;

    // Instruction based animations

    public async Task Die()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            // TODO
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
    }

    public async Task Move_asJump(HexCoordinates toPos)
    {
        Vector3 target = HexCoordinates.ToWorldPosition(toPos);
        await ParabolicLerpPosTo(target, new Vector3(0, 5, 0), 0.5f);
    }

    public async Task Move_asJump_FAIL(HexCoordinates toPos)
    {
        Vector3 start = transform.position;
        Vector3 target = HexCoordinates.ToWorldPosition(toPos) + new Vector3(0, 4, 0);
        await ParabolicLerpPosTo(target, new Vector3(0, 5, 0), 0.25f);
        await ParabolicLerpPosTo(start, new Vector3(0, 5, 0), 0.25f);
    }

    public async Task Move_asSlide(HexCoordinates toPos)
    {
        Vector3 target = HexCoordinates.ToWorldPosition(toPos);
        await ParabolicLerpPosTo(target, new Vector3(0, 5, 0), 0.5f);
    }

    public async Task Attack_asJump_1(HexCoordinates toPos)
    {
        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            // TODO
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
        transform.position = HexCoordinates.ToWorldPosition(toPos);
    }
    public async Task Attack_asJump_2(HexCoordinates toPos)
    {
        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            // TODO
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
        transform.position = HexCoordinates.ToWorldPosition(toPos);
    }

    public async Task TakeDamage()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Quaternion start = transform.rotation;
        Quaternion target = Quaternion.Euler(start.eulerAngles + new Quaternion(0, 0.3826834f, 0, 0.9238795f).eulerAngles);

        while (elapsed < duration)
        {
            float t = Easing_Jolt(elapsed / duration);
            transform.rotation = Quaternion.Slerp(start, target, t);
            elapsed += Time.deltaTime;
            await Task.Yield();
        }

        transform.rotation = start;
    }

    // UTILITY FUNCTIONS
    private async Task LerpPosTo(Vector3 destination, float duration)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, destination, elapsed / duration);
            elapsed += Time.deltaTime;
            await Task.Yield(); // Crucial to yield control per frame
        }

        transform.position = destination;
    }

    private async Task ParabolicLerpPosTo(Vector3 destination, Vector3 midpointMax, float duration)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 relativeEnd = destination - start;

        while (elapsed < duration)
        {
            float t = Easing_Smooth(elapsed / duration);
            transform.position = start + relativeEnd * t + midpointMax * 4 * t * (1 - t);
            elapsed += Time.deltaTime;
            await Task.Yield(); // Crucial to yield control per frame
        }

        transform.position = destination;
    }

    // Easing functions are polynomial maps f: [0,1] -> [0,1], such that im(f) = [0,1]

    private float Easing_Smooth(float t)
    {
        // Start: 0
        return 3 * t * t - 2 * t * t * t;
        // End: 1
    }

    private float Easing_Jolt(float t)
    {
        // Start: 0
        return t * (6.75f + t * (-13.5f + t * 6.75f));
        // End: 0
    }





}
