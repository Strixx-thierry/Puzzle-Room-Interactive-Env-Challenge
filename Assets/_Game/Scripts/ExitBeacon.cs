using UnityEngine;

/// <summary>
/// Lights up the exit with a green hue once every puzzle is solved, so the player knows
/// where to go. Wire PuzzleManager.onAllSolved -> ExitBeacon.Activate().
///
/// Drag the exit door (and/or a floor marker, arrow, etc.) renderers into <see cref="targets"/>.
/// Optionally drag objects into <see cref="enableWhenActive"/> (e.g. a green spotlight) to switch
/// them on at the same moment.
/// </summary>
public class ExitBeacon : MonoBehaviour
{
    [Tooltip("Renderers that should glow green when all puzzles are solved (e.g. the exit door).")]
    public Renderer[] targets;

    [Tooltip("Optional objects switched ON when activated (a light, an arrow, a particle...).")]
    public GameObject[] enableWhenActive;

    [Tooltip("The green hue.")]
    public Color glowColor = new Color(0.2f, 1f, 0.35f);

    [Tooltip("Base brightness of the glow.")]
    public float intensity = 1.4f;

    [Tooltip("How much it pulses (0 = steady).")]
    public float pulseAmount = 0.5f;

    [Tooltip("Pulse speed.")]
    public float pulseSpeed = 2f;

    bool active;

    /// <summary>Wire PuzzleManager.onAllSolved to this.</summary>
    public void Activate()
    {
        if (active) return;
        active = true;

        if (enableWhenActive != null)
            foreach (var go in enableWhenActive)
                if (go != null) go.SetActive(true);

        Apply(intensity);
    }

    void Update()
    {
        if (!active || pulseAmount <= 0f) return;
        float p = intensity + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        Apply(Mathf.Max(0f, p));
    }

    void Apply(float mult)
    {
        if (targets == null) return;
        Color c = glowColor * mult;
        foreach (var r in targets)
        {
            if (r == null) continue;
            Material m = r.material; // per-object instance
            if (!m.HasProperty("_EmissionColor")) continue;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c);
        }
    }
}
