using UnityEngine;
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

    private AudioSource audioPlayer;

    public enum BGMTheme
    {
        None, //hack for uhh when you DON'T have a theme playing (i.e. at game start and you want the menu)
        Menu,
        Battle,
        Winning,
        Losing
    }
    [SerializeField] private BGMTheme currentTheme;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioPlayer = GetComponent<AudioSource>();

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
                audioPlayer.clip = themeLookup[theme].loopClip;
                audioPlayer.Play();
                currentTheme = theme;
            }
        }
        catch
        {
            Debug.LogWarning(string.Format("Can't find theme {0}", theme));
        }
    }

    public void StopPlaying()
    {
        audioPlayer.Stop();
    }

    public void UpdateVolume(float volume)
    {
        audioPlayer.volume = volume;
    }
}
