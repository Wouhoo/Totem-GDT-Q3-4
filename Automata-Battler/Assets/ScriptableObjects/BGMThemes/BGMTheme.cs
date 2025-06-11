using UnityEngine;

[CreateAssetMenu(fileName = "NewBGMTheme", menuName = "Scriptable Objects/BGM Theme")]
public class BGMThemeSO : ScriptableObject
{
    public BGMPlayer.BGMTheme theme;
    public AudioClip introClip;
    public AudioClip loopClip;
}