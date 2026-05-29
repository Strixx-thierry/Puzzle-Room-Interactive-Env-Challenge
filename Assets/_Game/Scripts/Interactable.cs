using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Put this on anything the player can interact with (lock box, switch, key, door...).
///
/// When the player's <see cref="PlayerInteractor"/> looks at this object from close
/// range it calls <see cref="OnFocus"/>, which makes every Renderer on the object glow
/// (emission outline) so the player knows it is interactable. Pressing the interact key
/// fires <see cref="onInteract"/> — wire that to whatever should happen (open a keypad
/// canvas, swing a door, pick up the key) straight from the Inspector.
/// </summary>
public class Interactable : MonoBehaviour
{
    [Tooltip("Shown in the on-screen prompt, e.g. \"Lock box\" -> the HUD reads \"[F] Lock box\".")]
    public string promptMessage = "Interact";

    [Header("Highlight")]
    [Tooltip("Glow colour applied to the object's materials while the player is looking at it.")]
    public Color highlightColor = new Color(1f, 0.85f, 0.2f);

    [Tooltip("If true the object can no longer be interacted with after the first interaction.")]
    public bool oneShot = false;

    [Header("Event")]
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

        // Cache each material's starting emission so we can restore it exactly.
        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].material; // .material -> per-object instance, safe to edit
            hadEmission[i] = m.IsKeywordEnabled("_EMISSION");
            originalEmission[i] = m.HasProperty("_EmissionColor")
                ? m.GetColor("_EmissionColor")
                : Color.black;
        }
    }

    /// <summary>True if this object still accepts interaction.</summary>
    public bool CanInteract => !(oneShot && used);

    /// <summary>Called by the PlayerInteractor when the player starts looking at this.</summary>
    public void OnFocus()
    {
        if (highlighted || !CanInteract) return;
        highlighted = true;
        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].material;
            if (!m.HasProperty("_EmissionColor")) continue;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", highlightColor);
        }
    }

    /// <summary>Called when the player looks away — restore the original look.</summary>
    public void OnUnfocus()
    {
        if (!highlighted) return;
        highlighted = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].material;
            if (!m.HasProperty("_EmissionColor")) continue;
            m.SetColor("_EmissionColor", originalEmission[i]);
            if (!hadEmission[i]) m.DisableKeyword("_EMISSION");
        }
    }

    /// <summary>Fire the interaction. Called by the PlayerInteractor on key press.</summary>
    public void Interact()
    {
        if (!CanInteract) return;
        used = true;
        if (oneShot) OnUnfocus();
        onInteract?.Invoke();
    }
}
