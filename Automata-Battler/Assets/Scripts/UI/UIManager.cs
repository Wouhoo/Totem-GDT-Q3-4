using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // Class to manage player UI (e.g. mana text and commander health)
    public static UIManager Instance { get; private set; }

    [Header("Error effect")]
    [SerializeField] Color errorColor = Color.red;
    [SerializeField] float errorDuration = 0.8f;
    [SerializeField] int increments = 20;
    private bool alreadyAnimating;
    // Note: this^ bool is shared by all text elements that can have an error animation, meaning only one can be animating at a time.
    // This is fine for now since only one should be able to happen anyway, but maybe refactor later if we add this effect to more things.

    [Header("Mana text")]
    [SerializeField] TextMeshProUGUI manaText;

    [Header("Turn indicator")]
    [SerializeField] TextMeshProUGUI turnText;
    [SerializeField] Material p1Material;
    [SerializeField] Material p2Material;
    [SerializeField] Color executionColor;
    private Color turnTextColor;
    private Color p1Color;
    private Color p2Color;

    [Header("Commander Health Text")]
    [SerializeField] TextMeshProUGUI commanderHealthText;

    [Header("End Turn Button")]
    [SerializeField] TextMeshProUGUI endTurnText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        p1Color = p1Material.color;
        p2Color = p2Material.color;

        UpdateManaText(3);
        ChangeTurnIndicator(1);
    }

    /* MANA TEXT */
    public void UpdateManaText(int mana)
    {
        manaText.text = string.Format("$ {0}/3", mana);
    }

    public void PlayNotEnoughManaEffect()
    {
        if (!alreadyAnimating)
            StartCoroutine("ErrorEffect", manaText);
    }


    /* TURN INDICATOR */
    public void ChangeTurnIndicator(ulong playerId)
    {
        if (playerId == 1) // Orange player
        {
            turnText.text = "Current Player: Orange";
            turnTextColor = p1Color;
        }
        else if (playerId == 2) // Blue player
        {
            turnText.text = "Current Player: Blue";
            turnTextColor = p2Color;
        }
        else // No player active; executing cards
        {
            turnText.text = "Executing cards...";
            turnTextColor = executionColor;
        }
        turnText.color = turnTextColor;
    }

    public void PlayNotYourTurnEffect()
    {
        if (!alreadyAnimating)
            StartCoroutine("ErrorEffect", turnText);
    }


    /* COMMANDER HEALTH */
    public void UpdateCommanderHealthText(int health)
    {
        commanderHealthText.text = string.Format("♡ {0}/10", health);
        if (!alreadyAnimating)
            StartCoroutine("ErrorEffect", commanderHealthText);
    }


    /* COOL EFFECTS */
    IEnumerator ErrorEffect(TextMeshProUGUI textToHighlight)
    {
        // Instantly change the color to red, then change it back to the starting color over time
        alreadyAnimating = true;
        Color currColor = textToHighlight.color;
        textToHighlight.color = errorColor;
        for (int i = 0; i <= increments; i++)
        {
            textToHighlight.color = Color.Lerp(errorColor, currColor, (float)i / (float)increments);
            yield return new WaitForSeconds(errorDuration / increments);
        }
        alreadyAnimating = false;
        yield break;
    }
}
