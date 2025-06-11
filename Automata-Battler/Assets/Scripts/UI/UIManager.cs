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
    [SerializeField] TextMeshProUGUI p1CommanderHealthText; // Note: NO LONGER SET FROM INSPECTOR
    [SerializeField] TextMeshProUGUI p2CommanderHealthText;

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

    [Header("Your Turn Flash Screen")]
    [SerializeField] GameObject yourTurnScreen;
    [SerializeField] Image yourTurnScreenBG;
    [SerializeField] TextMeshProUGUI yourTurnText;
    [SerializeField] float flashDuration = 1.0f;
    [SerializeField] int flashIncrements = 30;

    private bool startUp = true; // to prevent certain sound effects from playing on startup

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

        if(Player.Instance.playerId == 1) // Set "YOUR TURN!" text popup to the correct color
            yourTurnText.color = p1Color;
        else if (Player.Instance.playerId == 2)
            yourTurnText.color = p2Color;
        yourTurnScreen.SetActive(false);

        ShowTutorialScreen();
        SetButtonActive(prevButton, false);
        SetButtonActive(nextButton, true);
        startUp = false;
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
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.Error);
        if (!alreadyAnimating)
            StartCoroutine("ErrorEffect", manaText);
    }


    /* COMMANDER HEALTH */
    public void InitializeCommanderHealthText(ulong playerId)
    {
        if(playerId == 1)
        {
            p1CommanderHealthText = GameObject.Find("YourCommanderHealthText").GetComponent<TextMeshProUGUI>();
            p2CommanderHealthText = GameObject.Find("EnemyCommanderHealthText").GetComponent<TextMeshProUGUI>();
        }
        else if (playerId == 2)
        {
            p1CommanderHealthText = GameObject.Find("EnemyCommanderHealthText").GetComponent<TextMeshProUGUI>();
            p2CommanderHealthText = GameObject.Find("YourCommanderHealthText").GetComponent<TextMeshProUGUI>();
        }
    }

    public void UpdateCommanderHealthText(ulong playerId, int health)
    {
        if (playerId == 1)
        {
            p1CommanderHealthText.text = string.Format("♡ {0}/10", health);
            if (!alreadyAnimating)
                StartCoroutine("ErrorEffect", p1CommanderHealthText);
        }
        else
        {
            p2CommanderHealthText.text = string.Format("♡ {0}/10", health);
            if (!alreadyAnimating)
                StartCoroutine("ErrorEffect", p2CommanderHealthText);
        }
    }


    /* TURN INDICATOR */
    public void ChangeTurnIndicator(ulong playerId)
    {
        if (playerId == 1) // Blue player
        {
            turnText.text = "Current Player: Blue";
            turnTextColor = p1Color;
        }
        else if (playerId == 2) // Orange player
        {
            turnText.text = "Current Player: Orange";
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
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.Error);
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
            SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.YouWin);
        }
        else // Other player won; you lose
        {
            winLoseText.text = "YOU LOSE...";
            SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.YouLose);
        }
        // Stop playing music / switch to win/lose outro
    }

    public void TriggerRematch()
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.ButtonClick);
        Referee.Instance.RematchRpc();
    }


    /* TUTORIAL SCREEN */
    public void ShowTutorialScreen()
    {
        if(!startUp) // Don't play when screen is shown on startup
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
        {
            SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.Error);
            return;
        }
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
        {
            SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.Error);
            return;
        }
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

    /* YOUR TURN FLASH SCREEN */
    public void FlashYourTurnScreen()
    {
        yourTurnScreen.SetActive(true);
        StartCoroutine(FlashYourTurnScreenCoroutine());
    }

    IEnumerator FlashYourTurnScreenCoroutine()
    {
        // Increase, then decrease, opacity of your turn background and text.
        Color bgColor = yourTurnScreenBG.color;
        Color textColor = yourTurnText.color;

        for (int i = 0; i <= flashIncrements; i++)
        {
            bgColor.a = Mathf.Lerp(0f, 1f, YourTurnFlashCurve((float)i / (float)flashIncrements));
            textColor.a = Mathf.Lerp(0f, 1f, YourTurnFlashCurve((float)i / (float)flashIncrements));
            yourTurnScreenBG.color = bgColor;
            yourTurnText.color = textColor;

            yield return new WaitForSeconds(flashDuration / flashIncrements);
        }
        yourTurnScreen.SetActive(false);
        yield break;
    }

    // Custom curve for making the your turn flash screen show up at full opacity for longer
    // The return value linearly goes from 0 to 1 between t=0 and t=t1, then stays at 1 between t=t1 and t=t2, and linearly goes back down to 0 between t=t2 and t=1.
    private float t1 = 0.2f; 
    private float t2 = 0.8f;
    private float YourTurnFlashCurve(float t)
    {
        if (t < t1)
            return (1 / t1) * t;
        else if (t > t2)
            return 1 - (1 / (1-t2)) * (t - t2);
        else
            return 1;
    }
}
