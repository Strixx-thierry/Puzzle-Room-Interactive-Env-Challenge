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
    [Tooltip("Countdown length in seconds. 10 for testing; set to 300 for the real 5:00.")]
    [SerializeField] private float startTime = 10f;

    [Header("UI")]
    [Tooltip("Text at the top of the screen that shows the remaining time.")]
    [SerializeField] private Text timerText;

    [Tooltip("Panel shown when the timer runs out.")]
    [SerializeField] private GameObject losePanel;

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
        UpdateTimerText();
    }

    private void Update()
    {
        if (gameOver) return;

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
        Debug.Log("[GameManager] Lose: lose panel shown, cursor freed.");
    }

    /// <summary>Hook this to the lose-screen Restart button. Reloads the scene.</summary>
    public void Restart()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        Debug.Log("[GameManager] Restart clicked. Reloading scene build index " + index + ".");
        Time.timeScale = 1f; // clear the pause before reloading
        SceneManager.LoadScene(index);
    }
}
