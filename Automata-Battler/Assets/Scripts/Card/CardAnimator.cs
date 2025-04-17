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

    public void Die()
    {

    }

    public void Move_asJump(HexCoordinates toPos)
    {
        transform.position = HexCoordinates.ToWorldPosition(toPos);
    }

    public void Move_asSlide(HexCoordinates toPos)
    {
        transform.position = HexCoordinates.ToWorldPosition(toPos);
    }

    public void Attack_asJump_1(HexCoordinates toPos)
    {
        transform.position = HexCoordinates.ToWorldPosition(toPos);
    }
    public void Attack_asJump_2(HexCoordinates toPos)
    {
        transform.position = HexCoordinates.ToWorldPosition(toPos);
    }

    public void TakeDamage()
    {

    }




}
