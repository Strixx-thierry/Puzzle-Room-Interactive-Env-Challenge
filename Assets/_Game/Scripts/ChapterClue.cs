using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// One chapter of the story, attached to a puzzle item. When the item's Interactable fires
/// (player pressed F), wire its On Interact to this component's <see cref="Reveal"/>:
///   • shows the chapter text in the shared <see cref="InfoPanel"/> overlay,
///   • ticks this puzzle off on the <see cref="PuzzleManager"/> (X / 5),
///   • enables the NEXT item so the puzzles must be done in order.
///
/// Put one of these on each clue object, type its chapter in the Inspector, set its index,
/// and drag the next item into Unlock Next.
/// </summary>
public class ChapterClue : MonoBehaviour
{
    [Header("Overlay")]
    [Tooltip("The shared chapter overlay in the scene.")]
    public InfoPanel infoPanel;

    [Tooltip("Heading, e.g. 'Chapter 1 — Why he did it'.")]
    public string chapterTitle = "Chapter 1";

    [TextArea(4, 12)]
    [Tooltip("The chapter story text. Tuck the puzzle number for the bomb in here somewhere.")]
    public string chapterText = "";

    [Header("Progress")]
    [Tooltip("The room's PuzzleManager.")]
    public PuzzleManager puzzleManager;

    [Tooltip("Which puzzle this is (0-based: chapter 1 = 0).")]
    public int puzzleIndex = 0;

    [Header("Order gate")]
    [Tooltip("If on, this chapter can only be read once the PREVIOUS puzzle (index - 1) is solved. " +
             "Keeps all props visible but forces the player to go in order. The first chapter (index 0) " +
             "is never gated.")]
    public bool requirePreviousChapter = true;

    [Tooltip("Title shown when the player tries this chapter too early.")]
    public string lockedTitle = "Not yet…";

    [TextArea(2, 5)]
    [Tooltip("Message shown when the player tries this chapter too early.")]
    public string lockedMessage = "There are earlier pages you haven't read. Piece the story together in order first.";

    [Header("Optional")]
    [Tooltip("Rarely needed: a GameObject to enable when this is read (Option A style). Leave empty.")]
    public GameObject unlockNext;

    [Tooltip("Extra actions when revealed (open the safe lid, play a sound, etc.).")]
    public UnityEvent onRevealed;

    bool revealed;

    /// <summary>Hook the item's Interactable.onInteract to this.</summary>
    public void Reveal()
    {
        if (revealed) return;

        // Order gate: can't read this chapter until the previous one is read.
        if (requirePreviousChapter && puzzleManager != null && puzzleIndex > 0 &&
            !puzzleManager.IsSolved(puzzleIndex - 1))
        {
            if (infoPanel != null) infoPanel.Show(lockedTitle, lockedMessage);
            return; // not revealed, not counted — they can try again later
        }

        revealed = true;
        if (infoPanel != null) infoPanel.Show(chapterTitle, chapterText);
        if (puzzleManager != null) puzzleManager.Solve(puzzleIndex);
        if (unlockNext != null) unlockNext.SetActive(true);
        onRevealed?.Invoke();
    }
}
