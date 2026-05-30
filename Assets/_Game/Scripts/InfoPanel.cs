using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// A reusable full-screen "chapter" overlay (styled like the win screen). Other scripts
/// call <see cref="Show"/> with a title + body to display a chapter of the story; the
/// player reads it and clicks Close. While it's open the player look/move is frozen and
/// the cursor is freed.
///
/// Setup: one Canvas panel with a Title text, a Body text (big TextArea), and a Close
/// button whose OnClick calls <see cref="Close"/>. Put this script on the panel root.
/// </summary>
public class InfoPanel : MonoBehaviour
{
    [Tooltip("Root object of the overlay (toggled on/off).")]
    public GameObject panelRoot;

    [Tooltip("Title line, e.g. 'Chapter 1'.")]
    public Text titleText;

    [Tooltip("Body text — the chapter / story content.")]
    public Text bodyText;

    [Tooltip("Frozen while the overlay is open (FirstPersonLook, FirstPersonMovement, PlayerInteractor).")]
    public Behaviour[] disableWhileOpen;

    [Tooltip("Player Rigidbody — its motion is zeroed while reading so it can't drift. Auto-found if empty.")]
    public Rigidbody playerBody;

    [Tooltip("Called when the overlay is closed.")]
    public UnityEvent onClosed;

    void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (playerBody == null)
        {
            var move = Object.FindFirstObjectByType<FirstPersonMovement>();
            if (move != null) playerBody = move.GetComponent<Rigidbody>();
        }
    }

    /// <summary>Show a chapter. Usually called by a ChapterClue, not the Inspector directly.</summary>
    public void Show(string title, string body)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
        if (panelRoot != null) panelRoot.SetActive(true);
        SetControl(false);

        // Stop any leftover drift/spin so the player is dead still while reading.
        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Hook the Close button here.</summary>
    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        SetControl(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onClosed?.Invoke();
    }

    void SetControl(bool enabled)
    {
        if (disableWhileOpen == null) return;
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = enabled;
    }
}
