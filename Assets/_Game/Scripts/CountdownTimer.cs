using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Dead-simple, self-contained countdown timer. Put this component directly on the UI
/// Text object that should show the time — it grabs that Text automatically. It counts
/// down from <see cref="durationSeconds"/>, shows it as M:SS, and when it hits zero it
/// reveals the lose panel and freezes the game.
///
/// Setup (no other scripts needed):
///   1. Add this to your Timer Text object.
///   2. Set Duration Seconds (300 = 5:00).
///   3. Drag your Lose Panel into Lose Panel (optional but recommended).
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    [Tooltip("Start time in seconds. 300 = 5:00.")]
    public float durationSeconds = 300f;

    [Tooltip("Text that shows the time. Leave empty to use the Text on THIS object.")]
    public Text timerText;

    [Tooltip("Panel shown when the timer hits zero.")]
    public GameObject losePanel;

    [Tooltip("If true the countdown ignores Time.timeScale (keeps ticking even if paused).")]
    public bool useUnscaledTime = true;

    float timeLeft;
    bool finished;

    void Awake()
    {
        if (timerText == null) timerText = GetComponent<Text>();
        if (losePanel != null) losePanel.SetActive(false);
        timeLeft = durationSeconds;
        UpdateText();
    }

    void Update()
    {
        if (finished) return;

        timeLeft -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateText();
            Lose();
            return;
        }
        UpdateText();
    }

    void UpdateText()
    {
        if (timerText == null) return;
        int total = Mathf.CeilToInt(timeLeft);
        timerText.text = string.Format("{0}:{1:00}", total / 60, total % 60);
    }

    void Lose()
    {
        finished = true;
        if (losePanel != null) losePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Trigger the lose state from outside (e.g. the bomb running out of attempts).</summary>
    public void ForceLose()
    {
        if (!finished) Lose();
    }

    /// <summary>Hook to a Restart button if you want; reloads the current scene.</summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Call this when the player escapes so the timer stops counting.</summary>
    public void Stop() => finished = true;
}
