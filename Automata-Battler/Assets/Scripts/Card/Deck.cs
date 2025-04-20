using UnityEngine;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private List<GameObject> deck = new List<GameObject>();


    void Awake()
    {
        deck = new List<GameObject>(prefabs);
    }

    public Card DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning("Error: Deck is empty!");
            return null;
        }

        int index = Random.Range(0, deck.Count);
        GameObject drawnObject = Instantiate(deck[index]);
        deck.RemoveAt(index);
        return drawnObject.GetComponent<Card>();
    }
}
