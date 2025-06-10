using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using UnityEngine.UI;
using Unity.Netcode;

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

    [Header("Pause Screen")]
    [SerializeField] GameObject pauseScreen;
    private bool paused;

    [Header("Game End Screen")]
    [SerializeField] GameObject gameEndScreen;
    [SerializeField] TextMeshProUGUI winLoseText;

    [Header("Tutorial Screen")]
    [SerializeField] GameObject tutorialScreen;
    [SerializeField] Image tutorialImage;
    [SerializeField] Image prevButton;
    [SerializeField] Image nextButton;
    [SerializeField] Sprite[] tutorialSlides;
    private int currentSlide = 0;

    // If you were looking for the game end screen, that's done on the GameEndManager
    // (that has to be a NetworkObject, which is not needed for the rest of the UI, hence why we handle it separately)

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        p1Color = p1Material.color;
        p2Color = p2Material.color;
    }

    private void Start()
    {
        UpdateManaText(3);
        ChangeTurnIndicator(1);

        pauseScreen.SetActive(false);
        ShowTutorialScreen();
        SetButtonActive(prevButton, false);
        SetButtonActive(nextButton, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
                Pause();
            else
                Unpause();
        }
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


    /* COMMANDER HEALTH */
    public void UpdateCommanderHealthText(int health)
    {
        commanderHealthText.text = string.Format("♡ {0}/10", health);
        if (!alreadyAnimating)
            StartCoroutine("ErrorEffect", commanderHealthText);
    }


    /* TURN INDICATOR */
    public void ChangeTurnIndicator(ulong playerId)
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.TurnChange);
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


    /* PAUSE SCREEN */
    public void Pause()
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.ButtonClick);
        pauseScreen.SetActive(true);
        paused = true;
        SelectionManager.Instance.inputAllowed = false;
        //Time.timeScale = 0; // W: This is fine for the client; however, if Time.timeScale = 0 on the server, the client's inputs won't be processed.
        // For now I'm disabling it; this means pausing technically doesn't actually pause the game, but it's turn-based anyway, so who really cares
    }

    public void Unpause()
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.ButtonClick);
        tutorialScreen.SetActive(false);
        pauseScreen.SetActive(false);
        paused = false;
        SelectionManager.Instance.inputAllowed = true;
        //Time.timeScale = 1;
    }

    public void BackToMenu()
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.ButtonClick);
        Referee.Instance.BackToMenuServerRpc();
    }


    /* GAME END SCREEN */
    public void ShowEndScreen(ulong winningPlayer)
    {
        gameEndScreen.SetActive(true);
        SelectionManager.Instance.inputAllowed = false;
        if(winningPlayer == Player.Instance.playerId) // This player won
        {
            winLoseText.text = "YOU WIN!";
            // Play winning theme
        }
        else // Other player won; you lose
        {
            winLoseText.text = "YOU LOSE...";
            // Play losing theme
        }
    }

    public void TriggerRematch()
    {
        Referee.Instance.RematchRpc();
    }


    /* TUTORIAL SCREEN */
    public void ShowTutorialScreen()
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.ButtonClick);
        pauseScreen.SetActive(false);
        tutorialScreen.SetActive(true);
        paused = true;
        SelectionManager.Instance.inputAllowed = false;
    }

    public void NextTutorialSlide()
    {
        // Do nothing if this is the final slide
        if (currentSlide == tutorialSlides.Length - 1)
            return;
        // If not, go to next slide
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.ButtonClick);
        currentSlide++;
        tutorialImage.sprite = tutorialSlides[currentSlide];
        // If we were at the first slide, set the prev button active again
        if (currentSlide == 1)
            SetButtonActive(prevButton, true);
        // If we're now at the final slide, set the next button inactive
        if (currentSlide == tutorialSlides.Length - 1)
            SetButtonActive(nextButton, false);
    }

    public void PrevTutorialSlide()
    {
        // Do nothing if this is the first slide
        if (currentSlide == 0)
            return;
        // If not, go to prev slide
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.ButtonClick);
        currentSlide--;
        tutorialImage.sprite = tutorialSlides[currentSlide];
        // If we were at the last slide, set the prev button active again
        if (currentSlide == tutorialSlides.Length - 2)
            SetButtonActive(nextButton, true);
        // If we're now at the first slide, set the prev button inactive
        if (currentSlide == 0)
            SetButtonActive(prevButton, false);
    }

    private void SetButtonActive(Image button, bool active)
    {
        // Visually make button look active (fully opaque) or inactive (slightly transparent)
        // Note: doesn't actually disallow you from pressing the button, that's handled by the functions above
        Color color = button.color;
        if (active)
            color.a = 1.0f;
        else
            color.a = 0.7f;
        button.color = color;
    }
}
