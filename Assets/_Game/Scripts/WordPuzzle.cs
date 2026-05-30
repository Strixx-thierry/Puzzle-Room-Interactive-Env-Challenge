using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Word-arrangement puzzle. Scrambled word buttons are shown; the player clicks them in order
/// to rebuild a sentence. When the whole sentence is placed it's checked automatically.
///
///   • Open()             — shows the panel, resets the board, freezes the player.
///   • PickWord(int)      — wire each word button's OnClick with its index.
///   • Undo() / ResetBoard() — wire the Undo and Reset buttons (Backspace also undoes).
///   • Correct order -> onSolved (wire to ChapterClue.Reveal — the reward).
///
/// Self-contained: the correct order is the grammatical sentence, no external clue needed.
/// Put this on the always-active Canvas; <see cref="canvasRoot"/> is the child panel toggled.
/// </summary>
public class WordPuzzle : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Child panel toggled on/off. Leave THIS script on the always-active Canvas.")]
    public GameObject canvasRoot;

    public Text titleText;

    [Tooltip("Shows the sentence as the player builds it.")]
    public Text answerText;

    public Text statusText;

    [Header("Puzzle")]
    [Tooltip("The correct sentence, words separated by single spaces.")]
    [TextArea(2, 3)] public string correctSentence = "the truth is locked inside this room";

    [Tooltip("The word buttons, in the SCRAMBLED on-screen order.")]
    public Button[] wordButtons;

    [Tooltip("The word printed on each button, parallel to Word Buttons (scrambled order).")]
    public string[] wordLabels;

    [Header("Player control")]
    public Behaviour[] disableWhileOpen;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Order gate (optional)")]
    public PuzzleManager puzzleManager;
    public int puzzleIndex = 2;
    public bool requirePrevious = false;

    [Header("Events")]
    public UnityEvent onSolved;
    public UnityEvent onWrong;

    string[] correctWords;
    readonly List<int> picked = new List<int>();
    bool solved;
    bool isOpen;

    void Awake()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        correctWords = Split(correctSentence);
    }

    void Update()
    {
        if (!isOpen) return;
        if (Input.GetKeyDown(closeKey)) Close();
        if (Input.GetKeyDown(KeyCode.Backspace)) Undo();
    }

    /// <summary>Wire the prop's Interactable.onInteract to this.</summary>
    public void Open()
    {
        if (solved) return;
        if (requirePrevious && puzzleManager != null && puzzleIndex > 0 &&
            !puzzleManager.IsSolved(puzzleIndex - 1)) return;
        if (correctWords == null || correctWords.Length == 0) correctWords = Split(correctSentence);

        if (canvasRoot != null) canvasRoot.SetActive(true);
        isOpen = true;
        SetPlayerControl(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ResetBoard();
    }

    public void Close()
    {
        if (canvasRoot != null) canvasRoot.SetActive(false);
        isOpen = false;
        SetPlayerControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Wire each word button's OnClick to this with that button's index.</summary>
    public void PickWord(int displayIndex)
    {
        if (!isOpen || solved || wordLabels == null) return;
        if (displayIndex < 0 || displayIndex >= wordLabels.Length) return;
        if (picked.Contains(displayIndex)) return;

        picked.Add(displayIndex);
        if (wordButtons != null && displayIndex < wordButtons.Length && wordButtons[displayIndex] != null)
            wordButtons[displayIndex].interactable = false;
        UpdateAnswer();

        if (picked.Count >= correctWords.Length) Check();
    }

    /// <summary>Wire an Undo button here (Backspace also works).</summary>
    public void Undo()
    {
        if (!isOpen || solved || picked.Count == 0) return;
        int last = picked[picked.Count - 1];
        picked.RemoveAt(picked.Count - 1);
        if (wordButtons != null && last < wordButtons.Length && wordButtons[last] != null)
            wordButtons[last].interactable = true;
        UpdateAnswer();
        if (statusText != null) statusText.text = "";
    }

    /// <summary>Wire a Reset button here to clear the whole attempt.</summary>
    public void ResetBoard()
    {
        picked.Clear();
        if (wordButtons != null)
            foreach (var b in wordButtons)
                if (b != null) b.interactable = true;
        UpdateAnswer();
        if (statusText != null) statusText.text = "Click the words in the right order.";
    }

    void Check()
    {
        for (int k = 0; k < correctWords.Length; k++)
        {
            string w = wordLabels[picked[k]];
            if (!string.Equals(w.Trim(), correctWords[k].Trim(),
                System.StringComparison.OrdinalIgnoreCase))
            {
                if (statusText != null) statusText.text = "Not quite — Backspace and rethink the last words.";
                onWrong?.Invoke();
                return;
            }
        }
        solved = true;
        if (statusText != null) statusText.text = "Unlocked!";
        Close();                 // restore control first...
        onSolved?.Invoke();      // ...then the chapter overlay grabs the cursor
    }

    void UpdateAnswer()
    {
        if (answerText == null) return;
        var sb = new System.Text.StringBuilder();
        foreach (int i in picked) { sb.Append(wordLabels[i]); sb.Append(' '); }
        answerText.text = sb.ToString().TrimEnd();
    }

    static string[] Split(string s)
    {
        return string.IsNullOrEmpty(s) ? new string[0]
            : s.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    void SetPlayerControl(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = enabled;
    }
}
