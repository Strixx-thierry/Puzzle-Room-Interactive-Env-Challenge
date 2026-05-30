using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// "Truth or Lie" quiz. Statements are shown one at a time; the player answers TRUE or FALSE.
/// Get one wrong and the quiz restarts from the top (keeps it tense but always recoverable).
/// Answer them all correctly to solve.
///
///   • Open()              — shows the panel and the first statement, freezes the player.
///   • AnswerTrue() / AnswerFalse() — wire the two answer buttons here.
///   • All correct -> onSolved (wire to ChapterClue.Reveal — the reward).
///
/// Put this on the always-active Canvas; <see cref="canvasRoot"/> is the child panel toggled.
/// </summary>
public class QuizPuzzle : MonoBehaviour
{
    [System.Serializable]
    public class Statement
    {
        [TextArea(2, 4)] public string text = "";
        [Tooltip("Tick if this statement is TRUE.")]
        public bool isTrue;
    }

    [Header("UI")]
    [Tooltip("Child panel toggled on/off. Leave THIS script on the always-active Canvas.")]
    public GameObject canvasRoot;

    public Text titleText;

    [Tooltip("Shows the current statement.")]
    public Text questionText;

    [Tooltip("Progress / feedback line ('Statement 2 of 5', 'Wrong — starting over').")]
    public Text statusText;

    [Header("Quiz")]
    [Tooltip("The statements, asked in order. Tick isTrue for the true ones.")]
    public Statement[] statements;

    [Header("Player control")]
    public Behaviour[] disableWhileOpen;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Order gate (optional)")]
    public PuzzleManager puzzleManager;
    public int puzzleIndex = 1;
    public bool requirePrevious = false;

    [Header("Events")]
    public UnityEvent onSolved;
    public UnityEvent onWrong;

    int index;
    bool solved;
    bool isOpen;

    void Awake()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey)) Close();
    }

    /// <summary>Wire the prop's Interactable.onInteract to this.</summary>
    public void Open()
    {
        if (solved) return;
        if (requirePrevious && puzzleManager != null && puzzleIndex > 0 &&
            !puzzleManager.IsSolved(puzzleIndex - 1)) return;

        if (canvasRoot != null) canvasRoot.SetActive(true);
        isOpen = true;
        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        index = 0;
        ShowCurrent();
    }

    public void Close()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        isOpen = false;
        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AnswerTrue() { Answer(true); }
    public void AnswerFalse() { Answer(false); }

    void Answer(bool playerSaysTrue)
    {
        if (!isOpen || solved || statements == null || statements.Length == 0) return;

        if (statements[index].isTrue == playerSaysTrue)
        {
            index++;
            if (index >= statements.Length)
            {
                solved = true;
                if (statusText != null) statusText.text = "Correct — all true!";
                Close();                 // restore control first...
                onSolved?.Invoke();      // ...then the chapter overlay grabs the cursor
                return;
            }
            ShowCurrent();
        }
        else
        {
            index = 0;
            onWrong?.Invoke();
            ShowCurrent();
            if (statusText != null) statusText.text = "Wrong — start over from the top.";
        }
    }

    void ShowCurrent()
    {
        if (statements == null || statements.Length == 0) return;
        if (questionText != null) questionText.text = statements[index].text;
        if (statusText != null) statusText.text = "Statement " + (index + 1) + " of " + statements.Length;
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = enabled;
    }
}
