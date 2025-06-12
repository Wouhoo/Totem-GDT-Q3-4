using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;

public static class CardAnimator
{
    public static async Task Lerp_SlideTo(Transform transform, Vector3 target, float duration)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < duration)
        {
            float t = Easing_Smooth(elapsed / duration);
            transform.position = Vector3.Lerp(start, target, t);
            elapsed += Time.deltaTime;
            await Task.Yield(); // Crucial to yield control per frame
        }
        transform.position = target;
    }

    public static async Task Lerp_JumpTo(Transform transform, Vector3 target, float duration)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;
        Vector3 midpointMax = new Vector3(0, 2, 0);
        Vector3 relativeEnd = target - start;

        while (elapsed < duration)
        {
            float t = Easing_Smooth(elapsed / duration);
            transform.position = start + relativeEnd * t + midpointMax * 4 * t * (1 - t);
            elapsed += Time.deltaTime;
            await Task.Yield(); // Crucial to yield control per frame
        }
        transform.position = target;
    }

    // Easing functions are polynomial maps f: [0,1] -> [0,1], such that im(f) = [0,1]

    private static float Easing_Smooth(float t)
    {
        // Start: 0
        return 3 * t * t - 2 * t * t * t;
        // End: 1
    }

    private static float Easing_Jolt(float t)
    {
        // Start: 0
        return t * (6.75f + t * (-13.5f + t * 6.75f));
        // End: 0
    }

    // Kill me plz :(

    [Rpc(SendTo.Server)]
    public static async Task Card_FlyIn_Rpc(Transform transform, Vector3 targetPos, Quaternion targetRot, float cardScale)
    {
        // await Lerp_SlideTo(transform, transform.position + new Vector3(0, 100, 0), 0.2f);
        transform.position += new Vector3(0, 100, 0);
        transform.rotation = targetRot;
        transform.position = targetPos + new Vector3(0, 100, 0);
        transform.localScale = new Vector3(8 * cardScale, 1 * cardScale, 8 * cardScale);
        await Lerp_SlideTo(transform, targetPos, 0.5f);
    }
}
