using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns the round: counts the bomb timer down, shows the lose screen when it hits
/// zero, and restarts the scene.
///
/// The countdown uses scaled time (Time.deltaTime), so it is automatically frozen
/// while the start menu has the game paused (Time.timeScale = 0) and only ticks once
/// the player presses Start.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Timer")]
    [Tooltip("Countdown length in seconds. 300 = the real 5:00; drop to ~10 for quick testing.")]
    [SerializeField] private float startTime = 300f;

    [Header("UI")]
    [Tooltip("Text at the top of the screen that shows the remaining time.")]
    [SerializeField] private Text timerText;

    [Tooltip("Panel shown when the timer runs out.")]
    [SerializeField] private GameObject losePanel;

    [Tooltip("Panel shown when the player escapes (wins).")]
    [SerializeField] private GameObject winPanel;

    [Header("Player")]
    [Tooltip("Disabled when the round ends so the camera stops moving.")]
    [SerializeField] private FirstPersonLook playerLook;

    private float timeRemaining;
    private bool gameOver;

    private void Start()
    {
        timeRemaining = startTime;
        gameOver = false;
        if (losePanel != null) losePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        UpdateTimerText();
    }

    private void Update()
    {
        if (gameOver) return;

        // TEMPORARY: press K in the editor to preview the win screen before the
        // puzzles exist. Remove this block once DoorController calls Win() for real.
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            Win();
            return;
        }
#endif

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerText();
            Lose();
            return;
        }

        UpdateTimerText();
    }

    /// <summary>Format the remaining time as M:SS and push it to the HUD.</summary>
    private void UpdateTimerText()
    {
        if (timerText == null) return;
        int total = Mathf.CeilToInt(timeRemaining);
        int minutes = total / 60;
        int seconds = total % 60;
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }

    /// <summary>Timer hit zero: show the lose screen and pause the game.</summary>
    private void Lose()
    {
        gameOver = true;
        if (losePanel != null) losePanel.SetActive(true);
        if (playerLook != null) playerLook.enabled = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Call this to win the round (e.g. from DoorController once the door opens).
    /// Shows the win screen and freezes the game.
    /// </summary>
    public void Win()
    {
        if (gameOver) return;
        gameOver = true;
        if (winPanel != null) winPanel.SetActive(true);
        if (playerLook != null) playerLook.enabled = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Hook this to the lose-screen Restart button. Reloads the scene.</summary>
    public void Restart()
    {
        Time.timeScale = 1f; // clear the pause before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
