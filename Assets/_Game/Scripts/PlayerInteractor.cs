using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Proximity interaction. Each frame it finds the NEAREST <see cref="Interactable"/> within
/// <see cref="range"/> of the player. That object lights up (focus glow) and the HUD shows
/// a "[F] {message}" prompt. Pressing the interact key fires it — no aiming needed, you just
/// have to be standing near it.
///
/// Put this on the player (or the camera). It uses this object's position as the centre of
/// the proximity check, so it does not depend on any camera tag.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Proximity")]
    [Tooltip("How close (metres) the player must be to interact.")]
    public float range = 3f;

    [Tooltip("Which layers count as interactable. Default = Everything.")]
    public LayerMask interactMask = ~0;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.F;

    [Header("UI")]
    [Tooltip("Root object of the prompt (enabled only while near something).")]
    public GameObject promptRoot;

    [Tooltip("Text label that displays the prompt message.")]
    public Text promptText;

    Interactable current;

    void Awake()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
        Debug.Log("[PlayerInteractor] proximity interactor active on '" + name + "', range " + range + "m.");
    }

    void Update()
    {
        Interactable found = FindNearest();

        if (found != current)
        {
            if (current != null) current.OnUnfocus();
            current = found;
            if (current != null) current.OnFocus();
            RefreshPrompt();
            if (current != null)
                Debug.Log("[PlayerInteractor] near: " + current.name + " (press " + interactKey + ")");
        }

        if (current != null && Input.GetKeyDown(interactKey))
        {
            current.Interact();
            if (!current.CanInteract)
            {
                current.OnUnfocus();
                current = null;
                RefreshPrompt();
            }
        }
    }

    /// <summary>Nearest interactable whose collider is within range of this object.</summary>
    Interactable FindNearest()
    {
        Vector3 origin = transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, range, interactMask, QueryTriggerInteraction.Ignore);

        Interactable best = null;
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            Interactable it = h.GetComponentInParent<Interactable>();
            if (it == null || !it.CanInteract) continue;
            float d = (it.transform.position - origin).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = it; }
        }
        return best;
    }

    void RefreshPrompt()
    {
        if (promptRoot != null) promptRoot.SetActive(current != null);
        if (promptText != null && current != null)
            promptText.text = "[" + interactKey + "] " + current.promptMessage;
    }
}
