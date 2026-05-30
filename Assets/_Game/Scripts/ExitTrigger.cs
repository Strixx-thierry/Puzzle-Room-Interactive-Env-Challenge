using UnityEngine;

/// <summary>
/// A walk-through win zone at the exit door. Put this on an object with a Box Collider set
/// to "Is Trigger". When the player is inside it AND the gate is satisfied, the win panel
/// shows and the game freezes.
///
/// Gating (so the door only opens after the puzzles):
///   • Assign a <see cref="puzzleManager"/> -> the player can only escape once EVERY puzzle
///     in it is solved (PuzzleManager.AllSolved). Leave it empty to allow escaping freely.
///   • Or use the simple <see cref="locked"/> bool and call <see cref="Unlock"/> from a puzzle.
///
/// It re-checks every frame while the player stands inside, so solving the final puzzle
/// while already in the doorway still wins.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    [Tooltip("Panel shown when the player escapes.")]
    public GameObject winPanel;

    [Tooltip("Optional: the countdown timer to stop on win.")]
    public CountdownTimer timer;

    [Header("Gate — escape only when puzzles are done")]
    [Tooltip("If set, the player can only escape once ALL puzzles here are solved (X / 5).")]
    public PuzzleManager puzzleManager;

    [Tooltip("Simple manual lock. If true, Unlock() must be called before escaping.")]
    public bool locked = false;

    bool won;
    bool playerInside;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true; // auto when first added
    }

    public void Unlock() => locked = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<FirstPersonMovement>() != null) playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<FirstPersonMovement>() != null) playerInside = false;
    }

    void Update()
    {
        if (won || !playerInside) return;
        if (locked) return;
        // Gate on the puzzles: if a manager is set, every puzzle must be solved.
        if (puzzleManager != null && !puzzleManager.AllSolved) return;

        Win();
    }

    void Win()
    {
        won = true;
        if (timer != null) timer.Stop();
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
