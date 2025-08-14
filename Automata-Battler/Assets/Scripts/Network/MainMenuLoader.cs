using UnityEngine;
using UnityEngine.SceneManagement;

// Script to load the main menu from the launch scene.
// Previously, reloading the main menu after finishing a match created a duplicate NetworkManager, causing all sorts of problems;
// the launch scene exists to make sure there is never a duplicate NetworkManager.
public class MainMenuLoader : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("Scenes/MainMenu"); // Load the main menu. That's it. That's literally all this thing does. That's its entire purpose in life
    }
}
