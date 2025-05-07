using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<GameObject> cards = new List<GameObject>();

    private Referee referee;

    void Awake()
    {
        referee = FindFirstObjectByType<Referee>();
    }

    public AbstractCard DrawCard(Player player)
    {
        int index = UnityEngine.Random.Range(0, cards.Count);
        GameObject cardObject = Instantiate(cards[index], transform); // To Wouter: other transform?
        AbstractCard card = cardObject.GetComponent<AbstractCard>();
        card.Set_Owner(player); // To Wouter: change to players int
        if (player != referee._player1 && card is Card card1)
            card1.Rotate(3); // align with player view (temp sorta?)
        return card; // To Wouter: return to the correct player somehow?
    }
}
