using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class I_TakeDamage : MonoBehaviour
{
    public static async Task Execute(Card card, int damageAmount)
    {
        Debug.Log($"Executing Instruction: I_TakeDamage ({card}, {damageAmount})");
        Board board = FindFirstObjectByType<Board>();
        Referee referee = FindFirstObjectByType<Referee>();

        card.SetHealthRpc(math.max(0, card._health - damageAmount));

        await Animate(card);

        if (card._health == 0)
            await I_Die.Execute(card);
    }

    public static string GetVisual()
    {
        return "X";
    }

    public static async Task Animate(Card card)
    {
        Transform transform = card.transform;
        float elapsed = 0f;
        float duration = 0.5f;
        Quaternion start = transform.rotation;
        Quaternion target = Quaternion.Euler(start.eulerAngles + new Quaternion(0, 0.3826834f, 0, 0.9238795f).eulerAngles);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * (6.75f + t * (-13.5f + t * 6.75f)); // Easing Jolt
            transform.rotation = Quaternion.Slerp(start, target, t);
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
        transform.rotation = start;
    }
}