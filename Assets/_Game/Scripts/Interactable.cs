using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Put this on anything the player can interact with (lock box, switch, key, door...).
///
/// It glows in TWO states so you can always read what's going on:
///   • IDLE  (green, soft)  — the object is interactable and this script is running.
///   • FOCUS (yellow, bright) — the player is looking right at it; press the key now.
///   • (no glow) — the object was used up (one-shot), or something isn't working.
///
/// Pressing the interact key fires <see cref="onInteract"/> — wire that in the Inspector
/// to whatever should happen (open the safe, swing a door, pick up the key).
/// </summary>
public class Interactable : MonoBehaviour
{
    [Tooltip("Shown in the on-screen prompt, e.g. \"Open the safe\".")]
    public string promptMessage = "Interact";

    [Header("Glow — Idle state (always on)")]
    [Tooltip("If true the object shows a constant soft glow so you can spot it as interactable.")]
    public bool glowWhenIdle = true;

    [Tooltip("Idle glow colour. GREEN = 'I'm interactable and awake'.")]
    public Color idleColor = new Color(0.15f, 1f, 0.3f);

    [Tooltip("Idle brightness multiplier.")]
    public float idleIntensity = 0.5f;

    [Header("Glow — Focus state (looking at it)")]
    [Tooltip("Focus glow colour. YELLOW = 'you're looking at me, press F'.")]
    public Color highlightColor = new Color(1f, 0.85f, 0.2f);

    [Tooltip("Focus brightness multiplier (a bit stronger than idle, but not blinding).")]
    public float focusIntensity = 1.6f;

    [Header("Behaviour")]
    [Tooltip("If true the object can no longer be interacted with after the first interaction.")]
    public bool oneShot = false;

    [Tooltip("Called when the player presses the interact key while focused on this object.")]
    public UnityEvent onInteract;

    Renderer[] renderers;
    Color[] originalEmission;
    bool[] hadEmission;
    bool highlighted;
    bool used;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalEmission = new Color[renderers.Length];
        hadEmission = new bool[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].material; // per-object instance, safe to edit
            hadEmission[i] = m.IsKeywordEnabled("_EMISSION");
            originalEmission[i] = m.HasProperty("_EmissionColor")
                ? m.GetColor("_EmissionColor")
                : Color.black;
        }

        // Start in the idle (green) state so interactable objects are visible right away.
        if (glowWhenIdle) ApplyEmission(idleColor * idleIntensity);
    }

    /// <summary>True if this object still accepts interaction.</summary>
    public bool CanInteract => !(oneShot && used);

    /// <summary>Player started looking at this — go to the bright focus glow.</summary>
    public void OnFocus()
    {
        if (highlighted || !CanInteract) return;
        highlighted = true;
        ApplyEmission(highlightColor * focusIntensity);
    }

    /// <summary>Player looked away — drop back to the idle glow (or off if used).</summary>
    public void OnUnfocus()
    {
        if (!highlighted) return;
        highlighted = false;
        if (glowWhenIdle && CanInteract) ApplyEmission(idleColor * idleIntensity);
        else RestoreOriginal();
    }

    /// <summary>Fire the interaction. Called by the PlayerInteractor on key press.</summary>
    public void Interact()
    {
        if (!CanInteract) return;
        used = true;
        Debug.Log("[Interactable] F pressed on '" + name + "' — firing onInteract (" +
                  (onInteract == null ? 0 : onInteract.GetPersistentEventCount()) + " listeners).");
        if (oneShot) { highlighted = false; RestoreOriginal(); } // used up -> stop glowing
        onInteract?.Invoke();
    }

    void ApplyEmission(Color c)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].material;
            if (!m.HasProperty("_EmissionColor")) continue;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c);
        }
    }

    void RestoreOriginal()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].material;
            if (!m.HasProperty("_EmissionColor")) continue;
            m.SetColor("_EmissionColor", originalEmission[i]);
            if (!hadEmission[i]) m.DisableKeyword("_EMISSION");
        }
    }
}
