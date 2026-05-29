using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the start-screen overlay that lives inside the gameplay scene.
///
/// Flow:
///   - On play, the menu shows and the game is paused (Time.timeScale = 0,
///     cursor unlocked) so the player reads the menu before anything moves.
///   - Start  -> hides the menu, resumes time, locks the cursor for FPS play.
///   - Settings -> swaps the main buttons for the settings panel.
///   - Quit   -> closes the application (and stops Play mode in the editor).
///
/// This is a custom script written for the assignment. It demonstrates
/// UI button events, conditional state (which panel is shown), and a
/// volume setting persisted with PlayerPrefs.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Root object holding the Start / Settings / Quit buttons.")]
    [SerializeField] private GameObject mainPanel;

    [Tooltip("Root object holding the volume slider and Back button.")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings")]
    [Tooltip("Slider that controls master volume (0..1).")]
    [SerializeField] private Slider volumeSlider;

    // PlayerPrefs key so the chosen volume is remembered between sessions.
    private const string VolumeKey = "MasterVolume";

    private void Start()
    {
        // Load saved volume (default to full) and apply it immediately.
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = savedVolume;
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            // React to slider changes at runtime.
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Show the menu and pause the game until the player presses Start.
        ShowMenu();
    }

    /// <summary>Display the menu, pause gameplay, and free the cursor.</summary>
    public void ShowMenu()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;                 // freeze gameplay
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Hook this to the Start button. Begins gameplay.</summary>
    public void StartGame()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;                 // resume gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Hook this to the Settings button. Shows the settings panel.</summary>
    public void OpenSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    /// <summary>Hook this to the settings Back button. Returns to the menu.</summary>
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    /// <summary>Hook this to the volume slider. Sets and saves master volume.</summary>
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    /// <summary>Hook this to the Quit button.</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
