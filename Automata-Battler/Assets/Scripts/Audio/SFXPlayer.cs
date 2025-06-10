using UnityEngine;
using System.Collections.Generic;

public class SFXPlayer : MonoBehaviour
{
    public static SFXPlayer Instance { get; private set; }

    [SerializeField] List<AudioClip> soundEffects;
    private AudioSource audioPlayer;

    public enum SoundEffect
    {
        ButtonClick,
        TurnChange,
        Error,
        ChangeView,
        DrawCard,
        PlayCard,
        SelectCard,
        CardMove,
        CardAttack,
        CardRotate, // Can be the same as CardMove
        Damage,
        YouWin,
        YouLose
        // @Kerem add more here as required
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioPlayer = GetComponent<AudioSource>();
    }

    public void PlaySoundEffect(SoundEffect soundEffect)
    {
        // Play given sound effect. Can be called from anywhere where a sound effect is needed
        // (this does mean we can't have positional audio, but who fucking cares)
        try
        {
            AudioClip clip = soundEffects[(int)soundEffect];
            audioPlayer.PlayOneShot(clip);
        }
        catch
        {
            Debug.LogWarning(string.Format("Couldn't find clip corresponding to sound effect {0}", soundEffect));
        }
    }
}
