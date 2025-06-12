using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static SFXPlayer;
using System.Linq; //dictionary

public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer Instance { get; private set; }

    [SerializeField] List<AudioClip> themes;

    [SerializeField] private List<BGMThemeSO> themeAssets;
    //dictionaries are not serializable, so that is why we have to build it on the fly
    private Dictionary<BGMTheme, BGMThemeSO> themeLookup;

    [SerializeField] private AudioSource introPlayer;
    [SerializeField] private AudioSource loopPlayer;

    [SerializeField] private float fadeOutDuration = 1.0f;

    public enum BGMTheme
    {
        None, //hack for uhh when you DON'T have a theme playing (i.e. at game start and you want the menu)
        Menu,
        Battle,
        Winning,
        Losing,
        Tutorial
    }
    [SerializeField] private BGMTheme currentTheme;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //audioPlayer = GetComponent<AudioSource>();

        //populate the dictionary
        themeLookup = themeAssets.ToDictionary(t => t.theme, t => t);
        Debug.Log(themeLookup.ToString());
    }
    


    public void PlayBGMTheme(BGMTheme theme)
    {
        Debug.Log("new theme attempted: " + theme);
        try
        {
            if (theme != currentTheme) // Don't replay the theme when it's already playing
            {
                //audioPlayer.clip = themes[(int)theme];
                var themeSO = themeLookup[theme];
                StartCoroutine(SwitchThemeWithFade(themeSO));
                currentTheme = theme;
            }
        }
        catch
        {
            Debug.LogWarning(string.Format("Can't find theme {0}", theme));
        }
    }

    private void PlayIntroAndLoop(BGMThemeSO theme)
    {
        // Stop previous
        introPlayer.Stop();
        loopPlayer.Stop();
      
        // Get timing right
        double batonPassTime = AudioSettings.dspTime + theme.introClip.length;

        // Play intro
        introPlayer.PlayOneShot(theme.introClip);
        introPlayer.SetScheduledEndTime(batonPassTime);
      
        // Schedule loop
        loopPlayer.clip = theme.loopClip;
        loopPlayer.loop = true;
        loopPlayer.PlayScheduled(batonPassTime);
    }

    //switch themes with fade-out
    private IEnumerator SwitchThemeWithFade(BGMThemeSO newTheme)
    {
        // Debug.Log("START Fade out");
        // Fade out both players
        yield return StartCoroutine(FadeOut(introPlayer, fadeOutDuration));
        yield return StartCoroutine(FadeOut(loopPlayer, fadeOutDuration));
        // Debug.Log("Fade out DONE");
        // Delay slightly to ensure clean stop
        yield return new WaitForSecondsRealtime(0.05f);

        // Debug.Log("STARTING NEW THEME");
        // Play new theme
        if (newTheme.introClip != null)
        {
            PlayIntroAndLoop(newTheme);
        }
        else
        {
            introPlayer.Stop();
            loopPlayer.Stop();
            loopPlayer.clip = newTheme.loopClip;
            loopPlayer.Play();
        }
    }

    //fade out coroutine for given audiosource
    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        if (source.isPlaying)//only if it is playing, else skip
        {
            float startVolume = source.volume;

            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
                yield return null;
            }

            source.Stop();
            source.volume = startVolume; //reset volume in case we reuse it
        }
    }

    public void StopPlaying()
    {
        introPlayer.Stop();
        loopPlayer.Stop();
    }

    public void UpdateVolume(float volume)
    {
        introPlayer.volume = volume;
        loopPlayer.volume = volume;
    }
}
