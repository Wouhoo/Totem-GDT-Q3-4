using UnityEngine;

public class Button_Play : MonoBehaviour, Interactable
{
    private Referee referee;
    void Start()
    {
        referee = FindFirstObjectByType<Referee>();
    }

    public void OnSelect()
    {
        referee.EndTurn();
    }
    public void OnDeselect() { }
    public void OnHover() { }
    public void OnDehover() { }

}
