using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CardRenderer : MonoBehaviour
{
    [SerializeField] private GameObject move;
    [SerializeField] private Card card;


    void Awake()
    {
        card = gameObject.GetComponent<Card>();
    }

    // Update is called once per frame
    public void RenderInstructions()
    {

    }
}
