using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Tracks how many of the puzzles are solved and drives the on-screen
/// "Puzzle Progress: X / 5" counter plus an optional row of indicator icons under it
/// (one icon lights up per solved puzzle). Other scripts call <see cref="Solve"/> when
/// their puzzle is completed; this is idempotent, so solving the same puzzle twice does
/// nothing extra.
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    [Tooltip("Total number of puzzles in the room.")]
    public int totalPuzzles = 5;

    [Header("UI")]
    [Tooltip("Text that shows the X / N progress.")]
    public Text progressText;

    [Tooltip("Optional: one indicator per puzzle, shown under the counter. " +
             "Each is enabled as its puzzle is solved (index 0 = puzzle 1).")]
    public GameObject[] solvedIndicators;

    [Header("Events")]
    [Tooltip("Fires once when every puzzle is solved — wire this to open the key box / unlock the door.")]
    public UnityEvent onAllSolved;

    bool[] solved;
    int solvedCount;

    void Awake()
    {
        solved = new bool[Mathf.Max(totalPuzzles, 1)];
        // Hide all indicators at the start.
        if (solvedIndicators != null)
            foreach (var go in solvedIndicators)
                if (go != null) go.SetActive(false);
        UpdateUI();
    }

    /// <summary>Mark a specific puzzle (1-based or 0-based, your choice) as solved.</summary>
    public void Solve(int puzzleIndex)
    {
        Debug.Log("[PuzzleManager] Solve(" + puzzleIndex + ") called on '" + name +
                  "'. progressText set? " + (progressText != null) +
                  ". array size " + solved.Length);
        if (puzzleIndex < 0 || puzzleIndex >= solved.Length) return;
        if (solved[puzzleIndex]) { Debug.Log("[PuzzleManager] index " + puzzleIndex + " already solved — ignored."); return; }

        solved[puzzleIndex] = true;
        solvedCount++;
        Debug.Log("[PuzzleManager] -> now " + solvedCount + " / " + totalPuzzles);

        if (solvedIndicators != null && puzzleIndex < solvedIndicators.Length &&
            solvedIndicators[puzzleIndex] != null)
            solvedIndicators[puzzleIndex].SetActive(true);

        UpdateUI();

        if (solvedCount >= totalPuzzles)
            onAllSolved?.Invoke();
    }

    /// <summary>Convenience: mark the next unsolved puzzle as done (use when order doesn't matter).</summary>
    public void SolveNext()
    {
        for (int i = 0; i < solved.Length; i++)
            if (!solved[i]) { Solve(i); return; }
    }

    public bool IsSolved(int puzzleIndex) =>
        puzzleIndex >= 0 && puzzleIndex < solved.Length && solved[puzzleIndex];

    public bool AllSolved => solvedCount >= totalPuzzles;

    void UpdateUI()
    {
        if (progressText != null)
            progressText.text = "Puzzle Progress: " + solvedCount + " / " + totalPuzzles;
    }
}
