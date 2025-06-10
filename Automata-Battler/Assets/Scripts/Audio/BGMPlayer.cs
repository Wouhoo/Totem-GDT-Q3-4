using UnityEngine;
using System.Collections.Generic;
using static SFXPlayer;

public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer Instance { get; private set; }

    [SerializeField] List<AudioClip> themes;

    private AudioSource audioPlayer;

    public enum BGMTheme
    {
        Menu,
        Battle,
        Winning,
        Losing
    }
    private BGMTheme currentTheme;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioPlayer = GetComponent<AudioSource>();
    }

    public void PlayBGMTheme(BGMTheme theme)
    {
        try
        {
            if(theme != currentTheme) // Don't replay the theme when it's already playing
            {
                audioPlayer.clip = themes[(int)theme];
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
}
