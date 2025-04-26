using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor.ProjectWindowCallback;

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


    /*
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
        */


}
