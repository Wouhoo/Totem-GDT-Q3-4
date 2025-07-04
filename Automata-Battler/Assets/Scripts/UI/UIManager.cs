﻿using UnityEngine;
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
    [SerializeField] Sprite p1EnergyIcon;
    [SerializeField] Sprite p2EnergyIcon;

    [Header("Turn indicator")]
    [SerializeField] GameObject turnIndicatorArrow;
    private Renderer turnIndicatorMesh;
    [SerializeField] TextMeshProUGUI terminalCurrentPText;
    [SerializeField] Material p1Material;
    [SerializeField] Material p2Material;
    [SerializeField] Color executionColor;
    [SerializeField] float rotationDuration = 1.5f;
    [SerializeField] int rotationIncrements = 60;
    private Color turnTextColor;
    private Color p1Color;
    private Color p2Color;

    [Header("Commander Health Text")]
    [SerializeField] TextMeshProUGUI terminalP1CommanderHealthText;
    [SerializeField] TextMeshProUGUI terminalP2CommanderHealthText;
    [SerializeField] Sprite p1CommanderHealthIcon;
    [SerializeField] Sprite p2CommanderHealthIcon;
    private TextMeshProUGUI boardP1CommanderHealthText; 
    private TextMeshProUGUI boardP2CommanderHealthText;

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
    [SerializeField] GameObject waitingForOpponentScreen;
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
    private bool playerReady = false;

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
        turnIndicatorMesh = turnIndicatorArrow.GetComponent<Renderer>();
    }

    private void Start()
    {
        UpdateManaText(3);
        ChangeTurnIndicator(1);
        pauseScreen.SetActive(false);
        waitingForOpponentScreen.SetActive(false);

        if(Player.Instance.playerId == 1) // Set UI texts to the correct color
        {
            yourTurnText.color = p1Color;
            manaText.color = p1Color;
        }
        else if (Player.Instance.playerId == 2)
        {
            yourTurnText.color = p2Color;
            manaText.color = p2Color;
        }
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
        manaText.text = string.Format("{0}/3", mana);
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
            boardP1CommanderHealthText = GameObject.Find("YourCommanderHealthText").GetComponent<TextMeshProUGUI>();
            boardP2CommanderHealthText = GameObject.Find("EnemyCommanderHealthText").GetComponent<TextMeshProUGUI>();
            GameObject.Find("YourCommanderHealthIcon").GetComponent<Image>().sprite = p1CommanderHealthIcon;
            GameObject.Find("EnemyCommanderHealthIcon").GetComponent<Image>().sprite = p2CommanderHealthIcon;
            GameObject.Find("EnergyIcon").GetComponent<Image>().sprite = p1EnergyIcon;
        }
        else if (playerId == 2)
        {
            boardP1CommanderHealthText = GameObject.Find("EnemyCommanderHealthText").GetComponent<TextMeshProUGUI>();
            boardP2CommanderHealthText = GameObject.Find("YourCommanderHealthText").GetComponent<TextMeshProUGUI>();
            GameObject.Find("EnemyCommanderHealthIcon").GetComponent<Image>().sprite = p1CommanderHealthIcon;
            GameObject.Find("YourCommanderHealthIcon").GetComponent<Image>().sprite = p2CommanderHealthIcon;
            GameObject.Find("EnergyIcon").GetComponent<Image>().sprite = p2EnergyIcon;
        }
    }

    public void UpdateCommanderHealthText(ulong playerId, int health)
    {
        if (playerId == 1)
        {
            boardP1CommanderHealthText.text = string.Format("{0}", health);
            terminalP1CommanderHealthText.text = string.Format("{0}", health);
            if (!alreadyAnimating)
                StartCoroutine("ErrorEffect", boardP1CommanderHealthText); // Terminal text doesn't need to animate since you can't watch the terminal during execution anyway
        }
        else
        {
            boardP2CommanderHealthText.text = string.Format("{0}", health);
            terminalP1CommanderHealthText.text = string.Format("{0}", health);
            if (!alreadyAnimating)
                StartCoroutine("ErrorEffect", boardP2CommanderHealthText);
        }
    }


    /* TURN INDICATOR */
    private float playerRotationTarget = 0f;     // y-angle corresponding to the arrow being rotated towards the player (x- and z-rotation remain 0)
    private float opponentRotationTarget = 180f; // same but for rotation towards opponent
    private float boardRotationTarget = 90f;     // same but for rotation towards board
    private float targetRotation;
    Color targetColor = Color.gray;

    public void ChangeTurnIndicator(ulong currPlayerId)
    {
        // Determine position to rotate the target to
        if (currPlayerId == 0)                             // Execution phase; point towards board
            targetRotation = boardRotationTarget;
        else if (currPlayerId == Player.Instance.playerId) // This player's turn; point towards player
            targetRotation = playerRotationTarget;
        else                                               // Opponent's turn: point to other side of board
            targetRotation = opponentRotationTarget;

        // Determine color to give the arrow. Also change turn indicator on player's terminal
        if (currPlayerId == 1)      // Player 1 (blue)
        {
            targetColor = p1Color;
            terminalCurrentPText.text = "> Blue";
            terminalCurrentPText.color = p1Color;
        }  
        else if (currPlayerId == 2) // Player 2 (orange)
        {
            targetColor = p2Color;
            terminalCurrentPText.text = "> Orange";
            terminalCurrentPText.color = p2Color;
        }
        else                        // No player active; executing cards
        {
            targetColor = executionColor;
            terminalCurrentPText.text = "> Executing";
            terminalCurrentPText.color = executionColor;
        }
        StartCoroutine("RotateIndicatorArrow");
    }

    IEnumerator RotateIndicatorArrow()
    {
        // Change turn indicator arrow's rotation and color over time towards the correct target
        float startRotation = turnIndicatorArrow.transform.localRotation.eulerAngles.y;
        float currRotation = startRotation;
        //Debug.Log(string.Format("ROTATION START {0}, TARGET {1}", startRotation, targetRotation));
        Color startColor = turnIndicatorMesh.material.color;
        Color currColor = startColor;

        for (int i = 0; i <= rotationIncrements; i++)
        {
            currRotation = Mathf.Lerp(startRotation, targetRotation, (float)i / (float)rotationIncrements);
            currColor = Color.Lerp(startColor, targetColor, (float)i / (float)rotationIncrements);

            turnIndicatorArrow.transform.localRotation = Quaternion.Euler(0, currRotation, 0);
            turnIndicatorMesh.material.color = currColor;

            yield return new WaitForSeconds(rotationDuration / rotationIncrements);
        }
        yield break;
    }

    public void PlayNotYourTurnEffect()
    {
        SFXPlayer.Instance.PlaySoundEffect(SFXPlayer.SoundEffect.Error);
        if (!alreadyAnimating)
            StartCoroutine("ErrorEffect", terminalCurrentPText);
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

    public void TutorialDone()
    {
        if (!playerReady)
        {
            Referee.Instance.PlayerReadyRpc(Player.Instance.playerId); // Let the server referee know that this player is ready
            waitingForOpponentScreen.SetActive(true);
            playerReady = true;
        }
    }

    public void HideWaitingForOpponentScreen()
    {
        waitingForOpponentScreen.SetActive(false);
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
