using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PersistentData : MonoBehaviour
{
    // Storage class for data that needs to persist between scenes (currently only the music/sfx volume levels)
    public static PersistentData Instance { get; private set; }

    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.5f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        // When new scene is loaded, set sliders to the stored values
        Slider musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        Slider sfxSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;
        // and listen to their value change events
        musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        sfxSlider.onValueChanged.AddListener(UpdateSFXVolume);
        // Also set BGM and SFX player volumes with stored values
        BGMPlayer.Instance.UpdateVolume(musicVolume);
        SFXPlayer.Instance.UpdateVolume(sfxVolume);
    }

    public void UpdateMusicVolume(float volume)
    {
        musicVolume = volume;
        BGMPlayer.Instance.UpdateVolume(musicVolume);
    }

    public void UpdateSFXVolume(float volume)
    {
        sfxVolume = volume;
        SFXPlayer.Instance.UpdateVolume(sfxVolume);
    }
}
