using UnityEngine;

/// <summary>
/// A walk-through win zone. Put this on an empty GameObject at the exit/door with a
/// Box Collider set to "Is Trigger". When the player walks into it, the win panel shows
/// and the game freezes — no raycasting or F-press needed, so it's rock solid.
///
/// Optionally gate it: leave <see cref="locked"/> on and call <see cref="Unlock"/> from a
/// solved puzzle, so the player can only win after the puzzles are done.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    [Tooltip("Panel shown when the player escapes.")]
    public GameObject winPanel;

    [Tooltip("Optional: the countdown timer to stop on win.")]
    public CountdownTimer timer;

    [Tooltip("If true, the player can't win until Unlock() is called (e.g. after the safe puzzle).")]
    public bool locked = false;

    bool won;

    void Reset()
    {
        // Make the collider a trigger automatically when first added.
        GetComponent<Collider>().isTrigger = true;
    }

    public void Unlock() => locked = false;

    void OnTriggerEnter(Collider other)
    {
        if (won || locked) return;
        // Only the player should trigger this (the FPC has FirstPersonMovement).
        if (other.GetComponentInParent<FirstPersonMovement>() == null) return;

        won = true;
        if (timer != null) timer.Stop();
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
