using UnityEngine;

/// <summary>
/// Drives the start-screen overlay that lives inside the gameplay scene.
///
/// Flow:
///   - On play, the menu shows and the game is paused (Time.timeScale = 0,
///     cursor unlocked) so the player reads the title before anything moves.
///   - Start -> hides the menu, resumes time, locks the cursor for FPS play.
///
/// While the menu is up it also disables the player's look/movement scripts.
/// That matters because FirstPersonLook locks the cursor and runs every frame
/// regardless of Time.timeScale, which would otherwise steal the cursor and make
/// the menu buttons unclickable.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Root object holding the title image and the Start button.")]
    [SerializeField] private GameObject mainPanel;

    [Header("Player (disabled while the menu is up)")]
    [SerializeField] private FirstPersonLook playerLook;
    [SerializeField] private FirstPersonMovement playerMovement;

    private void Start()
    {
        // Show the menu and pause the game until the player presses Start.
        ShowMenu();
    }

    /// <summary>Display the menu, pause gameplay, free the cursor, freeze the player.</summary>
    public void ShowMenu()
    {
        if (mainPanel != null) mainPanel.SetActive(true);

        SetPlayerControlsEnabled(false);

        Time.timeScale = 0f;                 // freeze gameplay
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Hook this to the Start button. Begins gameplay.</summary>
    public void StartGame()
    {
        if (mainPanel != null) mainPanel.SetActive(false);

        Time.timeScale = 1f;                 // resume gameplay

        SetPlayerControlsEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Enable/disable the player's look + movement scripts.</summary>
    private void SetPlayerControlsEnabled(bool enabled)
    {
        if (playerLook != null) playerLook.enabled = enabled;
        if (playerMovement != null) playerMovement.enabled = enabled;
    }
}
