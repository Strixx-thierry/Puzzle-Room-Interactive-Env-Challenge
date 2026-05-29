using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the story / rules panel the moment the room loads and freezes the game until the
/// player reads it. The bomb timer in GameManager uses scaled time, so setting
/// Time.timeScale = 0 here pauses the countdown until the player clicks "Begin".
///
/// Setup: a full-screen Canvas panel with the story text and a Begin button whose OnClick
/// calls <see cref="Begin"/>.
/// </summary>
public class IntroController : MonoBehaviour
{
    [Tooltip("The intro panel (story + rules). Shown on start, hidden on Begin.")]
    public GameObject panel;

    [Tooltip("Optional: text element to fill the story into (or just type it in the panel directly).")]
    public Text bodyText;

    [TextArea(4, 10)]
    public string story =
        "You are trapped in the library.\n\n" +
        "To escape you must solve 5 puzzles before the timer runs out.\n\n" +
        "Walk up to anything that glows and press F to interact.\n" +
        "Solve each puzzle to reveal the next clue, open the lock box,\n" +
        "and find your way out.";

    [Tooltip("Behaviours frozen while the intro is up (FirstPersonLook, FirstPersonMovement, PlayerInteractor).")]
    public Behaviour[] disableWhileOpen;

    void Start()
    {
        if (bodyText != null) bodyText.text = story;
        Show();
    }

    void Show()
    {
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;             // freeze bomb timer + everything
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetPlayerControl(false);
    }

    /// <summary>Hook the Begin button here.</summary>
    public void Begin()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;             // start the countdown
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetPlayerControl(true);
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = enabled;
    }
}
