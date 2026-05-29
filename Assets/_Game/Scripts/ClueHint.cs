using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single on-screen hint line used by puzzles 1-3 to reveal a number when solved.
/// Put this on the HUD next to one Text element. A puzzle object's Interactable.onInteract
/// calls <see cref="Show"/> with the message (typed straight into the Inspector event),
/// e.g. "You found a number: 4". The message fades out by itself after a few seconds.
/// </summary>
public class ClueHint : MonoBehaviour
{
    [Tooltip("Text line that displays the hint.")]
    public Text hintText;

    [Tooltip("How long the message stays on screen (seconds).")]
    public float duration = 3.5f;

    Coroutine routine;

    void Awake()
    {
        if (hintText != null) hintText.text = "";
    }

    /// <summary>Show a hint message. Hook this to Interactable.onInteract (string argument).</summary>
    public void Show(string message)
    {
        if (hintText == null) return;
        hintText.text = message;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HideAfter());
    }

    IEnumerator HideAfter()
    {
        // Unscaled so it still counts down if the game is paused.
        yield return new WaitForSecondsRealtime(duration);
        if (hintText != null) hintText.text = "";
    }
}
