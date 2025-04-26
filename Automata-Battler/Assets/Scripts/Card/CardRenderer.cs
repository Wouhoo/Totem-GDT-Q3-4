using UnityEngine;
using TMPro;
using System.Linq;

[RequireComponent(typeof(Card))]
[DisallowMultipleComponent]
public class CardRenderer : MonoBehaviour
{
    private Card card;
    [SerializeField] private TextMeshProUGUI textCost;
    [SerializeField] private TextMeshProUGUI textHealth;
    [SerializeField] private TextMeshProUGUI textDamage;
    [SerializeField] private TextMeshProUGUI textInitiative;
    [SerializeField] private TextMeshProUGUI textInstructions;

    void Awake()
    {
        card = GetComponent<Card>();
    }

    public void Render_All()
    {
        Render_Cost();
        Render_Health();
        Render_Damage();
        Render_Initiative();
        Render_Instructions();
    }

    public void Render_Cost()
    {
        textCost.text = $"$:{card._cost}";
    }

    public void Render_Health()
    {
        textHealth.text = $"♡:{card._health}";
    }

    public void Render_Damage()
    {
        textDamage.text = $"X:{card._damage}";
    }

    public void Render_Initiative()
    {
        textInitiative.text = $"Init: {card._initiative}";
    }

    public void Render_Instructions()
    {
        textInstructions.text = string.Join(" ", card._instructions.Select(instruction =>
        {
            return instruction.GetVisual();
        }));
    }
}
